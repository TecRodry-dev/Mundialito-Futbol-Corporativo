using Mundialito.Domain.Teams;

namespace Mundialito.Application.Abstractions.Persistence;

/// <summary>
/// Acceso de escritura a equipos (lado Command, EF Core). Los cambios se guardan con <see cref="IUnitOfWork"/>.
/// </summary>
public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Team?> GetByIdWithPlayersAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, Guid? excludingTeamId, CancellationToken cancellationToken);

    /// <summary>
    /// Devuelve a qué equipo pertenece cada jugador (clave: PlayerId, valor: TeamId).
    /// Los jugadores que no existen no aparecen en el resultado.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, Guid>> GetTeamIdsByPlayerIdsAsync(
        IReadOnlyCollection<Guid> playerIds,
        CancellationToken cancellationToken);

    void Add(Team team);

    void Remove(Team team);
}
