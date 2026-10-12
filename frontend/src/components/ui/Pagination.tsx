const PAGE_SIZE_OPTIONS = [5, 10, 20, 50];

type PaginationProps = {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  onPageChange: (pageNumber: number) => void;
  onPageSizeChange: (pageSize: number) => void;
};

export function Pagination({
  pageNumber,
  pageSize,
  totalPages,
  totalRecords,
  onPageChange,
  onPageSizeChange,
}: PaginationProps) {
  const lastPage = Math.max(totalPages, 1);
  const buttonClass =
    "rounded-md border border-zinc-300 bg-white px-3 py-1.5 text-sm font-medium text-zinc-700 hover:bg-zinc-50 disabled:cursor-not-allowed disabled:opacity-40";

  return (
    <div className="flex flex-col gap-3 border-t border-zinc-200 px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-sm text-zinc-600">
        {totalRecords} {totalRecords === 1 ? "registro" : "registros"} · Página {pageNumber} de {lastPage}
      </p>

      <div className="flex flex-wrap items-center gap-2">
        <label className="flex items-center gap-2 text-sm text-zinc-600">
          Por página
          <select
            value={pageSize}
            onChange={(event) => onPageSizeChange(Number(event.target.value))}
            className="rounded-md border border-zinc-300 bg-white px-2 py-1.5 text-sm"
          >
            {PAGE_SIZE_OPTIONS.map((size) => (
              <option key={size} value={size}>
                {size}
              </option>
            ))}
          </select>
        </label>

        <button
          type="button"
          className={buttonClass}
          disabled={pageNumber <= 1}
          onClick={() => onPageChange(pageNumber - 1)}
        >
          Anterior
        </button>
        <button
          type="button"
          className={buttonClass}
          disabled={pageNumber >= lastPage}
          onClick={() => onPageChange(pageNumber + 1)}
        >
          Siguiente
        </button>
      </div>
    </div>
  );
}
