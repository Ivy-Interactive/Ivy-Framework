using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Ivy;
using Ivy.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Benchmarks;

[MemoryDiagnoser]
public class StateTriggerBenchmark
{
    [Params(100, 1000)]
    public int FormCount { get; set; }

    private WidgetTree _tree = null!;
    private MassiveFormsApp _app = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _app = new MassiveFormsApp(FormCount, 0);
        var services = new ServiceCollection().BuildServiceProvider();
        _tree = new WidgetTree(_app, new DefaultContentBuilder(), services);
        
        await _tree.BuildAsync();
    }

    [Benchmark]
    public async Task FrameRenderCascade()
    {
        var completion = new TaskCompletionSource<bool>();
        var sub = _tree.Take(1).Subscribe(changes => {
            completion.SetResult(true);
        });

        _app.MutateState();

        await completion.Task;
        sub.Dispose();
    }
}
