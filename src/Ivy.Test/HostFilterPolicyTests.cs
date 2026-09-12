using Ivy.Core.Server;

namespace Ivy.Test;

public class HostFilterPolicyTests
{
    private static readonly string[] LoopbackDefault = ["localhost", "127.0.0.1", "[::1]"];

    #region Loopback binds get the default list

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("localhost")]
    [InlineData("LocalHost")]
    [InlineData("127.0.0.1")]
    [InlineData("[::1]")]
    [InlineData("::1")]
    public void ResolveDefaultAllowedHosts_LoopbackBind_ReturnsLoopbackHosts(string? bindHost)
    {
        var resolved = HostFilterPolicy.ResolveDefaultAllowedHosts(bindHost, null);

        Assert.Equal(LoopbackDefault, resolved);
    }

    [Fact]
    public void ResolveDefaultAllowedHosts_OtherLoopbackLiteral_AddsTheBindHost()
    {
        var resolved = HostFilterPolicy.ResolveDefaultAllowedHosts("127.0.0.2", null);

        Assert.NotNull(resolved);
        Assert.Equal(["localhost", "127.0.0.1", "[::1]", "127.0.0.2"], resolved);
    }

    #endregion

    #region Non-loopback binds are left alone

    [Theory]
    [InlineData("*")]
    [InlineData("+")]
    [InlineData("0.0.0.0")]
    [InlineData("10.0.0.5")]
    [InlineData("example.com")]
    public void ResolveDefaultAllowedHosts_NonLoopbackBind_ReturnsNull(string bindHost)
    {
        Assert.Null(HostFilterPolicy.ResolveDefaultAllowedHosts(bindHost, null));
    }

    #endregion

    #region A configured key wins

    [Theory]
    [InlineData("*")]
    [InlineData("")]
    [InlineData("example.com")]
    public void ResolveDefaultAllowedHosts_ConfiguredKey_ReturnsNull(string configuredAllowedHosts)
    {
        Assert.Null(HostFilterPolicy.ResolveDefaultAllowedHosts("localhost", configuredAllowedHosts));
    }

    #endregion
}
