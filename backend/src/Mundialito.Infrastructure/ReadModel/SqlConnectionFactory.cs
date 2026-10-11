using Microsoft.Data.SqlClient;

namespace Mundialito.Infrastructure.ReadModel;

/// <summary>
/// Crea conexiones a SQL Server para las consultas con Dapper (lado de lectura).
/// </summary>
internal sealed class SqlConnectionFactory(string connectionString)
{
    public SqlConnection Create() => new(connectionString);
}
