namespace Mundialito.Domain.Matches;

/// <summary>
/// Datos de un gol a registrar: quién lo marcó, para qué equipo y en qué minuto (opcional).
/// </summary>
public sealed record ScoredGoal(Guid PlayerId, Guid TeamId, int? Minute);
