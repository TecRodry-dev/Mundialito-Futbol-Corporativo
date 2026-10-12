using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mundialito.Api.Idempotency;

/// <summary>
/// Hace que Swagger muestre el campo Idempotency-Key en los endpoints marcados con [Idempotent].
/// </summary>
internal sealed class IdempotencyKeyHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.GetCustomAttribute<IdempotentAttribute>() is null)
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = IdempotencyFilter.HeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Clave única por operación (ej. un GUID nuevo). Repetirla devuelve la respuesta original.",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
