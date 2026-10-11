using Mundialito.Domain.Standings;

namespace Mundialito.UnitTests.Domain.Standings;

public class StandingsTests
{
    private static readonly Guid Sistemas = Guid.NewGuid();
    private static readonly Guid Ventas = Guid.NewGuid();
    private static readonly Guid Rrhh = Guid.NewGuid();
    private static readonly Guid Finanzas = Guid.NewGuid();

    private static readonly Dictionary<Guid, string> Teams = new()
    {
        [Sistemas] = "Sistemas FC",
        [Ventas] = "Ventas United",
        [Rrhh] = "RRHH Team",
        [Finanzas] = "Finanzas City"
    };

    private static TeamStanding RowOf(IReadOnlyList<TeamStanding> table, Guid teamId) =>
        table.Single(standing => standing.TeamId == teamId);

    [Theory]
    [InlineData(2, 1, 3)]
    [InlineData(1, 1, 1)]
    [InlineData(0, 2, 0)]
    public void Points_WinDrawLoss_AreThreeOneZero(int goalsFor, int goalsAgainst, int expectedPoints)
    {
        var outcome = StandingRules.GetOutcome(goalsFor, goalsAgainst);

        Assert.Equal(expectedPoints, StandingRules.GetPoints(outcome));
    }

    [Fact]
    public void Calculate_Win_GivesThreePointsToWinnerAndZeroToLoser()
    {
        var table = StandingsCalculator.Calculate(Teams, [new PlayedMatch(Sistemas, Ventas, 2, 1)]);

        var winner = RowOf(table, Sistemas);
        var loser = RowOf(table, Ventas);
        Assert.Equal(3, winner.Points);
        Assert.Equal(1, winner.Won);
        Assert.Equal(0, loser.Points);
        Assert.Equal(1, loser.Lost);
    }

    [Fact]
    public void Calculate_Draw_GivesOnePointToEachTeam()
    {
        var table = StandingsCalculator.Calculate(Teams, [new PlayedMatch(Sistemas, Ventas, 1, 1)]);

        Assert.Equal(1, RowOf(table, Sistemas).Points);
        Assert.Equal(1, RowOf(table, Ventas).Points);
        Assert.Equal(1, RowOf(table, Ventas).Drawn);
    }

    [Fact]
    public void Calculate_ComputesGoalsAndGoalDifference()
    {
        var table = StandingsCalculator.Calculate(Teams, [new PlayedMatch(Sistemas, Ventas, 3, 1)]);

        var sistemas = RowOf(table, Sistemas);
        var ventas = RowOf(table, Ventas);
        Assert.Equal(3, sistemas.GoalsFor);
        Assert.Equal(1, sistemas.GoalsAgainst);
        Assert.Equal(2, sistemas.GoalDifference);
        Assert.Equal(-2, ventas.GoalDifference);
    }

    [Fact]
    public void Calculate_AccumulatesSeveralMatches()
    {
        var table = StandingsCalculator.Calculate(Teams,
        [
            new PlayedMatch(Sistemas, Ventas, 2, 0),
            new PlayedMatch(Rrhh, Sistemas, 1, 1)
        ]);

        var sistemas = RowOf(table, Sistemas);
        Assert.Equal(2, sistemas.Played);
        Assert.Equal(4, sistemas.Points);
        Assert.Equal(3, sistemas.GoalsFor);
        Assert.Equal(1, sistemas.GoalsAgainst);
    }

    [Fact]
    public void Calculate_OrdersByPointsFirst()
    {
        var table = StandingsCalculator.Calculate(Teams,
        [
            new PlayedMatch(Sistemas, Ventas, 1, 0),
            new PlayedMatch(Rrhh, Finanzas, 0, 0)
        ]);

        Assert.Equal(Sistemas, table[0].TeamId);
        Assert.Equal(Ventas, table[table.Count - 1].TeamId);
    }

    [Fact]
    public void Calculate_SamePoints_OrdersByGoalDifference()
    {
        var table = StandingsCalculator.Calculate(Teams,
        [
            new PlayedMatch(Rrhh, Finanzas, 1, 0),
            new PlayedMatch(Sistemas, Ventas, 3, 0)
        ]);

        Assert.Equal(Sistemas, table[0].TeamId);
        Assert.Equal(Rrhh, table[1].TeamId);
    }

    [Fact]
    public void Calculate_SamePointsAndGoalDifference_OrdersByGoalsFor()
    {
        var table = StandingsCalculator.Calculate(Teams,
        [
            new PlayedMatch(Rrhh, Finanzas, 1, 0),
            new PlayedMatch(Sistemas, Ventas, 3, 2)
        ]);

        Assert.Equal(Sistemas, table[0].TeamId);
        Assert.Equal(Rrhh, table[1].TeamId);
    }

    [Fact]
    public void Calculate_TeamWithoutMatches_AppearsWithZeros()
    {
        var table = StandingsCalculator.Calculate(Teams, []);

        Assert.Equal(4, table.Count);
        Assert.All(table, standing =>
        {
            Assert.Equal(0, standing.Played);
            Assert.Equal(0, standing.Points);
        });
    }
}
