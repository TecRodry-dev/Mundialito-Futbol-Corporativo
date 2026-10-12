using System.Security.Cryptography;
using System.Text;

namespace Mundialito.Application.Abstractions.Idempotency;

/// <summary>
/// Huella (SHA-256) de un request: método + ruta + body. Si llega la misma Idempotency-Key
/// con otra huella, es la misma key con un payload distinto (409).
/// </summary>
public static class RequestFingerprint
{
    public static string Compute(string method, string path, string body)
    {
        var canonical = $"{method.ToUpperInvariant()} {path.ToLowerInvariant()}\n{body.Trim()}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));

        return Convert.ToHexString(hash);
    }
}
