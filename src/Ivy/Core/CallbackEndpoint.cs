namespace Ivy.Core;

public record CallbackEndpoint(string Id, string BaseUrl)
{
    public CallbackEndpoint(string id, string scheme, string host) : this(id, BuildBaseUrl(scheme, host))
    {
    }

    public static string BuildBaseUrl(string scheme, string host) => $"{scheme}://{host}/ivy/webhook";

    public Uri GetUri(bool includeIdInPath = true) => includeIdInPath
        ? new Uri($"{BaseUrl}/{Id}")
        : new Uri(BaseUrl);
}
