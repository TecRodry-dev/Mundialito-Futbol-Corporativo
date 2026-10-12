import { teamsService } from "@/services/teams.service";
import { useApiQuery } from "./useApiQuery";

/** Lista de equipos para los selectores de filtros. */
export function useTeamOptions() {
  const { data } = useApiQuery("team-options", (signal) =>
    teamsService.list({ pageNumber: 1, pageSize: 100, sortBy: "name" }, {}, signal),
  );

  return data?.data ?? [];
}
