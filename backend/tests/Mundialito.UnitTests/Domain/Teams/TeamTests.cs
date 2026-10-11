using Mundialito.Domain.Teams;

namespace Mundialito.UnitTests.Domain.Teams;

public class TeamTests
{
    [Fact]
    public void Create_WithValidData_ReturnsTeam()
    {
        var result = Team.Create("  Sistemas FC  ", "Juan Pérez");

        Assert.True(result.IsSuccess);
        var team = result.Value;
        Assert.NotEqual(Guid.Empty, team.Id);
        Assert.Equal("Sistemas FC", team.Name);
        Assert.Equal("Juan Pérez", team.Coach);
        Assert.Null(team.UpdatedAtUtc);
    }

    [Fact]
    public void Create_RaisesTeamCreatedEvent()
    {
        var team = Team.Create("Sistemas FC", "Juan Pérez").Value;

        var domainEvent = Assert.Single(team.DomainEvents);
        var teamCreated = Assert.IsType<TeamCreated>(domainEvent);
        Assert.Equal(team.Id, teamCreated.TeamId);
        Assert.Equal("Sistemas FC", teamCreated.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ReturnsNameRequired(string name)
    {
        var result = Team.Create(name, "Juan Pérez");

        Assert.True(result.IsFailure);
        Assert.Equal(TeamErrors.NameRequired, result.Error);
    }

    [Fact]
    public void Create_WithTooLongName_ReturnsNameTooLong()
    {
        var longName = new string('A', Team.NameMaxLength + 1);

        var result = Team.Create(longName, "Juan Pérez");

        Assert.True(result.IsFailure);
        Assert.Equal(TeamErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithEmptyCoach_ReturnsCoachRequired()
    {
        var result = Team.Create("Sistemas FC", "   ");

        Assert.True(result.IsFailure);
        Assert.Equal(TeamErrors.CoachRequired, result.Error);
    }

    [Fact]
    public void Update_WithValidData_ReplacesNameAndCoach()
    {
        var team = Team.Create("Sistemas FC", "Juan Pérez").Value;

        var result = team.Update("Sistemas United", "Ana Gómez");

        Assert.True(result.IsSuccess);
        Assert.Equal("Sistemas United", team.Name);
        Assert.Equal("Ana Gómez", team.Coach);
        Assert.NotNull(team.UpdatedAtUtc);
    }

    [Fact]
    public void Update_WithInvalidData_ReturnsFailureAndKeepsOriginalValues()
    {
        var team = Team.Create("Sistemas FC", "Juan Pérez").Value;

        var result = team.Update("", "Ana Gómez");

        Assert.True(result.IsFailure);
        Assert.Equal(TeamErrors.NameRequired, result.Error);
        Assert.Equal("Sistemas FC", team.Name);
        Assert.Equal("Juan Pérez", team.Coach);
    }

    [Fact]
    public void ClearDomainEvents_RemovesRaisedEvents()
    {
        var team = Team.Create("Sistemas FC", "Juan Pérez").Value;

        team.ClearDomainEvents();

        Assert.Empty(team.DomainEvents);
    }
}
