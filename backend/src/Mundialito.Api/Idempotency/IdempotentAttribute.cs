using Microsoft.AspNetCore.Mvc;

namespace Mundialito.Api.Idempotency;

/// <summary>
/// Marca un endpoint POST como idempotente: exige el header Idempotency-Key.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute : TypeFilterAttribute
{
    public IdempotentAttribute()
        : base(typeof(IdempotencyFilter))
    {
    }
}
