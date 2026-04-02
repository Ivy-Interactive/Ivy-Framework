using BenchmarkDotNet.Attributes;
using Ivy;

namespace Ivy.Benchmarks;

[MemoryDiagnoser]
public class AppInteractionBenchmark
{
    [Params(100, 1000)]
    public int FormCount { get; set; }

    [Benchmark]
    public object? ComponentGenerationSpeed()
    {
        var app = new MassiveFormsApp(FormCount, 1);
        return app.Build();
    }
}
