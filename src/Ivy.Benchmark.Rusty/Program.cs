using System;
using System.Threading.Tasks;
using Ivy;

namespace Ivy.Benchmark.Rusty;

public static class Program 
{
    public static async Task Main(string[] args)
    {
        Environment.SetEnvironmentVariable("IVY_DUMP_WIDGET_TREES", "1"); 

        Console.WriteLine("==========================================================");
        Console.WriteLine($"[IVY BENCHMARK] VERSION {typeof(Server).Assembly.GetName().Version}");
        Console.WriteLine("Mode: NATIVE RUST CDYLIB ENGINE");
        Console.WriteLine("INSTRUCTIONS:");
        Console.WriteLine("1. Open localhost:5255 in your browser");
        Console.WriteLine("2. Open Activity Monitor or Task Manager to view RAM Usage");
        Console.WriteLine("3. Click [Simulate Massive 10,000 Node Mutation] repeatedly.");
        Console.WriteLine("4. Watch the Latency printed to the console output!");
        Console.WriteLine("==========================================================");

        var server = new Server(new ServerArgs { Port = 5255 });
        server.AddAppsFromAssembly(typeof(Program).Assembly);
        await server.RunAsync();
    }
}
