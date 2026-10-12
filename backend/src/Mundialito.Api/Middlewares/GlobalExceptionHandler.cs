using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Mundialito.Api.Middlewares;

/// <summary>
/// Último recurso para fallos técnicos inesperados (BD caída, bug): loggea el error completo
/// y responde 500 sin exponer detalles internos, solo el traceId para buscarlo en los logs.
/// Los errores de negocio NO llegan aquí: viajan como Result.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Error no controlado en {RequestMethod} {RequestPath}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Error interno del servidor",
            Detail = "Ocurrió un error inesperado. Use el traceId para rastrearlo en los logs.",
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }
}
