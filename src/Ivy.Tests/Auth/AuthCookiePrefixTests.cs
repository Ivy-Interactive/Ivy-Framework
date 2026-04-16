using Ivy.Core.Auth;

namespace Ivy.Tests.Auth;

[Collection("AuthCookiePrefix")]
public class AuthCookiePrefixTests : IDisposable
{
    public AuthCookiePrefixTests()
    {
        Server.AuthCookiePrefix = null;
    }

    public void Dispose()
    {
        Server.AuthCookiePrefix = null;
    }

    [Fact]
    public void PrefixCookieName_NoPrefix_ReturnsOriginalName()
    {
        Server.AuthCookiePrefix = null;
        Assert.Equal("access_token", CookieRegistryExtensions.PrefixCookieName("access_token"));
    }

    [Fact]
    public void PrefixCookieName_EmptyPrefix_ReturnsOriginalName()
    {
        Server.AuthCookiePrefix = "";
        Assert.Equal("access_token", CookieRegistryExtensions.PrefixCookieName("access_token"));
    }

    [Fact]
    public void PrefixCookieName_WithPrefix_ReturnsPrefixedName()
    {
        Server.AuthCookiePrefix = "clerk";
        Assert.Equal("clerk__access_token", CookieRegistryExtensions.PrefixCookieName("access_token"));
    }

    [Fact]
    public void PrefixCookieName_WithPrefix_AllCoreNames()
    {
        Server.AuthCookiePrefix = "basic";
        Assert.Equal("basic__access_token", CookieRegistryExtensions.PrefixCookieName("access_token"));
        Assert.Equal("basic__refresh_token", CookieRegistryExtensions.PrefixCookieName("refresh_token"));
        Assert.Equal("basic__auth_tag", CookieRegistryExtensions.PrefixCookieName("auth_tag"));
        Assert.Equal("basic__auth_session_data", CookieRegistryExtensions.PrefixCookieName("auth_session_data"));
    }

    [Fact]
    public void PrefixCookieName_WithPrefix_BrokeredSessionCookies()
    {
        Server.AuthCookiePrefix = "clerk";
        Assert.Equal("clerk__google_access_token", CookieRegistryExtensions.PrefixCookieName("google_access_token"));
        Assert.Equal("clerk__google_refresh_token", CookieRegistryExtensions.PrefixCookieName("google_refresh_token"));
        Assert.Equal("clerk__google_auth_tag", CookieRegistryExtensions.PrefixCookieName("google_auth_tag"));
    }

    [Fact]
    public void PrefixCookieName_DoubleUnderscoreDelimiter_AvoidsSingleUnderscoreAmbiguity()
    {
        Server.AuthCookiePrefix = "app";
        var prefixed = CookieRegistryExtensions.PrefixCookieName("google_access_token");
        Assert.Equal("app__google_access_token", prefixed);
        Assert.Contains("__", prefixed);
        // The first __ separates prefix from cookie name
        var parts = prefixed.Split("__", 2);
        Assert.Equal("app", parts[0]);
        Assert.Equal("google_access_token", parts[1]);
    }
}
