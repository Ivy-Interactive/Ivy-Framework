using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Ivy;
using Ivy.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Benchmarks;

[MemoryDiagnoser]
public class ConnectionScalingBenchmark
{
    [Params(10, 50)]
    public int ConcurrentConnections { get; set; }

    [Benchmark]
    public async Task InstantiateServerConnections()
    {
        var trees = new List<WidgetTree>();
        for(int i = 0; i < ConcurrentConnections; i++)
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var tree = new WidgetTree(new MassiveFormsApp(100, 0), new DefaultContentBuilder(), services);
            trees.Add(tree);
            
            await tree.BuildAsync();
            // GC will hold ALL 100 trees in memory simultaneously during the test
        }
    }
}
