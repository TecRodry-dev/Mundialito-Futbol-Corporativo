using System.Diagnostics;
using Mundialito.Infrastructure.Observability;
using Serilog.Context;

namespace Mundialito.Api.Middlewares;

/// <summary>
/// Asigna a cada request un CorrelationId (el que envía el cliente en X-Correlation-Id, o uno nuevo)
/// y su TraceId. Los devuelve en los headers de la respuesta y los agrega a todos los logs del request.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string CorrelationIdHeader = "X-Correlation-Id";
    public const string TraceIdHeader = "X-Trace-Id";

    private const int MaxCorrelationIdLength = 100;

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        var incoming = context.Request.Headers[CorrelationIdHeader].ToString();
        var correlationId = IsValid(incoming) ? incoming : traceId;

        // Lo guarda en el Activity para que Infrastructure (eventos de dominio) lo lea sin depender de HTTP.
        Activity.Current?.SetBaggage(CorrelationContext.BaggageKey, correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            context.Response.Headers[TraceIdHeader] = traceId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("TraceId", traceId))
        {
            await next(context);
        }
    }

    // Solo se acepta un valor corto y "seguro" para no meter texto arbitrario en logs y headers.
    private static bool IsValid(string value) =>
        value.Length is > 0 and <= MaxCorrelationIdLength &&
        value.All(character => char.IsLetterOrDigit(character) || character is '-' or '_' or '.');
}
