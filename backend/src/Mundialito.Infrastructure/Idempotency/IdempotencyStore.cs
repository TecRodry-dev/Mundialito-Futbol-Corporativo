using Dapper;
using Microsoft.Data.SqlClient;
using Mundialito.Application.Abstractions.Idempotency;
using Mundialito.Infrastructure.ReadModel;

namespace Mundialito.Infrastructure.Idempotency;

/// <summary>
/// Guarda las Idempotency-Key con Dapper y su propia conexión, a propósito separado del DbContext:
/// así guardar la key nunca arrastra cambios pendientes de un command que falló.
/// La tabla la crea la migración de EF Core.
/// </summary>
internal sealed class IdempotencyStore(SqlConnectionFactory connectionFactory) : IIdempotencyStore
{
    // Códigos de SQL Server para clave duplicada.
    private const int UniqueKeyViolation = 2627;
    private const int UniqueIndexViolation = 2601;

    public async Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT [Key], RequestHash, StatusCode, ResponseBody, CreatedAtUtc
            FROM IdempotencyRecords
            WHERE [Key] = @Key;
            """;

        await using var connection = connectionFactory.Create();

        return await connection.QuerySingleOrDefaultAsync<IdempotencyRecord>(
            new CommandDefinition(sql, new { Key = key }, cancellationToken: cancellationToken));
    }

    public async Task SaveAsync(IdempotencyRecord record, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO IdempotencyRecords ([Key], RequestHash, StatusCode, ResponseBody, CreatedAtUtc)
            VALUES (@Key, @RequestHash, @StatusCode, @ResponseBody, @CreatedAtUtc);
            """;

        await using var connection = connectionFactory.Create();

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(sql, record, cancellationToken: cancellationToken));
        }
        catch (SqlException exception) when (exception.Number is UniqueKeyViolation or UniqueIndexViolation)
        {
            // Dos requests simultáneos con la misma key: el primero ya guardó la respuesta. No hay nada que hacer.
        }
    }
}
