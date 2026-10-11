namespace Mundialito.Application.Teams;

public sealed class TeamResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Coach { get; init; } = string.Empty;

    public int PlayersCount { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}
