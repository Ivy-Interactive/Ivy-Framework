using System.Diagnostics.CodeAnalysis;
using Ivy.Core.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Ivy.Test.Auth;

[Collection("AuthCookiePrefix")]
public class AuthHelperConnectedAccountTests : IDisposable
{
    public AuthHelperConnectedAccountTests()
    {
        Server.AuthCookiePrefix = null;
    }

    public void Dispose()
    {
        Server.AuthCookiePrefix = null;
    }

    [Fact]
    public void ExtractConnectedAccountsFromCookies_ExtractsConnPrefixedCookies()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["access_token"] = "main-token",
            ["conn_github_access_token"] = "github-token",
            ["conn_github_refresh_token"] = "github-refresh",
            ["conn_github_auth_tag"] = "github-tag",
        });

        var result = AuthHelper.ExtractConnectedAccountsFromCookies(cookies);

        Assert.Single(result);
        Assert.True(result.ContainsKey("github"));
        Assert.Equal("github-token", result["github"].AuthToken?.AccessToken);
        Assert.Equal("github-refresh", result["github"].AuthToken?.RefreshToken);
        Assert.Equal("github-tag", result["github"].AuthToken?.Tag);
    }

    [Fact]
    public void ExtractConnectedAccountsFromCookies_ReturnsIAuthSessionInstances()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["access_token"] = "main-token",
            ["conn_slack_access_token"] = "slack-token",
        });

        var result = AuthHelper.ExtractConnectedAccountsFromCookies(cookies);

        Assert.Single(result);
        Assert.IsAssignableFrom<IAuthSession>(result["slack"]);
    }

    [Fact]
    public void ExtractConnectedAccountsFromCookies_IgnoresNonConnectedCookies()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["access_token"] = "main-token",
            ["google_access_token"] = "google-brokered-token",
            ["conn_github_access_token"] = "github-connected-token",
        });

        var result = AuthHelper.ExtractConnectedAccountsFromCookies(cookies);

        Assert.Single(result);
        Assert.True(result.ContainsKey("github"));
        Assert.False(result.ContainsKey("google"));
    }

    [Fact]
    public void ExtractConnectedAccountsFromCookies_WithPrefix_ExtractsOwnPrefixedConnCookies()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["clerk__access_token"] = "main-token",
            ["clerk__conn_github_access_token"] = "github-token",
            ["basic__conn_slack_access_token"] = "should-be-ignored",
        });

        var result = AuthHelper.ExtractConnectedAccountsFromCookies(cookies);

        Assert.Single(result);
        Assert.True(result.ContainsKey("github"));
        Assert.False(result.ContainsKey("slack"));
    }

    [Fact]
    public void ExtractBrokeredSessionsFromCookies_ExcludesConnPrefixedCookies()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["access_token"] = "main-token",
            ["google_access_token"] = "google-token",
            ["conn_github_access_token"] = "github-connected-token",
        });

        var result = AuthHelper.ExtractBrokeredSessionsFromCookies(cookies);

        Assert.Single(result);
        Assert.True(result.ContainsKey("google"));
        Assert.False(result.ContainsKey("conn_github"));
        Assert.False(result.ContainsKey("github"));
    }

    [Fact]
    public void ExtractConnectedAccountsFromCookieHeader_ExtractsConnPrefixedCookies()
    {
        Server.AuthCookiePrefix = null;
        var cookieHeader = CookieHeaderValue.ParseList(new[]
        {
            "access_token=main-token; conn_github_access_token=github-token; conn_github_refresh_token=github-refresh"
        }).ToList();

        var result = AuthHelper.ExtractConnectedAccountsFromCookieHeader(cookieHeader);

        Assert.Single(result);
        Assert.True(result.ContainsKey("github"));
        Assert.Equal("github-token", result["github"].AuthToken?.AccessToken);
        Assert.Equal("github-refresh", result["github"].AuthToken?.RefreshToken);
    }

    [Fact]
    public void ExtractBrokeredSessionsFromCookieHeader_ExcludesConnPrefixedCookies()
    {
        Server.AuthCookiePrefix = null;
        var cookieHeader = CookieHeaderValue.ParseList(new[]
        {
            "access_token=main-token; google_access_token=google-token; conn_github_access_token=github-token"
        }).ToList();

        var result = AuthHelper.ExtractBrokeredSessionsFromCookieHeader(cookieHeader);

        Assert.Single(result);
        Assert.True(result.ContainsKey("google"));
        Assert.False(result.ContainsKey("github"));
    }

    private class TestRequestCookieCollection : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _cookies;

        public TestRequestCookieCollection(Dictionary<string, string> cookies)
        {
            _cookies = cookies;
        }

        public string? this[string key] => _cookies.TryGetValue(key, out var value) ? value : null;

        public int Count => _cookies.Count;

        public ICollection<string> Keys => _cookies.Keys;

        public bool ContainsKey(string key) => _cookies.ContainsKey(key);

        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            if (_cookies.TryGetValue(key, out var v))
            {
                value = v;
                return true;
            }
            value = null;
            return false;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
