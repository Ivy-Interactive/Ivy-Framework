using System.Text.Json;

namespace Ivy.Apps;

public class AppArgs
{
    internal AppArgs(string connectionId, string appId, string? argsJson, string scheme, string host)
    {
        AppId = appId;
        ArgsJson = argsJson;
        ConnectionId = connectionId;
        Scheme = scheme;
        Host = host;
    }

    public string Scheme { get; set; }

    public string Host { get; set; }

    public string AppId { get; set; }

    public string ConnectionId { get; set; }

    private string? ArgsJson { get; set; }

    public T? GetArgs<T>() where T : class
    {
        if (ArgsJson == null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(ArgsJson);
    }
}