namespace Mundialito.Domain.Standings;

/// <summary>
/// Reglas del torneo para la tabla: victoria 3, empate 1, derrota 0.
/// La consulta Dapper de la tabla usa estas mismas constantes como parámetros.
/// </summary>
public static class StandingRules
{
    public const int PointsPerWin = 3;
    public const int PointsPerDraw = 1;
    public const int PointsPerLoss = 0;

    public static MatchOutcome GetOutcome(int goalsFor, int goalsAgainst)
    {
        if (goalsFor > goalsAgainst)
        {
            return MatchOutcome.Win;
        }

        return goalsFor == goalsAgainst ? MatchOutcome.Draw : MatchOutcome.Loss;
    }

    public static int GetPoints(MatchOutcome outcome) => outcome switch
    {
        MatchOutcome.Win => PointsPerWin,
        MatchOutcome.Draw => PointsPerDraw,
        _ => PointsPerLoss
    };
}
