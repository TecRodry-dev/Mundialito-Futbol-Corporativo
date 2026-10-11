namespace Mundialito.Application.Players;

public sealed class PlayerResponse
{
    public Guid Id { get; init; }

    public Guid TeamId { get; init; }

    public string TeamName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public int ShirtNumber { get; init; }

    public string Position { get; init; } = string.Empty;

    public int Goals { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
