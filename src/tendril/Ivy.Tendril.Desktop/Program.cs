using Ivy.Desktop;
using Ivy.Tendril;
using Ivy.Tendril.Database;
using Velopack;

namespace Ivy.Tendril.Desktop;

public class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        VelopackApp.Build().Run();

        // Handle database CLI commands before starting the server/GUI
        var dbExitCode = DatabaseCommands.Handle(args);
        if (dbExitCode >= 0)
            return dbExitCode;

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Console.WriteLine($"[FATAL] Unhandled exception: {e.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Console.WriteLine($"[FATAL] Unobserved task exception: {e.Exception}");
            e.SetObserved();
        };

        var server = TendrilServer.Create(args);

        var window = new DesktopWindow(server)
            .Title("Ivy Tendril — Multi-host AI Tool")
            .Size(1400, 900);

        return window.Run();
    }
}