using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Common;
using Mundialito.Domain.Teams;

namespace Mundialito.Application.Teams;

public sealed record CreateTeamCommand(string Name, string Coach) : ICommand<Guid>;

public sealed class CreateTeamCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTeamCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var teamResult = Team.Create(command.Name, command.Coach);
        if (teamResult.IsFailure)
        {
            return Result.Failure<Guid>(teamResult.Error);
        }

        var team = teamResult.Value;

        if (await teamRepository.NameExistsAsync(team.Name, null, cancellationToken))
        {
            return Result.Failure<Guid>(TeamErrors.NameAlreadyExists);
        }

        teamRepository.Add(team);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(team.Id);
    }
}
