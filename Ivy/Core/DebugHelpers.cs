using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ivy.Core;

internal static class DebugHelpers
{
    private static readonly Lock FileLock = new();
    private static readonly string DumpFilePath = Path.Combine(Directory.GetCurrentDirectory(), "dump.ljson");

    public static void LogUpdatedTree(JsonNode? oldTree, JsonNode? newTree, JsonNode? patch, long elapsed, int iteration, string? treeHash)
    {
        var entry = new JsonObject
        {
            ["iteration"] = iteration,
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["old"] = oldTree?.DeepClone(),
            ["new"] = newTree?.DeepClone(),
            ["patch"] = patch?.DeepClone(),
            ["elapsed"] = elapsed,
            ["treeHash"] = treeHash
        };

        var json = entry.ToJsonString(new JsonSerializerOptions { WriteIndented = false });

        lock (FileLock)
        {
            File.AppendAllText(DumpFilePath, json + Environment.NewLine);
        }
    }

    public static void CheckIfIdUsedMultipleTimes(WidgetTreeNode? nodeTree)
    {
        if (nodeTree == null) return;

        var idCounts = new Dictionary<string, int>();
        CollectIds(nodeTree, idCounts);

        var duplicates = idCounts.Where(kvp => kvp.Value > 1).ToList();
        if (duplicates.Count > 0)
        {
            var duplicateIds = string.Join(", ", duplicates.Select(d => $"'{d.Key}' ({d.Value}x)"));
            throw new InvalidOperationException($"Duplicate IDs found in widget tree: {duplicateIds}");
        }
    }

    private static void CollectIds(WidgetTreeNode node, Dictionary<string, int> idCounts)
    {
        if (!idCounts.TryAdd(node.Id, 1))
        {
            idCounts[node.Id]++;
        }
        foreach (var child in node.Children)
        {
            CollectIds(child, idCounts);
        }
    }

    public static string? CalculateTreeHash(JsonNode? widgetTree)
    {
        if (widgetTree == null) return null;

        var sb = new System.Text.StringBuilder();
        BuildTreeSignature(widgetTree, sb);

        var signature = sb.ToString();

        return Djb2Hash(signature);
    }

    private static string Djb2Hash(string str)
    {
        // djb2 hash - matches frontend implementation
        unchecked
        {
            uint hash = 5381;
            foreach (var c in str)
            {
                hash = (hash * 33) ^ c;
            }
            return hash.ToString("x8");
        }
    }

    private static void BuildTreeSignature(JsonNode node, System.Text.StringBuilder sb)
    {
        var id = node["id"]?.GetValue<string>() ?? "";
        var type = node["type"]?.GetValue<string>() ?? "";

        sb.Append(id);
        sb.Append(':');
        sb.Append(type);
        sb.Append('[');

        var children = node["children"]?.AsArray();
        if (children != null)
        {
            var first = true;
            foreach (var child in children)
            {
                if (child != null)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    BuildTreeSignature(child, sb);
                }
            }
        }

        sb.Append(']');
    }
}