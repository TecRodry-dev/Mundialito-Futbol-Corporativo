using Mundialito.Domain.Matches;

namespace Mundialito.UnitTests.Domain.Matches;

public class MatchTests
{
    private static readonly Guid HomeTeamId = Guid.NewGuid();
    private static readonly Guid AwayTeamId = Guid.NewGuid();

    private static Match CreateMatch() =>
        Match.Create(HomeTeamId, AwayTeamId, new DateTime(2026, 10, 15, 18, 0, 0), "Cancha 1").Value;

    private static ScoredGoal HomeGoal(int? minute = null) => new(Guid.NewGuid(), HomeTeamId, minute);

    private static ScoredGoal AwayGoal(int? minute = null) => new(Guid.NewGuid(), AwayTeamId, minute);

    [Fact]
    public void Create_WithValidData_ReturnsScheduledMatchWithoutScore()
    {
        var result = Match.Create(HomeTeamId, AwayTeamId, new DateTime(2026, 10, 15, 18, 0, 0), "  Cancha 1  ");

        Assert.True(result.IsSuccess);
        var match = result.Value;
        Assert.Equal(MatchStatus.Scheduled, match.Status);
        Assert.Equal("Cancha 1", match.Venue);
        Assert.Null(match.HomeGoals);
        Assert.Null(match.AwayGoals);
        Assert.Empty(match.Goals);
    }

    [Fact]
    public void Create_WithSameTeams_ReturnsSameTeams()
    {
        var result = Match.Create(HomeTeamId, HomeTeamId, new DateTime(2026, 10, 15), null);

        Assert.Equal(MatchErrors.SameTeams, result.Error);
    }

    [Fact]
    public void Create_WithoutTeam_ReturnsTeamsRequired()
    {
        var result = Match.Create(Guid.Empty, AwayTeamId, new DateTime(2026, 10, 15), null);

        Assert.Equal(MatchErrors.TeamsRequired, result.Error);
    }

    [Fact]
    public void RegisterResult_WithValidData_MarksMatchAsPlayed()
    {
        var match = CreateMatch();

        var result = match.RegisterResult(2, 1, [HomeGoal(10), HomeGoal(55), AwayGoal(80)]);

        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.Played, match.Status);
        Assert.Equal(2, match.HomeGoals);
        Assert.Equal(1, match.AwayGoals);
        Assert.Equal(3, match.Goals.Count);
        Assert.All(match.Goals, goal => Assert.Equal(match.Id, goal.MatchId));
    }

    [Fact]
    public void RegisterResult_RaisesMatchResultRegisteredEvent()
    {
        var match = CreateMatch();

        match.RegisterResult(2, 1, [HomeGoal(), HomeGoal(), AwayGoal()]);

        var domainEvent = Assert.Single(match.DomainEvents);
        var registered = Assert.IsType<MatchResultRegistered>(domainEvent);
        Assert.Equal(match.Id, registered.MatchId);
        Assert.Equal(HomeTeamId, registered.HomeTeamId);
        Assert.Equal(AwayTeamId, registered.AwayTeamId);
        Assert.Equal(2, registered.HomeGoals);
        Assert.Equal(1, registered.AwayGoals);
    }

    [Fact]
    public void RegisterResult_GoallessDraw_NeedsNoScorers()
    {
        var match = CreateMatch();

        var result = match.RegisterResult(0, 0, []);

        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.Played, match.Status);
    }

    [Fact]
    public void RegisterResult_WhenAlreadyPlayed_ReturnsConflictAndKeepsFirstResult()
    {
        var match = CreateMatch();
        match.RegisterResult(1, 0, [HomeGoal()]);

        var result = match.RegisterResult(0, 3, [AwayGoal(), AwayGoal(), AwayGoal()]);

        Assert.Equal(MatchErrors.AlreadyPlayed, result.Error);
        Assert.Equal(1, match.HomeGoals);
        Assert.Equal(0, match.AwayGoals);
        Assert.Single(match.DomainEvents);
    }

    [Fact]
    public void RegisterResult_WithNegativeGoals_ReturnsInvalidScore()
    {
        var match = CreateMatch();

        var result = match.RegisterResult(-1, 0, []);

        Assert.Equal(MatchErrors.InvalidScore, result.Error);
    }

    [Fact]
    public void RegisterResult_WithScorerFromAnotherTeam_ReturnsScorerNotInMatch()
    {
        var match = CreateMatch();
        var goalFromOtherTeam = new ScoredGoal(Guid.NewGuid(), Guid.NewGuid(), null);

        var result = match.RegisterResult(1, 0, [goalFromOtherTeam]);

        Assert.Equal(MatchErrors.ScorerNotInMatch, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(121)]
    public void RegisterResult_WithMinuteOutOfRange_ReturnsInvalidGoalMinute(int minute)
    {
        var match = CreateMatch();

        var result = match.RegisterResult(1, 0, [HomeGoal(minute)]);

        Assert.Equal(MatchErrors.InvalidGoalMinute, result.Error);
    }

    [Fact]
    public void RegisterResult_WhenScorersDoNotMatchScore_FailsAndDoesNotChangeMatch()
    {
        var match = CreateMatch();

        var result = match.RegisterResult(2, 0, [HomeGoal()]);

        Assert.Equal(MatchErrors.GoalsDoNotMatchScore, result.Error);
        Assert.Equal(MatchStatus.Scheduled, match.Status);
        Assert.Null(match.HomeGoals);
        Assert.Empty(match.Goals);
        Assert.Empty(match.DomainEvents);
    }
}
