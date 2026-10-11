namespace Mundialito.Domain.Standings;

/// <summary>
/// Fila de la tabla de posiciones de un equipo.
/// </summary>
public sealed record TeamStanding(
    Guid TeamId,
    string TeamName,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst)
{
    public int Points =>
        Won * StandingRules.PointsPerWin +
        Drawn * StandingRules.PointsPerDraw +
        Lost * StandingRules.PointsPerLoss;

    public int GoalDifference => GoalsFor - GoalsAgainst;

    public static TeamStanding Empty(Guid teamId, string teamName) =>
        new(teamId, teamName, 0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Devuelve una fila nueva sumando un partido jugado.
    /// </summary>
    public TeamStanding AddMatch(int goalsFor, int goalsAgainst)
    {
        var outcome = StandingRules.GetOutcome(goalsFor, goalsAgainst);

        return this with
        {
            Played = Played + 1,
            Won = Won + (outcome == MatchOutcome.Win ? 1 : 0),
            Drawn = Drawn + (outcome == MatchOutcome.Draw ? 1 : 0),
            Lost = Lost + (outcome == MatchOutcome.Loss ? 1 : 0),
            GoalsFor = GoalsFor + goalsFor,
            GoalsAgainst = GoalsAgainst + goalsAgainst
        };
    }
}
