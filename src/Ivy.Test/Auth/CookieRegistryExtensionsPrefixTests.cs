using Ivy.Core.Auth;

namespace Ivy.Test.Auth;

[Collection("AuthCookiePrefix")]
public class CookieRegistryExtensionsPrefixTests : IDisposable
{
    public CookieRegistryExtensionsPrefixTests()
    {
        Server.AuthCookiePrefix = null;
    }

    public void Dispose()
    {
        Server.AuthCookiePrefix = null;
    }

    [Fact]
    public void AddCookiesForAuthToken_NoPrefix_UsesUnprefixedNames()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new CookieJar();
        var token = new AuthToken("test-access", "test-refresh", "test-tag");

        cookies.AddCookiesForAuthToken(token);

        Assert.True(cookies.TryGet("access_token", out var accessToken));
        Assert.Equal("test-access", accessToken);
        Assert.True(cookies.TryGet("refresh_token", out var refreshToken));
        Assert.Equal("test-refresh", refreshToken);
        Assert.True(cookies.TryGet("auth_tag", out var tag));
        Assert.Equal("test-tag", tag);
    }

    [Fact]
    public void AddCookiesForAuthToken_WithPrefix_UsesPrefixedNames()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookies = new CookieJar();
        var token = new AuthToken("test-access", "test-refresh", "test-tag");

        cookies.AddCookiesForAuthToken(token);

        Assert.True(cookies.TryGet("clerk__access_token", out var accessToken));
        Assert.Equal("test-access", accessToken);
        Assert.True(cookies.TryGet("clerk__refresh_token", out var refreshToken));
        Assert.Equal("test-refresh", refreshToken);
        Assert.True(cookies.TryGet("clerk__auth_tag", out var tag));
        Assert.Equal("test-tag", tag);

        // Should NOT have unprefixed cookies
        Assert.False(cookies.TryGet("access_token", out _));
    }

    [Fact]
    public void AddCookiesForAuthSessionData_WithPrefix_UsesPrefixedName()
    {
        Server.AuthCookiePrefix = "basic";
        var cookies = new CookieJar();

        cookies.AddCookiesForAuthSessionData("session-data-value");

        Assert.True(cookies.TryGet("basic__auth_session_data", out var sessionData));
        Assert.Equal("session-data-value", sessionData);
        Assert.False(cookies.TryGet("auth_session_data", out _));
    }

    [Fact]
    public void AddCookiesForBrokeredSessions_WithPrefix_UsesPrefixedNames()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookies = new CookieJar();
        var brokeredSessions = new Dictionary<string, IAuthTokenHandlerSession>
        {
            ["google"] = new AuthTokenHandlerSession(authToken: new AuthToken("google-access", "google-refresh", "google-tag"))
        };

        cookies.AddCookiesForBrokeredSessions(brokeredSessions);

        Assert.True(cookies.TryGet("clerk__google_access_token", out var accessToken));
        Assert.Equal("google-access", accessToken);
        Assert.True(cookies.TryGet("clerk__google_refresh_token", out var refreshToken));
        Assert.Equal("google-refresh", refreshToken);
        Assert.True(cookies.TryGet("clerk__google_auth_tag", out var tag));
        Assert.Equal("google-tag", tag);

        // Should NOT have unprefixed brokered cookies
        Assert.False(cookies.TryGet("google_access_token", out _));
    }

    [Fact]
    public void AddCookiesForAuthToken_NullToken_DeletesPrefixedNames()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookies = new CookieJar();

        cookies.AddCookiesForAuthToken(null);

        // Should attempt to delete prefixed cookies (CookieJar.Delete adds with empty value)
        Assert.True(cookies.TryGet("clerk__access_token", out var value));
        Assert.Equal(string.Empty, value); // Delete sets empty value
    }
}
