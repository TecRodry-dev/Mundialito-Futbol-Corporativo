using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Domain.Common;
using Mundialito.Domain.Matches;

namespace Mundialito.Application.Matches;

/// <summary>
/// Lista de partidos paginada, con filtros por rango de fechas, equipo (local o visitante) y estado.
/// </summary>
public sealed record GetMatchesQuery(
    PageRequest Paging,
    DateTime? From,
    DateTime? To,
    Guid? TeamId,
    MatchStatus? Status) : IQuery<PagedResult<MatchResponse>>
{
    public static readonly IReadOnlyCollection<string> SortFields = ["scheduledAt", "status", "homeTeam", "awayTeam"];

    public static readonly Error InvalidDateRange = Error.Validation(
        "La fecha inicial (from) no puede ser mayor que la fecha final (to).", "Matches.InvalidDateRange");
}

public sealed class GetMatchesQueryHandler(IMatchReadService matchReadService)
    : IQueryHandler<GetMatchesQuery, PagedResult<MatchResponse>>
{
    public async Task<Result<PagedResult<MatchResponse>>> Handle(GetMatchesQuery query, CancellationToken cancellationToken)
    {
        var validation = query.Paging.Validate(GetMatchesQuery.SortFields);
        if (validation.IsFailure)
        {
            return Result.Failure<PagedResult<MatchResponse>>(validation.Error);
        }

        if (query.From > query.To)
        {
            return Result.Failure<PagedResult<MatchResponse>>(GetMatchesQuery.InvalidDateRange);
        }

        var matches = await matchReadService.GetMatchesAsync(query, cancellationToken);

        return Result.Success(matches);
    }
}
