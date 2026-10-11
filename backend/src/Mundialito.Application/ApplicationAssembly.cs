using System.Reflection;

namespace Mundialito.Application;

/// <summary>
/// Referencia al ensamblado de Application. La Api la usa para registrar todos los handlers por reflexión.
/// </summary>
public static class ApplicationAssembly
{
    public static readonly Assembly Instance = typeof(ApplicationAssembly).Assembly;
}
