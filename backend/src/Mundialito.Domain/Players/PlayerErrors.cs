using Mundialito.Domain.Common;

namespace Mundialito.Domain.Players;

/// <summary>
/// Todos los errores de jugadores en un solo lugar.
/// </summary>
public static class PlayerErrors
{
    public static readonly Error FullNameRequired = Error.Validation(
        "El nombre del jugador es obligatorio.", "Player.FullNameRequired");

    public static readonly Error FullNameTooLong = Error.Validation(
        $"El nombre del jugador no puede superar {Player.FullNameMaxLength} caracteres.", "Player.FullNameTooLong");

    public static readonly Error InvalidShirtNumber = Error.Validation(
        $"El número de camiseta debe estar entre {Player.MinShirtNumber} y {Player.MaxShirtNumber}.", "Player.InvalidShirtNumber");

    public static readonly Error InvalidPosition = Error.Validation(
        "La posición del jugador no es válida.", "Player.InvalidPosition");

    public static readonly Error ShirtNumberTaken = Error.Conflict(
        "Ya hay un jugador con ese número de camiseta en el equipo.", "Player.ShirtNumberTaken");

    public static readonly Error NotFound = Error.NotFound(
        "El jugador no existe.", "Player.NotFound");
}
