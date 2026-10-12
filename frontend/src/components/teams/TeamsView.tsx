"use client";

import { Fragment, useState, type FormEvent } from "react";
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
import { formatDate } from "@/lib/format";
import { teamsService, type TeamFilters } from "@/services/teams.service";
import { TeamPlayersPanel } from "./TeamPlayersPanel";

const COLUMN_COUNT = 5;

export function TeamsView() {
  const { paging, setPage, setPageSize, toggleSort, resetPage } = usePaging({ sortBy: "name" });
  const [nameDraft, setNameDraft] = useState("");
  const [coachDraft, setCoachDraft] = useState("");
  const [filters, setFilters] = useState<TeamFilters>({});
  const [expandedTeamId, setExpandedTeamId] = useState<string | null>(null);

  const { data, error, isLoading, reload } = useApiQuery(`teams:${JSON.stringify({ paging, filters })}`, (signal) =>
    teamsService.list(paging, filters, signal),
  );

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFilters({ name: nameDraft.trim() || undefined, coach: coachDraft.trim() || undefined });
    resetPage();
  }

  function toggleTeam(teamId: string) {
    setExpandedTeamId((current) => (current === teamId ? null : teamId));
  }

  const rows = data?.data ?? [];

  return (
    <section>
      <PageHeader title="Equipos" description="Equipos inscritos en el torneo. Despliega un equipo para ver su plantel." />

      <form onSubmit={handleSearch} className="mb-4 flex flex-col gap-2 sm:flex-row">
        <input
          type="search"
          value={nameDraft}
          onChange={(event) => setNameDraft(event.target.value)}
          placeholder="Nombre del equipo..."
          aria-label="Filtrar por nombre"
          className={`${inputClass} sm:max-w-xs`}
        />
        <input
          type="search"
          value={coachDraft}
          onChange={(event) => setCoachDraft(event.target.value)}
          placeholder="Entrenador..."
          aria-label="Filtrar por entrenador"
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
                <SortableHeader label="Equipo" field="name" paging={paging} onSort={toggleSort} />
                <SortableHeader label="Entrenador" field="coach" paging={paging} onSort={toggleSort} />
                <SortableHeader label="Jugadores" field="playersCount" paging={paging} onSort={toggleSort} defaultDirection="Desc" align="center" />
                <SortableHeader label="Inscrito" field="createdAt" paging={paging} onSort={toggleSort} defaultDirection="Desc" />
                <th scope="col" className="px-3 py-2 text-right font-medium">
                  <span className="sr-only">Acciones</span>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100">
              {!data && isLoading && <TableMessage colSpan={COLUMN_COUNT}>Cargando equipos...</TableMessage>}
              {data && rows.length === 0 && <TableMessage colSpan={COLUMN_COUNT}>No hay equipos que coincidan.</TableMessage>}
              {rows.map((team) => {
                const isExpanded = expandedTeamId === team.id;

                return (
                  <Fragment key={team.id}>
                    <tr className={rowClass}>
                      <td className={`${cellClass} font-medium text-zinc-900`}>{team.name}</td>
                      <td className={cellClass}>{team.coach}</td>
                      <td className={`${cellClass} text-center`}>{team.playersCount}</td>
                      <td className={cellClass}>{formatDate(team.createdAtUtc)}</td>
                      <td className={`${cellClass} text-right`}>
                        <button
                          type="button"
                          onClick={() => toggleTeam(team.id)}
                          aria-expanded={isExpanded}
                          className="text-sm font-medium text-emerald-700 hover:text-emerald-900"
                        >
                          {isExpanded ? "Ocultar plantel" : "Ver plantel"}
                        </button>
                      </td>
                    </tr>
                    {isExpanded && (
                      <tr>
                        <td colSpan={COLUMN_COUNT} className="bg-zinc-50 px-3 py-4">
                          <TeamPlayersPanel teamId={team.id} />
                        </td>
                      </tr>
                    )}
                  </Fragment>
                );
              })}
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
