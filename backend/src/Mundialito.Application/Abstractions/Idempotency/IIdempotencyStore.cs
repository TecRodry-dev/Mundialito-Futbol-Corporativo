namespace Mundialito.Application.Abstractions.Idempotency;

/// <summary>
/// Almacén persistente de las Idempotency-Key ya procesadas.
/// </summary>
public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken);

    Task SaveAsync(IdempotencyRecord record, CancellationToken cancellationToken);
}
