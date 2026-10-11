using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Common;
using Mundialito.Domain.Matches;
using Mundialito.Domain.Players;

namespace Mundialito.Application.Matches;

/// <summary>
/// Un gol enviado por el cliente: quién lo marcó y en qué minuto (opcional).
/// El equipo del goleador se toma de la base de datos, no del cliente.
/// </summary>
public sealed record GoalInput(Guid PlayerId, int? Minute);

public sealed record RegisterMatchResultCommand(
    Guid MatchId,
    int HomeGoals,
    int AwayGoals,
    IReadOnlyList<GoalInput>? Goals) : ICommand;

public sealed class RegisterMatchResultCommandHandler(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterMatchResultCommand>
{
    public async Task<Result> Handle(RegisterMatchResultCommand command, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(command.MatchId, cancellationToken);
        if (match is null)
        {
            return Result.Failure(MatchErrors.NotFound);
        }

        var goals = command.Goals ?? Array.Empty<GoalInput>();
        var playerIds = goals.Select(goal => goal.PlayerId).Distinct().ToList();

        var teamIdsByPlayer = await teamRepository.GetTeamIdsByPlayerIdsAsync(playerIds, cancellationToken);
        if (playerIds.Any(playerId => !teamIdsByPlayer.ContainsKey(playerId)))
        {
            return Result.Failure(PlayerErrors.NotFound);
        }

        var scoredGoals = goals
            .Select(goal => new ScoredGoal(goal.PlayerId, teamIdsByPlayer[goal.PlayerId], goal.Minute))
            .ToList();

        var result = match.RegisterResult(command.HomeGoals, command.AwayGoals, scoredGoals);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
