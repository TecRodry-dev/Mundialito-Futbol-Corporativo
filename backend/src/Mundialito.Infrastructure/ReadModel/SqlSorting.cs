using Mundialito.Application.Common.Paging;

namespace Mundialito.Infrastructure.ReadModel;

/// <summary>
/// Arma el ORDER BY a partir de una lista blanca: el texto del cliente nunca llega al SQL,
/// solo se usa para elegir una columna ya escrita por nosotros.
/// </summary>
internal static class SqlSorting
{
    public static string BuildOrderBy(
        PageRequest paging,
        IReadOnlyDictionary<string, string> sortColumns,
        string defaultOrderBy,
        string tieBreaker)
    {
        var orderBy = paging.SortBy is not null && sortColumns.TryGetValue(paging.SortBy, out var column)
            ? $"{column} {(paging.SortDirection == SortDirection.Desc ? "DESC" : "ASC")}"
            : defaultOrderBy;

        // El desempate por Id hace que la paginación sea estable (sin filas repetidas entre páginas).
        return $"{orderBy}, {tieBreaker}";
    }
}
