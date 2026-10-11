namespace Mundialito.Application.Standings;

/// <summary>
/// Fila de la tabla de posiciones. <see cref="Position"/> es la posición oficial
/// (puntos → diferencia de gol → goles a favor), aunque se ordene por otra columna.
/// </summary>
public sealed class StandingResponse
{
    public int Position { get; init; }

    public Guid TeamId { get; init; }

    public string TeamName { get; init; } = string.Empty;

    public int Played { get; init; }

    public int Won { get; init; }

    public int Drawn { get; init; }

    public int Lost { get; init; }

    public int GoalsFor { get; init; }

    public int GoalsAgainst { get; init; }

    public int GoalDifference { get; init; }

    public int Points { get; init; }
}
