using Dapper;
using Mundialito.Application.Common.Paging;
using Mundialito.Application.Teams;

namespace Mundialito.Infrastructure.ReadModel;

internal sealed class TeamReadService(SqlConnectionFactory connectionFactory) : ITeamReadService
{
    private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = "t.Name",
        ["coach"] = "t.Coach",
        ["playersCount"] = "PlayersCount",
        ["createdAt"] = "t.CreatedAtUtc"
    };

    private const string SelectColumns = """
        t.Id, t.Name, t.Coach, t.CreatedAtUtc, t.UpdatedAtUtc,
        (SELECT COUNT(*) FROM Players p WHERE p.TeamId = t.Id) AS PlayersCount
        """;

    private const string Filters = """
        (@Name IS NULL OR t.Name LIKE @Name)
        AND (@Coach IS NULL OR t.Coach LIKE @Coach)
        """;

    public async Task<PagedResult<TeamResponse>> GetTeamsAsync(GetTeamsQuery query, CancellationToken cancellationToken)
    {
        var orderBy = SqlSorting.BuildOrderBy(query.Paging, SortColumns, "t.Name ASC", "t.Id");

        var sql = $"""
            SELECT COUNT(*) FROM Teams t WHERE {Filters};

            SELECT {SelectColumns}
            FROM Teams t
            WHERE {Filters}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            Name = SqlText.ContainsPattern(query.Name),
            Coach = SqlText.ContainsPattern(query.Coach),
            query.Paging.Offset,
            query.Paging.PageSize
        };

        await using var connection = connectionFactory.Create();
        using var results = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var totalRecords = await results.ReadSingleAsync<int>();
        var teams = (await results.ReadAsync<TeamResponse>()).AsList();

        return new PagedResult<TeamResponse>(teams, query.Paging.PageNumber, query.Paging.PageSize, totalRecords);
    }

    public async Task<TeamResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"SELECT {SelectColumns} FROM Teams t WHERE t.Id = @Id;";

        await using var connection = connectionFactory.Create();

        return await connection.QuerySingleOrDefaultAsync<TeamResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
