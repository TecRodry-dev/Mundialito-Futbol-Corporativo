import type { ReactNode } from "react";

type TableMessageProps = {
  colSpan: number;
  children: ReactNode;
};

/** Fila que ocupa toda la tabla para mostrar "Cargando..." o "Sin resultados". */
export function TableMessage({ colSpan, children }: TableMessageProps) {
  return (
    <tr>
      <td colSpan={colSpan} className="px-3 py-10 text-center text-sm text-zinc-500">
        {children}
      </td>
    </tr>
  );
}
