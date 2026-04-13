using System.Diagnostics;
using System.Runtime.InteropServices;
using Ivy.Desktop;
using Ivy.Helpers;
using Ivy.Tendril.Commands;
using Ivy.Tendril.Database;
using Ivy.Tendril.Services;
using Velopack;

namespace Ivy.Tendril;

public class Program
{
    // Native console control handler to detect why the process is being killed.
    // This fires BEFORE .NET's ProcessExit and catches CTRL_CLOSE_EVENT which
    // .NET's AppDomain.ProcessExit may not see (Windows force-kills after 5s).
    private delegate bool ConsoleCtrlHandlerDelegate(int ctrlType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handler, bool add);

    // Must be a static field to prevent GC from collecting the delegate
    private static ConsoleCtrlHandlerDelegate? _consoleCtrlHandler;

    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        VelopackApp.Build().Run();

        var fileName = Path.GetFileNameWithoutExtension(Environment.ProcessPath ?? "");
        bool isTool = fileName.Equals("tendril", StringComparison.OrdinalIgnoreCase);
        bool forceDesktop = args.Contains("--desktop") || args.Contains("--photino");
        bool forceWeb = args.Contains("--web");

        bool useDesktop = (isTool || forceDesktop) && !forceWeb;

        var filteredArgs = args.Where(a => a != "--desktop" && a != "--photino" && a != "--web").ToArray();

        // Handle database CLI commands before starting the server/GUI
        var dbExitCode = DatabaseCommands.Handle(filteredArgs);
        if (dbExitCode >= 0)
            return dbExitCode;

        var pwExitCode = PromptwareCommands.Handle(filteredArgs);
        if (pwExitCode >= 0)
            return pwExitCode;

        var doctorExitCode = DoctorCommand.Handle(filteredArgs);
        if (doctorExitCode >= 0)
            return doctorExitCode;

        var hashExitCode = HashPasswordCommand.Handle(filteredArgs);
        if (hashExitCode >= 0)
            return hashExitCode;

        CrashLog.Write($"[{DateTime.UtcNow:O}] Tendril starting (PID {Environment.ProcessId}) | {GetMemoryStats()}");

        // Install native console control handler FIRST — this catches CTRL_CLOSE_EVENT
        // (console window closed), CTRL_C_EVENT, CTRL_BREAK_EVENT, CTRL_LOGOFF_EVENT,
        // and CTRL_SHUTDOWN_EVENT. Logging here tells us exactly WHY the process is dying.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _consoleCtrlHandler = ctrlType =>
            {
                var name = ctrlType switch
                {
                    0 => "CTRL_C_EVENT",
                    1 => "CTRL_BREAK_EVENT",
                    2 => "CTRL_CLOSE_EVENT",
                    5 => "CTRL_LOGOFF_EVENT",
                    6 => "CTRL_SHUTDOWN_EVENT",
                    _ => $"UNKNOWN({ctrlType})"
                };
                CrashLog.Write(
                    $"[{DateTime.UtcNow:O}] ConsoleCtrlHandler: {name} (PID {Environment.ProcessId}) | {GetMemoryStats()}");
                return false; // Let default handling proceed
            };
            SetConsoleCtrlHandler(_consoleCtrlHandler, true);
        }

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var msg = $"[{DateTime.UtcNow:O}] FATAL UnhandledException (IsTerminating={e.IsTerminating}) | {GetMemoryStats()}\n  {e.ExceptionObject}";
            Console.WriteLine($"[FATAL] Unhandled exception: {e.ExceptionObject}");
            CrashLog.Write(msg);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            var msg = $"[{DateTime.UtcNow:O}] FATAL UnobservedTaskException | {GetMemoryStats()}\n  {e.Exception}";
            Console.WriteLine($"[FATAL] Unobserved task exception: {e.Exception}");
            CrashLog.Write(msg);
            e.SetObserved();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            CrashLog.Write($"[{DateTime.UtcNow:O}] ProcessExit event fired (PID {Environment.ProcessId}) | {GetMemoryStats()}");
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
                        CrashLog.Write($"[{DateTime.UtcNow:O}] MEMORY WARNING | {GetMemoryStats()}");
                }
                catch { /* best-effort */ }
            }
        });

        if (useDesktop && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IVY_TLS")))
        {
            // WKWebView on macOS does not support ignoring invalid developer certificates natively.
            // When running bundled in the Desktop container, simply fall back to HTTP to bypass cert issues.
            Environment.SetEnvironmentVariable("IVY_TLS", "0");
        }

        var server = TendrilServer.Create(filteredArgs);

        if (useDesktop)
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

    internal static void WriteCrashLog(string message) => CrashLog.Write(message);

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
