using Mundialito.Domain.Common;

namespace Mundialito.Application.Common.Paging;

/// <summary>
/// Parámetros comunes de todos los GET: página, tamaño y ordenamiento.
/// </summary>
public sealed record PageRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Asc)
{
    public const int MaxPageSize = 100;

    /// <summary>Cuántas filas saltar en SQL (OFFSET).</summary>
    public int Offset => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Valida la página y que <see cref="SortBy"/> esté en la lista blanca de la consulta.
    /// </summary>
    public Result Validate(IReadOnlyCollection<string> allowedSortFields)
    {
        if (PageNumber < 1)
        {
            return Result.Failure(PagingErrors.InvalidPageNumber);
        }

        if (PageSize < 1 || PageSize > MaxPageSize)
        {
            return Result.Failure(PagingErrors.InvalidPageSize);
        }

        if (SortBy is not null && !allowedSortFields.Contains(SortBy, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(PagingErrors.InvalidSortBy(allowedSortFields));
        }

        return Result.Success();
    }
}
