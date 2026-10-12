"use client";

import { ErrorAlert } from "@/components/ui/ErrorAlert";
import { useApiQuery } from "@/hooks/useApiQuery";
import { positionLabels } from "@/lib/format";
import { teamsService } from "@/services/teams.service";

type TeamPlayersPanelProps = {
  teamId: string;
};

/** Plantel de un equipo (GET /api/teams/{id}/players). */
export function TeamPlayersPanel({ teamId }: TeamPlayersPanelProps) {
  const { data, error, isLoading, reload } = useApiQuery(`team-players:${teamId}`, (signal) =>
    teamsService.listPlayers(teamId, { pageNumber: 1, pageSize: 50, sortBy: "shirtNumber" }, signal),
  );

  if (error) {
    return <ErrorAlert error={error} onRetry={reload} />;
  }

  if (!data && isLoading) {
    return <p className="text-sm text-zinc-500">Cargando jugadores...</p>;
  }

  const players = data?.data ?? [];
  if (players.length === 0) {
    return <p className="text-sm text-zinc-500">Este equipo todavía no tiene jugadores.</p>;
  }

  return (
    <ul className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
      {players.map((player) => (
        <li key={player.id} className="flex items-center gap-3 rounded-lg border border-zinc-200 bg-white px-3 py-2">
          <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-emerald-100 text-sm font-semibold text-emerald-800">
            {player.shirtNumber}
          </span>
          <div className="min-w-0">
            <p className="truncate text-sm font-medium text-zinc-900">{player.fullName}</p>
            <p className="text-xs text-zinc-500">
              {positionLabels[player.position]} · {player.goals} {player.goals === 1 ? "gol" : "goles"}
            </p>
          </div>
        </li>
      ))}
    </ul>
  );
}
