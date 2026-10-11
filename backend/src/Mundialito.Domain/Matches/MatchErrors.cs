using Mundialito.Domain.Common;

namespace Mundialito.Domain.Matches;

/// <summary>
/// Todos los errores de partidos en un solo lugar.
/// </summary>
public static class MatchErrors
{
    public static readonly Error TeamsRequired = Error.Validation(
        "Debe indicar el equipo local y el equipo visitante.", "Match.TeamsRequired");

    public static readonly Error SameTeams = Error.Validation(
        "Un equipo no puede jugar contra sí mismo.", "Match.SameTeams");

    public static readonly Error ScheduledAtRequired = Error.Validation(
        "La fecha del partido es obligatoria.", "Match.ScheduledAtRequired");

    public static readonly Error VenueTooLong = Error.Validation(
        $"El nombre de la cancha no puede superar {Match.VenueMaxLength} caracteres.", "Match.VenueTooLong");

    public static readonly Error NotFound = Error.NotFound(
        "El partido no existe.", "Match.NotFound");

    public static readonly Error AlreadyPlayed = Error.Conflict(
        "El partido ya tiene un resultado registrado.", "Match.AlreadyPlayed");

    public static readonly Error InvalidScore = Error.Validation(
        "Los goles no pueden ser negativos.", "Match.InvalidScore");

    public static readonly Error ScorerNotInMatch = Error.Validation(
        "Cada goleador debe pertenecer a uno de los dos equipos del partido.", "Match.ScorerNotInMatch");

    public static readonly Error InvalidGoalMinute = Error.Validation(
        $"El minuto del gol debe estar entre {Match.MinGoalMinute} y {Match.MaxGoalMinute}.", "Match.InvalidGoalMinute");

    public static readonly Error GoalsDoNotMatchScore = Error.Validation(
        "La cantidad de goleadores de cada equipo debe coincidir con el marcador.", "Match.GoalsDoNotMatchScore");
}
