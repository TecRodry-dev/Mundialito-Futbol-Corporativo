import type { MatchStatus, PlayerPosition } from "@/types/api";

const dateTimeFormatter = new Intl.DateTimeFormat("es", { dateStyle: "medium", timeStyle: "short" });
const dateFormatter = new Intl.DateTimeFormat("es", { dateStyle: "medium" });

export function formatDateTime(value: string): string {
  return dateTimeFormatter.format(new Date(value));
}

export function formatDate(value: string): string {
  return dateFormatter.format(new Date(value));
}

export const positionLabels: Record<PlayerPosition, string> = {
  Goalkeeper: "Portero",
  Defender: "Defensa",
  Midfielder: "Mediocampista",
  Forward: "Delantero",
};

export const statusLabels: Record<MatchStatus, string> = {
  Scheduled: "Programado",
  Played: "Jugado",
};
