namespace Mundialito.Infrastructure.ReadModel;

internal static class SqlText
{
    /// <summary>
    /// Convierte un filtro de texto en un patrón LIKE de "contiene", escapando los comodines del usuario.
    /// Devuelve null si el filtro viene vacío (el SQL lo interpreta como "sin filtro").
    /// </summary>
    public static string? ContainsPattern(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var escaped = value.Trim()
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");

        return $"%{escaped}%";
    }
}
