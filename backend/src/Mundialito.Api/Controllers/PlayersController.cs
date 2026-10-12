using Microsoft.AspNetCore.Mvc;
using Mundialito.Api.Contracts;
using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Application.Players;
using Mundialito.Domain.Players;

namespace Mundialito.Api.Controllers;

[Route("api/players")]
public sealed class PlayersController : ApiControllerBase
{
    /// <summary>Lista jugadores con filtros (equipo, nombre, posición), paginación y orden.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<PlayerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPlayers(
        [FromQuery] PagingQuery paging,
        [FromQuery] Guid? teamId,
        [FromQuery] string? name,
        [FromQuery] PlayerPosition? position,
        [FromServices] IQueryHandler<GetPlayersQuery, PagedResult<PlayerResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetPlayersQuery(paging.ToPageRequest(), teamId, name, position);

        return OkOrError(await handler.Handle(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PlayerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] IQueryHandler<GetPlayerByIdQuery, PlayerResponse> handler,
        CancellationToken cancellationToken)
    {
        return OkOrError(await handler.Handle(new GetPlayerByIdQuery(id), cancellationToken));
    }
}
