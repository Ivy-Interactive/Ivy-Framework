using System.ComponentModel;
using System.Diagnostics;
using Spectre.Console.Cli;

namespace Ivy.IvyML.Console;

/// <summary>
/// Serves an IvyML file as a live Ivy application. Unlike <see cref="DrawCommand"/>, which captures
/// a single frame and exits, this hosts the widget tree until the process is stopped (Ctrl+C).
/// </summary>
public sealed class RunCommand : AsyncCommand<RunCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<PATH>")]
        [Description("Path to an IvyML file.")]
        public string FilePath { get; init; } = "";

        [CommandOption("-p|--port <PORT>")]
        [Description("Port to listen on. If it is taken, the next free port is used.")]
        [DefaultValue(ServerArgs.DefaultPort)]
        public int Port { get; init; }

        [CommandOption("-b|--browse")]
        [Description("Open the app in the default browser once it is listening.")]
        [DefaultValue(false)]
        public bool Browse { get; init; }

        [CommandOption("--theme <PATH|NAME>")]
        [Description("Theme YAML file, or a built-in theme name. Defaults to a theme.yaml next to "
                     + "the IvyML file, if there is one. Pass 'none' to ignore that.")]
        public string? Theme { get; init; }

        [CommandOption("--no-theme")]
        [Description("Turn theming off: ignore --theme and any neighbouring theme.yaml.")]
        [DefaultValue(false)]
        public bool NoTheme { get; init; }

        [CommandOption("--parent-pid <PID>")]
        [Description("Shut down when the process with this id exits. Used by tools that host the "
                     + "server as a child process so a hard kill cannot orphan it.")]
        public int? ParentPid { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(settings.FilePath))
        {
            System.Console.Error.WriteLine("Error: Provide a path to an IvyML file.");
            return 1;
        }

        if (!File.Exists(settings.FilePath))
        {
            System.Console.Error.WriteLine($"Error: File not found: {settings.FilePath}");
            return 1;
        }

        if (settings.Port is <= 0 or > 65535)
        {
            System.Console.Error.WriteLine("Error: Port must be between 1 and 65535.");
            return 1;
        }

        var ivyml = await File.ReadAllTextAsync(settings.FilePath, ct);

        var validation = IvyMLValidator.Validate(ivyml);
        if (!validation.IsValid)
        {
            System.Console.Error.WriteLine($"Error: {validation.ErrorMessage}");
            return 1;
        }

        var widget = validation.Widget!;

        Theme? theme;
        try
        {
            theme = ThemeResolver.Resolve(settings.Theme, settings.FilePath, settings.NoTheme);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or YamlDotNet.Core.YamlException)
        {
            System.Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }

        var server = new Server(new ServerArgs
        {
            Port = settings.Port,
            // Hosting a wireframe is a throwaway, side-by-side activity, so never fail on a busy
            // port -- walk up until a free one is found.
            FindAvailablePort = true,
            Browse = settings.Browse
        });

        if (theme is not null)
            server.UseTheme(theme);

        server.AddApp(new AppDescriptor
        {
            Id = AppIds.Default,
            Title = Path.GetFileName(settings.FilePath),
            ViewFunc = _ => widget,
            Group = [],
            IsVisible = true
        });

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Held for the lifetime of the call: disposing the Process drops the Exited subscription.
        using var parent = WatchParent(settings.ParentPid, cts);

        await server.RunAsync(cts);

        return 0;
    }

    /// <summary>
    /// Cancels <paramref name="cts"/> when the given process exits, so a host that is killed
    /// abruptly -- Ctrl+C, a stopped debugger, a crash -- cannot leave this server running and
    /// holding its port. Returns the watched process so the caller can keep it alive.
    /// </summary>
    private static Process? WatchParent(int? parentPid, CancellationTokenSource cts)
    {
        if (parentPid is not { } pid)
            return null;

        Process parent;
        try
        {
            parent = Process.GetProcessById(pid);
        }
        catch (ArgumentException)
        {
            // Already gone before we got here; there is nothing to serve.
            cts.Cancel();
            return null;
        }

        parent.EnableRaisingEvents = true;
        parent.Exited += (_, _) => cts.Cancel();

        // Covers the parent exiting between GetProcessById and the subscription above.
        if (parent.HasExited)
            cts.Cancel();

        return parent;
    }
}
