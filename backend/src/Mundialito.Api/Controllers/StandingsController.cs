using Microsoft.AspNetCore.Mvc;
using Mundialito.Api.Contracts;
using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Application.Standings;

namespace Mundialito.Api.Controllers;

[Route("api")]
public sealed class StandingsController : ApiControllerBase
{
    /// <summary>
    /// Tabla de posiciones (victoria 3, empate 1, derrota 0). Sin sortBy usa el orden oficial:
    /// puntos → diferencia de gol → goles a favor.
    /// </summary>
    [HttpGet("standings")]
    [ProducesResponseType<PagedResult<StandingResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStandings(
        [FromQuery] PagingQuery paging,
        [FromQuery] string? teamName,
        [FromServices] IQueryHandler<GetStandingsQuery, PagedResult<StandingResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetStandingsQuery(paging.ToPageRequest(), teamName);

        return OkOrError(await handler.Handle(query, cancellationToken));
    }

    /// <summary>Goleadores. Sin sortBy ordena por más goles.</summary>
    [HttpGet("top-scorers")]
    [ProducesResponseType<PagedResult<TopScorerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopScorers(
        [FromQuery] PagingQuery paging,
        [FromQuery] Guid? teamId,
        [FromQuery] string? playerName,
        [FromServices] IQueryHandler<GetTopScorersQuery, PagedResult<TopScorerResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetTopScorersQuery(paging.ToPageRequest(), teamId, playerName);

        return OkOrError(await handler.Handle(query, cancellationToken));
    }
}
