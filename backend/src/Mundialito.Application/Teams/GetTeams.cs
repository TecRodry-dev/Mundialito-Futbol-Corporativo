using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Domain.Common;

namespace Mundialito.Application.Teams;

/// <summary>
/// Lista de equipos paginada, con filtros por nombre y entrenador (búsqueda parcial).
/// </summary>
public sealed record GetTeamsQuery(PageRequest Paging, string? Name, string? Coach)
    : IQuery<PagedResult<TeamResponse>>
{
    public static readonly IReadOnlyCollection<string> SortFields = ["name", "coach", "playersCount", "createdAt"];
}

public sealed class GetTeamsQueryHandler(ITeamReadService teamReadService)
    : IQueryHandler<GetTeamsQuery, PagedResult<TeamResponse>>
{
    public async Task<Result<PagedResult<TeamResponse>>> Handle(GetTeamsQuery query, CancellationToken cancellationToken)
    {
        var validation = query.Paging.Validate(GetTeamsQuery.SortFields);
        if (validation.IsFailure)
        {
            return Result.Failure<PagedResult<TeamResponse>>(validation.Error);
        }

        var teams = await teamReadService.GetTeamsAsync(query, cancellationToken);

        return Result.Success(teams);
    }
}
