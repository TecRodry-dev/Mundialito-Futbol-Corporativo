using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Mundialito.Domain.Matches;
using Mundialito.Domain.Players;
using Mundialito.Domain.Teams;
using Mundialito.Infrastructure.Persistence;

namespace Mundialito.UnitTests.Infrastructure;

/// <summary>
/// Comportamiento del commit del Unit of Work sobre una base SQLite en memoria (con transacciones reales).
/// Cada test usa su propia base; se verifica siempre con un DbContext nuevo, leyendo lo que quedó guardado.
/// </summary>
public sealed class UnitOfWorkTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public UnitOfWorkTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private MundialitoDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<MundialitoDbContext>().UseSqlite(_connection).Options);

    private static UnitOfWork CreateUnitOfWork(MundialitoDbContext context) =>
        new(context, NullLogger<UnitOfWork>.Instance);

    [Fact]
    public async Task CommitAsync_SavesChangesFromSeveralAggregatesTogether()
    {
        await using (var context = CreateContext())
        {
            var home = Team.Create("Sistemas FC", "Laura Méndez").Value;
            var away = Team.Create("Ventas United", "Carlos Rojas").Value;
            var match = Match.Create(home.Id, away.Id, new DateTime(2026, 10, 20, 18, 0, 0), null).Value;

            context.Teams.AddRange(home, away);
            context.Matches.Add(match);

            await CreateUnitOfWork(context).CommitAsync();
        }

        await using var verification = CreateContext();
        Assert.Equal(2, await verification.Teams.CountAsync());
        Assert.Equal(1, await verification.Matches.CountAsync());
    }

    [Fact]
    public async Task CommitAsync_RegistersDomainEventsInTheSameCommit()
    {
        var team = Team.Create("Sistemas FC", "Laura Méndez").Value;

        await using (var context = CreateContext())
        {
            context.Teams.Add(team);
            await CreateUnitOfWork(context).CommitAsync();
        }

        await using var verification = CreateContext();
        var storedEvent = Assert.Single(await verification.DomainEvents.ToListAsync());
        Assert.Equal(nameof(TeamCreated), storedEvent.EventType);
        Assert.Contains(team.Id.ToString(), storedEvent.Payload);
    }

    [Fact]
    public async Task CommitAsync_ClearsDomainEventsFromAggregates()
    {
        var team = Team.Create("Sistemas FC", "Laura Méndez").Value;

        await using var context = CreateContext();
        context.Teams.Add(team);
        await CreateUnitOfWork(context).CommitAsync();

        Assert.Empty(team.DomainEvents);
    }

    [Fact]
    public async Task CommitAsync_WhenSaveFails_RollsBackEverything()
    {
        var team = Team.Create("Sistemas FC", "Laura Méndez").Value;
        var duplicated = Team.Create("Sistemas FC", "Otro entrenador").Value;

        await using (var context = CreateContext())
        {
            // El índice único de Name hace fallar el segundo INSERT dentro de la misma transacción.
            context.Teams.AddRange(team, duplicated);

            await Assert.ThrowsAsync<DbUpdateException>(() => CreateUnitOfWork(context).CommitAsync());

            // Como no hubo commit, los eventos no se limpian ni se pierden.
            Assert.Single(team.DomainEvents);
        }

        await using var verification = CreateContext();
        Assert.Equal(0, await verification.Teams.CountAsync());
        Assert.Equal(0, await verification.DomainEvents.CountAsync());
    }

    [Fact]
    public async Task CommitAsync_SavesPlayersAddedToAnExistingTeam()
    {
        var team = Team.Create("Sistemas FC", "Laura Méndez").Value;

        await using (var context = CreateContext())
        {
            context.Teams.Add(team);
            await CreateUnitOfWork(context).CommitAsync();
        }

        await using (var context = CreateContext())
        {
            var storedTeam = await context.Teams
                .Include(existing => existing.Players)
                .SingleAsync(existing => existing.Id == team.Id);

            storedTeam.AddPlayer("Carlos Ruiz", 10, PlayerPosition.Forward);
            await CreateUnitOfWork(context).CommitAsync();
        }

        await using var verification = CreateContext();
        var player = Assert.Single(await verification.Players.ToListAsync());
        Assert.Equal(team.Id, player.TeamId);
        Assert.Equal(10, player.ShirtNumber);
    }

    [Fact]
    public async Task CommitAsync_WithoutChanges_DoesNotFail()
    {
        await using var context = CreateContext();

        await CreateUnitOfWork(context).CommitAsync();

        Assert.Equal(0, await context.Teams.CountAsync());
    }
}
