using Ivy;

namespace Ivy.Test.Helpers;

public class ValidationHelperTests
{
    #region IsValidRequired

    [Theory]
    [InlineData(null, false)]
    [InlineData(0, false)]
    [InlineData(0.0, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsValidRequired_InvalidValues_ReturnsFalse(object? value, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidRequired(value));
    }

    [Fact]
    public void IsValidRequired_EmptyGuid_ReturnsFalse()
    {
        Assert.False(ValidationHelper.IsValidRequired(Guid.Empty));
    }

    [Fact]
    public void IsValidRequired_MinDateTime_ReturnsFalse()
    {
        Assert.False(ValidationHelper.IsValidRequired(DateTime.MinValue));
    }

    [Fact]
    public void IsValidRequired_ValidString_ReturnsTrue()
    {
        Assert.True(ValidationHelper.IsValidRequired("hello"));
    }

    [Fact]
    public void IsValidRequired_NonZeroInt_ReturnsTrue()
    {
        Assert.True(ValidationHelper.IsValidRequired(42));
    }

    [Fact]
    public void IsValidRequired_ValidGuid_ReturnsTrue()
    {
        Assert.True(ValidationHelper.IsValidRequired(Guid.NewGuid()));
    }

    #endregion

    #region IsEmptyContent

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData(false, true)]
    public void IsEmptyContent_EmptyValues_ReturnsTrue(object? value, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsEmptyContent(value));
    }

    [Fact]
    public void IsEmptyContent_NonEmptyString_ReturnsFalse()
    {
        Assert.False(ValidationHelper.IsEmptyContent("hello"));
    }

    [Fact]
    public void IsEmptyContent_TrueBool_ReturnsFalse()
    {
        Assert.False(ValidationHelper.IsEmptyContent(true));
    }

    [Fact]
    public void IsEmptyContent_Object_ReturnsFalse()
    {
        Assert.False(ValidationHelper.IsEmptyContent(42));
    }

    #endregion

    #region ValidateRedirectUrl

    [Theory]
    [InlineData("/dashboard")]
    [InlineData("/users/profile")]
    public void ValidateRedirectUrl_ValidRelativePath_ReturnsUrl(string url)
    {
        Assert.Equal(url, ValidationHelper.ValidateRedirectUrl(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRedirectUrl_NullOrWhitespace_ReturnsNull(string? url)
    {
        Assert.Null(ValidationHelper.ValidateRedirectUrl(url));
    }

    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("data:text/html,<script>alert('xss')</script>")]
    public void ValidateRedirectUrl_DangerousProtocol_ReturnsNull(string url)
    {
        Assert.Null(ValidationHelper.ValidateRedirectUrl(url, allowExternal: true));
    }

    [Fact]
    public void ValidateRedirectUrl_ExternalNotAllowed_NoOrigin_ReturnsNull()
    {
        Assert.Null(ValidationHelper.ValidateRedirectUrl("https://example.com", allowExternal: false));
    }

    [Fact]
    public void ValidateRedirectUrl_ExternalAllowed_ReturnsUrl()
    {
        var result = ValidationHelper.ValidateRedirectUrl("https://example.com", allowExternal: true);
        Assert.NotNull(result);
    }

    [Fact]
    public void ValidateRedirectUrl_SameOrigin_ReturnsUrl()
    {
        var result = ValidationHelper.ValidateRedirectUrl("http://localhost:5000", allowExternal: false, "http://localhost:5000");
        Assert.NotNull(result);
    }

    [Fact]
    public void ValidateRedirectUrl_DifferentOrigin_ReturnsNull()
    {
        Assert.Null(ValidationHelper.ValidateRedirectUrl("http://localhost:5001", allowExternal: false, "http://localhost:5000"));
    }

    #endregion

    #region ValidateLinkUrl

    [Theory]
    [InlineData("/dashboard")]
    [InlineData("https://example.com")]
    [InlineData("app://dashboard")]
    [InlineData("#section1")]
    public void ValidateLinkUrl_ValidUrls_ReturnsNonNull(string url)
    {
        Assert.NotNull(ValidationHelper.ValidateLinkUrl(url));
    }

    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("data:text/html,test")]
    [InlineData(null)]
    [InlineData("")]
    public void ValidateLinkUrl_InvalidUrls_ReturnsNull(string? url)
    {
        Assert.Null(ValidationHelper.ValidateLinkUrl(url));
    }

    [Fact]
    public void ValidateLinkUrl_MailtoUrl_ReturnsUrl()
    {
        var result = ValidationHelper.ValidateLinkUrl("mailto:test@example.com");
        Assert.Equal("mailto:test@example.com", result);
    }

    [Fact]
    public void ValidateLinkUrl_AppUrlWithQuery_ReturnsUrl()
    {
        Assert.Equal("app://dashboard?tab=1", ValidationHelper.ValidateLinkUrl("app://dashboard?tab=1"));
    }

    [Fact]
    public void ValidateLinkUrl_AppUrlWithFragment_ReturnsNull()
    {
        Assert.Null(ValidationHelper.ValidateLinkUrl("app://path#fragment"));
    }

    #endregion

    #region IsSafeAppId

    [Theory]
    [InlineData("dashboard", true)]
    [InlineData("my-app", true)]
    [InlineData("app123", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("/dashboard", false)]
    [InlineData("app:id", false)]
    [InlineData("app?q", false)]
    public void IsSafeAppId_ReturnsExpected(string? appId, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsSafeAppId(appId));
    }

    [Fact]
    public void IsSafeAppId_ControlCharacters_ReturnsFalse()
    {
        Assert.False(ValidationHelper.IsSafeAppId("app\0id"));
    }

    #endregion
}
