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

    public static WebhookEndpoint CreateWebhook(string id, string scheme, string host, string? pathBase = null)
    {
        return new(id, BuildWebhookBaseUrl(scheme, host, pathBase));
    }

    public static WebhookEndpoint CreateAuthCallback(string id, string scheme, string host, string? pathBase = null)
    {
        return new(id, BuildAuthCallbackBaseUrl(scheme, host, pathBase));
    }

    public static string BuildWebhookBaseUrl(string scheme, string host, string? pathBase = null)
        => $"{scheme}://{host}{(string.IsNullOrEmpty(pathBase) ? "" : "/" + pathBase.Trim('/'))}/ivy/webhook";

    public static string BuildAuthCallbackBaseUrl(string scheme, string host, string? pathBase = null)
        => $"{scheme}://{host}{(string.IsNullOrEmpty(pathBase) ? "" : "/" + pathBase.Trim('/'))}/ivy/auth/callback";

    public Uri GetUri(bool includeIdInPath = true) => includeIdInPath
        ? new Uri($"{BaseUrl}/{Id}")
        : new Uri(BaseUrl);
}
