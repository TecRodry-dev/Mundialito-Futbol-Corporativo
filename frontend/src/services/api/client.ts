import { API_URL } from "./config";
import { ApiError, type ProblemDetails } from "./errors";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

type RequestOptions = {
  method?: HttpMethod;
  body?: unknown;
  signal?: AbortSignal;
  /** Para los POST: la misma key en un reintento devuelve la respuesta original sin duplicar. */
  idempotencyKey?: string;
};

/**
 * Único punto de salida HTTP del frontend. Los componentes nunca llaman a fetch directamente:
 * usan los servicios, que a su vez usan esta función.
 */
export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: "application/json" };

  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  if (options.idempotencyKey) {
    headers["Idempotency-Key"] = options.idempotencyKey;
  }

  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method: options.method ?? "GET",
      headers,
      body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
      signal: options.signal,
    });
  } catch (error) {
    // Si el request se canceló a propósito (cambio de filtros), se propaga tal cual.
    if (options.signal?.aborted) {
      throw error;
    }

    throw ApiError.connection();
  }

  if (!response.ok) {
    throw ApiError.fromProblem(await readProblem(response), response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function readProblem(response: Response): Promise<ProblemDetails> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return { title: response.statusText, status: response.status };
  }
}
