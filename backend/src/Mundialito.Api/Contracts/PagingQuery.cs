using Mundialito.Application.Common.Paging;

namespace Mundialito.Api.Contracts;

/// <summary>
/// Parámetros de paginación y orden que aceptan todos los GET por query string.
/// </summary>
public sealed class PagingQuery
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Asc;

    public PageRequest ToPageRequest() => new(PageNumber, PageSize, SortBy, SortDirection);
}
