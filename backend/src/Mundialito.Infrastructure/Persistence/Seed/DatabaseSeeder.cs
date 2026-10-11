using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Common;
using Mundialito.Domain.Matches;
using Mundialito.Domain.Players;
using Mundialito.Domain.Teams;

namespace Mundialito.Infrastructure.Persistence.Seed;

/// <summary>
/// Carga datos de ejemplo solo si la base está vacía: 4 equipos, 5 jugadores por equipo
/// y 6 partidos (todos contra todos), 3 de ellos con resultado.
/// Usa las fábricas del dominio (datos siempre válidos) y guarda con el Unit of Work.
/// </summary>
public sealed class DatabaseSeeder(
    MundialitoDbContext dbContext,
    IUnitOfWork unitOfWork,
    ILogger<DatabaseSeeder> logger)
{
    private static readonly (int ShirtNumber, PlayerPosition Position)[] Lineup =
    [
        (1, PlayerPosition.Goalkeeper),
        (4, PlayerPosition.Defender),
        (8, PlayerPosition.Midfielder),
        (10, PlayerPosition.Midfielder),
        (9, PlayerPosition.Forward)
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Teams.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Seed omitido: la base de datos ya tiene datos");
            return;
        }

        var sistemas = Value(Team.Create("Sistemas FC", "Laura Méndez"));
        var ventas = Value(Team.Create("Ventas United", "Carlos Rojas"));
        var rrhh = Value(Team.Create("RRHH Team", "Ana Torres"));
        var finanzas = Value(Team.Create("Finanzas City", "Jorge Salas"));

        var sistemasPlayers = AddPlayers(sistemas, "Andrés Gómez", "Bruno Díaz", "Camilo Ruiz", "Diego Herrera", "Esteban Mora");
        var ventasPlayers = AddPlayers(ventas, "Fabián Castro", "Gabriel Rivas", "Hugo Peña", "Iván Soto", "Julián Vega");
        var rrhhPlayers = AddPlayers(rrhh, "Kevin Ortiz", "Luis Navarro", "Mateo Silva", "Nicolás Paredes", "Óscar Ramos");
        var finanzasPlayers = AddPlayers(finanzas, "Pablo Fuentes", "Quique Lara", "Rafael Campos", "Sergio Luna", "Tomás Ibarra");

        var today = DateTime.UtcNow.Date;
        DateTime At(int days) => today.AddDays(days).AddHours(18);

        // Partidos jugados (con resultado y goleadores).
        var sistemasVsVentas = Value(Match.Create(sistemas.Id, ventas.Id, At(-10), "Cancha principal"));
        EnsureSuccess(sistemasVsVentas.RegisterResult(3, 1,
        [
            ScoredBy(sistemasPlayers[4], 12),
            ScoredBy(sistemasPlayers[4], 47),
            ScoredBy(sistemasPlayers[3], 70),
            ScoredBy(ventasPlayers[4], 85)
        ]));

        var rrhhVsFinanzas = Value(Match.Create(rrhh.Id, finanzas.Id, At(-7), "Cancha 2"));
        EnsureSuccess(rrhhVsFinanzas.RegisterResult(2, 2,
        [
            ScoredBy(rrhhPlayers[4], 20),
            ScoredBy(rrhhPlayers[2], 66),
            ScoredBy(finanzasPlayers[4], 35),
            ScoredBy(finanzasPlayers[3], 90)
        ]));

        var ventasVsRrhh = Value(Match.Create(ventas.Id, rrhh.Id, At(-3), "Cancha principal"));
        EnsureSuccess(ventasVsRrhh.RegisterResult(2, 0,
        [
            ScoredBy(ventasPlayers[4], 15),
            ScoredBy(ventasPlayers[2], 58)
        ]));

        // Partidos programados (sin resultado).
        var sistemasVsRrhh = Value(Match.Create(sistemas.Id, rrhh.Id, At(3), "Cancha 2"));
        var finanzasVsVentas = Value(Match.Create(finanzas.Id, ventas.Id, At(7), "Cancha principal"));
        var sistemasVsFinanzas = Value(Match.Create(sistemas.Id, finanzas.Id, At(10), "Cancha 2"));

        dbContext.Teams.AddRange(sistemas, ventas, rrhh, finanzas);
        dbContext.Matches.AddRange(
            sistemasVsVentas, rrhhVsFinanzas, ventasVsRrhh,
            sistemasVsRrhh, finanzasVsVentas, sistemasVsFinanzas);

        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Seed aplicado: {TeamCount} equipos, {PlayerCount} jugadores y {MatchCount} partidos ({PlayedCount} con resultado)",
            4, 20, 6, 3);
    }

    private static List<Player> AddPlayers(Team team, params string[] names) =>
        names
            .Select((name, index) => Value(team.AddPlayer(name, Lineup[index].ShirtNumber, Lineup[index].Position)))
            .ToList();

    private static ScoredGoal ScoredBy(Player player, int minute) => new(player.Id, player.TeamId, minute);

    // Los datos del seed son fijos y válidos: si el dominio los rechaza es un error de programación.
    private static T Value<T>(Result<T> result) =>
        result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"Seed inválido: {result.Error.Message}");

    private static void EnsureSuccess(Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Seed inválido: {result.Error.Message}");
        }
    }
}
