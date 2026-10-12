using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Domain.Common;
using Mundialito.Domain.Players;

namespace Mundialito.Application.Players;

public sealed record GetPlayerByIdQuery(Guid Id) : IQuery<PlayerResponse>;

public sealed class GetPlayerByIdQueryHandler(IPlayerReadService playerReadService)
    : IQueryHandler<GetPlayerByIdQuery, PlayerResponse>
{
    public async Task<Result<PlayerResponse>> Handle(GetPlayerByIdQuery query, CancellationToken cancellationToken)
    {
        var player = await playerReadService.GetByIdAsync(query.Id, cancellationToken);

        return player is null
            ? Result.Failure<PlayerResponse>(PlayerErrors.NotFound)
            : Result.Success(player);
    }
}
