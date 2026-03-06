using Ivy.Core.Server;

namespace Ivy.Core;

public record WebhookEndpoint
{
    public string Id { get; init; }
    public string BaseUrl { get; init; }

    private WebhookEndpoint(string id, string baseUrl)
    {
        Id = id;
        BaseUrl = baseUrl;
    }

    public static WebhookEndpoint CreateWebhook(string id, string scheme, string host)
    {
        return new(id, BuildWebhookBaseUrl(scheme, host));
    }

    public static WebhookEndpoint CreateAuthCallback(string id, string scheme, string host)
    {
        return new(id, BuildAuthCallbackBaseUrl(scheme, host));
    }

    /// <summary>
    /// Creates a webhook endpoint using the RequestContext from DI.
    /// </summary>
    public static WebhookEndpoint CreateWebhook(string id, RequestContext requestContext)
    {
        var (scheme, host) = requestContext.GetRequired();
        return CreateWebhook(id, scheme, host);
    }

    /// <summary>
    /// Creates an auth callback endpoint using the RequestContext from DI.
    /// </summary>
    public static WebhookEndpoint CreateAuthCallback(string id, RequestContext requestContext)
    {
        var (scheme, host) = requestContext.GetRequired();
        return CreateAuthCallback(id, scheme, host);
    }

    public static string BuildWebhookBaseUrl(string scheme, string host) => $"{scheme}://{host}/ivy/webhook";

    public static string BuildAuthCallbackBaseUrl(string scheme, string host) => $"{scheme}://{host}/ivy/auth/callback";

    public Uri GetUri(bool includeIdInPath = true) => includeIdInPath
        ? new Uri($"{BaseUrl}/{Id}")
        : new Uri(BaseUrl);
}
