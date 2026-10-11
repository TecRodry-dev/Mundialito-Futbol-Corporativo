using Mundialito.Domain.Common;

namespace Mundialito.Domain.Players;

/// <summary>
/// Jugador de un equipo. Solo se crea a través de <see cref="Teams.Team.AddPlayer"/>.
/// </summary>
public sealed class Player : Entity
{
    public const int FullNameMaxLength = 100;
    public const int MinShirtNumber = 1;
    public const int MaxShirtNumber = 99;

    private Player(Guid id, Guid teamId, string fullName, int shirtNumber, PlayerPosition position, DateTime createdAtUtc)
        : base(id)
    {
        TeamId = teamId;
        FullName = fullName;
        ShirtNumber = shirtNumber;
        Position = position;
        CreatedAtUtc = createdAtUtc;
    }

    // Constructor vacío que usa EF Core.
    private Player()
    {
    }

    public Guid TeamId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public int ShirtNumber { get; private set; }

    public PlayerPosition Position { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    // internal: fuera del Domain nadie puede crear jugadores sueltos; se hace con team.AddPlayer(...).
    internal static Result<Player> Create(Guid teamId, string fullName, int shirtNumber, PlayerPosition position)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<Player>(PlayerErrors.FullNameRequired);
        }

        if (fullName.Trim().Length > FullNameMaxLength)
        {
            return Result.Failure<Player>(PlayerErrors.FullNameTooLong);
        }

        if (shirtNumber < MinShirtNumber || shirtNumber > MaxShirtNumber)
        {
            return Result.Failure<Player>(PlayerErrors.InvalidShirtNumber);
        }

        if (!Enum.IsDefined(position))
        {
            return Result.Failure<Player>(PlayerErrors.InvalidPosition);
        }

        var player = new Player(Guid.NewGuid(), teamId, fullName.Trim(), shirtNumber, position, DateTime.UtcNow);

        return Result.Success(player);
    }
}
