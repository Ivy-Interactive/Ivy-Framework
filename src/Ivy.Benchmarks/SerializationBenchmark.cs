using System.Text.Json;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;

namespace Ivy.Benchmarks;

[MemoryDiagnoser]
public class SerializationBenchmark
{
    [Params(100, 1000)]
    public int Iterations { get; set; }

    private JsonNode _oldNode = null!;

    [GlobalSetup]
    public void Setup()
    {
        var root = new JsonObject();
        var children = new JsonArray();
        for (int i = 0; i < Iterations; i++)
        {
            var form = new JsonObject { ["id"] = $"form-{i}", ["type"] = "div" };
            var inputs = new JsonArray();
            for (int f = 0; f < 10; f++)
            {
                inputs.Add(new JsonObject { ["id"] = $"input-{i}-{f}", ["type"] = "text", ["props"] = new JsonObject { ["value"] = $"Field Value {f}" } });
            }
            form["children"] = inputs;
            children.Add(form);
        }
        root["children"] = children;
        _oldNode = root;
    }

    [Benchmark(Baseline = true)]
    public byte[] SerializeToUtf8Bytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(_oldNode);
    }
    
    [Benchmark]
    public string SerializeToString()
    {
        return _oldNode.ToJsonString();
    }
}
