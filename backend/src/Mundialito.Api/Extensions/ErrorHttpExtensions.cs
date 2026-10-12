using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Mundialito.Domain.Common;

namespace Mundialito.Api.Extensions;

/// <summary>
/// Traduce un <see cref="Error"/> del dominio a una respuesta HTTP con formato ProblemDetails.
/// Es el único lugar donde se decide el código HTTP de un error de negocio.
/// </summary>
public static class ErrorHttpExtensions
{
    public static ObjectResult ToProblemResult(this Error error, HttpContext httpContext)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflicto con el estado actual"),
            _ => (StatusCodes.Status500InternalServerError, "Error inesperado")
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error.Message,
            Instance = httpContext.Request.Path
        };

        if (error.Code is not null)
        {
            problem.Extensions["code"] = error.Code;
        }

        problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }
}
