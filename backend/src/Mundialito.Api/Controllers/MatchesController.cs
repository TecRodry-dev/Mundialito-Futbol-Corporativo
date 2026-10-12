using Microsoft.AspNetCore.Mvc;
using Mundialito.Api.Contracts;
using Mundialito.Api.Idempotency;
using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Application.Matches;
using Mundialito.Domain.Matches;

namespace Mundialito.Api.Controllers;

[Route("api/matches")]
public sealed class MatchesController : ApiControllerBase
{
    /// <summary>
    /// Lista partidos con filtros por fecha (from/to, día completo), equipo (local o visitante) y estado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<MatchResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMatches(
        [FromQuery] PagingQuery paging,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? teamId,
        [FromQuery] MatchStatus? status,
        [FromServices] IQueryHandler<GetMatchesQuery, PagedResult<MatchResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetMatchesQuery(paging.ToPageRequest(), from, to, teamId, status);

        return OkOrError(await handler.Handle(query, cancellationToken));
    }

    /// <summary>Detalle de un partido con sus goles.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<MatchDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] IQueryHandler<GetMatchByIdQuery, MatchDetailResponse> handler,
        CancellationToken cancellationToken)
    {
        return OkOrError(await handler.Handle(new GetMatchByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Idempotent]
    [ProducesResponseType<MatchDetailResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CreateMatchRequest request,
        [FromServices] ICommandHandler<CreateMatchCommand, Guid> createHandler,
        [FromServices] IQueryHandler<GetMatchByIdQuery, MatchDetailResponse> getHandler,
        CancellationToken cancellationToken)
    {
        var command = new CreateMatchCommand(request.HomeTeamId, request.AwayTeamId, request.ScheduledAt, request.Venue);
        var result = await createHandler.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return ErrorResult(result.Error);
        }

        var match = await getHandler.Handle(new GetMatchByIdQuery(result.Value), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, match.Value);
    }

    /// <summary>Registra el resultado (marcador y goleadores). Devuelve el partido actualizado.</summary>
    [HttpPost("{id:guid}/result")]
    [Idempotent]
    [ProducesResponseType<MatchDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterResult(
        Guid id,
        RegisterMatchResultRequest request,
        [FromServices] ICommandHandler<RegisterMatchResultCommand> registerHandler,
        [FromServices] IQueryHandler<GetMatchByIdQuery, MatchDetailResponse> getHandler,
        CancellationToken cancellationToken)
    {
        var goals = request.Goals?.Select(goal => new GoalInput(goal.PlayerId, goal.Minute)).ToList();
        var command = new RegisterMatchResultCommand(id, request.HomeGoals, request.AwayGoals, goals);

        var result = await registerHandler.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return ErrorResult(result.Error);
        }

        return OkOrError(await getHandler.Handle(new GetMatchByIdQuery(id), cancellationToken));
    }
}
