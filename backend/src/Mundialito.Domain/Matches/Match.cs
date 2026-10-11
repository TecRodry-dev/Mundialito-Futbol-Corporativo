using Mundialito.Domain.Common;

namespace Mundialito.Domain.Matches;

/// <summary>
/// Partido entre un equipo local y uno visitante. Es la entidad principal (AggregateRoot) de sus goles.
/// </summary>
public sealed class Match : AggregateRoot
{
    public const int VenueMaxLength = 100;
    public const int MinGoalMinute = 1;
    public const int MaxGoalMinute = 120;

    private readonly List<Goal> _goals = [];

    private Match(Guid id, Guid homeTeamId, Guid awayTeamId, DateTime scheduledAt, string? venue, DateTime createdAtUtc)
        : base(id)
    {
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        ScheduledAt = scheduledAt;
        Venue = venue;
        Status = MatchStatus.Scheduled;
        CreatedAtUtc = createdAtUtc;
    }

    // Constructor vacío que usa EF Core.
    private Match()
    {
    }

    public Guid HomeTeamId { get; private set; }

    public Guid AwayTeamId { get; private set; }

    public DateTime ScheduledAt { get; private set; }

    public string? Venue { get; private set; }

    public MatchStatus Status { get; private set; }

    public int? HomeGoals { get; private set; }

    public int? AwayGoals { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<Goal> Goals => _goals.AsReadOnly();

    /// <summary>
    /// Programa un partido nuevo. Que los equipos existan lo valida Application contra la base de datos.
    /// </summary>
    public static Result<Match> Create(Guid homeTeamId, Guid awayTeamId, DateTime scheduledAt, string? venue)
    {
        if (homeTeamId == Guid.Empty || awayTeamId == Guid.Empty)
        {
            return Result.Failure<Match>(MatchErrors.TeamsRequired);
        }

        if (homeTeamId == awayTeamId)
        {
            return Result.Failure<Match>(MatchErrors.SameTeams);
        }

        if (scheduledAt == default)
        {
            return Result.Failure<Match>(MatchErrors.ScheduledAtRequired);
        }

        var trimmedVenue = string.IsNullOrWhiteSpace(venue) ? null : venue.Trim();
        if (trimmedVenue?.Length > VenueMaxLength)
        {
            return Result.Failure<Match>(MatchErrors.VenueTooLong);
        }

        var match = new Match(Guid.NewGuid(), homeTeamId, awayTeamId, scheduledAt, trimmedVenue, DateTime.UtcNow);

        return Result.Success(match);
    }

    /// <summary>
    /// Registra el marcador y los goleadores, y registra el evento <see cref="MatchResultRegistered"/>.
    /// Los goles de cada equipo deben coincidir con el marcador (no se contemplan autogoles).
    /// </summary>
    public Result RegisterResult(int homeGoals, int awayGoals, IReadOnlyCollection<ScoredGoal> scoredGoals)
    {
        if (Status == MatchStatus.Played)
        {
            return Result.Failure(MatchErrors.AlreadyPlayed);
        }

        if (homeGoals < 0 || awayGoals < 0)
        {
            return Result.Failure(MatchErrors.InvalidScore);
        }

        if (scoredGoals.Any(goal => goal.TeamId != HomeTeamId && goal.TeamId != AwayTeamId))
        {
            return Result.Failure(MatchErrors.ScorerNotInMatch);
        }

        if (scoredGoals.Any(goal => goal.Minute is < MinGoalMinute or > MaxGoalMinute))
        {
            return Result.Failure(MatchErrors.InvalidGoalMinute);
        }

        var homeScorers = scoredGoals.Count(goal => goal.TeamId == HomeTeamId);
        var awayScorers = scoredGoals.Count(goal => goal.TeamId == AwayTeamId);
        if (homeScorers != homeGoals || awayScorers != awayGoals)
        {
            return Result.Failure(MatchErrors.GoalsDoNotMatchScore);
        }

        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
        Status = MatchStatus.Played;
        _goals.AddRange(scoredGoals.Select(goal => Goal.Create(Id, goal)));

        RaiseDomainEvent(new MatchResultRegistered(Id, HomeTeamId, AwayTeamId, homeGoals, awayGoals));

        return Result.Success();
    }
}
