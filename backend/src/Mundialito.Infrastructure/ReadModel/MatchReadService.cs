using Dapper;
using Mundialito.Application.Common.Paging;
using Mundialito.Application.Matches;

namespace Mundialito.Infrastructure.ReadModel;

internal sealed class MatchReadService(SqlConnectionFactory connectionFactory) : IMatchReadService
{
    private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["scheduledAt"] = "m.ScheduledAt",
        ["status"] = "m.Status",
        ["homeTeam"] = "homeTeam.Name",
        ["awayTeam"] = "awayTeam.Name"
    };

    private const string SelectColumns = """
        m.Id, m.HomeTeamId, homeTeam.Name AS HomeTeamName, m.AwayTeamId, awayTeam.Name AS AwayTeamName,
        m.ScheduledAt, m.Venue, m.Status, m.HomeGoals, m.AwayGoals
        """;

    private const string FromTeams = """
        FROM Matches m
        INNER JOIN Teams homeTeam ON homeTeam.Id = m.HomeTeamId
        INNER JOIN Teams awayTeam ON awayTeam.Id = m.AwayTeamId
        """;

    // Las fechas se filtran por día completo: from = desde las 00:00, to = hasta el final de ese día.
    private const string Filters = """
        WHERE (@From IS NULL OR m.ScheduledAt >= @From)
          AND (@ToExclusive IS NULL OR m.ScheduledAt < @ToExclusive)
          AND (@TeamId IS NULL OR m.HomeTeamId = @TeamId OR m.AwayTeamId = @TeamId)
          AND (@Status IS NULL OR m.Status = @Status)
        """;

    public async Task<PagedResult<MatchResponse>> GetMatchesAsync(GetMatchesQuery query, CancellationToken cancellationToken)
    {
        var orderBy = SqlSorting.BuildOrderBy(query.Paging, SortColumns, "m.ScheduledAt ASC", "m.Id");

        var sql = $"""
            SELECT COUNT(*) {FromTeams} {Filters};

            SELECT {SelectColumns}
            {FromTeams}
            {Filters}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            From = query.From?.Date,
            ToExclusive = query.To?.Date.AddDays(1),
            query.TeamId,
            Status = query.Status?.ToString(),
            query.Paging.Offset,
            query.Paging.PageSize
        };

        await using var connection = connectionFactory.Create();
        using var results = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var totalRecords = await results.ReadSingleAsync<int>();
        var matches = (await results.ReadAsync<MatchResponse>()).AsList();

        return new PagedResult<MatchResponse>(matches, query.Paging.PageNumber, query.Paging.PageSize, totalRecords);
    }

    public async Task<MatchDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT {SelectColumns}
            {FromTeams}
            WHERE m.Id = @Id;

            SELECT g.PlayerId, p.FullName AS PlayerName, g.TeamId, t.Name AS TeamName, g.Minute
            FROM Goals g
            INNER JOIN Players p ON p.Id = g.PlayerId
            INNER JOIN Teams t ON t.Id = g.TeamId
            WHERE g.MatchId = @Id
            ORDER BY CASE WHEN g.Minute IS NULL THEN 1 ELSE 0 END, g.Minute;
            """;

        await using var connection = connectionFactory.Create();
        using var results = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        var match = await results.ReadSingleOrDefaultAsync<MatchDetailResponse>();
        if (match is null)
        {
            return null;
        }

        match.Goals = (await results.ReadAsync<MatchGoalResponse>()).AsList();

        return match;
    }
}
