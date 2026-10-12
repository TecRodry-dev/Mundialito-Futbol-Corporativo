"use client";

import { useState } from "react";
import { ErrorAlert } from "@/components/ui/ErrorAlert";
import { PageHeader } from "@/components/ui/PageHeader";
import { Pagination } from "@/components/ui/Pagination";
import { SortableHeader } from "@/components/ui/SortableHeader";
import { TableMessage } from "@/components/ui/TableMessage";
import {
  cardClass,
  cellClass,
  inputClass,
  labelClass,
  rowClass,
  secondaryButtonClass,
  tableClass,
  theadClass,
} from "@/components/ui/styles";
import { useApiQuery } from "@/hooks/useApiQuery";
import { usePaging } from "@/hooks/usePaging";
import { useTeamOptions } from "@/hooks/useTeamOptions";
import { formatDateTime } from "@/lib/format";
import { matchesService, type MatchFilters } from "@/services/matches.service";
import type { Match, MatchDetail, MatchStatus } from "@/types/api";
import { MatchStatusBadge } from "./MatchStatusBadge";
import { RegisterResultDialog } from "./RegisterResultDialog";

const COLUMN_COUNT = 5;

export function MatchesView() {
  const { paging, setPage, setPageSize, toggleSort, resetPage } = usePaging();
  const teams = useTeamOptions();
  const [filters, setFilters] = useState<MatchFilters>({});
  const [selectedMatch, setSelectedMatch] = useState<Match | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const { data, error, isLoading, reload } = useApiQuery(`matches:${JSON.stringify({ paging, filters })}`, (signal) =>
    matchesService.list(paging, filters, signal),
  );

  function changeFilter(change: Partial<MatchFilters>) {
    setFilters((current) => ({ ...current, ...change }));
    resetPage();
  }

  function clearFilters() {
    setFilters({});
    resetPage();
  }

  function handleRegistered(match: MatchDetail) {
    setSelectedMatch(null);
    setSuccessMessage(
      `Resultado registrado: ${match.homeTeamName} ${match.homeGoals} - ${match.awayGoals} ${match.awayTeamName}.`,
    );
    reload();
  }

  const rows = data?.data ?? [];
  const hasFilters = Object.values(filters).some(Boolean);

  return (
    <section>
      <PageHeader title="Partidos" description="Calendario y resultados. Filtra por fechas, equipo o estado." />

      <div className="mb-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-5 lg:items-end">
        <label className={labelClass}>
          Desde
          <input
            type="date"
            value={filters.from ?? ""}
            onChange={(event) => changeFilter({ from: event.target.value || undefined })}
            className={inputClass}
          />
        </label>
        <label className={labelClass}>
          Hasta
          <input
            type="date"
            value={filters.to ?? ""}
            onChange={(event) => changeFilter({ to: event.target.value || undefined })}
            className={inputClass}
          />
        </label>
        <label className={labelClass}>
          Equipo
          <select
            value={filters.teamId ?? ""}
            onChange={(event) => changeFilter({ teamId: event.target.value || undefined })}
            className={inputClass}
          >
            <option value="">Todos</option>
            {teams.map((team) => (
              <option key={team.id} value={team.id}>
                {team.name}
              </option>
            ))}
          </select>
        </label>
        <label className={labelClass}>
          Estado
          <select
            value={filters.status ?? ""}
            onChange={(event) => changeFilter({ status: (event.target.value as MatchStatus) || undefined })}
            className={inputClass}
          >
            <option value="">Todos</option>
            <option value="Scheduled">Programados</option>
            <option value="Played">Jugados</option>
          </select>
        </label>
        <button type="button" onClick={clearFilters} disabled={!hasFilters} className={`${secondaryButtonClass} disabled:opacity-40`}>
          Limpiar filtros
        </button>
      </div>

      {successMessage && (
        <div
          role="status"
          className="mb-4 flex items-start justify-between gap-3 rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-800"
        >
          <p>{successMessage}</p>
          <button type="button" onClick={() => setSuccessMessage(null)} aria-label="Cerrar mensaje" className="font-medium">
            ✕
          </button>
        </div>
      )}

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
                <SortableHeader label="Fecha" field="scheduledAt" paging={paging} onSort={toggleSort} />
                <SortableHeader label="Partido" field="homeTeam" paging={paging} onSort={toggleSort} />
                <th scope="col" className="px-3 py-2 text-left font-medium">Cancha</th>
                <SortableHeader label="Estado" field="status" paging={paging} onSort={toggleSort} />
                <th scope="col" className="px-3 py-2 text-right font-medium">
                  <span className="sr-only">Acciones</span>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100">
              {!data && isLoading && <TableMessage colSpan={COLUMN_COUNT}>Cargando partidos...</TableMessage>}
              {data && rows.length === 0 && <TableMessage colSpan={COLUMN_COUNT}>No hay partidos que coincidan.</TableMessage>}
              {rows.map((match) => (
                <tr key={match.id} className={rowClass}>
                  <td className={`${cellClass} text-zinc-600`}>{formatDateTime(match.scheduledAt)}</td>
                  <td className={cellClass}>
                    <span className="font-medium text-zinc-900">{match.homeTeamName}</span>
                    <span className="mx-2 inline-block min-w-12 rounded bg-zinc-100 px-2 py-0.5 text-center font-semibold text-zinc-800">
                      {match.status === "Played" ? `${match.homeGoals} - ${match.awayGoals}` : "vs"}
                    </span>
                    <span className="font-medium text-zinc-900">{match.awayTeamName}</span>
                  </td>
                  <td className={`${cellClass} text-zinc-600`}>{match.venue ?? "—"}</td>
                  <td className={cellClass}>
                    <MatchStatusBadge status={match.status} />
                  </td>
                  <td className={`${cellClass} text-right`}>
                    {match.status === "Scheduled" && (
                      <button
                        type="button"
                        onClick={() => setSelectedMatch(match)}
                        className="text-sm font-medium text-emerald-700 hover:text-emerald-900"
                      >
                        Registrar resultado
                      </button>
                    )}
                  </td>
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

      {selectedMatch && (
        <RegisterResultDialog
          match={selectedMatch}
          onClose={() => setSelectedMatch(null)}
          onRegistered={handleRegistered}
        />
      )}
    </section>
  );
}
