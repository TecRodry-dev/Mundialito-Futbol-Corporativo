namespace Mundialito.Domain.Standings;

/// <summary>
/// Marcador de un partido ya jugado, lo único que necesita la tabla.
/// </summary>
public sealed record PlayedMatch(Guid HomeTeamId, Guid AwayTeamId, int HomeGoals, int AwayGoals);
