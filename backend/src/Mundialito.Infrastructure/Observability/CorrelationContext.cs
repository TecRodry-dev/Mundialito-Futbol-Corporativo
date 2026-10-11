using System.Diagnostics;

namespace Mundialito.Infrastructure.Observability;

/// <summary>
/// Acceso al TraceId y al CorrelationId del request actual sin depender de HTTP.
/// La Api guarda el CorrelationId como "baggage" del Activity del request.
/// </summary>
public static class CorrelationContext
{
    public const string BaggageKey = "correlation_id";

    public static string? CurrentTraceId => Activity.Current?.TraceId.ToString();

    public static string? CurrentCorrelationId => Activity.Current?.GetBaggageItem(BaggageKey);
}
