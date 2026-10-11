using Mundialito.Domain.Common;

namespace Mundialito.Application.Common.Paging;

public static class PagingErrors
{
    public static readonly Error InvalidPageNumber = Error.Validation(
        "pageNumber debe ser mayor o igual a 1.", "Paging.InvalidPageNumber");

    public static readonly Error InvalidPageSize = Error.Validation(
        $"pageSize debe estar entre 1 y {PageRequest.MaxPageSize}.", "Paging.InvalidPageSize");

    public static Error InvalidSortBy(IEnumerable<string> allowedSortFields) => Error.Validation(
        $"sortBy no es válido. Valores permitidos: {string.Join(", ", allowedSortFields)}.", "Paging.InvalidSortBy");
}
