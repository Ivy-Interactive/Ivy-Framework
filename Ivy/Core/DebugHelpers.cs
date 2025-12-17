using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ivy.Core;

internal static class DebugHelpers
{
    private static readonly Lock FileLock = new();
    private static readonly string DumpFilePath = Path.Combine(Directory.GetCurrentDirectory(), "dump.ljson");

    public static void LogUpdatedTree(JsonNode? oldTree, JsonNode? newTree, JsonNode? patch, long elapsed)
    {
        var entry = new JsonObject
        {
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["old"] = oldTree?.DeepClone(),
            ["new"] = newTree?.DeepClone(),
            ["patch"] = patch?.DeepClone(),
            ["elapsed"] = elapsed
        };

        var json = entry.ToJsonString(new JsonSerializerOptions { WriteIndented = false });

        lock (FileLock)
        {
            File.AppendAllText(DumpFilePath, json + Environment.NewLine);
        }
    }
}