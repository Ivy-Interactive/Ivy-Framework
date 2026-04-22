using Ivy.Core.Auth;

namespace Ivy.Test.Auth;

[Collection("AuthCookiePrefix")]
public class CookieRegistryExtensionsConnectedTests : IDisposable
{
    public CookieRegistryExtensionsConnectedTests()
    {
        Server.AuthCookiePrefix = null;
    }

    public void Dispose()
    {
        Server.AuthCookiePrefix = null;
    }

    [Fact]
    public void AddCookiesForConnectedAccounts_WritesCookiesWithConnPrefix()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new CookieJar();
        var connectedAccounts = new Dictionary<string, IAuthSession>
        {
            ["github"] = new AuthSession(authToken: new AuthToken("github-access", "github-refresh", "github-tag"))
        };

        cookies.AddCookiesForConnectedAccounts(connectedAccounts);

        Assert.True(cookies.TryGet("conn_github_access_token", out var accessToken));
        Assert.Equal("github-access", accessToken);
        Assert.True(cookies.TryGet("conn_github_refresh_token", out var refreshToken));
        Assert.Equal("github-refresh", refreshToken);
        Assert.True(cookies.TryGet("conn_github_auth_tag", out var tag));
        Assert.Equal("github-tag", tag);
    }

    [Fact]
    public void AddCookiesForConnectedAccounts_WithPrefix_UsesPrefixedNames()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookies = new CookieJar();
        var connectedAccounts = new Dictionary<string, IAuthSession>
        {
            ["github"] = new AuthSession(authToken: new AuthToken("github-access"))
        };

        cookies.AddCookiesForConnectedAccounts(connectedAccounts);

        Assert.True(cookies.TryGet("clerk__conn_github_access_token", out var accessToken));
        Assert.Equal("github-access", accessToken);
        Assert.False(cookies.TryGet("conn_github_access_token", out _));
    }

    [Fact]
    public void AddCookiesForConnectedAccounts_HandlesNullTokens()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new CookieJar();
        var connectedAccounts = new Dictionary<string, IAuthSession>
        {
            ["github"] = new AuthSession(authToken: null)
        };

        cookies.AddCookiesForConnectedAccounts(connectedAccounts);

        // Should attempt to delete cookies (empty value)
        Assert.True(cookies.TryGet("conn_github_access_token", out var value));
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void AddCookiesForConnectedAccounts_MultipleProviders()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new CookieJar();
        var connectedAccounts = new Dictionary<string, IAuthSession>
        {
            ["github"] = new AuthSession(authToken: new AuthToken("github-access")),
            ["slack"] = new AuthSession(authToken: new AuthToken("slack-access"))
        };

        cookies.AddCookiesForConnectedAccounts(connectedAccounts);

        Assert.True(cookies.TryGet("conn_github_access_token", out var githubToken));
        Assert.Equal("github-access", githubToken);
        Assert.True(cookies.TryGet("conn_slack_access_token", out var slackToken));
        Assert.Equal("slack-access", slackToken);
    }
}
