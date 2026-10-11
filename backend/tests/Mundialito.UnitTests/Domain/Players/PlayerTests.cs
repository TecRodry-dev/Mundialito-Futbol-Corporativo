using Mundialito.Domain.Players;
using Mundialito.Domain.Teams;

namespace Mundialito.UnitTests.Domain.Players;

public class PlayerTests
{
    private static Team CreateTeam(string name = "Sistemas FC") =>
        Team.Create(name, "Juan Pérez").Value;

    [Fact]
    public void AddPlayer_WithValidData_AddsPlayerToTeam()
    {
        var team = CreateTeam();

        var result = team.AddPlayer("  Carlos Ruiz  ", 10, PlayerPosition.Forward);

        Assert.True(result.IsSuccess);
        var player = Assert.Single(team.Players);
        Assert.Equal(result.Value.Id, player.Id);
        Assert.Equal(team.Id, player.TeamId);
        Assert.Equal("Carlos Ruiz", player.FullName);
        Assert.Equal(10, player.ShirtNumber);
        Assert.Equal(PlayerPosition.Forward, player.Position);
    }

    [Fact]
    public void AddPlayer_WithRepeatedShirtNumber_ReturnsConflictAndDoesNotAdd()
    {
        var team = CreateTeam();
        team.AddPlayer("Carlos Ruiz", 10, PlayerPosition.Forward);

        var result = team.AddPlayer("Luis Díaz", 10, PlayerPosition.Midfielder);

        Assert.True(result.IsFailure);
        Assert.Equal(PlayerErrors.ShirtNumberTaken, result.Error);
        Assert.Single(team.Players);
    }

    [Fact]
    public void AddPlayer_SameShirtNumberInDifferentTeams_IsAllowed()
    {
        var sistemas = CreateTeam("Sistemas FC");
        var ventas = CreateTeam("Ventas United");

        var first = sistemas.AddPlayer("Carlos Ruiz", 10, PlayerPosition.Forward);
        var second = ventas.AddPlayer("Luis Díaz", 10, PlayerPosition.Forward);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
    }

    [Fact]
    public void AddPlayer_WithEmptyName_ReturnsFullNameRequired()
    {
        var team = CreateTeam();

        var result = team.AddPlayer("   ", 10, PlayerPosition.Forward);

        Assert.Equal(PlayerErrors.FullNameRequired, result.Error);
        Assert.Empty(team.Players);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-5)]
    public void AddPlayer_WithShirtNumberOutOfRange_ReturnsInvalidShirtNumber(int shirtNumber)
    {
        var team = CreateTeam();

        var result = team.AddPlayer("Carlos Ruiz", shirtNumber, PlayerPosition.Forward);

        Assert.Equal(PlayerErrors.InvalidShirtNumber, result.Error);
    }

    [Fact]
    public void AddPlayer_WithUndefinedPosition_ReturnsInvalidPosition()
    {
        var team = CreateTeam();

        var result = team.AddPlayer("Carlos Ruiz", 10, (PlayerPosition)99);

        Assert.Equal(PlayerErrors.InvalidPosition, result.Error);
    }
}
