using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Infrastructure.Events;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class DomainEventLogConfiguration : IEntityTypeConfiguration<DomainEventLog>
{
    public void Configure(EntityTypeBuilder<DomainEventLog> builder)
    {
        builder.ToTable("DomainEvents");

        builder.HasKey(domainEvent => domainEvent.Id);
        builder.Property(domainEvent => domainEvent.Id).ValueGeneratedNever();

        builder.Property(domainEvent => domainEvent.EventType).HasMaxLength(200).IsRequired();
        builder.Property(domainEvent => domainEvent.Payload).IsRequired();
        builder.Property(domainEvent => domainEvent.TraceId).HasMaxLength(64);
        builder.Property(domainEvent => domainEvent.CorrelationId).HasMaxLength(100);

        // Para buscar todos los eventos de un mismo request.
        builder.HasIndex(domainEvent => domainEvent.TraceId);
    }
}
