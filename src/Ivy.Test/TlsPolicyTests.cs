using Ivy.Core.Server;

namespace Ivy.Test;

public class TlsPolicyTests
{
    #region Explicit value wins

    [Theory]
    [InlineData("0", true, false, false)]
    [InlineData("0", false, false, false)]
    [InlineData("0", true, true, false)]
    [InlineData("0", false, true, true)]
    public void Resolve_ExplicitTrue_WinsOverEnvironmentAndDefaults(string ivyTlsEnv, bool isContainer, bool hasPortEnv, bool isWindows)
    {
        var result = TlsPolicy.Resolve(explicitUseTls: true, ivyTlsEnv, isContainer, hasPortEnv, isWindows);

        Assert.True(result);
    }

    [Theory]
    [InlineData("1", true, false, false)]
    [InlineData("1", false, false, false)]
    [InlineData("1", true, true, false)]
    [InlineData("1", false, true, true)]
    public void Resolve_ExplicitFalse_WinsOverEnvironmentAndDefaults(string ivyTlsEnv, bool isContainer, bool hasPortEnv, bool isWindows)
    {
        var result = TlsPolicy.Resolve(explicitUseTls: false, ivyTlsEnv, isContainer, hasPortEnv, isWindows);

        Assert.False(result);
    }

    #endregion

    #region Environment variable parsing

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("yes")]
    [InlineData("on")]
    public void Resolve_TruthyEnvironmentVariable_ReturnsTrue(string ivyTlsEnv)
    {
        var result = TlsPolicy.Resolve(explicitUseTls: null, ivyTlsEnv, isContainer: true, hasPortEnv: false, isWindows: false);

        Assert.True(result);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("false")]
    [InlineData("no")]
    [InlineData("off")]
    [InlineData("unrecognized")]
    public void Resolve_FalsyOrUnrecognizedEnvironmentVariable_ReturnsFalse(string ivyTlsEnv)
    {
        var result = TlsPolicy.Resolve(explicitUseTls: null, ivyTlsEnv, isContainer: false, hasPortEnv: false, isWindows: true);

        Assert.False(result);
    }

    #endregion

    #region Null and empty environment fall through to default

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Resolve_NullOrEmptyEnvironment_FallsThroughToDefault(string? ivyTlsEnv)
    {
        var result = TlsPolicy.Resolve(explicitUseTls: null, ivyTlsEnv, isContainer: false, hasPortEnv: false, isWindows: true);

        Assert.True(result);
    }

    #endregion

    #region Default matrix

    [Fact]
    public void Resolve_WindowsLocalDevelopment_ReturnsTrue()
    {
        var result = TlsPolicy.Resolve(explicitUseTls: null, ivyTlsEnv: null, isContainer: false, hasPortEnv: false, isWindows: true);

        Assert.True(result);
    }

    [Fact]
    public void Resolve_Container_ReturnsFalse()
    {
        var result = TlsPolicy.Resolve(explicitUseTls: null, ivyTlsEnv: null, isContainer: true, hasPortEnv: false, isWindows: true);

        Assert.False(result);
    }

    [Fact]
    public void Resolve_HasPortEnvironment_ReturnsFalse()
    {
        var result = TlsPolicy.Resolve(explicitUseTls: null, ivyTlsEnv: null, isContainer: false, hasPortEnv: true, isWindows: true);

        Assert.False(result);
    }

    [Fact]
    public void Resolve_NonWindows_ReturnsFalse()
    {
        var result = TlsPolicy.Resolve(explicitUseTls: null, ivyTlsEnv: null, isContainer: false, hasPortEnv: false, isWindows: false);

        Assert.False(result);
    }

    #endregion
}
