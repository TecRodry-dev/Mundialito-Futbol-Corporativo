using Mundialito.Application.Common.Paging;

namespace Mundialito.Application.Players;

/// <summary>
/// Lectura de jugadores (lado Query). Infrastructure lo implementa con Dapper.
/// </summary>
public interface IPlayerReadService
{
    Task<PagedResult<PlayerResponse>> GetPlayersAsync(GetPlayersQuery query, CancellationToken cancellationToken);
}
