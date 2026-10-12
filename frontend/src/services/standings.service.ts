import type { PagedResult, PagingParams, Standing, TopScorer } from "@/types/api";
import { apiRequest } from "./api/client";
import { toQueryString } from "./api/query";

export type StandingFilters = {
  teamName?: string;
};

export type TopScorerFilters = {
  teamId?: string;
  playerName?: string;
};

export const standingsService = {
  standings(paging: PagingParams, filters: StandingFilters = {}, signal?: AbortSignal) {
    return apiRequest<PagedResult<Standing>>(`/api/standings${toQueryString({ ...paging, ...filters })}`, { signal });
  },

  topScorers(paging: PagingParams, filters: TopScorerFilters = {}, signal?: AbortSignal) {
    return apiRequest<PagedResult<TopScorer>>(`/api/top-scorers${toQueryString({ ...paging, ...filters })}`, {
      signal,
    });
  },
};
