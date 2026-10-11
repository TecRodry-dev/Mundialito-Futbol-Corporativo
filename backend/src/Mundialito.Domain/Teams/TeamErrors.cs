using Mundialito.Domain.Common;

namespace Mundialito.Domain.Teams;

/// <summary>
/// Todos los errores de equipos en un solo lugar.
/// </summary>
public static class TeamErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "El nombre del equipo es obligatorio.", "Team.NameRequired");

    public static readonly Error NameTooLong = Error.Validation(
        $"El nombre del equipo no puede superar {Team.NameMaxLength} caracteres.", "Team.NameTooLong");

    public static readonly Error CoachRequired = Error.Validation(
        "El nombre del entrenador es obligatorio.", "Team.CoachRequired");

    public static readonly Error CoachTooLong = Error.Validation(
        $"El nombre del entrenador no puede superar {Team.CoachMaxLength} caracteres.", "Team.CoachTooLong");

    public static readonly Error NotFound = Error.NotFound(
        "El equipo no existe.", "Team.NotFound");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Ya existe un equipo con ese nombre.", "Team.NameAlreadyExists");

    public static readonly Error HasMatches = Error.Conflict(
        "No se puede eliminar un equipo que tiene partidos registrados.", "Team.HasMatches");
}
