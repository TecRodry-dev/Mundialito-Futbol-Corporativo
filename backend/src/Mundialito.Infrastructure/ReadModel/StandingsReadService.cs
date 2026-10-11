using Dapper;
using Mundialito.Application.Common.Paging;
using Mundialito.Application.Standings;
using Mundialito.Domain.Matches;
using Mundialito.Domain.Standings;

namespace Mundialito.Infrastructure.ReadModel;

/// <summary>
/// Tabla de posiciones y goleadores calculados completamente en SQL (agregación, orden y paginación),
/// usando como parámetros las reglas del dominio (<see cref="StandingRules"/>).
/// </summary>
internal sealed class StandingsReadService(SqlConnectionFactory connectionFactory) : IStandingsReadService
{
    private static readonly Dictionary<string, string> StandingSortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["position"] = "[Position]",
        ["points"] = "Points",
        ["goalDifference"] = "GoalDifference",
        ["goalsFor"] = "GoalsFor",
        ["played"] = "Played",
        ["teamName"] = "TeamName"
    };

    private static readonly Dictionary<string, string> TopScorerSortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["goals"] = "Goals",
        ["playerName"] = "PlayerName",
        ["teamName"] = "TeamName"
    };

    // Cada partido jugado aporta dos filas (local y visitante). Luego se suma por equipo,
    // se calculan puntos y diferencia de gol, y se numera la posición oficial.
    private const string StandingsCte = """
        WITH TeamResults AS (
            SELECT m.HomeTeamId AS TeamId, m.HomeGoals AS GoalsFor, m.AwayGoals AS GoalsAgainst
            FROM Matches m
            WHERE m.Status = @PlayedStatus
            UNION ALL
            SELECT m.AwayTeamId, m.AwayGoals, m.HomeGoals
            FROM Matches m
            WHERE m.Status = @PlayedStatus
        ),
        Totals AS (
            SELECT t.Id AS TeamId,
                   t.Name AS TeamName,
                   COUNT(r.TeamId) AS Played,
                   SUM(CASE WHEN r.GoalsFor > r.GoalsAgainst THEN 1 ELSE 0 END) AS Won,
                   SUM(CASE WHEN r.GoalsFor = r.GoalsAgainst THEN 1 ELSE 0 END) AS Drawn,
                   SUM(CASE WHEN r.GoalsFor < r.GoalsAgainst THEN 1 ELSE 0 END) AS Lost,
                   COALESCE(SUM(r.GoalsFor), 0) AS GoalsFor,
                   COALESCE(SUM(r.GoalsAgainst), 0) AS GoalsAgainst
            FROM Teams t
            LEFT JOIN TeamResults r ON r.TeamId = t.Id
            GROUP BY t.Id, t.Name
        ),
        Scored AS (
            SELECT Totals.*,
                   GoalsFor - GoalsAgainst AS GoalDifference,
                   Won * @PointsPerWin + Drawn * @PointsPerDraw + Lost * @PointsPerLoss AS Points
            FROM Totals
        ),
        Positioned AS (
            SELECT Scored.*,
                   CAST(ROW_NUMBER() OVER (
                       ORDER BY Points DESC, GoalDifference DESC, GoalsFor DESC, TeamName ASC) AS int) AS [Position]
            FROM Scored
        )
        """;

    private const string TopScorersCte = """
        WITH PlayerGoals AS (
            SELECT g.PlayerId, COUNT(*) AS Goals
            FROM Goals g
            GROUP BY g.PlayerId
        ),
        Ranked AS (
            SELECT CAST(RANK() OVER (ORDER BY pg.Goals DESC) AS int) AS [Position],
                   p.Id AS PlayerId,
                   p.FullName AS PlayerName,
                   t.Id AS TeamId,
                   t.Name AS TeamName,
                   pg.Goals
            FROM PlayerGoals pg
            INNER JOIN Players p ON p.Id = pg.PlayerId
            INNER JOIN Teams t ON t.Id = p.TeamId
        )
        """;

    public async Task<PagedResult<StandingResponse>> GetStandingsAsync(GetStandingsQuery query, CancellationToken cancellationToken)
    {
        var orderBy = SqlSorting.BuildOrderBy(query.Paging, StandingSortColumns, "[Position] ASC", "TeamId");

        var sql = $"""
            {StandingsCte}
            SELECT COUNT(*) FROM Positioned WHERE (@TeamName IS NULL OR TeamName LIKE @TeamName);

            {StandingsCte}
            SELECT [Position], TeamId, TeamName, Played, Won, Drawn, Lost,
                   GoalsFor, GoalsAgainst, GoalDifference, Points
            FROM Positioned
            WHERE (@TeamName IS NULL OR TeamName LIKE @TeamName)
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            PlayedStatus = MatchStatus.Played.ToString(),
            StandingRules.PointsPerWin,
            StandingRules.PointsPerDraw,
            StandingRules.PointsPerLoss,
            TeamName = SqlText.ContainsPattern(query.TeamName),
            query.Paging.Offset,
            query.Paging.PageSize
        };

        await using var connection = connectionFactory.Create();
        using var results = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var totalRecords = await results.ReadSingleAsync<int>();
        var standings = (await results.ReadAsync<StandingResponse>()).AsList();

        return new PagedResult<StandingResponse>(standings, query.Paging.PageNumber, query.Paging.PageSize, totalRecords);
    }

    public async Task<PagedResult<TopScorerResponse>> GetTopScorersAsync(GetTopScorersQuery query, CancellationToken cancellationToken)
    {
        var orderBy = SqlSorting.BuildOrderBy(query.Paging, TopScorerSortColumns, "Goals DESC, PlayerName ASC", "PlayerId");

        const string filters = """
            (@TeamId IS NULL OR TeamId = @TeamId)
            AND (@PlayerName IS NULL OR PlayerName LIKE @PlayerName)
            """;

        var sql = $"""
            {TopScorersCte}
            SELECT COUNT(*) FROM Ranked WHERE {filters};

            {TopScorersCte}
            SELECT [Position], PlayerId, PlayerName, TeamId, TeamName, Goals
            FROM Ranked
            WHERE {filters}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            query.TeamId,
            PlayerName = SqlText.ContainsPattern(query.PlayerName),
            query.Paging.Offset,
            query.Paging.PageSize
        };

        await using var connection = connectionFactory.Create();
        using var results = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var totalRecords = await results.ReadSingleAsync<int>();
        var topScorers = (await results.ReadAsync<TopScorerResponse>()).AsList();

        return new PagedResult<TopScorerResponse>(topScorers, query.Paging.PageNumber, query.Paging.PageSize, totalRecords);
    }
}
