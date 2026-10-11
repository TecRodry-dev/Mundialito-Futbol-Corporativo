using Dapper;
using Mundialito.Application.Common.Paging;
using Mundialito.Application.Players;

namespace Mundialito.Infrastructure.ReadModel;

internal sealed class PlayerReadService(SqlConnectionFactory connectionFactory) : IPlayerReadService
{
    private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fullName"] = "p.FullName",
        ["shirtNumber"] = "p.ShirtNumber",
        ["position"] = "p.Position",
        ["teamName"] = "t.Name",
        ["goals"] = "Goals"
    };

    private const string FromAndFilters = """
        FROM Players p
        INNER JOIN Teams t ON t.Id = p.TeamId
        WHERE (@TeamId IS NULL OR p.TeamId = @TeamId)
          AND (@Name IS NULL OR p.FullName LIKE @Name)
          AND (@Position IS NULL OR p.Position = @Position)
        """;

    public async Task<PagedResult<PlayerResponse>> GetPlayersAsync(GetPlayersQuery query, CancellationToken cancellationToken)
    {
        var orderBy = SqlSorting.BuildOrderBy(query.Paging, SortColumns, "t.Name ASC, p.ShirtNumber ASC", "p.Id");

        var sql = $"""
            SELECT COUNT(*) {FromAndFilters};

            SELECT p.Id, p.TeamId, t.Name AS TeamName, p.FullName, p.ShirtNumber, p.Position, p.CreatedAtUtc,
                   (SELECT COUNT(*) FROM Goals g WHERE g.PlayerId = p.Id) AS Goals
            {FromAndFilters}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            query.TeamId,
            Name = SqlText.ContainsPattern(query.Name),
            Position = query.Position?.ToString(),
            query.Paging.Offset,
            query.Paging.PageSize
        };

        await using var connection = connectionFactory.Create();
        using var results = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var totalRecords = await results.ReadSingleAsync<int>();
        var players = (await results.ReadAsync<PlayerResponse>()).AsList();

        return new PagedResult<PlayerResponse>(players, query.Paging.PageNumber, query.Paging.PageSize, totalRecords);
    }
}
