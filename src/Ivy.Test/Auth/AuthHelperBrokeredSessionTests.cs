using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Ivy.Core.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Ivy.Test.Auth;

[Collection("AuthCookiePrefix")]
public class AuthHelperBrokeredSessionTests : IDisposable
{
    public AuthHelperBrokeredSessionTests()
    {
        Server.AuthCookiePrefix = null;
    }

    public void Dispose()
    {
        Server.AuthCookiePrefix = null;
    }

    [Fact]
    public void ExtractBrokeredSessionsFromCookies_WithPrefix_IgnoresCookiesFromOtherPrefixes()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["clerk__access_token"] = "clerk-main-token",
            ["clerk__google_access_token"] = "clerk-google-token",
            ["basic__access_token"] = "basic-main-token",
            ["basic__github_access_token"] = "basic-github-token",
        });

        var result = AuthHelper.ExtractBrokeredSessionsFromCookies(cookies);

        Assert.Single(result);
        Assert.True(result.ContainsKey("google"));
        Assert.False(result.ContainsKey("basic_"));
        Assert.False(result.ContainsKey("github"));
    }

    [Fact]
    public void ExtractBrokeredSessionsFromCookies_WithPrefix_ExtractsOwnPrefixedCookies()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["clerk__access_token"] = "clerk-main-token",
            ["clerk__google_access_token"] = "google-token",
            ["clerk__github_access_token"] = "github-token",
        });

        var result = AuthHelper.ExtractBrokeredSessionsFromCookies(cookies);

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("google"));
        Assert.True(result.ContainsKey("github"));
    }

    [Fact]
    public void ExtractBrokeredSessionsFromCookies_NoPrefix_ExtractsAllAccessTokenCookies()
    {
        Server.AuthCookiePrefix = null;
        var cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["access_token"] = "main-token",
            ["google_access_token"] = "google-token",
            ["github_access_token"] = "github-token",
        });

        var result = AuthHelper.ExtractBrokeredSessionsFromCookies(cookies);

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("google"));
        Assert.True(result.ContainsKey("github"));
    }

    [Fact]
    public void ExtractBrokeredSessionsFromCookieHeader_WithPrefix_IgnoresCookiesFromOtherPrefixes()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookieHeader = CookieHeaderValue.ParseList(new[]
        {
            "clerk__access_token=clerk-main-token; clerk__google_access_token=clerk-google-token; basic__access_token=basic-main-token; basic__github_access_token=basic-github-token"
        }).ToList();

        var result = AuthHelper.ExtractBrokeredSessionsFromCookieHeader(cookieHeader);

        Assert.Single(result);
        Assert.True(result.ContainsKey("google"));
        Assert.False(result.ContainsKey("basic_"));
        Assert.False(result.ContainsKey("github"));
    }

    [Fact]
    public void ExtractBrokeredSessionsFromCookieHeader_WithPrefix_ExtractsOwnPrefixedCookies()
    {
        Server.AuthCookiePrefix = "clerk";
        var cookieHeader = CookieHeaderValue.ParseList(new[]
        {
            "clerk__access_token=clerk-main-token; clerk__google_access_token=google-token; clerk__github_access_token=github-token"
        }).ToList();

        var result = AuthHelper.ExtractBrokeredSessionsFromCookieHeader(cookieHeader);

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("google"));
        Assert.True(result.ContainsKey("github"));
    }

    [Fact]
    public void ExtractBrokeredSessionsFromCookieHeader_NoPrefix_ExtractsAllAccessTokenCookies()
    {
        Server.AuthCookiePrefix = null;
        var cookieHeader = CookieHeaderValue.ParseList(new[]
        {
            "access_token=main-token; google_access_token=google-token; github_access_token=github-token"
        }).ToList();

        var result = AuthHelper.ExtractBrokeredSessionsFromCookieHeader(cookieHeader);

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("google"));
        Assert.True(result.ContainsKey("github"));
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
