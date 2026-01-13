using Ivy;
using Ivy.Views.Builders;
using Xunit;

namespace Ivy.Test;

/// <summary>
/// Tests for LinkBuilder URL validation, especially for link buttons.
/// 
/// This test file verifies that LinkBuilder correctly validates URLs
/// using Utils.ValidateLinkUrl, ensuring that:
/// 1. Safe URLs are validated and create enabled buttons
/// 2. Dangerous URLs are sanitized and create disabled buttons
/// 3. Invalid URLs result in disabled buttons
/// </summary>
public class LinkBuilderTests
{
    #region Happy Path Tests

    [Theory]
    [InlineData("https://example.com/page")]
    [InlineData("http://example.com/page")]
    public void LinkBuilder_ValidAbsoluteUrl_CreatesEnabledButton(string url)
    {
        // Arrange
        var builder = new LinkBuilder<object>(url, "Link Text");
        // Note: LinkBuilder requires a non-null value, so we pass a dummy value
        var dummyValue = "dummy";

        // Act
        var result = builder.Build(dummyValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.False(button.Disabled);
        Assert.Equal("Link Text", button.Title);
        // URL may be normalized (e.g., trailing slash added)
        Assert.NotNull(button.Url);
        Assert.Contains(url.Split('/')[0] + "//" + url.Split('/')[2], button.Url);
    }

    [Theory]
    [InlineData("/dashboard")]
    [InlineData("/users/profile")]
    [InlineData("app://dashboard")]
    [InlineData("#section1")]
    public void LinkBuilder_ValidRelativeOrSpecialUrl_CreatesEnabledButton(string url)
    {
        // Arrange
        var builder = new LinkBuilder<object>(url, "Link Text");
        var dummyValue = "dummy";

        // Act
        var result = builder.Build(dummyValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.False(button.Disabled);
        Assert.Equal("Link Text", button.Title);
        Assert.Equal(url, button.Url);
    }

    [Theory]
    [InlineData("https://example.com/page", "Custom Label")]
    [InlineData("/dashboard", "Go to Dashboard")]
    [InlineData("app://my-app", "Open App")]
    public void LinkBuilder_ValidUrlWithCustomLabel_UsesCustomLabel(string url, string label)
    {
        // Arrange
        var builder = new LinkBuilder<object>(url, label);
        var dummyValue = "dummy";

        // Act
        var result = builder.Build(dummyValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.Equal(label, button.Title);
        // URL may be normalized for absolute URLs
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            Assert.NotNull(button.Url);
            Assert.Contains(url.Split('/')[0] + "//" + url.Split('/')[2], button.Url);
        }
        else
        {
            Assert.Equal(url, button.Url);
        }
    }

    [Fact]
    public void LinkBuilder_ValidUrlFromValue_UsesValueAsUrl()
    {
        // Arrange
        var builder = new LinkBuilder<object>(label: "Link Text");
        var value = "https://example.com/page";

        // Act
        var result = builder.Build(value, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.Equal("Link Text", button.Title);
        Assert.Equal(value, button.Url);
    }

    [Fact]
    public void LinkBuilder_ValidUrlFromValue_NoLabel_UsesUrlAsLabel()
    {
        // Arrange
        var builder = new LinkBuilder<object>();
        var value = "https://example.com/page";

        // Act
        var result = builder.Build(value, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.Equal(value, button.Title);
        Assert.Equal(value, button.Url);
    }

    [Theory]
    [InlineData("https://example.com/page?param=value")]
    [InlineData("/dashboard?tab=settings")]
    [InlineData("https://example.com/page#section")]
    [InlineData("/page#section")]
    public void LinkBuilder_ValidUrlWithQueryOrFragment_HandlesCorrectly(string url)
    {
        // Arrange
        var builder = new LinkBuilder<object>(url, "Link");
        var dummyValue = "dummy";

        // Act
        var result = builder.Build(dummyValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.False(button.Disabled);
        // URL may be normalized, so check that it contains the key parts
        Assert.NotNull(button.Url);
        if (url.StartsWith("http"))
        {
            Assert.Contains(url.Split('?')[0].Split('#')[0], button.Url);
        }
        else
        {
            Assert.Equal(url, button.Url);
        }
    }

    #endregion

    #region Sad Path Tests

    [Theory]
    [InlineData("javascript:alert(\"xss\")")]
    [InlineData("data:text/html,<script>alert(\"xss\")</script>")]
    [InlineData("file:///etc/passwd")]
    [InlineData("vbscript:msgbox(\"xss\")")]
    public void LinkBuilder_DangerousUrl_CreatesDisabledButton(string dangerousUrl)
    {
        // Arrange
        var builder = new LinkBuilder<object>(dangerousUrl, "Dangerous Link");
        var dummyValue = "dummy";

        // Act
        var result = builder.Build(dummyValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.True(button.Disabled, "Dangerous URLs should result in disabled buttons");
        Assert.Equal("Dangerous Link", button.Title);
        // URL should be null or empty for disabled buttons
        Assert.Null(button.Url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LinkBuilder_InvalidUrl_CreatesDisabledButton(string invalidUrl)
    {
        // Arrange
        var builder = new LinkBuilder<object>(invalidUrl, "Invalid Link");
        var dummyValue = "dummy";

        // Act
        var result = builder.Build(dummyValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.True(button.Disabled, "Invalid URLs should result in disabled buttons");
        Assert.Equal("Invalid Link", button.Title);
    }

    [Fact]
    public void LinkBuilder_NullUrl_WithNullValue_ReturnsNull()
    {
        // Arrange
        var builder = new LinkBuilder<object>(null, "Link");

        // Act
        var result = builder.Build(null, new object());

        // Assert
        // LinkBuilder returns null when value is null (regardless of url parameter)
        Assert.Null(result);
    }

    [Fact]
    public void LinkBuilder_NullValue_ReturnsNull()
    {
        // Arrange
        var builder = new LinkBuilder<object>("https://example.com", "Link");

        // Act
        var result = builder.Build(null, new object());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void LinkBuilder_DangerousUrlFromValue_CreatesDisabledButton()
    {
        // Arrange
        var builder = new LinkBuilder<object>(label: "Link");
        var dangerousValue = "javascript:alert(\"xss\")";

        // Act
        var result = builder.Build(dangerousValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.True(button.Disabled, "Dangerous URLs from value should result in disabled buttons");
        Assert.Equal("Link", button.Title); // Uses the label when provided
        Assert.Null(button.Url);
    }

    [Theory]
    [InlineData("://malformed")]
    [InlineData("http://")]
    [InlineData("https://")]
    [InlineData("invalid://protocol")]
    public void LinkBuilder_MalformedUrl_CreatesDisabledButton(string malformedUrl)
    {
        // Arrange
        var builder = new LinkBuilder<object>(malformedUrl, "Malformed Link");
        var dummyValue = "dummy";

        // Act
        var result = builder.Build(dummyValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        // Malformed URLs should result in disabled buttons
        Assert.True(button.Disabled);
    }

    [Fact]
    public void LinkBuilder_InvalidUrl_NoLabel_UsesSafeDefault()
    {
        // Arrange
        var builder = new LinkBuilder<object>();
        var invalidUrl = "javascript:alert(\"xss\")";

        // Act
        var result = builder.Build(invalidUrl, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.True(button.Disabled);
        // Invalid URLs should always use safe default label to avoid displaying potentially dangerous content
        Assert.Equal("Invalid Link", button.Title);
    }

    [Fact]
    public void LinkBuilder_InvalidButSafeUrl_NoLabel_UsesSafeDefault()
    {
        // Arrange
        var builder = new LinkBuilder<object>();
        var invalidButSafeUrl = "://malformed";

        // Act
        var result = builder.Build(invalidButSafeUrl, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.True(button.Disabled);
        // All invalid URLs use safe default, regardless of whether they're dangerous
        Assert.Equal("Invalid Link", button.Title);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void LinkBuilder_UrlTakesPrecedenceOverValue()
    {
        // Arrange
        var builder = new LinkBuilder<object>(url: "https://example.com", label: "Link");
        var value = "https://other.com";

        // Act
        var result = builder.Build(value, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        // URL parameter takes precedence, may be normalized with trailing slash
        Assert.NotNull(button.Url);
        Assert.Contains("https://example.com", button.Url);
        Assert.Equal("Link", button.Title);
    }

    [Fact]
    public void LinkBuilder_ButtonVariant_IsAlwaysInline()
    {
        // Arrange
        var builder = new LinkBuilder<object>("https://example.com", "Link");
        var dummyValue = "dummy";

        // Act
        var result = builder.Build(dummyValue, new object());

        // Assert
        Assert.NotNull(result);
        var button = Assert.IsType<Button>(result);
        Assert.Equal(ButtonVariant.Inline, button.Variant);
    }

    [Fact]
    public void LinkBuilder_MixedValidAndInvalidUrls_HandlesCorrectly()
    {
        // Arrange
        var validUrls = new[] { "https://example.com", "/dashboard", "app://my-app" };
        var invalidUrls = new[] { "javascript:alert(\"xss\")", "data:text/html,<script>" };
        var dummyValue = "dummy";

        // Act & Assert - Valid URLs
        foreach (var url in validUrls)
        {
            var builder = new LinkBuilder<object>(url, "Link");
            var result = builder.Build(dummyValue, new object());
            Assert.NotNull(result);
            var button = Assert.IsType<Button>(result);
            Assert.False(button.Disabled, $"Valid URL {url} should create enabled button");
        }

        // Act & Assert - Invalid URLs
        foreach (var url in invalidUrls)
        {
            var builder = new LinkBuilder<object>(url, "Link");
            var result = builder.Build(dummyValue, new object());
            Assert.NotNull(result);
            var button = Assert.IsType<Button>(result);
            Assert.True(button.Disabled, $"Invalid URL {url} should create disabled button");
        }
    }

    #endregion
}

