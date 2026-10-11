namespace Mundialito.Application.Standings;

public sealed class TopScorerResponse
{
    public int Position { get; init; }

    public Guid PlayerId { get; init; }

    public string PlayerName { get; init; } = string.Empty;

    public Guid TeamId { get; init; }

    public string TeamName { get; init; } = string.Empty;

    public int Goals { get; init; }
}
