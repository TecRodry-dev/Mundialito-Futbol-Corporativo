namespace Mundialito.Application.Abstractions.Persistence;

/// <summary>
/// Confirma en una sola transacción todos los cambios hechos con los repositorios
/// y después publica los eventos de dominio. Todo Command guarda con este método.
/// </summary>
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
