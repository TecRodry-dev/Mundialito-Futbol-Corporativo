namespace Mundialito.Application.Common.Paging;

/// <summary>
/// Respuesta paginada de todos los GET: { data, pageNumber, pageSize, totalRecords, totalPages }.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Data, int PageNumber, int PageSize, int TotalRecords)
{
    public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
}
