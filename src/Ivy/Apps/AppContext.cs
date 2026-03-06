using System.Text.Json;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class AppContext
{
    internal AppContext(string connectionId, string machineId, string appId, string? navigationAppId, string? argsJson)
    {
        MachineId = machineId;
        AppId = appId;
        NavigationAppId = navigationAppId;
        ArgsJson = argsJson;
        ConnectionId = connectionId;
    }

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