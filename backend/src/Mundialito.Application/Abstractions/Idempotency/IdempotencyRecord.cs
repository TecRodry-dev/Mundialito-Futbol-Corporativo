namespace Mundialito.Application.Abstractions.Idempotency;

/// <summary>
/// Respuesta original de un POST, guardada por su Idempotency-Key para devolverla igual si se repite.
/// </summary>
public sealed class IdempotencyRecord
{
    public required string Key { get; init; }

    /// <summary>Huella del request (método + ruta + body) para detectar la misma key con otro payload.</summary>
    public required string RequestHash { get; init; }

    public required int StatusCode { get; init; }

    public required string ResponseBody { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
