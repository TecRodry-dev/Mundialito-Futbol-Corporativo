using Mundialito.Domain.Matches;

namespace Mundialito.Application.Abstractions.Persistence;

/// <summary>
/// Acceso de escritura a partidos (lado Command, EF Core). Los cambios se guardan con <see cref="IUnitOfWork"/>.
/// </summary>
public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> AnyForTeamAsync(Guid teamId, CancellationToken cancellationToken);

    void Add(Match match);
}
