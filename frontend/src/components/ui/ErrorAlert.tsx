import type { ApiError } from "@/services/api/errors";

type ErrorAlertProps = {
  error: ApiError;
  onRetry?: () => void;
};

/** Muestra el error de la API con su código y traceId (para rastrearlo en los logs). */
export function ErrorAlert({ error, onRetry }: ErrorAlertProps) {
  return (
    <div role="alert" className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-800">
      <p className="font-medium">{error.message}</p>
      {error.code && <p className="mt-1 text-xs text-red-700">Código: {error.code}</p>}
      {error.traceId && <p className="mt-1 font-mono text-xs break-all text-red-700">TraceId: {error.traceId}</p>}
      {onRetry && (
        <button
          type="button"
          onClick={onRetry}
          className="mt-3 rounded-md border border-red-300 bg-white px-3 py-1.5 text-xs font-medium text-red-700 hover:bg-red-100"
        >
          Reintentar
        </button>
      )}
    </div>
  );
}
