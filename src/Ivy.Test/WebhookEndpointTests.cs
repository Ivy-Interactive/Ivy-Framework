using Ivy.Core;

namespace Ivy.Test;

public class WebhookEndpointTests
{
    [Fact]
    public void BuildWebhookBaseUrl_NoBasePath_ReturnsUrlWithoutBasePath()
    {
        var result = WebhookEndpoint.BuildWebhookBaseUrl("https", "example.com");
        Assert.Equal("https://example.com/ivy/webhook", result);
    }

    [Fact]
    public void BuildWebhookBaseUrl_WithLeadingSlash_ReturnsCorrectUrl()
    {
        var result = WebhookEndpoint.BuildWebhookBaseUrl("https", "example.com", "/ivy");
        Assert.Equal("https://example.com/ivy/ivy/webhook", result);
    }

    [Fact]
    public void BuildWebhookBaseUrl_WithoutLeadingSlash_ReturnsCorrectUrl()
    {
        var result = WebhookEndpoint.BuildWebhookBaseUrl("https", "example.com", "ivy");
        Assert.Equal("https://example.com/ivy/ivy/webhook", result);
    }

    [Fact]
    public void BuildWebhookBaseUrl_WithTrailingSlash_ReturnsCorrectUrl()
    {
        var result = WebhookEndpoint.BuildWebhookBaseUrl("https", "example.com", "/ivy/");
        Assert.Equal("https://example.com/ivy/ivy/webhook", result);
    }

    [Fact]
    public void BuildAuthCallbackBaseUrl_NoBasePath_ReturnsUrlWithoutBasePath()
    {
        var result = WebhookEndpoint.BuildAuthCallbackBaseUrl("https", "example.com");
        Assert.Equal("https://example.com/ivy/auth/callback", result);
    }

    [Fact]
    public void BuildAuthCallbackBaseUrl_WithLeadingSlash_ReturnsCorrectUrl()
    {
        var result = WebhookEndpoint.BuildAuthCallbackBaseUrl("https", "example.com", "/ivy");
        Assert.Equal("https://example.com/ivy/ivy/auth/callback", result);
    }

    [Fact]
    public void BuildAuthCallbackBaseUrl_WithoutLeadingSlash_ReturnsCorrectUrl()
    {
        var result = WebhookEndpoint.BuildAuthCallbackBaseUrl("https", "example.com", "ivy");
        Assert.Equal("https://example.com/ivy/ivy/auth/callback", result);
    }

    [Fact]
    public void BuildAuthCallbackBaseUrl_WithTrailingSlash_ReturnsCorrectUrl()
    {
        var result = WebhookEndpoint.BuildAuthCallbackBaseUrl("https", "example.com", "/ivy/");
        Assert.Equal("https://example.com/ivy/ivy/auth/callback", result);
    }

    [Fact]
    public void BuildWebhookBaseUrl_NullBasePath_ReturnsUrlWithoutBasePath()
    {
        var result = WebhookEndpoint.BuildWebhookBaseUrl("https", "sliplane.app", null);
        Assert.Equal("https://sliplane.app/ivy/webhook", result);
    }

    [Fact]
    public void BuildAuthCallbackBaseUrl_NullBasePath_ReturnsUrlWithoutBasePath()
    {
        var result = WebhookEndpoint.BuildAuthCallbackBaseUrl("https", "sliplane.app", null);
        Assert.Equal("https://sliplane.app/ivy/auth/callback", result);
    }
}
