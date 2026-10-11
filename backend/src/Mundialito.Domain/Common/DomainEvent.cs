namespace Mundialito.Domain.Common;

/// <summary>
/// Base de los eventos de dominio: algo importante que ocurrió (ej. se creó un equipo).
/// Cada evento nace con su propio Id y la fecha en que ocurrió.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
