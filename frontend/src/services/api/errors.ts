// Formato ProblemDetails que devuelve la API en los errores (400, 404, 409, 500).
export type ProblemDetails = {
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
};

const CONNECTION_ERROR_MESSAGE = "No se pudo conectar con la API. Verifique que esté en ejecución.";

/**
 * Error de la API ya traducido para mostrarlo en pantalla:
 * mensaje legible, código de negocio y traceId para rastrearlo en los logs.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly traceId?: string;

  constructor(message: string, status: number, code?: string, traceId?: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.traceId = traceId;
  }

  static fromProblem(problem: ProblemDetails, status: number): ApiError {
    const validationMessages = problem.errors ? Object.values(problem.errors).flat().join(" ") : "";
    const message = problem.detail || validationMessages || problem.title || "Ocurrió un error inesperado.";

    return new ApiError(message, status, problem.code, problem.traceId);
  }

  static connection(): ApiError {
    return new ApiError(CONNECTION_ERROR_MESSAGE, 0);
  }
}

export function toApiError(error: unknown): ApiError {
  return error instanceof ApiError ? error : ApiError.connection();
}
