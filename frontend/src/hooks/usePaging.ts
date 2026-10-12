import { useState } from "react";
import type { PagingParams, SortDirection } from "@/types/api";

/**
 * Estado de paginación y orden de una tabla. Cambiar el orden o el tamaño vuelve a la página 1.
 */
export function usePaging(initial: Partial<PagingParams> = {}) {
  const [paging, setPaging] = useState<PagingParams>({ pageNumber: 1, pageSize: 10, ...initial });

  function setPage(pageNumber: number) {
    setPaging((current) => ({ ...current, pageNumber }));
  }

  function setPageSize(pageSize: number) {
    setPaging((current) => ({ ...current, pageSize, pageNumber: 1 }));
  }

  /** Mismo campo: invierte la dirección. Campo nuevo: usa la dirección por defecto de esa columna. */
  function toggleSort(field: string, defaultDirection: SortDirection = "Asc") {
    setPaging((current) => {
      const currentDirection = current.sortDirection ?? "Asc";
      const sortDirection: SortDirection =
        current.sortBy === field ? (currentDirection === "Asc" ? "Desc" : "Asc") : defaultDirection;

      return { ...current, pageNumber: 1, sortBy: field, sortDirection };
    });
  }

  function resetPage() {
    setPaging((current) => ({ ...current, pageNumber: 1 }));
  }

  return { paging, setPage, setPageSize, toggleSort, resetPage };
}
