using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs.Formatters;
using BenchmarkDotNet.Attributes;
using Ivy.Core;
using Ivy.NativeJsonDiff;

namespace Ivy.Benchmarks;

[MemoryDiagnoser]
public class JsonPatchBenchmark
{
    [Params(100, 1000)]
    public int Iterations { get; set; }

    private byte[] _oldBytes = null!;
    private byte[] _newBytes = null!;
    private JsonNode _oldNode = null!;
    private JsonNode _newNode = null!;

    [GlobalSetup]
    public void Setup()
    {
        _oldNode = BuildSyntheticTree(0);
        _newNode = BuildSyntheticTree(1); // Muted values mutated

        _oldBytes = JsonSerializer.SerializeToUtf8Bytes(_oldNode);
        _newBytes = JsonSerializer.SerializeToUtf8Bytes(_newNode);
    }

    private JsonNode BuildSyntheticTree(int triggerVal)
    {
        var root = new JsonObject();
        var children = new JsonArray();
        for (int i = 0; i < Iterations; i++)
        {
            var form = new JsonObject
            {
                ["id"] = $"form-{i}",
                ["type"] = "div"
            };
            var inputs = new JsonArray();
            for (int f = 0; f < 10; f++)
            {
                inputs.Add(new JsonObject
                {
                    ["id"] = $"input-{i}-{f}",
                    ["type"] = "text",
                    ["props"] = new JsonObject { ["value"] = $"Field Value {f} [Mutated {triggerVal} Times!]" }
                });
            }
            form["children"] = inputs;
            children.Add(form);
        }
        root["children"] = children;
        return root;
    }

    [Benchmark(Baseline = true)]
    public JsonNode? LegacyCSharpMath_JsonDiffPatch()
    {
        var oldParsed = JsonNode.Parse(_oldBytes)!;
        var newParsed = JsonNode.Parse(_newBytes)!;
        return oldParsed.Diff(newParsed, new JsonPatchDeltaFormatter(), WidgetTree.JsonDiffOptions);
    }

    [Benchmark]
    public JsonNode? NativeJsonDiff_ComputePatch()
    {
        return JsonDiffer.ComputePatch(_oldBytes, _newBytes);
    }
}
