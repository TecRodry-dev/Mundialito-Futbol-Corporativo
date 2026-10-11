using Mundialito.Domain.Common;

namespace Mundialito.Domain.Matches;

/// <summary>
/// Gol de un partido. Solo se crea a través de <see cref="Match.RegisterResult"/>.
/// </summary>
public sealed class Goal : Entity
{
    private Goal(Guid id, Guid matchId, Guid playerId, Guid teamId, int? minute)
        : base(id)
    {
        MatchId = matchId;
        PlayerId = playerId;
        TeamId = teamId;
        Minute = minute;
    }

    // Constructor vacío que usa EF Core.
    private Goal()
    {
    }

    public Guid MatchId { get; private set; }

    public Guid PlayerId { get; private set; }

    public Guid TeamId { get; private set; }

    public int? Minute { get; private set; }

    internal static Goal Create(Guid matchId, ScoredGoal scoredGoal) =>
        new(Guid.NewGuid(), matchId, scoredGoal.PlayerId, scoredGoal.TeamId, scoredGoal.Minute);
}
