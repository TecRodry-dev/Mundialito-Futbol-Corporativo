import type { Match, MatchDetail, MatchStatus, PagedResult, PagingParams, RegisterResultPayload } from "@/types/api";
import { apiRequest } from "./api/client";
import { toQueryString } from "./api/query";

export type MatchFilters = {
  /** Fecha "YYYY-MM-DD" (día completo). */
  from?: string;
  to?: string;
  teamId?: string;
  status?: MatchStatus;
};

export const matchesService = {
  list(paging: PagingParams, filters: MatchFilters = {}, signal?: AbortSignal) {
    return apiRequest<PagedResult<Match>>(`/api/matches${toQueryString({ ...paging, ...filters })}`, { signal });
  },

  getById(id: string, signal?: AbortSignal) {
    return apiRequest<MatchDetail>(`/api/matches/${encodeURIComponent(id)}`, { signal });
  },

  registerResult(id: string, payload: RegisterResultPayload, idempotencyKey: string) {
    return apiRequest<MatchDetail>(`/api/matches/${encodeURIComponent(id)}/result`, {
      method: "POST",
      body: payload,
      idempotencyKey,
    });
  },
};
