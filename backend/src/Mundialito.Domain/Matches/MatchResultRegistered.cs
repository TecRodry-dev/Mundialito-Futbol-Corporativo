using Mundialito.Domain.Common;

namespace Mundialito.Domain.Matches;

/// <summary>
/// Evento: se registró el resultado de un partido.
/// </summary>
public sealed record MatchResultRegistered(
    Guid MatchId,
    Guid HomeTeamId,
    Guid AwayTeamId,
    int HomeGoals,
    int AwayGoals) : DomainEvent;
