namespace Mundialito.Application.Abstractions.Idempotency;

/// <summary>
/// Respuesta original de un POST, guardada por su Idempotency-Key para devolverla igual si se repite.
/// </summary>
public sealed class IdempotencyRecord
{
    public string Key { get; init; } = string.Empty;

    /// <summary>Huella del request (método + ruta + body) para detectar la misma key con otro payload.</summary>
    public string RequestHash { get; init; } = string.Empty;

    public int StatusCode { get; init; }

    public string ResponseBody { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }
}
