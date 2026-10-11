using Mundialito.Domain.Common;

namespace Mundialito.Domain.Teams;

/// <summary>
/// Evento: se creó un equipo nuevo.
/// </summary>
public sealed record TeamCreated(Guid TeamId, string Name) : DomainEvent;
