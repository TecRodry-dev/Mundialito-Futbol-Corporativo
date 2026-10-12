import type { PagedResult, PagingParams, Player, Team } from "@/types/api";
import { apiRequest } from "./api/client";
import { toQueryString } from "./api/query";

export type TeamFilters = {
  name?: string;
  coach?: string;
};

export const teamsService = {
  list(paging: PagingParams, filters: TeamFilters = {}, signal?: AbortSignal) {
    return apiRequest<PagedResult<Team>>(`/api/teams${toQueryString({ ...paging, ...filters })}`, { signal });
  },

  listPlayers(teamId: string, paging: PagingParams, signal?: AbortSignal) {
    return apiRequest<PagedResult<Player>>(
      `/api/teams/${encodeURIComponent(teamId)}/players${toQueryString({ ...paging })}`,
      { signal },
    );
  },
};
