using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Common;
using Mundialito.Domain.Teams;

namespace Mundialito.Application.Teams;

/// <summary>
/// PUT: reemplaza todos los datos del equipo.
/// </summary>
public sealed record UpdateTeamCommand(Guid Id, string Name, string Coach) : ICommand;

public sealed class UpdateTeamCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTeamCommand>
{
    public async Task<Result> Handle(UpdateTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdAsync(command.Id, cancellationToken);
        if (team is null)
        {
            return Result.Failure(TeamErrors.NotFound);
        }

        var updateResult = team.Update(command.Name, command.Coach);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        // Si hay conflicto no se llama a CommitAsync, así que el cambio en memoria se descarta.
        if (await teamRepository.NameExistsAsync(team.Name, team.Id, cancellationToken))
        {
            return Result.Failure(TeamErrors.NameAlreadyExists);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
