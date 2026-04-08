using Ivy.Desktop;
using Ivy.Tendril;
using Ivy.Tendril.Database;
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

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Console.WriteLine($"[FATAL] Unhandled exception: {e.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Console.WriteLine($"[FATAL] Unobserved task exception: {e.Exception}");
            e.SetObserved();
        };

        var server = TendrilServer.Create(filteredArgs);

        if (usePhotino)
        {
            var window = new DesktopWindow(server)
                .Title("Ivy Tendril — Multi-host AI Tool")
                .Size(1400, 900);

            return window.Run();
        }
        else
        {
            await server.RunAsync();
            return 0;
        }
    }
}
