"use client";

import { useEffect, useEffectEvent, useRef, useState, type FormEvent } from "react";
import { ErrorAlert } from "@/components/ui/ErrorAlert";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "@/components/ui/styles";
import { useApiQuery } from "@/hooks/useApiQuery";
import { formatDateTime } from "@/lib/format";
import { ApiError, toApiError } from "@/services/api/errors";
import { matchesService } from "@/services/matches.service";
import { teamsService } from "@/services/teams.service";
import type { Match, MatchDetail, PagingParams, Player } from "@/types/api";

const MAX_GOALS = 30;
const ROSTER_PAGING: PagingParams = { pageNumber: 1, pageSize: 50, sortBy: "shirtNumber" };

type ScorerRow = {
  playerId: string;
  minute: string;
};

type Side = "home" | "away";

type RegisterResultDialogProps = {
  match: Match;
  onClose: () => void;
  onRegistered: (match: MatchDetail) => void;
};

/**
 * Formulario para registrar el resultado de un partido: marcador + goleadores.
 * Hay una fila de goleador por cada gol, así la cantidad siempre cuadra con el marcador.
 */
export function RegisterResultDialog({ match, onClose, onRegistered }: RegisterResultDialogProps) {
  const [homeGoals, setHomeGoals] = useState(0);
  const [awayGoals, setAwayGoals] = useState(0);
  const [homeScorers, setHomeScorers] = useState<ScorerRow[]>([]);
  const [awayScorers, setAwayScorers] = useState<ScorerRow[]>([]);
  const [formError, setFormError] = useState<string | null>(null);
  const [apiError, setApiError] = useState<ApiError | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Una key por intento: si el usuario reintenta SIN cambiar nada (doble clic, red caída),
  // se reutiliza y la API devuelve la respuesta original sin registrar dos veces.
  const idempotencyKeyRef = useRef<string | null>(null);

  const homeRoster = useApiQuery(`roster:${match.homeTeamId}`, (signal) =>
    teamsService.listPlayers(match.homeTeamId, ROSTER_PAGING, signal),
  );
  const awayRoster = useApiQuery(`roster:${match.awayTeamId}`, (signal) =>
    teamsService.listPlayers(match.awayTeamId, ROSTER_PAGING, signal),
  );

  const closeOnEscape = useEffectEvent((event: KeyboardEvent) => {
    if (event.key === "Escape" && !isSubmitting) {
      onClose();
    }
  });

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => closeOnEscape(event);
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, []);

  const rosterError = homeRoster.error ?? awayRoster.error;

  function changeGoals(side: Side, rawValue: string) {
    const goals = Math.min(MAX_GOALS, Math.max(0, Math.trunc(Number(rawValue) || 0)));
    idempotencyKeyRef.current = null;

    if (side === "home") {
      setHomeGoals(goals);
      setHomeScorers((rows) => resizeRows(rows, goals));
    } else {
      setAwayGoals(goals);
      setAwayScorers((rows) => resizeRows(rows, goals));
    }
  }

  function changeScorer(side: Side, index: number, change: Partial<ScorerRow>) {
    idempotencyKeyRef.current = null;
    const update = (rows: ScorerRow[]) => rows.map((row, i) => (i === index ? { ...row, ...change } : row));

    if (side === "home") {
      setHomeScorers(update);
    } else {
      setAwayScorers(update);
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError(null);
    setApiError(null);

    const scorers = [...homeScorers, ...awayScorers];
    if (scorers.some((scorer) => !scorer.playerId)) {
      setFormError("Seleccione el jugador que marcó cada gol.");
      return;
    }

    idempotencyKeyRef.current ??= crypto.randomUUID();
    setIsSubmitting(true);

    try {
      const updated = await matchesService.registerResult(
        match.id,
        {
          homeGoals,
          awayGoals,
          goals: scorers.map((scorer) => ({
            playerId: scorer.playerId,
            minute: scorer.minute ? Number(scorer.minute) : null,
          })),
        },
        idempotencyKeyRef.current,
      );
      onRegistered(updated);
    } catch (error) {
      setApiError(toApiError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 p-0 sm:items-center sm:p-4">
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="register-result-title"
        className="max-h-[92vh] w-full max-w-2xl overflow-y-auto rounded-t-2xl bg-white p-5 shadow-xl sm:rounded-2xl sm:p-6"
      >
        <div className="mb-4 flex items-start justify-between gap-4">
          <div>
            <h2 id="register-result-title" className="text-lg font-semibold text-zinc-900">
              Registrar resultado
            </h2>
            <p className="text-sm text-zinc-600">
              {match.homeTeamName} vs {match.awayTeamName} · {formatDateTime(match.scheduledAt)}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            disabled={isSubmitting}
            aria-label="Cerrar"
            className="rounded-md p-1 text-zinc-500 hover:bg-zinc-100 hover:text-zinc-800"
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="grid grid-cols-2 gap-4">
            <label className={labelClass}>
              Goles {match.homeTeamName}
              <input
                type="number"
                min={0}
                max={MAX_GOALS}
                value={homeGoals}
                onChange={(event) => changeGoals("home", event.target.value)}
                className={inputClass}
              />
            </label>
            <label className={labelClass}>
              Goles {match.awayTeamName}
              <input
                type="number"
                min={0}
                max={MAX_GOALS}
                value={awayGoals}
                onChange={(event) => changeGoals("away", event.target.value)}
                className={inputClass}
              />
            </label>
          </div>

          <ScorerList
            title={`Goleadores de ${match.homeTeamName}`}
            rows={homeScorers}
            players={homeRoster.data?.data ?? []}
            isLoadingPlayers={homeRoster.isLoading}
            onChange={(index, change) => changeScorer("home", index, change)}
          />
          <ScorerList
            title={`Goleadores de ${match.awayTeamName}`}
            rows={awayScorers}
            players={awayRoster.data?.data ?? []}
            isLoadingPlayers={awayRoster.isLoading}
            onChange={(index, change) => changeScorer("away", index, change)}
          />

          {rosterError && <ErrorAlert error={rosterError} />}
          {formError && (
            <p role="alert" className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
              {formError}
            </p>
          )}
          {apiError && <ErrorAlert error={apiError} />}

          <div className="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
            <button type="button" onClick={onClose} disabled={isSubmitting} className={secondaryButtonClass}>
              Cancelar
            </button>
            <button type="submit" disabled={isSubmitting} className={primaryButtonClass}>
              {isSubmitting ? "Registrando..." : "Registrar resultado"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

type ScorerListProps = {
  title: string;
  rows: ScorerRow[];
  players: Player[];
  isLoadingPlayers: boolean;
  onChange: (index: number, change: Partial<ScorerRow>) => void;
};

function ScorerList({ title, rows, players, isLoadingPlayers, onChange }: ScorerListProps) {
  if (rows.length === 0) {
    return null;
  }

  return (
    <fieldset className="space-y-2">
      <legend className="mb-1 text-sm font-semibold text-zinc-800">{title}</legend>
      {rows.map((row, index) => (
        <div key={index} className="grid grid-cols-[1fr_6rem] gap-2">
          <select
            value={row.playerId}
            onChange={(event) => onChange(index, { playerId: event.target.value })}
            aria-label={`Jugador del gol ${index + 1}`}
            className={inputClass}
          >
            <option value="">{isLoadingPlayers ? "Cargando jugadores..." : `Gol ${index + 1}: elegir jugador`}</option>
            {players.map((player) => (
              <option key={player.id} value={player.id}>
                #{player.shirtNumber} {player.fullName}
              </option>
            ))}
          </select>
          <input
            type="number"
            min={1}
            max={120}
            value={row.minute}
            onChange={(event) => onChange(index, { minute: event.target.value })}
            placeholder="Min."
            aria-label={`Minuto del gol ${index + 1}`}
            className={inputClass}
          />
        </div>
      ))}
    </fieldset>
  );
}

function resizeRows(rows: ScorerRow[], length: number): ScorerRow[] {
  return Array.from({ length }, (_, index) => rows[index] ?? { playerId: "", minute: "" });
}
