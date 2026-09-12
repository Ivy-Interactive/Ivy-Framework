using Ivy.Core.Server;

namespace Ivy.Test;

public class BindHostPolicyTests
{
    #region Host Resolution (Non-CLI)

    [Theory]
    [InlineData(null, false, false, "localhost")]
    [InlineData(null, true, false, "*")]
    [InlineData(null, false, true, "*")]
    [InlineData(null, true, true, "*")]
    [InlineData("0.0.0.0", false, false, "0.0.0.0")]
    [InlineData("0.0.0.0", true, true, "0.0.0.0")]
    [InlineData("127.0.0.1", true, true, "127.0.0.1")]
    public void Resolve_NonCli_ReturnsExpectedHost(string? argsHost, bool isContainer, bool hasPortEnv, string expectedHost)
    {
        var environment = new BindEnvironment(isContainer, hasPortEnv, null, false);
        var result = BindHostPolicy.Resolve(environment, argsHost, 5010, isCliCommand: false);

        Assert.Equal(expectedHost, result.Host);
    }

    #endregion

    #region CLI Command Wins Over Everything

    [Fact]
    public void Resolve_CliCommand_AlwaysReturnsLocalhostPort0Http()
    {
        // CLI command should return localhost:0 over http, regardless of other settings
        var environment = new BindEnvironment(
            IsContainer: true,
            HasPortEnv: true,
            IvyTls: "1",
            IsWindows: true);

        var result = BindHostPolicy.Resolve(environment, argsHost: "*", argsPort: 5010, isCliCommand: true);

        Assert.Equal(new BindAddress("http", "localhost", 0), result);
    }

    [Theory]
    [InlineData("*", 5010, true, true, "1", true)]
    [InlineData("0.0.0.0", 8080, true, true, "true", true)]
    [InlineData(null, 3000, false, false, null, false)]
    [InlineData("127.0.0.1", 5000, false, true, "yes", false)]
    public void Resolve_CliCommand_IgnoresAllSettings(
        string? argsHost,
        int argsPort,
        bool isContainer,
        bool hasPortEnv,
        string? ivyTls,
        bool isWindows)
    {
        var environment = new BindEnvironment(isContainer, hasPortEnv, ivyTls, isWindows);
        var result = BindHostPolicy.Resolve(environment, argsHost, argsPort, isCliCommand: true);

        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(0, result.Port);
    }

    #endregion

    #region Scheme / TLS

    [Theory]
    [InlineData("1", true, true, false, "https")]
    [InlineData("true", true, true, false, "https")]
    [InlineData("TRUE", true, true, false, "https")]
    [InlineData("yes", true, true, false, "https")]
    [InlineData("Yes", true, true, false, "https")]
    [InlineData("on", true, true, false, "https")]
    [InlineData("ON", true, true, false, "https")]
    public void UseTls_ExplicitTrue_ReturnsHttps(
        string ivyTls,
        bool isContainer,
        bool hasPortEnv,
        bool isWindows,
        string expectedScheme)
    {
        var environment = new BindEnvironment(isContainer, hasPortEnv, ivyTls, isWindows);
        var useTls = BindHostPolicy.UseTls(environment);

        Assert.True(useTls);

        // Also verify via Resolve for one representative case
        if (ivyTls == "1")
        {
            var result = BindHostPolicy.Resolve(environment, null, 5010, isCliCommand: false);
            Assert.Equal(expectedScheme, result.Scheme);
        }
    }

    [Theory]
    [InlineData("0", true, false, "http")]
    [InlineData("false", true, false, "http")]
    [InlineData("False", true, false, "http")]
    [InlineData("no", true, false, "http")]
    [InlineData("No", true, false, "http")]
    [InlineData("off", true, false, "http")]
    [InlineData("OFF", true, false, "http")]
    [InlineData("enabled", true, false, "http")]
    [InlineData("2", true, false, "http")]
    [InlineData("invalid", true, false, "http")]
    public void UseTls_ExplicitFalse_ReturnsHttp(string ivyTls, bool isWindows, bool hasPortEnv, string expectedScheme)
    {
        // These values should result in http even on Windows with no container/PORT
        var environment = new BindEnvironment(false, hasPortEnv, ivyTls, isWindows);
        var useTls = BindHostPolicy.UseTls(environment);

        Assert.False(useTls);

        // Verify via Resolve for one representative case
        if (ivyTls == "0")
        {
            var result = BindHostPolicy.Resolve(environment, null, 5010, isCliCommand: false);
            Assert.Equal(expectedScheme, result.Scheme);
        }
    }

    [Theory]
    [InlineData(null, true, false, false, "http")]
    [InlineData(null, false, true, false, "http")]
    [InlineData(null, false, false, false, "http")]
    [InlineData(null, true, true, false, "http")]
    [InlineData(null, true, false, true, "http")]
    [InlineData(null, false, true, true, "http")]
    [InlineData(null, true, true, true, "http")]
    [InlineData(null, false, false, true, "https")]
    [InlineData("", false, false, true, "https")]
    [InlineData("", true, false, false, "http")]
    [InlineData("", false, true, false, "http")]
    public void UseTls_DefaultBehavior_OnlyHttpsOnWindowsLocalDev(
        string? ivyTls,
        bool isContainer,
        bool hasPortEnv,
        bool isWindows,
        string expectedScheme)
    {
        var environment = new BindEnvironment(isContainer, hasPortEnv, ivyTls, isWindows);
        var result = BindHostPolicy.Resolve(environment, null, 5010, isCliCommand: false);

        Assert.Equal(expectedScheme, result.Scheme);
    }

    #endregion

    #region URL Composition

    [Fact]
    public void BindAddress_Url_ComposesCorrectly()
    {
        var address = new BindAddress("https", "localhost", 5010);
        Assert.Equal("https://localhost:5010", address.Url);
    }

    [Fact]
    public void Resolve_CliCommand_ComposesCorrectUrl()
    {
        var environment = new BindEnvironment(false, false, null, false);
        var result = BindHostPolicy.Resolve(environment, null, 0, isCliCommand: true);

        Assert.Equal("http://localhost:0", result.Url);
    }

    #endregion

    #region Cross-Check Against Consumers

    [Fact]
    public void Resolve_CliCommand_BindsLoopback()
    {
        // CLI command should always result in loopback binding
        var environment = new BindEnvironment(true, true, null, false);
        var result = BindHostPolicy.Resolve(environment, "*", 5010, isCliCommand: true);

        var isLoopback = CorsOriginPolicy.IsLoopbackBound(result.Host);
        Assert.True(isLoopback);
    }

    [Fact]
    public void Resolve_NonCliWithWildcard_DoesNotBindLoopback()
    {
        // Non-CLI with wildcard host should not bind loopback
        var environment = new BindEnvironment(true, true, null, false);
        var result = BindHostPolicy.Resolve(environment, "*", 5010, isCliCommand: false);

        var isLoopback = CorsOriginPolicy.IsLoopbackBound(result.Host);
        Assert.False(isLoopback);
    }

    #endregion
}
