using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mundialito.Application.Abstractions.Idempotency;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Application.Matches;
using Mundialito.Application.Players;
using Mundialito.Application.Standings;
using Mundialito.Application.Teams;
using Mundialito.Infrastructure.Idempotency;
using Mundialito.Infrastructure.Persistence;
using Mundialito.Infrastructure.Persistence.Repositories;
using Mundialito.Infrastructure.Persistence.Seed;
using Mundialito.Infrastructure.ReadModel;

namespace Mundialito.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra las implementaciones de los contratos de Application.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Escritura (Commands): EF Core + Unit of Work + repositorios.
        services.AddDbContext<MundialitoDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();

        // Lectura (Queries): Dapper.
        services.AddSingleton(new SqlConnectionFactory(connectionString));
        services.AddScoped<ITeamReadService, TeamReadService>();
        services.AddScoped<IPlayerReadService, PlayerReadService>();
        services.AddScoped<IMatchReadService, MatchReadService>();
        services.AddScoped<IStandingsReadService, StandingsReadService>();

        // Idempotencia y seed.
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
