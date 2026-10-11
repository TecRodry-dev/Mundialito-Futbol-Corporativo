using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Abstractions.Idempotency;
using Mundialito.Domain.Matches;
using Mundialito.Domain.Players;
using Mundialito.Domain.Teams;
using Mundialito.Infrastructure.Events;

namespace Mundialito.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core (lado de escritura). Solo <see cref="UnitOfWork"/> llama a SaveChanges.
/// </summary>
public sealed class MundialitoDbContext(DbContextOptions<MundialitoDbContext> options) : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Player> Players => Set<Player>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    public DbSet<DomainEventLog> DomainEvents => Set<DomainEventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MundialitoDbContext).Assembly);
    }
}
