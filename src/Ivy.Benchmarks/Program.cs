using BenchmarkDotNet.Running;

namespace Ivy.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0].ToLower() == "analyze")
        {
            PayloadSizeAnalyzer.Run(100);
            return;
        }

        var config = BenchmarkDotNet.Configs.ManualConfig.Create(BenchmarkDotNet.Configs.DefaultConfig.Instance)
            .AddJob(BenchmarkDotNet.Jobs.Job.ShortRun);

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}
