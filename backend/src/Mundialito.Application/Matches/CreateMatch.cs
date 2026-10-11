using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Common;
using Mundialito.Domain.Matches;
using Mundialito.Domain.Teams;

namespace Mundialito.Application.Matches;

public sealed record CreateMatchCommand(Guid HomeTeamId, Guid AwayTeamId, DateTime ScheduledAt, string? Venue)
    : ICommand<Guid>;

public sealed class CreateMatchCommandHandler(
    ITeamRepository teamRepository,
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMatchCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateMatchCommand command, CancellationToken cancellationToken)
    {
        var matchResult = Match.Create(command.HomeTeamId, command.AwayTeamId, command.ScheduledAt, command.Venue);
        if (matchResult.IsFailure)
        {
            return Result.Failure<Guid>(matchResult.Error);
        }

        var homeTeamExists = await teamRepository.ExistsAsync(command.HomeTeamId, cancellationToken);
        var awayTeamExists = await teamRepository.ExistsAsync(command.AwayTeamId, cancellationToken);
        if (!homeTeamExists || !awayTeamExists)
        {
            return Result.Failure<Guid>(TeamErrors.NotFound);
        }

        var match = matchResult.Value;
        matchRepository.Add(match);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(match.Id);
    }
}
