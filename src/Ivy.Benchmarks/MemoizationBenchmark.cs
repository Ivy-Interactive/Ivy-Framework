using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Ivy;
using Ivy.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Benchmarks;

[MemoryDiagnoser]
public class MemoizationBenchmark
{
    private WidgetTree _treeUnmemoized = null!;
    private MassiveBailoutApp _appUnmemoized = null!;
    
    private WidgetTree _treeMemoized = null!;
    private MassiveBailoutMemoizedApp _appMemoized = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        
        _appUnmemoized = new MassiveBailoutApp(1000, 0);
        _treeUnmemoized = new WidgetTree(_appUnmemoized, new DefaultContentBuilder(), services);
        await _treeUnmemoized.BuildAsync();

        _appMemoized = new MassiveBailoutMemoizedApp(1000, 0);
        _treeMemoized = new WidgetTree(_appMemoized, new DefaultContentBuilder(), services);
        await _treeMemoized.BuildAsync();
    }

    [Benchmark(Baseline = true)]
    public async Task UpdateUnmemoized_NoBailout()
    {
        var completion = new TaskCompletionSource<bool>();
        var sub = _treeUnmemoized.Take(1).Subscribe(changes => completion.SetResult(true));
        _appUnmemoized.MutateState();
        await completion.Task;
        sub.Dispose();
    }

    [Benchmark]
    public async Task UpdateMemoized_999Bailouts()
    {
        var completion = new TaskCompletionSource<bool>();
        var sub = _treeMemoized.Take(1).Subscribe(changes => completion.SetResult(true));
        _appMemoized.MutateState();
        await completion.Task;
        sub.Dispose();
    }
}

public record struct ComponentProps(int Index, int Trigger);

public class MassiveBailoutApp : ViewBase
{
    private readonly int _formCount;
    public Action MutateState { get; private set; } = null!;

    public MassiveBailoutApp(int formCount, int _)
    {
        _formCount = formCount;
    }

    public override object? Build()
    {
        var runTrigger = UseState(0);
        MutateState = () => runTrigger.Set(x => x + 1);

        return Layout.Vertical(
            Text.H1("Deep Hierarchy Ivy Memoization Testing Payload"),
            Layout.Vertical(
                Enumerable.Range(0, _formCount).Select(formIndex => 
                    // To simulate unmemoized, we just instantiate identically without the Memo cache struct
                    new FormComponent(formIndex, formIndex == 0 ? runTrigger.Value : 0)
                ).ToArray()
            )
        );
    }
}

public class MassiveBailoutMemoizedApp : ViewBase
{
    private readonly int _formCount;
    public Action MutateState { get; private set; } = null!;

    public MassiveBailoutMemoizedApp(int formCount, int _)
    {
        _formCount = formCount;
    }

    public override object? Build()
    {
        var runTrigger = UseState(0);
        MutateState = () => runTrigger.Set(x => x + 1);

        return Layout.Vertical(
            Text.H1("Deep Hierarchy Ivy Memoization Testing Payload"),
            Layout.Vertical(
                Enumerable.Range(0, _formCount).Select(formIndex => 
                    new MemoizedFormComponent(formIndex, formIndex == 0 ? runTrigger.Value : 0)
                ).ToArray()
            )
        );
    }
}

public class MemoizedFormComponent(int index, int triggerVal) : ViewBase, IMemoized
{
    public object[] GetMemoValues() => new object[] { index, triggerVal };

    public override object? Build()
    {
        return Layout.Vertical(
            Text.H3($"Synthetic Form Prototype - Block {index}"),
            Layout.Vertical(
                Enumerable.Range(0, 10).Select(fieldIndex => 
                    Text.Muted($"Field Value {fieldIndex} [Mutated {triggerVal} Times!]")
                ).ToArray()
            )
        );
    }
}
