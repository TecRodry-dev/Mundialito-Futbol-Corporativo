using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mundialito.Infrastructure.Persistence.Seed;

public static class DatabaseInitializer
{
    /// <summary>
    /// Aplica las migraciones pendientes (crea la base si no existe) y carga el seed si está vacía.
    /// La Api lo llama al arrancar, también dentro de Docker.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MundialitoDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
