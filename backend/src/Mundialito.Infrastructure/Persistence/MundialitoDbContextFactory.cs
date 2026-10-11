using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mundialito.Infrastructure.Persistence;

/// <summary>
/// Solo lo usa "dotnet ef migrations add" para conocer el modelo. No abre ninguna conexión,
/// por eso la cadena no lleva credenciales.
/// </summary>
public sealed class MundialitoDbContextFactory : IDesignTimeDbContextFactory<MundialitoDbContext>
{
    public MundialitoDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MundialitoDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=Mundialito;TrustServerCertificate=True")
            .Options;

        return new MundialitoDbContext(options);
    }
}
