namespace Mundialito.Domain.Standings;

/// <summary>
/// Calcula la tabla de posiciones: puntos, diferencia de gol y orden.
/// Orden: puntos → diferencia de gol → goles a favor (y nombre, solo para desempatar de forma estable).
/// </summary>
public static class StandingsCalculator
{
    public static IReadOnlyList<TeamStanding> Calculate(
        IReadOnlyDictionary<Guid, string> teams,
        IEnumerable<PlayedMatch> playedMatches)
    {
        var table = teams.ToDictionary(
            team => team.Key,
            team => TeamStanding.Empty(team.Key, team.Value));

        foreach (var match in playedMatches)
        {
            table[match.HomeTeamId] = table[match.HomeTeamId].AddMatch(match.HomeGoals, match.AwayGoals);
            table[match.AwayTeamId] = table[match.AwayTeamId].AddMatch(match.AwayGoals, match.HomeGoals);
        }

        return Sort(table.Values);
    }

    public static IReadOnlyList<TeamStanding> Sort(IEnumerable<TeamStanding> standings) =>
        standings
            .OrderByDescending(standing => standing.Points)
            .ThenByDescending(standing => standing.GoalDifference)
            .ThenByDescending(standing => standing.GoalsFor)
            .ThenBy(standing => standing.TeamName, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
