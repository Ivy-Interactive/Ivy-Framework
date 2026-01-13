using Ivy;

namespace Ivy.Test;

public class UrlValidationTests
{
    #region ValidateRedirectUrl Tests

    [Theory]
    [InlineData("/dashboard", true)]
    [InlineData("/users/profile", true)]
    [InlineData("/app/settings", true)]
    [InlineData("/", true)]
    public void ValidateRedirectUrl_ValidRelativePath_ReturnsUrl(string url, bool allowExternal)
    {
        var result = Utils.ValidateRedirectUrl(url, allowExternal);
        Assert.Equal(url, result);
    }

    [Theory]
    [InlineData("http://example.com", false, null)]
    [InlineData("https://example.com", false, null)]
    [InlineData("http://localhost:5000", false, "http://localhost:5000")]
    [InlineData("https://localhost:5000", false, "https://localhost:5000")]
    [InlineData("http://example.com", true, null)]
    [InlineData("https://example.com", true, null)]
    public void ValidateRedirectUrl_ExternalUrl_ValidatesCorrectly(string url, bool allowExternal, string? currentOrigin)
    {
        var result = Utils.ValidateRedirectUrl(url, allowExternal, currentOrigin);

        if (allowExternal)
        {
            // Should allow external URLs when allowExternal is true
            // Note: Uri.ToString() may add trailing slash, so we check that result starts with url
            Assert.NotNull(result);
            Assert.StartsWith(url, result!);
        }
        else if (!string.IsNullOrEmpty(currentOrigin))
        {
            // Should only allow same-origin when allowExternal is false and origin is provided
            // Note: Uri.ToString() may add trailing slash, so we check that result starts with url
            if (url.StartsWith(currentOrigin))
            {
                Assert.NotNull(result);
                Assert.StartsWith(url, result!);
            }
            else
            {
                Assert.Null(result);
            }
        }
        else
        {
            // Should reject external URLs when allowExternal is false and no origin
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                Assert.Null(result);
            }
        }
    }

    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("data:text/html,<script>alert('xss')</script>")]
    [InlineData("file:///etc/passwd")]
    [InlineData("vbscript:msgbox('xss')")]
    [InlineData("onclick:alert('xss')")]
    public void ValidateRedirectUrl_DangerousProtocol_ReturnsNull(string url)
    {
        var result = Utils.ValidateRedirectUrl(url, allowExternal: true);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ValidateRedirectUrl_NullOrWhitespace_ReturnsNull(string? url)
    {
        var result = Utils.ValidateRedirectUrl(url);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("/path:with:colons")]
    public void ValidateRedirectUrl_RelativePathWithColons_ReturnsNull(string url)
    {
        var result = Utils.ValidateRedirectUrl(url);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("/path?query=value")]
    [InlineData("/path#fragment")]
    public void ValidateRedirectUrl_RelativePathWithQueryOrFragment_ReturnsUrl(string url)
    {
        // Query strings and fragments are valid in relative paths
        var result = Utils.ValidateRedirectUrl(url);
        Assert.Equal(url, result);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("invalid://url")]
    [InlineData("://malformed")]
    public void ValidateRedirectUrl_InvalidUrlFormat_ReturnsNull(string url)
    {
        var result = Utils.ValidateRedirectUrl(url, allowExternal: true);
        Assert.Null(result);
    }

    [Fact]
    public void ValidateRedirectUrl_SameOriginDifferentPort_ReturnsNull()
    {
        var url = "http://localhost:5001";
        var currentOrigin = "http://localhost:5000";

        var result = Utils.ValidateRedirectUrl(url, allowExternal: false, currentOrigin);

        Assert.Null(result);
    }

    [Fact]
    public void ValidateRedirectUrl_SameOriginDifferentScheme_ReturnsNull()
    {
        var url = "https://localhost:5000";
        var currentOrigin = "http://localhost:5000";

        var result = Utils.ValidateRedirectUrl(url, allowExternal: false, currentOrigin);

        Assert.Null(result);
    }

    #endregion

    #region ValidateLinkUrl Tests

    [Theory]
    [InlineData("/dashboard")]
    [InlineData("/users/profile")]
    [InlineData("/app/settings")]
    [InlineData("/")]
    public void ValidateLinkUrl_ValidRelativePath_ReturnsUrl(string url)
    {
        var result = Utils.ValidateLinkUrl(url);
        Assert.Equal(url, result);
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("http://subdomain.example.com")]
    [InlineData("https://example.com/path")]
    [InlineData("https://example.com/path?query=value")]
    [InlineData("https://example.com/path#fragment")]
    public void ValidateLinkUrl_ValidHttpHttpsUrl_ReturnsUrl(string url)
    {
        var result = Utils.ValidateLinkUrl(url);
        // Uri.ToString() may add trailing slash for URLs without paths, so check starts with
        Assert.NotNull(result);
        Assert.StartsWith(url, result!);
    }

    [Theory]
    [InlineData("app://dashboard")]
    [InlineData("app://users/profile")]
    [InlineData("app://my-app")]
    public void ValidateLinkUrl_ValidAppUrl_ReturnsUrl(string url)
    {
        var result = Utils.ValidateLinkUrl(url);
        Assert.Equal(url, result);
    }

    [Theory]
    [InlineData("#section1")]
    [InlineData("#heading-2")]
    [InlineData("#_anchor")]
    public void ValidateLinkUrl_ValidAnchorLink_ReturnsUrl(string url)
    {
        var result = Utils.ValidateLinkUrl(url);
        Assert.Equal(url, result);
    }

    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("data:text/html,<script>alert('xss')</script>")]
    [InlineData("file:///etc/passwd")]
    [InlineData("vbscript:msgbox('xss')")]
    [InlineData("onclick:alert('xss')")]
    public void ValidateLinkUrl_DangerousProtocol_ReturnsNull(string url)
    {
        var result = Utils.ValidateLinkUrl(url);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ValidateLinkUrl_NullOrWhitespace_ReturnsNull(string? url)
    {
        var result = Utils.ValidateLinkUrl(url);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("/path:with:colons")]
    public void ValidateLinkUrl_RelativePathWithColons_ReturnsNull(string url)
    {
        var result = Utils.ValidateLinkUrl(url);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("/path?query=value")]
    [InlineData("/path#fragment")]
    public void ValidateLinkUrl_RelativePathWithQueryOrFragment_ReturnsUrl(string url)
    {
        // Query strings and fragments are valid in relative paths
        var result = Utils.ValidateLinkUrl(url);
        Assert.Equal(url, result);
    }

    [Theory]
    [InlineData("app://path?query=value")]
    [InlineData("app://MyApp?param=value")]
    [InlineData("app://path?param1=value1&param2=value2")]
    public void ValidateLinkUrl_AppUrlWithQueryParameters_ReturnsUrl(string url)
    {
        // Query parameters are now allowed in app:// URLs
        var result = Utils.ValidateLinkUrl(url);
        Assert.Equal(url, result);
    }

    [Theory]
    [InlineData("app://path#fragment")]
    [InlineData("app://path:extra:colons")]
    public void ValidateLinkUrl_AppUrlWithDangerousCharacters_ReturnsNull(string url)
    {
        // Fragments and protocol injection attempts are still blocked
        var result = Utils.ValidateLinkUrl(url);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("#anchor:colons")]
    [InlineData("#section:value")]
    public void ValidateLinkUrl_AnchorLinkWithColons_ReturnsUrl(string url)
    {
        // Colons are now allowed in anchor links (HTML5 allows this)
        var result = Utils.ValidateLinkUrl(url);
        Assert.Equal(url, result);
    }

    [Theory]
    [InlineData("#anchor?query=value")]
    [InlineData("#anchor&evil")]
    public void ValidateLinkUrl_AnchorLinkWithDangerousCharacters_ReturnsNull(string url)
    {
        // Query parameters and ampersands are still blocked in anchor links
        var result = Utils.ValidateLinkUrl(url);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("relative-path")]
    [InlineData("path/without/leading/slash")]
    public void ValidateLinkUrl_RelativePathWithoutLeadingSlash_ReturnsWithSlash(string url)
    {
        var result = Utils.ValidateLinkUrl(url);
        Assert.NotNull(result);
        Assert.StartsWith("/", result);
    }

    [Theory]
    [InlineData("invalid://url")]
    [InlineData("://malformed")]
    public void ValidateLinkUrl_InvalidUrlFormat_ReturnsNull(string url)
    {
        var result = Utils.ValidateLinkUrl(url);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("path/without/leading/slash")]
    public void ValidateLinkUrl_RelativePathWithoutSlash_ReturnsWithSlash(string url)
    {
        // URLs without colons are treated as relative paths and get a leading slash
        var result = Utils.ValidateLinkUrl(url);
        Assert.NotNull(result);
        Assert.StartsWith("/", result!);
    }

    [Fact]
    public void ValidateLinkUrl_UrlWithWhitespace_TrimsWhitespace()
    {
        var url = "  /dashboard  ";
        var result = Utils.ValidateLinkUrl(url);
        Assert.Equal("/dashboard", result);
    }

    [Fact]
    public void ValidateLinkUrl_HttpUrlWithPort_ReturnsUrl()
    {
        var url = "http://example.com:8080/path";
        var result = Utils.ValidateLinkUrl(url);
        Assert.Equal(url, result);
    }

    [Fact]
    public void ValidateLinkUrl_HttpsUrlWithPort_ReturnsUrl()
    {
        var url = "https://example.com:443/path";
        var result = Utils.ValidateLinkUrl(url);
        // Uri normalizes default ports (443 for HTTPS), so port may be removed
        Assert.NotNull(result);
        // Should contain the path at minimum
        Assert.Contains("/path", result!);
    }

    #endregion

    #region IsSafeAppId Tests

    [Theory]
    [InlineData("dashboard")]
    [InlineData("users-profile")]
    [InlineData("my-app")]
    [InlineData("app123")]
    [InlineData("_index")]
    [InlineData("app_name")]
    public void IsSafeAppId_ValidAppId_ReturnsTrue(string appId)
    {
        var result = Utils.IsSafeAppId(appId);
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void IsSafeAppId_NullOrWhitespace_ReturnsFalse(string? appId)
    {
        var result = Utils.IsSafeAppId(appId);
        Assert.False(result);
    }

    [Theory]
    [InlineData("/dashboard")]
    [InlineData("/users/profile")]
    public void IsSafeAppId_StartsWithSlash_ReturnsFalse(string appId)
    {
        var result = Utils.IsSafeAppId(appId);
        Assert.False(result);
    }

    [Theory]
    [InlineData("app:protocol")]
    [InlineData("app?query")]
    [InlineData("app#fragment")]
    [InlineData("app&evil")]
    public void IsSafeAppId_ContainsDangerousCharacters_ReturnsFalse(string appId)
    {
        var result = Utils.IsSafeAppId(appId);
        Assert.False(result);
    }

    [Fact]
    public void IsSafeAppId_ContainsControlCharacters_ReturnsFalse()
    {
        var appId = "app\u0000with\u0001control\u0002chars";
        var result = Utils.IsSafeAppId(appId);
        Assert.False(result);
    }

    [Fact]
    public void IsSafeAppId_ValidAppIdWithNumbers_ReturnsTrue()
    {
        var appId = "app123";
        var result = Utils.IsSafeAppId(appId);
        Assert.True(result);
    }

    [Fact]
    public void IsSafeAppId_ValidAppIdWithUnderscores_ReturnsTrue()
    {
        var appId = "my_app_name";
        var result = Utils.IsSafeAppId(appId);
        Assert.True(result);
    }

    [Fact]
    public void IsSafeAppId_ValidAppIdWithHyphens_ReturnsTrue()
    {
        var appId = "my-app-name";
        var result = Utils.IsSafeAppId(appId);
        Assert.True(result);
    }

    [Fact]
    public void IsSafeAppId_AppIdWithNewline_ReturnsFalse()
    {
        var appId = "app\nwith\nnewlines";
        var result = Utils.IsSafeAppId(appId);
        Assert.False(result);
    }

    [Fact]
    public void IsSafeAppId_AppIdWithTab_ReturnsFalse()
    {
        var appId = "app\twith\ttabs";
        var result = Utils.IsSafeAppId(appId);
        Assert.False(result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ValidateRedirectUrl_ThenValidateLinkUrl_ConsistentResults()
    {
        var url = "https://example.com";

        var redirectResult = Utils.ValidateRedirectUrl(url, allowExternal: true);
        var linkResult = Utils.ValidateLinkUrl(url);

        Assert.NotNull(redirectResult);
        Assert.NotNull(linkResult);
        Assert.Equal(redirectResult, linkResult);
    }

    [Fact]
    public void ValidateLinkUrl_AppUrl_CanBeUsedInNavigation()
    {
        var url = "app://dashboard";
        var result = Utils.ValidateLinkUrl(url);

        Assert.NotNull(result);
        Assert.Equal(url, result);

        // Extract AppId from app:// URL
        var appId = url[6..]; // Remove "app://" prefix
        var isSafe = Utils.IsSafeAppId(appId);

        Assert.True(isSafe);
    }

    [Fact]
    public void ValidateRedirectUrl_RelativePath_WorksWithNavigateArgs()
    {
        var url = "/dashboard";
        var result = Utils.ValidateRedirectUrl(url);

        Assert.NotNull(result);
        Assert.Equal(url, result);

        // Extract AppId from relative path (remove leading slash)
        var appId = url.TrimStart('/');
        var isSafe = Utils.IsSafeAppId(appId);

        Assert.True(isSafe);
    }

    #endregion
}

