namespace Mundialito.Domain.Common;

/// <summary>
/// Motivo por el que falló una operación: mensaje obligatorio, código opcional y tipo.
/// </summary>
public sealed record Error(string Message, string? Code, ErrorType Type)
{
    /// <summary>Representa la ausencia de error (resultado exitoso).</summary>
    public static readonly Error None = new(string.Empty, null, ErrorType.None);

    public static Error Validation(string message, string? code = null) =>
        new(message, code, ErrorType.Validation);

    public static Error NotFound(string message, string? code = null) =>
        new(message, code, ErrorType.NotFound);

    public static Error Conflict(string message, string? code = null) =>
        new(message, code, ErrorType.Conflict);
}
