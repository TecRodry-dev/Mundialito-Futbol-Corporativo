using Mundialito.Domain.Common;
using Mundialito.Domain.Players;

namespace Mundialito.Domain.Teams;

/// <summary>
/// Equipo del torneo. Es la entidad principal (AggregateRoot) de sus jugadores.
/// </summary>
public sealed class Team : AggregateRoot
{
    public const int NameMaxLength = 100;
    public const int CoachMaxLength = 100;

    private readonly List<Player> _players = [];

    private Team(Guid id, string name, string coach, DateTime createdAtUtc)
        : base(id)
    {
        Name = name;
        Coach = coach;
        CreatedAtUtc = createdAtUtc;
    }

    // Constructor vacío que usa EF Core.
    private Team()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Coach { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Player> Players => _players.AsReadOnly();

    /// <summary>
    /// Crea un equipo nuevo y registra el evento <see cref="TeamCreated"/>.
    /// </summary>
    public static Result<Team> Create(string name, string coach)
    {
        var validation = Validate(name, coach);
        if (validation.IsFailure)
        {
            return Result.Failure<Team>(validation.Error);
        }

        var team = new Team(Guid.NewGuid(), name.Trim(), coach.Trim(), DateTime.UtcNow);
        team.RaiseDomainEvent(new TeamCreated(team.Id, team.Name));

        return Result.Success(team);
    }

    /// <summary>
    /// Reemplaza los datos del equipo (lo usarán PUT y PATCH).
    /// </summary>
    public Result Update(string name, string coach)
    {
        var validation = Validate(name, coach);
        if (validation.IsFailure)
        {
            return validation;
        }

        Name = name.Trim();
        Coach = coach.Trim();
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Registra un jugador en el equipo. El número de camiseta no se puede repetir dentro del equipo.
    /// </summary>
    public Result<Player> AddPlayer(string fullName, int shirtNumber, PlayerPosition position)
    {
        var playerResult = Player.Create(Id, fullName, shirtNumber, position);
        if (playerResult.IsFailure)
        {
            return playerResult;
        }

        if (_players.Any(player => player.ShirtNumber == shirtNumber))
        {
            return Result.Failure<Player>(PlayerErrors.ShirtNumberTaken);
        }

        _players.Add(playerResult.Value);

        return playerResult;
    }

    private static Result Validate(string name, string coach)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(TeamErrors.NameRequired);
        }

        if (name.Trim().Length > NameMaxLength)
        {
            return Result.Failure(TeamErrors.NameTooLong);
        }

        if (string.IsNullOrWhiteSpace(coach))
        {
            return Result.Failure(TeamErrors.CoachRequired);
        }

        if (coach.Trim().Length > CoachMaxLength)
        {
            return Result.Failure(TeamErrors.CoachTooLong);
        }

        return Result.Success();
    }
}
