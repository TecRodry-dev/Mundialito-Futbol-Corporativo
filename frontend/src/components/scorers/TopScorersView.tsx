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
import { useTeamOptions } from "@/hooks/useTeamOptions";
import { standingsService, type TopScorerFilters } from "@/services/standings.service";

const COLUMN_COUNT = 4;

export function TopScorersView() {
  const { paging, setPage, setPageSize, toggleSort, resetPage } = usePaging();
  const teams = useTeamOptions();
  const [playerNameDraft, setPlayerNameDraft] = useState("");
  const [filters, setFilters] = useState<TopScorerFilters>({});

  const { data, error, isLoading, reload } = useApiQuery(`top-scorers:${JSON.stringify({ paging, filters })}`, (signal) =>
    standingsService.topScorers(paging, filters, signal),
  );

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFilters((current) => ({ ...current, playerName: playerNameDraft.trim() || undefined }));
    resetPage();
  }

  function handleTeamChange(teamId: string) {
    setFilters((current) => ({ ...current, teamId: teamId || undefined }));
    resetPage();
  }

  const rows = data?.data ?? [];

  return (
    <section>
      <PageHeader title="Goleadores" description="Jugadores con más goles en partidos jugados." />

      <form onSubmit={handleSearch} className="mb-4 flex flex-col gap-2 sm:flex-row">
        <select
          value={filters.teamId ?? ""}
          onChange={(event) => handleTeamChange(event.target.value)}
          aria-label="Filtrar por equipo"
          className={`${inputClass} sm:max-w-xs`}
        >
          <option value="">Todos los equipos</option>
          {teams.map((team) => (
            <option key={team.id} value={team.id}>
              {team.name}
            </option>
          ))}
        </select>
        <input
          type="search"
          value={playerNameDraft}
          onChange={(event) => setPlayerNameDraft(event.target.value)}
          placeholder="Buscar jugador..."
          aria-label="Buscar jugador"
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
                <th scope="col" className="px-3 py-2 text-center font-medium">#</th>
                <SortableHeader label="Jugador" field="playerName" paging={paging} onSort={toggleSort} />
                <SortableHeader label="Equipo" field="teamName" paging={paging} onSort={toggleSort} />
                <SortableHeader label="Goles" field="goals" paging={paging} onSort={toggleSort} defaultDirection="Desc" align="center" />
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100">
              {!data && isLoading && <TableMessage colSpan={COLUMN_COUNT}>Cargando goleadores...</TableMessage>}
              {data && rows.length === 0 && <TableMessage colSpan={COLUMN_COUNT}>Todavía no hay goles registrados.</TableMessage>}
              {rows.map((scorer) => (
                <tr key={scorer.playerId} className={rowClass}>
                  <td className={`${cellClass} text-center font-semibold text-zinc-500`}>{scorer.position}</td>
                  <td className={`${cellClass} font-medium text-zinc-900`}>{scorer.playerName}</td>
                  <td className={cellClass}>{scorer.teamName}</td>
                  <td className={`${cellClass} text-center text-base font-bold text-emerald-700`}>{scorer.goals}</td>
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
