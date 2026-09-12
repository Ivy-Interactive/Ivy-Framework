using Ivy.Core.Server;

namespace Ivy.Test;

public class CorsOriginPolicyTests
{
    #region IsLoopbackBound

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("localhost", true)]
    [InlineData("LOCALHOST", true)]
    [InlineData("127.0.0.1", true)]
    [InlineData("127.0.0.5", true)]
    [InlineData("::1", true)]
    [InlineData("[::1]", true)]
    [InlineData("*", false)]
    [InlineData("+", false)]
    [InlineData("0.0.0.0", false)]
    [InlineData("app.example.com", false)]
    [InlineData("192.168.1.10", false)]
    public void IsLoopbackBound_ClassifiesBindHost(string? bindHost, bool expected)
    {
        Assert.Equal(expected, CorsOriginPolicy.IsLoopbackBound(bindHost));
    }

    #endregion

    #region IsOriginAllowed — loopback

    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("https://localhost:5010")]
    [InlineData("https://127.0.0.1:5010")]
    [InlineData("http://[::1]:1234")]
    [InlineData("http://localhost")]
    public void IsOriginAllowed_LoopbackOrigin_DependsOnAllowLoopback(string origin)
    {
        Assert.True(CorsOriginPolicy.IsOriginAllowed(origin, [], allowLoopback: true));
        Assert.False(CorsOriginPolicy.IsOriginAllowed(origin, [], allowLoopback: false));
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("http://attacker.example:8080")]
    [InlineData("null")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("file:///x")]
    [InlineData("chrome-extension://abc")]
    [InlineData("moz-extension://abc")]
    [InlineData("not a url")]
    [InlineData("/relative")]
    public void IsOriginAllowed_NonLoopbackOrUnusableOrigin_IsAlwaysRejected(string origin)
    {
        Assert.False(CorsOriginPolicy.IsOriginAllowed(origin, [], allowLoopback: true));
        Assert.False(CorsOriginPolicy.IsOriginAllowed(origin, [], allowLoopback: false));
    }

    [Fact]
    public void IsOriginAllowed_NullOrigin_IsRejected()
    {
        Assert.False(CorsOriginPolicy.IsOriginAllowed(null, [], allowLoopback: true));
    }

    #endregion

    #region IsOriginAllowed — allowlist

    [Fact]
    public void IsOriginAllowed_ConfiguredOrigin_IsAllowedWithoutLoopback()
    {
        string[] allowed = ["https://app.example.com"];

        Assert.True(CorsOriginPolicy.IsOriginAllowed("https://app.example.com", allowed, allowLoopback: false));
    }

    [Theory]
    [InlineData("https://app.example.com:8443")] // different port
    [InlineData("http://app.example.com")]       // different scheme
    [InlineData("https://sub.app.example.com")]  // subdomain of a configured origin
    [InlineData("https://app.example.com.evil")] // suffix attack
    [InlineData("https://example.com")]          // parent domain
    public void IsOriginAllowed_NearMissOfConfiguredOrigin_IsRejected(string origin)
    {
        string[] allowed = ["https://app.example.com"];

        Assert.False(CorsOriginPolicy.IsOriginAllowed(origin, allowed, allowLoopback: false));
    }

    [Theory]
    [InlineData("https://APP.example.com")]
    [InlineData("https://app.example.com/")]
    [InlineData("HTTPS://app.example.com")]
    public void IsOriginAllowed_ConfiguredOriginMatchesCaseAndTrailingSlashInsensitively(string origin)
    {
        string[] allowed = ["https://app.example.com"];

        Assert.True(CorsOriginPolicy.IsOriginAllowed(origin, allowed, allowLoopback: false));
    }

    [Fact]
    public void IsOriginAllowed_DefaultPortIsElidedOnBothSides()
    {
        Assert.True(CorsOriginPolicy.IsOriginAllowed("https://app.example.com", ["https://app.example.com:443"], allowLoopback: false));
        Assert.True(CorsOriginPolicy.IsOriginAllowed("https://app.example.com:443", ["https://app.example.com"], allowLoopback: false));
    }

    [Fact]
    public void IsOriginAllowed_WildcardEntry_IsNotHonoured()
    {
        Assert.False(CorsOriginPolicy.IsOriginAllowed("https://evil.example", ["*"], allowLoopback: false));
        Assert.False(CorsOriginPolicy.IsOriginAllowed("https://sub.example.com", ["https://*.example.com"], allowLoopback: false));
    }

    #endregion

    #region NormalizeOrigins

    [Fact]
    public void NormalizeOrigins_DropsUnusableEntriesAndDuplicates()
    {
        var normalized = CorsOriginPolicy.NormalizeOrigins(
        [
            "https://app.example.com",
            "https://APP.example.com/",
            "  http://localhost:5173  ",
            "file:///x",
            "not a url",
            "",
            "  "
        ]);

        Assert.Equal(new[] { "https://app.example.com", "http://localhost:5173" }, normalized);
    }

    [Fact]
    public void NormalizeOrigins_WithNull_ReturnsEmpty()
    {
        Assert.Empty(CorsOriginPolicy.NormalizeOrigins(null));
    }

    #endregion
}
