// Tipos que reflejan exactamente los contratos JSON de la API.

export type SortDirection = "Asc" | "Desc";

export type PagingParams = {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: SortDirection;
};

export type PagedResult<T> = {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
};

export type Team = {
  id: string;
  name: string;
  coach: string;
  playersCount: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

export type PlayerPosition = "Goalkeeper" | "Defender" | "Midfielder" | "Forward";

export type Player = {
  id: string;
  teamId: string;
  teamName: string;
  fullName: string;
  shirtNumber: number;
  position: PlayerPosition;
  goals: number;
  createdAtUtc: string;
};

export type MatchStatus = "Scheduled" | "Played";

export type Match = {
  id: string;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  scheduledAt: string;
  venue: string | null;
  status: MatchStatus;
  homeGoals: number | null;
  awayGoals: number | null;
};

export type MatchGoal = {
  playerId: string;
  playerName: string;
  teamId: string;
  teamName: string;
  minute: number | null;
};

export type MatchDetail = Match & {
  goals: MatchGoal[];
};

export type Standing = {
  position: number;
  teamId: string;
  teamName: string;
  played: number;
  won: number;
  drawn: number;
  lost: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
};

export type TopScorer = {
  position: number;
  playerId: string;
  playerName: string;
  teamId: string;
  teamName: string;
  goals: number;
};

export type GoalInput = {
  playerId: string;
  minute: number | null;
};

export type RegisterResultPayload = {
  homeGoals: number;
  awayGoals: number;
  goals: GoalInput[];
};
