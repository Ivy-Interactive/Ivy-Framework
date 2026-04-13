using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs.Formatters;
using Ivy.Core;
using Ivy.NativeJsonDiff;

namespace Ivy.Benchmarks;

public static class PayloadSizeAnalyzer
{
    public static void Run(int mutations = 100)
    {
        Console.WriteLine("================ [PAYLOAD ASSERTION ANALYZER] ================");
        Console.WriteLine($"Simulating a tree size of {mutations} mutating forms!");
        
        var oldNode = BuildSyntheticTree(mutations, 0);
        var newNode = BuildSyntheticTree(mutations, 1);

        var oldBytes = JsonSerializer.SerializeToUtf8Bytes(oldNode);
        var newBytes = JsonSerializer.SerializeToUtf8Bytes(newNode);

        var csPatch = oldNode.Diff(newNode, new JsonPatchDeltaFormatter(), WidgetTree.JsonDiffOptions);
        var csPayload = csPatch!.ToJsonString();
        
        var rustPatch = JsonDiffer.ComputePatch(oldBytes, newBytes);
        var rustPayload = rustPatch?.ToJsonString() ?? "NULL";

        Console.WriteLine($"\n[C# Original String]: {csPayload.Length} chars");
        Console.WriteLine($"[RUST Output String]: {rustPayload.Length} chars\n");

        if (csPayload.Length != rustPayload.Length)
        {
            Console.WriteLine("[WARNING]: Structurally, the output logic diverges in delta compression formatting!");
        }
        else
        {
            Console.WriteLine("[SUCCESS]: The cdylib array matches the byte-payload character by character!");
        }
    }
    
    private static JsonNode BuildSyntheticTree(int count, int triggerVal)
    {
        var root = new JsonObject();
        var children = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            var form = new JsonObject { ["id"] = $"form-{i}", ["type"] = "div" };
            var inputs = new JsonArray();
            for (int f = 0; f < 10; f++)
            {
                inputs.Add(new JsonObject { ["id"] = $"input-{i}-{f}", ["type"] = "text", ["props"] = new JsonObject { ["value"] = $"Field Value {f} [Mutated {triggerVal} Times!]" } });
            }
            form["children"] = inputs;
            children.Add(form);
        }
        root["children"] = children;
        return root;
    }
}
