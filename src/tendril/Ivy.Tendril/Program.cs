using System.Diagnostics;
using Ivy.Desktop;
using Ivy.Tendril.Database;
using Ivy.Tendril.Services;
using Velopack;

namespace Ivy.Tendril;

public class Program
{
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        VelopackApp.Build().Run();

        // Detect if we should use Photino (GUI) or Web (localhost)
        // 1. Force Photino if --photino flag is present
        // 2. Default to Photino if running as 'tendril' tool
        // 3. Otherwise default to Web server
        var fileName = Path.GetFileNameWithoutExtension(Environment.ProcessPath ?? "");
        bool isTool = fileName.Equals("tendril", StringComparison.OrdinalIgnoreCase);
        bool forcePhotino = args.Contains("--photino");
        bool forceWeb = args.Contains("--web");

        bool usePhotino = (isTool || forcePhotino) && !forceWeb;

        // Strip our flags before passing to the rest of the app
        var filteredArgs = args.Where(a => a != "--photino" && a != "--web").ToArray();

        // Handle database CLI commands before starting the server/GUI
        var dbExitCode = DatabaseCommands.Handle(filteredArgs);
        if (dbExitCode >= 0)
            return dbExitCode;

        var pwExitCode = PromptwareCommands.Handle(filteredArgs);
        if (pwExitCode >= 0)
            return pwExitCode;

        var crashLogPath = GetCrashLogPath();
        WriteCrashLog(crashLogPath, $"[{DateTime.UtcNow:O}] Tendril starting (PID {Environment.ProcessId}) | {GetMemoryStats()}");

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var msg = $"[{DateTime.UtcNow:O}] FATAL UnhandledException (IsTerminating={e.IsTerminating}) | {GetMemoryStats()}\n  {e.ExceptionObject}";
            Console.WriteLine($"[FATAL] Unhandled exception: {e.ExceptionObject}");
            WriteCrashLog(crashLogPath, msg);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            var msg = $"[{DateTime.UtcNow:O}] FATAL UnobservedTaskException | {GetMemoryStats()}\n  {e.Exception}";
            Console.WriteLine($"[FATAL] Unobserved task exception: {e.Exception}");
            WriteCrashLog(crashLogPath, msg);
            e.SetObserved();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            WriteCrashLog(crashLogPath, $"[{DateTime.UtcNow:O}] ProcessExit event fired (PID {Environment.ProcessId}) | {GetMemoryStats()}");
        };

        // Periodic memory watchdog — logs a warning when working set exceeds 1 GB
        _ = Task.Run(async () =>
        {
            const long warningThresholdBytes = 1L * 1024 * 1024 * 1024; // 1 GB
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(5));
                try
                {
                    using var proc = Process.GetCurrentProcess();
                    if (proc.WorkingSet64 > warningThresholdBytes)
                        WriteCrashLog(crashLogPath, $"[{DateTime.UtcNow:O}] MEMORY WARNING | {GetMemoryStats()}");
                }
                catch { /* best-effort */ }
            }
        });

        var server = TendrilServer.Create(filteredArgs);

        if (usePhotino)
        {
            var window = new DesktopWindow(server)
                .Title("Ivy Tendril")
                .Size(1400, 900);

            return window.Run();
        }
        else
        {
            await server.RunAsync();
            return 0;
        }
    }

    private static string GetCrashLogPath()
    {
        var tendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        var logDir = !string.IsNullOrEmpty(tendrilHome) ? tendrilHome : Path.GetTempPath();
        return Path.Combine(logDir, "crash.log");
    }

    internal static string CrashLogPath { get; } = GetCrashLogPath();

    internal static void WriteCrashLog(string message) => WriteCrashLog(CrashLogPath, message);

    private static void WriteCrashLog(string path, string message)
    {
        try
        {
            File.AppendAllText(path, message + Environment.NewLine);
        }
        catch
        {
            // Last-resort: don't let logging itself crash the process
        }
    }

    private static string GetMemoryStats()
    {
        try
        {
            using var proc = Process.GetCurrentProcess();
            var workingSet = proc.WorkingSet64;
            var gcHeap = GC.GetTotalMemory(false);
            return $"WorkingSet={workingSet / (1024 * 1024)}MB, GCHeap={gcHeap / (1024 * 1024)}MB, Gen0={GC.CollectionCount(0)}, Gen1={GC.CollectionCount(1)}, Gen2={GC.CollectionCount(2)}";
        }
        catch
        {
            return "Memory stats unavailable";
        }
    }
}
