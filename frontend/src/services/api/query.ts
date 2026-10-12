export type QueryValue = string | number | boolean | null | undefined;

/** Arma "?a=1&b=2" omitiendo los valores vacíos. */
export function toQueryString(params: Record<string, QueryValue>): string {
  const search = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === "") {
      continue;
    }

    search.append(key, String(value));
  }

  const query = search.toString();
  return query ? `?${query}` : "";
}
