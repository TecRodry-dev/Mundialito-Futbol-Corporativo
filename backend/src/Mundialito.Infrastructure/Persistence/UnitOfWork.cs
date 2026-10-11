using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Common;
using Mundialito.Infrastructure.Events;
using Mundialito.Infrastructure.Observability;

namespace Mundialito.Infrastructure.Persistence;

/// <summary>
/// Único lugar donde se llama a SaveChanges. En una sola transacción guarda los cambios de todos
/// los repositorios junto con el registro de los eventos de dominio; si algo falla, no se guarda nada.
/// Después del commit, loggea los eventos (con el TraceId del request) y los limpia de los agregados.
/// </summary>
public sealed class UnitOfWork(MundialitoDbContext dbContext, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        foreach (var domainEvent in domainEvents)
        {
            dbContext.DomainEvents.Add(DomainEventLog.From(
                domainEvent,
                CorrelationContext.CurrentTraceId,
                CorrelationContext.CurrentCorrelationId));
        }

        // Si no se llega a CommitAsync, al liberar la transacción (await using) se hace rollback.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogDebug("Transacción confirmada con {AffectedRows} filas afectadas", affectedRows);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Transacción revertida: no se guardó ningún cambio");
            throw;
        }

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            logger.LogInformation(
                "Evento de dominio {EventType} registrado con Id {EventId}: {@DomainEvent}",
                domainEvent.GetType().Name,
                domainEvent.EventId,
                domainEvent);
        }
    }
}
