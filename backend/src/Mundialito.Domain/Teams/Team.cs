using Mundialito.Domain.Common;

namespace Mundialito.Domain.Teams;

/// <summary>
/// Equipo del torneo.
/// </summary>
public sealed class Team : AggregateRoot
{
    public const int NameMaxLength = 100;
    public const int CoachMaxLength = 100;

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
