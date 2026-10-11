using System.Text.Json;
using Mundialito.Domain.Common;

namespace Mundialito.Infrastructure.Events;

/// <summary>
/// Registro persistente de un evento de dominio (tabla DomainEvents), con su TraceId para trazabilidad.
/// </summary>
public sealed class DomainEventLog
{
    public Guid Id { get; init; }

    public required string EventType { get; init; }

    public required string Payload { get; init; }

    public DateTime OccurredOnUtc { get; init; }

    public string? TraceId { get; init; }

    public string? CorrelationId { get; init; }

    public static DomainEventLog From(DomainEvent domainEvent, string? traceId, string? correlationId) => new()
    {
        Id = domainEvent.EventId,
        EventType = domainEvent.GetType().Name,
        Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
        OccurredOnUtc = domainEvent.OccurredOnUtc,
        TraceId = traceId,
        CorrelationId = correlationId
    };
}
