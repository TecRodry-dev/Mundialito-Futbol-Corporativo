namespace Mundialito.Domain.Common;

/// <summary>
/// Categoría de un error de negocio. La capa Api la traduce a un código HTTP.
/// </summary>
public enum ErrorType
{
    None = 0,

    /// <summary>Datos inválidos (Api → 400 Bad Request).</summary>
    Validation = 1,

    /// <summary>El recurso no existe (Api → 404 Not Found).</summary>
    NotFound = 2,

    /// <summary>Choca con el estado actual, ej. nombre repetido (Api → 409 Conflict).</summary>
    Conflict = 3
}
