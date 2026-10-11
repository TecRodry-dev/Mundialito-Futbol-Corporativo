using Mundialito.Application.Common.Paging;

namespace Mundialito.Application.Standings;

/// <summary>
/// Tabla de posiciones y goleadores (lado Query). Infrastructure lo implementa con Dapper,
/// calculando todo en SQL con las reglas de <c>StandingRules</c>.
/// </summary>
public interface IStandingsReadService
{
    Task<PagedResult<StandingResponse>> GetStandingsAsync(GetStandingsQuery query, CancellationToken cancellationToken);

    Task<PagedResult<TopScorerResponse>> GetTopScorersAsync(GetTopScorersQuery query, CancellationToken cancellationToken);
}
