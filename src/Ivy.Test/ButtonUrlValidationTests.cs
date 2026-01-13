using Ivy;
using Xunit;

namespace Ivy.Test;

/// <summary>
/// Tests for Button.Url() extension method URL validation.
/// 
/// This test file verifies that Button.Url() correctly validates URLs
/// using Utils.ValidateLinkUrl, ensuring that:
/// 1. Safe URLs are validated and set correctly
/// 2. Dangerous URLs throw ArgumentException
/// 3. Invalid URLs throw ArgumentException
/// </summary>
public class ButtonUrlValidationTests
{
    #region Happy Path Tests

    [Theory]
    [InlineData("https://example.com/page")]
    [InlineData("http://example.com/page")]
    [InlineData("/dashboard")]
    [InlineData("/users/profile")]
    [InlineData("app://dashboard")]
    [InlineData("#section1")]
    public void Button_Url_ValidUrl_SetsUrl(string url)
    {
        // Arrange
        var button = new Button("Click Me");

        // Act
        var result = button.Url(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(url, result.Url);
    }

    [Theory]
    [InlineData("https://example.com/page?param=value")]
    [InlineData("/dashboard?tab=settings")]
    [InlineData("https://example.com/page#section")]
    [InlineData("/page#section")]
    public void Button_Url_ValidUrlWithQueryOrFragment_SetsUrl(string url)
    {
        // Arrange
        var button = new Button("Click Me");

        // Act
        var result = button.Url(url);

        // Assert
        Assert.NotNull(result);
        // URL may be normalized, so check that it contains the key parts
        if (url.StartsWith("http"))
        {
            Assert.Contains(url.Split('?')[0].Split('#')[0], result.Url);
        }
        else
        {
            Assert.Equal(url, result.Url);
        }
    }

    [Fact]
    public void Button_Url_CanBeChained()
    {
        // Arrange
        var button = new Button("Click Me");

        // Act
        var result = button
            .Url("https://example.com")
            .Variant(ButtonVariant.Link)
            .Disabled(false);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("https://example.com", result.Url);
        Assert.Equal(ButtonVariant.Link, result.Variant);
        Assert.False(result.Disabled);
    }

    #endregion

    #region Sad Path Tests

    [Theory]
    [InlineData("javascript:alert(\"xss\")")]
    [InlineData("data:text/html,<script>alert(\"xss\")</script>")]
    [InlineData("file:///etc/passwd")]
    [InlineData("vbscript:msgbox(\"xss\")")]
    public void Button_Url_DangerousUrl_ThrowsArgumentException(string dangerousUrl)
    {
        // Arrange
        var button = new Button("Click Me");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => button.Url(dangerousUrl));
        Assert.Contains("Invalid URL", exception.Message);
        Assert.Equal("url", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("://malformed")]
    [InlineData("http://")]
    [InlineData("https://")]
    [InlineData("invalid://protocol")]
    public void Button_Url_InvalidUrl_ThrowsArgumentException(string invalidUrl)
    {
        // Arrange
        var button = new Button("Click Me");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => button.Url(invalidUrl));
        Assert.Contains("Invalid URL", exception.Message);
        Assert.Equal("url", exception.ParamName);
    }

    [Fact]
    public void Button_Url_NullUrl_ThrowsArgumentException()
    {
        // Arrange
        var button = new Button("Click Me");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => button.Url(null!));
        Assert.Contains("Invalid URL", exception.Message);
        Assert.Equal("url", exception.ParamName);
    }

    [Fact]
    public void Button_Url_WithLinkVariant_ValidatesUrl()
    {
        // Arrange
        var button = new Button("Link Button", variant: ButtonVariant.Link);

        // Act
        var result = button.Url("https://example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("https://example.com", result.Url);
        Assert.Equal(ButtonVariant.Link, result.Variant);
    }

    [Fact]
    public void Button_Url_WithLinkVariant_DangerousUrl_ThrowsException()
    {
        // Arrange
        var button = new Button("Link Button", variant: ButtonVariant.Link);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => button.Url("javascript:alert(\"xss\")"));
        Assert.Contains("Invalid URL", exception.Message);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Button_Url_ValidatesBeforeSetting()
    {
        // Arrange
        var button = new Button("Click Me");

        // Act - Valid URL
        var validResult = button.Url("/dashboard");
        Assert.NotNull(validResult);
        Assert.Equal("/dashboard", validResult.Url);

        // Act & Assert - Invalid URL throws before setting
        var exception = Assert.Throws<ArgumentException>(() => button.Url("javascript:alert(\"xss\")"));
        Assert.Contains("Invalid URL", exception.Message);
    }

    [Fact]
    public void Button_Url_MultipleValidUrls_AllWork()
    {
        // Arrange
        var validUrls = new[]
        {
            "https://example.com",
            "/dashboard",
            "app://my-app",
            "#section1"
        };

        foreach (var url in validUrls)
        {
            var button = new Button("Click Me");

            // Act
            var result = button.Url(url);

            // Assert
            Assert.NotNull(result);
            if (url.StartsWith("http"))
            {
                Assert.Contains(url.Split('/')[0] + "//" + url.Split('/')[2], result.Url);
            }
            else
            {
                Assert.Equal(url, result.Url);
            }
        }
    }

    #endregion
}

