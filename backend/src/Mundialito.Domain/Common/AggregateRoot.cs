namespace Mundialito.Domain.Common;

/// <summary>
/// Entidad principal de un grupo (ej. Team con sus jugadores, Match con sus goles).
/// Es la única que registra eventos de dominio.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected AggregateRoot(Guid id)
        : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
