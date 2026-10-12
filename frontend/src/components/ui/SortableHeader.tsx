import type { PagingParams, SortDirection } from "@/types/api";

type Align = "left" | "center" | "right";

const alignClasses: Record<Align, string> = {
  left: "text-left",
  center: "text-center",
  right: "text-right",
};

type SortableHeaderProps = {
  label: string;
  field: string;
  paging: PagingParams;
  onSort: (field: string, defaultDirection: SortDirection) => void;
  /** Dirección del primer clic: los números suelen ordenarse de mayor a menor. */
  defaultDirection?: SortDirection;
  align?: Align;
};

/** Encabezado de columna que ordena la tabla en la API (sortBy + sortDirection). */
export function SortableHeader({
  label,
  field,
  paging,
  onSort,
  defaultDirection = "Asc",
  align = "left",
}: SortableHeaderProps) {
  const isActive = paging.sortBy === field;
  const isDescending = paging.sortDirection === "Desc";
  const indicator = !isActive ? "↕" : isDescending ? "↓" : "↑";

  return (
    <th
      scope="col"
      aria-sort={isActive ? (isDescending ? "descending" : "ascending") : "none"}
      className={`px-3 py-2 font-medium whitespace-nowrap ${alignClasses[align]}`}
    >
      <button
        type="button"
        onClick={() => onSort(field, defaultDirection)}
        className={`inline-flex items-center gap-1 hover:text-zinc-900 ${isActive ? "text-zinc-900" : ""}`}
      >
        {label}
        <span aria-hidden="true" className="text-xs text-zinc-400">
          {indicator}
        </span>
      </button>
    </th>
  );
}
