using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Domain.Common;

namespace Mundialito.Application.Standings;

/// <summary>
/// Goleadores paginados. Sin sortBy ordena por más goles; filtros por equipo y nombre del jugador.
/// </summary>
public sealed record GetTopScorersQuery(PageRequest Paging, Guid? TeamId, string? PlayerName)
    : IQuery<PagedResult<TopScorerResponse>>
{
    public static readonly IReadOnlyCollection<string> SortFields = ["goals", "playerName", "teamName"];
}

public sealed class GetTopScorersQueryHandler(IStandingsReadService standingsReadService)
    : IQueryHandler<GetTopScorersQuery, PagedResult<TopScorerResponse>>
{
    public async Task<Result<PagedResult<TopScorerResponse>>> Handle(GetTopScorersQuery query, CancellationToken cancellationToken)
    {
        var validation = query.Paging.Validate(GetTopScorersQuery.SortFields);
        if (validation.IsFailure)
        {
            return Result.Failure<PagedResult<TopScorerResponse>>(validation.Error);
        }

        var topScorers = await standingsReadService.GetTopScorersAsync(query, cancellationToken);

        return Result.Success(topScorers);
    }
}
