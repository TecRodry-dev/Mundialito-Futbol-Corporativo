using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Common;
using Mundialito.Domain.Players;
using Mundialito.Domain.Teams;

namespace Mundialito.Application.Players;

/// <summary>
/// Registra un jugador en un equipo (a través del agregado Team).
/// </summary>
public sealed record AddPlayerCommand(Guid TeamId, string FullName, int ShirtNumber, PlayerPosition Position)
    : ICommand<Guid>;

public sealed class AddPlayerCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<AddPlayerCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddPlayerCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdWithPlayersAsync(command.TeamId, cancellationToken);
        if (team is null)
        {
            return Result.Failure<Guid>(TeamErrors.NotFound);
        }

        var playerResult = team.AddPlayer(command.FullName, command.ShirtNumber, command.Position);
        if (playerResult.IsFailure)
        {
            return Result.Failure<Guid>(playerResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(playerResult.Value.Id);
    }
}
