"use client";

import { useState, type FormEvent } from "react";
import { ErrorAlert } from "@/components/ui/ErrorAlert";
import { PageHeader } from "@/components/ui/PageHeader";
import { Pagination } from "@/components/ui/Pagination";
import { SortableHeader } from "@/components/ui/SortableHeader";
import { TableMessage } from "@/components/ui/TableMessage";
import {
  cardClass,
  cellClass,
  inputClass,
  primaryButtonClass,
  rowClass,
  tableClass,
  theadClass,
} from "@/components/ui/styles";
import { useApiQuery } from "@/hooks/useApiQuery";
import { usePaging } from "@/hooks/usePaging";
import { standingsService, type StandingFilters } from "@/services/standings.service";

const COLUMN_COUNT = 10;

export function StandingsView() {
  const { paging, setPage, setPageSize, toggleSort, resetPage } = usePaging();
  const [teamNameDraft, setTeamNameDraft] = useState("");
  const [filters, setFilters] = useState<StandingFilters>({});

  const { data, error, isLoading, reload } = useApiQuery(`standings:${JSON.stringify({ paging, filters })}`, (signal) =>
    standingsService.standings(paging, filters, signal),
  );

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFilters({ teamName: teamNameDraft.trim() || undefined });
    resetPage();
  }

  const rows = data?.data ?? [];

  return (
    <section>
      <PageHeader
        title="Tabla de posiciones"
        description="Victoria 3 pts · Empate 1 pt · Derrota 0 pts. Orden oficial: puntos → diferencia de gol → goles a favor."
      />

      <form onSubmit={handleSearch} className="mb-4 flex flex-col gap-2 sm:flex-row">
        <input
          type="search"
          value={teamNameDraft}
          onChange={(event) => setTeamNameDraft(event.target.value)}
          placeholder="Buscar equipo..."
          aria-label="Buscar equipo"
          className={`${inputClass} sm:max-w-xs`}
        />
        <button type="submit" className={primaryButtonClass}>
          Buscar
        </button>
      </form>

      {error && (
        <div className="mb-4">
          <ErrorAlert error={error} onRetry={reload} />
        </div>
      )}

      <div className={cardClass}>
        <div className={`overflow-x-auto transition-opacity ${isLoading && data ? "opacity-60" : ""}`}>
          <table className={tableClass}>
            <thead className={theadClass}>
              <tr>
                <SortableHeader label="#" field="position" paging={paging} onSort={toggleSort} align="center" />
                <SortableHeader label="Equipo" field="teamName" paging={paging} onSort={toggleSort} />
                <SortableHeader label="PJ" field="played" paging={paging} onSort={toggleSort} defaultDirection="Desc" align="center" />
                <th scope="col" className="px-3 py-2 text-center font-medium">G</th>
                <th scope="col" className="px-3 py-2 text-center font-medium">E</th>
                <th scope="col" className="px-3 py-2 text-center font-medium">P</th>
                <SortableHeader label="GF" field="goalsFor" paging={paging} onSort={toggleSort} defaultDirection="Desc" align="center" />
                <th scope="col" className="px-3 py-2 text-center font-medium">GC</th>
                <SortableHeader label="DG" field="goalDifference" paging={paging} onSort={toggleSort} defaultDirection="Desc" align="center" />
                <SortableHeader label="Pts" field="points" paging={paging} onSort={toggleSort} defaultDirection="Desc" align="center" />
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100">
              {!data && isLoading && <TableMessage colSpan={COLUMN_COUNT}>Cargando tabla...</TableMessage>}
              {data && rows.length === 0 && <TableMessage colSpan={COLUMN_COUNT}>No hay equipos que coincidan.</TableMessage>}
              {rows.map((standing) => (
                <tr key={standing.teamId} className={rowClass}>
                  <td className={`${cellClass} text-center font-semibold text-zinc-500`}>{standing.position}</td>
                  <td className={`${cellClass} font-medium text-zinc-900`}>{standing.teamName}</td>
                  <td className={`${cellClass} text-center`}>{standing.played}</td>
                  <td className={`${cellClass} text-center`}>{standing.won}</td>
                  <td className={`${cellClass} text-center`}>{standing.drawn}</td>
                  <td className={`${cellClass} text-center`}>{standing.lost}</td>
                  <td className={`${cellClass} text-center`}>{standing.goalsFor}</td>
                  <td className={`${cellClass} text-center`}>{standing.goalsAgainst}</td>
                  <td className={`${cellClass} text-center`}>
                    {standing.goalDifference > 0 ? `+${standing.goalDifference}` : standing.goalDifference}
                  </td>
                  <td className={`${cellClass} text-center text-base font-bold text-emerald-700`}>{standing.points}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {data && (
          <Pagination
            pageNumber={data.pageNumber}
            pageSize={data.pageSize}
            totalPages={data.totalPages}
            totalRecords={data.totalRecords}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
          />
        )}
      </div>
    </section>
  );
}
