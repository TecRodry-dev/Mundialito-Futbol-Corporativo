namespace Mundialito.Api.Contracts;

public sealed record CreateMatchRequest(Guid HomeTeamId, Guid AwayTeamId, DateTime ScheduledAt, string? Venue);

public sealed record RegisterMatchResultRequest(int HomeGoals, int AwayGoals, IReadOnlyList<GoalRequest>? Goals);

public sealed record GoalRequest(Guid PlayerId, int? Minute);
