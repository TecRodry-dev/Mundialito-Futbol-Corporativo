using Mundialito.Application.Abstractions.Idempotency;

namespace Mundialito.UnitTests.Application.Idempotency;

public class RequestFingerprintTests
{
    private const string Body = """{"name":"Sistemas FC","coach":"Laura Méndez"}""";

    [Fact]
    public void SameRequest_ProducesSameFingerprint()
    {
        var first = RequestFingerprint.Compute("POST", "/api/teams", Body);
        var second = RequestFingerprint.Compute("POST", "/api/teams", Body);

        Assert.Equal(first, second);
    }

    [Fact]
    public void DifferentPayload_ProducesDifferentFingerprint()
    {
        var original = RequestFingerprint.Compute("POST", "/api/teams", Body);
        var changed = RequestFingerprint.Compute("POST", "/api/teams", """{"name":"Ventas United","coach":"Laura Méndez"}""");

        Assert.NotEqual(original, changed);
    }

    [Fact]
    public void SamePayloadOnDifferentEndpoint_ProducesDifferentFingerprint()
    {
        var teams = RequestFingerprint.Compute("POST", "/api/teams", Body);
        var matches = RequestFingerprint.Compute("POST", "/api/matches", Body);

        Assert.NotEqual(teams, matches);
    }

    [Fact]
    public void MethodAndPathCasing_AreIgnored()
    {
        var lower = RequestFingerprint.Compute("post", "/api/teams", Body);
        var upper = RequestFingerprint.Compute("POST", "/API/Teams", Body);

        Assert.Equal(lower, upper);
    }

    [Fact]
    public void Fingerprint_IsSha256InHex()
    {
        var fingerprint = RequestFingerprint.Compute("POST", "/api/teams", Body);

        Assert.Equal(64, fingerprint.Length);
        Assert.Matches("^[0-9A-F]+$", fingerprint);
    }
}
