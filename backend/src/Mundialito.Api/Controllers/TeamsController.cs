using Microsoft.AspNetCore.Mvc;
using Mundialito.Api.Contracts;
using Mundialito.Api.Idempotency;
using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Application.Common.Paging;
using Mundialito.Application.Players;
using Mundialito.Application.Teams;
using Mundialito.Domain.Players;

namespace Mundialito.Api.Controllers;

[Route("api/teams")]
public sealed class TeamsController : ApiControllerBase
{
    /// <summary>Lista equipos con filtros (nombre, entrenador), paginación y orden.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<TeamResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTeams(
        [FromQuery] PagingQuery paging,
        [FromQuery] string? name,
        [FromQuery] string? coach,
        [FromServices] IQueryHandler<GetTeamsQuery, PagedResult<TeamResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetTeamsQuery(paging.ToPageRequest(), name, coach), cancellationToken);

        return OkOrError(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] IQueryHandler<GetTeamByIdQuery, TeamResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetTeamByIdQuery(id), cancellationToken);

        return OkOrError(result);
    }

    [HttpPost]
    [Idempotent]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateTeamRequest request,
        [FromServices] ICommandHandler<CreateTeamCommand, Guid> createHandler,
        [FromServices] IQueryHandler<GetTeamByIdQuery, TeamResponse> getHandler,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.Handle(new CreateTeamCommand(request.Name, request.Coach), cancellationToken);
        if (result.IsFailure)
        {
            return ErrorResult(result.Error);
        }

        var team = await getHandler.Handle(new GetTeamByIdQuery(result.Value), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, team.Value);
    }

    /// <summary>PUT: reemplaza nombre y entrenador.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateTeamRequest request,
        [FromServices] ICommandHandler<UpdateTeamCommand> updateHandler,
        [FromServices] IQueryHandler<GetTeamByIdQuery, TeamResponse> getHandler,
        CancellationToken cancellationToken)
    {
        var result = await updateHandler.Handle(new UpdateTeamCommand(id, request.Name, request.Coach), cancellationToken);
        if (result.IsFailure)
        {
            return ErrorResult(result.Error);
        }

        return OkOrError(await getHandler.Handle(new GetTeamByIdQuery(id), cancellationToken));
    }

    /// <summary>PATCH: cambia solo los campos enviados.</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Patch(
        Guid id,
        PatchTeamRequest request,
        [FromServices] ICommandHandler<PatchTeamCommand> patchHandler,
        [FromServices] IQueryHandler<GetTeamByIdQuery, TeamResponse> getHandler,
        CancellationToken cancellationToken)
    {
        var result = await patchHandler.Handle(new PatchTeamCommand(id, request.Name, request.Coach), cancellationToken);
        if (result.IsFailure)
        {
            return ErrorResult(result.Error);
        }

        return OkOrError(await getHandler.Handle(new GetTeamByIdQuery(id), cancellationToken));
    }

    /// <summary>DELETE idempotente: 204 aunque el equipo ya no exista; 409 si tiene partidos.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] ICommandHandler<DeleteTeamCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeleteTeamCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : ErrorResult(result.Error);
    }

    /// <summary>Jugadores de un equipo, con filtros y paginación.</summary>
    [HttpGet("{teamId:guid}/players")]
    [ProducesResponseType<PagedResult<PlayerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPlayers(
        Guid teamId,
        [FromQuery] PagingQuery paging,
        [FromQuery] string? name,
        [FromQuery] PlayerPosition? position,
        [FromServices] IQueryHandler<GetPlayersQuery, PagedResult<PlayerResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetPlayersQuery(paging.ToPageRequest(), teamId, name, position);

        return OkOrError(await handler.Handle(query, cancellationToken));
    }

    /// <summary>Registra un jugador en el equipo.</summary>
    [HttpPost("{teamId:guid}/players")]
    [Idempotent]
    [ProducesResponseType<PlayerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddPlayer(
        Guid teamId,
        AddPlayerRequest request,
        [FromServices] ICommandHandler<AddPlayerCommand, Guid> addHandler,
        [FromServices] IQueryHandler<GetPlayerByIdQuery, PlayerResponse> getHandler,
        CancellationToken cancellationToken)
    {
        var command = new AddPlayerCommand(teamId, request.FullName, request.ShirtNumber, request.Position);
        var result = await addHandler.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return ErrorResult(result.Error);
        }

        var player = await getHandler.Handle(new GetPlayerByIdQuery(result.Value), cancellationToken);

        return CreatedAtAction(nameof(PlayersController.GetById), "Players", new { id = result.Value }, player.Value);
    }
}
