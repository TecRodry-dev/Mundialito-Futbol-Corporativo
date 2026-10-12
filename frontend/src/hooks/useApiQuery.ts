import { useCallback, useEffect, useEffectEvent, useState } from "react";
import { ApiError, toApiError } from "@/services/api/errors";

type QueryState<T> = {
  key: string;
  data?: T;
  error?: ApiError;
};

/**
 * Ejecuta una lectura de la API cada vez que cambia `key` (filtros, página, orden).
 * - Cancela el request anterior si llega uno nuevo (AbortController).
 * - Mantiene los datos anteriores mientras carga, para que la tabla no "parpadee".
 * - `reload()` vuelve a pedir los datos (ej. después de registrar un resultado).
 */
export function useApiQuery<T>(key: string, fetcher: (signal: AbortSignal) => Promise<T>) {
  const [reloadCount, setReloadCount] = useState(0);
  const [state, setState] = useState<QueryState<T> | null>(null);
  const requestKey = `${key}#${reloadCount}`;

  // Siempre usa la versión más reciente de `fetcher` sin volver a disparar el efecto.
  const runFetcher = useEffectEvent((signal: AbortSignal) => fetcher(signal));

  useEffect(() => {
    const controller = new AbortController();

    runFetcher(controller.signal)
      .then((data) => setState({ key: requestKey, data }))
      .catch((error: unknown) => {
        if (!controller.signal.aborted) {
          setState({ key: requestKey, error: toApiError(error) });
        }
      });

    return () => controller.abort();
  }, [requestKey]);

  const reload = useCallback(() => setReloadCount((count) => count + 1), []);
  const isCurrent = state?.key === requestKey;

  return {
    data: state?.data,
    error: isCurrent ? state?.error : undefined,
    isLoading: !isCurrent,
    reload,
  };
}
