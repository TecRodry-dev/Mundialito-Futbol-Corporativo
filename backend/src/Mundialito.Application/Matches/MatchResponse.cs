namespace Mundialito.Application.Matches;

public class MatchResponse
{
    public Guid Id { get; init; }

    public Guid HomeTeamId { get; init; }

    public string HomeTeamName { get; init; } = string.Empty;

    public Guid AwayTeamId { get; init; }

    public string AwayTeamName { get; init; } = string.Empty;

    public DateTime ScheduledAt { get; init; }

    public string? Venue { get; init; }

    public string Status { get; init; } = string.Empty;

    public int? HomeGoals { get; init; }

    public int? AwayGoals { get; init; }
}

/// <summary>
/// Detalle de un partido: sus datos más la lista de goles.
/// </summary>
public sealed class MatchDetailResponse : MatchResponse
{
    public IReadOnlyList<MatchGoalResponse> Goals { get; set; } = [];
}

public sealed class MatchGoalResponse
{
    public Guid PlayerId { get; init; }

    public string PlayerName { get; init; } = string.Empty;

    public Guid TeamId { get; init; }

    public string TeamName { get; init; } = string.Empty;

    public int? Minute { get; init; }
}
