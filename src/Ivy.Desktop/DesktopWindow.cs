using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Photino.NET;

namespace Ivy.Desktop;

public class DesktopWindow(Server server)
{
    private string _title = "Ivy App";
    private int _width = 1280;
    private int _height = 800;
    private bool _resizable = true;
    private bool _topMost = false;
    private bool _useDpiScaling = true;
    private bool _center = true;
    private bool _devTools = false;
    private Assembly? _iconAssembly = null;
    private string? _iconResourceName = null;

    public DesktopWindow Title(string title) { _title = title; return this; }
    public DesktopWindow Size(int width, int height) { _width = width; _height = height; return this; }
    public DesktopWindow Resizable(bool resizable = true) { _resizable = resizable; return this; }
    public DesktopWindow TopMost(bool topMost = true) { _topMost = topMost; return this; }
    public DesktopWindow UseDpiScaling(bool enabled = true) { _useDpiScaling = enabled; return this; }
    public DesktopWindow Center(bool center = true) { _center = center; return this; }
    public DesktopWindow UseDevTools(bool enabled = true) { _devTools = enabled; return this; }

    /// <summary>
    /// Sets the window icon from an embedded resource.
    /// The resource must be embedded in the assembly containing <paramref name="typeInAssembly"/>.
    /// </summary>
    /// <param name="typeInAssembly">A type from the assembly that contains the embedded icon resource.</param>
    /// <param name="resourceName">The embedded resource name (e.g. "MyApp.icon.ico").</param>
    public DesktopWindow Icon(Type typeInAssembly, string resourceName)
    {
        _iconAssembly = typeInAssembly.Assembly;
        _iconResourceName = resourceName;
        return this;
    }

    public int Run()
    {
        try
        {
            return RunInternal();
        }
        catch (Exception ex)
        {
            ShowErrorDialog(ex);
            return 1;
        }
    }

    private int RunInternal()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

        var port = server.Args.Port;
        var url = $"http://localhost:{port}";
        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts);

        if (!CheckIfPortIsListening(port).GetAwaiter().GetResult())
        {
            ShowErrorDialog(new InvalidOperationException(
                $"Unable to connect to the Ivy server at {url}. The server may have failed to start."));
            return 1;
        }

        var windowWidth = _width;
        var windowHeight = _height;

        if (_useDpiScaling)
        {
            var scalingFactor = DpiDetector.GetSystemScalingFactor();
            windowWidth = (int)(_width * scalingFactor);
            windowHeight = (int)(_height * scalingFactor);
        }

        var window = new PhotinoWindow() { LogVerbosity = 0 };
        window
            .SetUseOsDefaultSize(false)
            .SetSize(windowWidth, windowHeight)
            .SetTitle(_title)
            .SetResizable(_resizable)
            .SetTopMost(_topMost)
            .SetJavascriptClipboardAccessEnabled(false)
            .SetDevToolsEnabled(_devTools)
            .SetIgnoreCertificateErrorsEnabled(true)
            .SetWebSecurityEnabled(false);

        if (_iconAssembly != null && _iconResourceName != null)
        {
            var iconPath = ExtractEmbeddedIcon(_iconAssembly, _iconResourceName);
            if (iconPath != null) window.SetIconFile(iconPath);
        }

        if (_center) window.Center();

        window.Load(new Uri(url));
        window.WaitForClose();

        cts.Cancel();
        serverTask.GetAwaiter().GetResult();

        return 0;
    }

    private void ShowErrorDialog(Exception ex)
    {
        try
        {
            var errorHtml = $"""
                <!DOCTYPE html>
                <html><body style="font-family:system-ui;padding:2rem;background:#1e1e2e;color:#cdd6f4">
                <h2 style="color:#f38ba8">Application Error</h2>
                <p>{WebUtility.HtmlEncode(ex.Message)}</p>
                <pre style="background:#313244;padding:1rem;border-radius:8px;overflow:auto;font-size:0.85rem;color:#a6adc8">{WebUtility.HtmlEncode(ex.ToString())}</pre>
                </body></html>
                """;

            var tempHtmlPath = Path.Combine(Path.GetTempPath(), $"ivy_error_{Guid.NewGuid():N}.html");
            File.WriteAllText(tempHtmlPath, errorHtml);

            var errorWindow = new PhotinoWindow() { LogVerbosity = 0 };
            errorWindow
                .SetUseOsDefaultSize(false)
                .SetSize(700, 500)
                .SetTitle($"{_title} - Error")
                .Center()
                .Load(new Uri(tempHtmlPath));
            errorWindow.WaitForClose();

            try { File.Delete(tempHtmlPath); } catch { }
        }
        catch
        {
            Console.Error.WriteLine($"Fatal error: {ex}");
        }
    }

    private static string? ExtractEmbeddedIcon(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        var tempPath = Path.Combine(Path.GetTempPath(), $"ivy_icon_{Path.GetFileName(resourceName)}");
        using var fileStream = File.Create(tempPath);
        stream.CopyTo(fileStream);
        return tempPath;
    }

    private static async Task<bool> CheckIfPortIsListening(int port, int maxAttempts = 10)
    {
        var delayMs = 1000;
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                bool isListening = IPGlobalProperties
                    .GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Any(endpoint => endpoint.Port == port);
                if (isListening) return true;
            }
            catch { 
                // Ignore
            }
            if (i == maxAttempts - 1) return false;
            await Task.Delay(delayMs);
        }
        return false;
    }
}
