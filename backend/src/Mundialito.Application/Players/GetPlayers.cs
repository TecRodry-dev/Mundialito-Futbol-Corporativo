using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Domain.Common;
using Mundialito.Domain.Players;

namespace Mundialito.Application.Players;

/// <summary>
/// Lista de jugadores paginada, con filtros por equipo, nombre (búsqueda parcial) y posición.
/// </summary>
public sealed record GetPlayersQuery(PageRequest Paging, Guid? TeamId, string? Name, PlayerPosition? Position)
    : IQuery<PagedResult<PlayerResponse>>
{
    public static readonly IReadOnlyCollection<string> SortFields =
        ["fullName", "shirtNumber", "position", "teamName", "goals"];
}

public sealed class GetPlayersQueryHandler(IPlayerReadService playerReadService)
    : IQueryHandler<GetPlayersQuery, PagedResult<PlayerResponse>>
{
    public async Task<Result<PagedResult<PlayerResponse>>> Handle(GetPlayersQuery query, CancellationToken cancellationToken)
    {
        var validation = query.Paging.Validate(GetPlayersQuery.SortFields);
        if (validation.IsFailure)
        {
            return Result.Failure<PagedResult<PlayerResponse>>(validation.Error);
        }

        var players = await playerReadService.GetPlayersAsync(query, cancellationToken);

        return Result.Success(players);
    }
}
