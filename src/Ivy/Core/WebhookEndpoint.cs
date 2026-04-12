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

    public static WebhookEndpoint CreateWebhook(string id, string scheme, string host, string? basePath = null)
    {
        return new(id, BuildWebhookBaseUrl(scheme, host, basePath));
    }

    public static WebhookEndpoint CreateAuthCallback(string id, string scheme, string host, string? basePath = null)
    {
        return new(id, BuildAuthCallbackBaseUrl(scheme, host, basePath));
    }

    public static string BuildWebhookBaseUrl(string scheme, string host, string? basePath = null)
        => basePath != null ? $"{scheme}://{host}/{basePath.Trim('/')}/ivy/webhook" : $"{scheme}://{host}/ivy/webhook";

    public static string BuildAuthCallbackBaseUrl(string scheme, string host, string? basePath = null)
        => basePath != null ? $"{scheme}://{host}/{basePath.Trim('/')}/ivy/auth/callback" : $"{scheme}://{host}/ivy/auth/callback";

    public Uri GetUri(bool includeIdInPath = true) => includeIdInPath
        ? new Uri($"{BaseUrl}/{Id}")
        : new Uri(BaseUrl);
}
