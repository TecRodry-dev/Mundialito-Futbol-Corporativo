using Mundialito.Application.Common.Paging;

namespace Mundialito.Application.Matches;

/// <summary>
/// Lectura de partidos (lado Query). Infrastructure lo implementa con Dapper.
/// </summary>
public interface IMatchReadService
{
    Task<PagedResult<MatchResponse>> GetMatchesAsync(GetMatchesQuery query, CancellationToken cancellationToken);

    Task<MatchDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
