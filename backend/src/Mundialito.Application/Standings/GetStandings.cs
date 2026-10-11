using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Domain.Common;

namespace Mundialito.Application.Standings;

/// <summary>
/// Tabla de posiciones paginada. Sin sortBy usa el orden oficial; filtro opcional por nombre de equipo.
/// </summary>
public sealed record GetStandingsQuery(PageRequest Paging, string? TeamName)
    : IQuery<PagedResult<StandingResponse>>
{
    public static readonly IReadOnlyCollection<string> SortFields =
        ["position", "points", "goalDifference", "goalsFor", "played", "teamName"];
}

public sealed class GetStandingsQueryHandler(IStandingsReadService standingsReadService)
    : IQueryHandler<GetStandingsQuery, PagedResult<StandingResponse>>
{
    public async Task<Result<PagedResult<StandingResponse>>> Handle(GetStandingsQuery query, CancellationToken cancellationToken)
    {
        var validation = query.Paging.Validate(GetStandingsQuery.SortFields);
        if (validation.IsFailure)
        {
            return Result.Failure<PagedResult<StandingResponse>>(validation.Error);
        }

        var standings = await standingsReadService.GetStandingsAsync(query, cancellationToken);

        return Result.Success(standings);
    }
}
