using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Mundialito.Api.Extensions;
using Mundialito.Application.Abstractions.Idempotency;
using Mundialito.Domain.Common;

namespace Mundialito.Api.Idempotency;

/// <summary>
/// Idempotencia de los POST:
/// 1. Sin header Idempotency-Key → 400.
/// 2. Key ya usada con el mismo payload → devuelve la respuesta original guardada (status + body), sin ejecutar nada.
/// 3. Key ya usada con otro payload → 409.
/// 4. Key nueva → ejecuta el endpoint y guarda su respuesta (salvo errores 5xx, para permitir reintentar).
/// Es un resource filter: corre antes del model binding, así puede leer el body original.
/// </summary>
internal sealed class IdempotencyFilter(
    IIdempotencyStore idempotencyStore,
    IOptions<JsonOptions> jsonOptions,
    ILogger<IdempotencyFilter> logger) : IAsyncResourceFilter
{
    public const string HeaderName = "Idempotency-Key";
    public const string ReplayedHeaderName = "Idempotent-Replayed";
    private const int MaxKeyLength = 100;

    private static readonly Error KeyRequired = Error.Validation(
        $"El header {HeaderName} es obligatorio en este endpoint.", "Idempotency.KeyRequired");

    private static readonly Error KeyTooLong = Error.Validation(
        $"El header {HeaderName} no puede superar {MaxKeyLength} caracteres.", "Idempotency.KeyTooLong");

    private static readonly Error KeyReusedWithDifferentPayload = Error.Conflict(
        $"El {HeaderName} ya se usó con un payload distinto. Use una key nueva para otra operación.",
        "Idempotency.KeyReused");

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var cancellationToken = httpContext.RequestAborted;
        var key = httpContext.Request.Headers[HeaderName].ToString().Trim();

        if (key.Length == 0)
        {
            context.Result = KeyRequired.ToProblemResult(httpContext);
            return;
        }

        if (key.Length > MaxKeyLength)
        {
            context.Result = KeyTooLong.ToProblemResult(httpContext);
            return;
        }

        var requestHash = RequestFingerprint.Compute(
            httpContext.Request.Method,
            httpContext.Request.Path,
            await ReadBodyAsync(httpContext.Request));

        var existing = await idempotencyStore.GetAsync(key, cancellationToken);
        if (existing is not null)
        {
            if (existing.RequestHash != requestHash)
            {
                logger.LogWarning("Idempotency-Key {IdempotencyKey} reutilizada con un payload distinto", key);
                context.Result = KeyReusedWithDifferentPayload.ToProblemResult(httpContext);
                return;
            }

            logger.LogInformation(
                "Idempotency-Key {IdempotencyKey} repetida: se devuelve la respuesta original ({StatusCode})",
                key, existing.StatusCode);

            httpContext.Response.Headers[ReplayedHeaderName] = "true";
            context.Result = new ContentResult
            {
                StatusCode = existing.StatusCode,
                Content = existing.ResponseBody,
                ContentType = existing.StatusCode >= 400 ? "application/problem+json" : "application/json"
            };
            return;
        }

        var executed = await next();

        if (executed.Exception is null && executed.Result is ObjectResult result)
        {
            var statusCode = result.StatusCode ?? StatusCodes.Status200OK;
            if (statusCode < StatusCodes.Status500InternalServerError)
            {
                await idempotencyStore.SaveAsync(new IdempotencyRecord
                {
                    Key = key,
                    RequestHash = requestHash,
                    StatusCode = statusCode,
                    ResponseBody = JsonSerializer.Serialize(result.Value, jsonOptions.Value.JsonSerializerOptions),
                    CreatedAtUtc = DateTime.UtcNow
                }, cancellationToken);
            }
        }
    }

    // Lee el body y lo deja listo para que el model binding lo vuelva a leer.
    private static async Task<string> ReadBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }
}
