using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Common;
using Mundialito.Domain.Teams;

namespace Mundialito.Application.Teams;

/// <summary>
/// PATCH: modifica solo los campos enviados; los que llegan en null se mantienen.
/// </summary>
public sealed record PatchTeamCommand(Guid Id, string? Name, string? Coach) : ICommand;

public sealed class PatchTeamCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<PatchTeamCommand>
{
    public async Task<Result> Handle(PatchTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdAsync(command.Id, cancellationToken);
        if (team is null)
        {
            return Result.Failure(TeamErrors.NotFound);
        }

        var updateResult = team.Update(command.Name ?? team.Name, command.Coach ?? team.Coach);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        if (await teamRepository.NameExistsAsync(team.Name, team.Id, cancellationToken))
        {
            return Result.Failure(TeamErrors.NameAlreadyExists);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
