using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Domain.Common;
using Mundialito.Domain.Teams;

namespace Mundialito.Application.Teams;

public sealed record GetTeamByIdQuery(Guid Id) : IQuery<TeamResponse>;

public sealed class GetTeamByIdQueryHandler(ITeamReadService teamReadService)
    : IQueryHandler<GetTeamByIdQuery, TeamResponse>
{
    public async Task<Result<TeamResponse>> Handle(GetTeamByIdQuery query, CancellationToken cancellationToken)
    {
        var team = await teamReadService.GetByIdAsync(query.Id, cancellationToken);

        return team is null
            ? Result.Failure<TeamResponse>(TeamErrors.NotFound)
            : Result.Success(team);
    }
}
