using System.Text.Json;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class AppContext
{
    internal AppContext(string connectionId, string machineId, string appId, string? navigationAppId, string? argsJson, string scheme, string host)
    {
        MachineId = machineId;
        AppId = appId;
        NavigationAppId = navigationAppId;
        ArgsJson = argsJson;
        ConnectionId = connectionId;
        Scheme = scheme;
        Host = host;
    }

    public string Scheme { get; set; }

    public string Host { get; set; }

    /// <summary>
    /// Gets the base URL of the application (scheme + host).
    /// Useful for constructing absolute URLs for shareable links, webhooks, OAuth callbacks, etc.
    /// Example: "https://example.com"
    /// </summary>
    public string BaseUrl => $"{Scheme}://{Host}";

    public string AppId { get; set; }

    public string? NavigationAppId { get; set; }

    public string ConnectionId { get; set; }

    public string MachineId { get; set; }

    private string? ArgsJson { get; set; }

    public T? GetArgs<T>() where T : class
    {
        if (ArgsJson == null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(ArgsJson, JsonHelper.DefaultOptions);
    }
}