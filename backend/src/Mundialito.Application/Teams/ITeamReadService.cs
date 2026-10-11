using Mundialito.Application.Common.Paging;

namespace Mundialito.Application.Teams;

/// <summary>
/// Lectura de equipos (lado Query). Infrastructure lo implementa con Dapper.
/// </summary>
public interface ITeamReadService
{
    Task<PagedResult<TeamResponse>> GetTeamsAsync(GetTeamsQuery query, CancellationToken cancellationToken);

    Task<TeamResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
