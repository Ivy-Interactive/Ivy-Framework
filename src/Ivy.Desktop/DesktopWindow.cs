using System.Globalization;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rustino.NET;

namespace Ivy.Desktop;

public class DesktopWindow(Server server)
{
    private string _title = Assembly.GetEntryAssembly()?.GetName().Name ?? "Ivy App";
    private int _width = 1280;
    private int _height = 800;
    private bool _resizable = true;
    private bool _topMost = false;
    private bool _useDpiScaling = true;
    private bool _center = true;
    private bool _devTools = false;
    private Assembly? _iconAssembly = null;
    private string? _iconResourceName = null;
    private bool _iconSet = false;
    private string? _iconFilePath = null;
    private (int X, int Y)? _position;
    private (int W, int H)? _minSize = (600, 400);
    private (int W, int H)? _maxSize;
    private bool _chromeless;
    private bool _transparent;
    private bool _maximized;
    private bool _fullscreen;
    private (byte R, byte G, byte B, byte A)? _backgroundColor;
    private string? _userAgent;
    private bool _zoomHotkeys;
    private bool _mediaAutoplay = true;
    private readonly List<string> _initScripts = new();
    private DesktopMenu? _menu;
    private Action<DesktopWindow>? _onReady;
    private RustinoWindow? _window;
    private string? _splashImagePath;
    private Assembly? _splashImageAssembly;
    private string? _splashImageResourceName;
    private int _splashWidth = 400;
    private int _splashHeight = 300;
    private string? _appId;

    public DesktopWindow Title(string title) { _title = title; return this; }
    public DesktopWindow Size(int width, int height) { _width = width; _height = height; return this; }
    public DesktopWindow Resizable(bool resizable = true) { _resizable = resizable; return this; }
    public DesktopWindow TopMost(bool topMost = true) { _topMost = topMost; return this; }
    public DesktopWindow UseDpiScaling(bool enabled = true) { _useDpiScaling = enabled; return this; }
    public DesktopWindow Center(bool center = true) { _center = center; return this; }
    public DesktopWindow UseDevTools(bool enabled = true) { _devTools = enabled; return this; }
    public DesktopWindow Position(int x, int y) { _position = (x, y); return this; }
    public DesktopWindow MinSize(int width, int height) { _minSize = (width, height); return this; }
    public DesktopWindow MaxSize(int width, int height) { _maxSize = (width, height); return this; }
    public DesktopWindow Chromeless(bool chromeless = true) { _chromeless = chromeless; return this; }
    public DesktopWindow Transparent(bool transparent = true) { _transparent = transparent; return this; }
    public DesktopWindow Maximized(bool maximized = true) { _maximized = maximized; return this; }
    public DesktopWindow Fullscreen(bool fullscreen = true) { _fullscreen = fullscreen; return this; }
    public DesktopWindow BackgroundColor(byte r, byte g, byte b, byte a = 255) { _backgroundColor = (r, g, b, a); return this; }
    public DesktopWindow UserAgent(string userAgent) { _userAgent = userAgent; return this; }
    public DesktopWindow ZoomHotkeys(bool enabled = true) { _zoomHotkeys = enabled; return this; }
    public DesktopWindow MediaAutoplay(bool enabled = true) { _mediaAutoplay = enabled; return this; }
    public DesktopWindow InitScript(string js) { _initScripts.Add(js); return this; }
    public DesktopWindow Menu(DesktopMenu menu) { _menu = menu; return this; }
    public DesktopWindow OnReady(Action<DesktopWindow> callback) { _onReady = callback; return this; }
    public DesktopWindow AppId(string appId) { _appId = appId; return this; }

    public DesktopWindow Splash(string imagePath, int width = 400, int height = 300)
    {
        _splashImagePath = imagePath;
        _splashWidth = width;
        _splashHeight = height;
        return this;
    }

    public DesktopWindow Splash(Type typeInAssembly, string resourceName, int width = 400, int height = 300)
    {
        _splashImageAssembly = typeInAssembly.Assembly;
        _splashImageResourceName = resourceName;
        _splashWidth = width;
        _splashHeight = height;
        return this;
    }

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
        _iconSet = true;
        return this;
    }

    public DesktopWindow IconFile(string path)
    {
        _iconFilePath = path;
        _iconSet = true;
        return this;
    }

    // ── Events ───────────────────────────────────────────────────────────

    public event System.EventHandler<System.ComponentModel.CancelEventArgs>? WindowClosing;
    public event System.EventHandler? WindowClosed;
    public event System.EventHandler<DesktopSizeEventArgs>? SizeChanged;
    public event System.EventHandler<DesktopPointEventArgs>? LocationChanged;
    public event System.EventHandler<bool>? FocusChanged;
    public event System.EventHandler<string>? WebMessageReceived;
    public event System.EventHandler<DesktopPageLoadEventArgs>? PageLoaded;
    public event System.EventHandler<DesktopNavigationEventArgs>? Navigating;
    public event System.EventHandler<string>? MenuItemClicked;
    public event System.EventHandler? TrayIconClicked;

    // ── Post-run properties ──────────────────────────────────────────────

    public bool IsMinimized => _window?.IsMinimized ?? false;
    public bool IsMaximized => _window?.IsMaximized ?? false;
    public bool IsFullscreen => _window?.IsFullscreen ?? false;
    public (int X, int Y) GetPosition() => _window?.GetPosition() ?? (0, 0);
    public (int Width, int Height) GetSize()
    {
        var size = _window?.GetSize() ?? (_width, _height);
        return NormalizeSizeFromNative(size.Width, size.Height);
    }

    // ── Post-run window state ────────────────────────────────────────────

    public void Minimize() => _window?.Minimize();
    public void Maximize() => _window?.Maximize();
    public void Restore() => _window?.Restore();
    public void SetFullscreen(bool fullscreen) => _window?.SetFullscreen(fullscreen);
    public void SetVisible(bool visible) => _window?.SetVisible(visible);
    public void Focus() => _window?.Focus();
    public void Close() => _window?.Close();

    // ── Post-run webview ─────────────────────────────────────────────────

    public void ExecuteScript(string js) => _window?.ExecuteScript(js);
    public void SendWebMessage(string message) => _window?.SendWebMessage(message);
    public void SetZoom(double factor) => _window?.SetZoom(factor);

    // ── Post-run menus ───────────────────────────────────────────────────

    public void SetMenu(DesktopMenu menu) => _window?.SetMenu(menu.ToRustinoMenu());
    public void RemoveMenu() => _window?.RemoveMenu();
    public void ShowContextMenu(DesktopMenu menu) => _window?.ShowContextMenu(menu.ToRustinoMenu());

    // ── Post-run tray icon ───────────────────────────────────────────────

    public void SetTrayIcon(string iconPath, string? tooltip = null, DesktopMenu? menu = null)
        => _window?.SetTrayIcon(iconPath, tooltip, menu?.ToRustinoMenu());

    public void SetTrayIcon(Type typeInAssembly, string resourceName, string? tooltip = null, DesktopMenu? menu = null)
    {
        var iconPath = ExtractEmbeddedIcon(typeInAssembly.Assembly, resourceName);
        if (iconPath != null)
            _window?.SetTrayIcon(iconPath, tooltip, menu?.ToRustinoMenu());
    }

    public void RemoveTrayIcon() => _window?.RemoveTrayIcon();

    // ── Post-run badge ───────────────────────────────────────────────────

    public void SetBadgeCount(int? count, string? background = null, string? foreground = null) => _window?.SetBadgeCount(count, background, foreground);
    public void ClearBadge() => _window?.ClearBadge();

    // ── Post-run dialogs ─────────────────────────────────────────────────

    public string[]? ShowOpenFileDialog(
        string? title = null, string? defaultPath = null,
        DesktopFileFilter[]? filters = null, bool multiSelect = false)
    {
        var rf = filters?.Select(f => new FileFilter(f.Name, f.Extensions)).ToArray();
        return _window?.ShowOpenFileDialog(title, defaultPath, rf, multiSelect);
    }

    public string? ShowSaveFileDialog(
        string? title = null, string? defaultPath = null,
        DesktopFileFilter[]? filters = null)
    {
        var rf = filters?.Select(f => new FileFilter(f.Name, f.Extensions)).ToArray();
        return _window?.ShowSaveFileDialog(title, defaultPath, rf);
    }

    public string[]? ShowSelectFolderDialog(
        string? title = null, string? defaultPath = null, bool multiSelect = false)
        => _window?.ShowSelectFolderDialog(title, defaultPath, multiSelect);

    // ── Post-run monitors ────────────────────────────────────────────────

    public DesktopMonitorInfo[] GetMonitors()
    {
        var monitors = _window?.GetMonitors();
        return monitors?
            .Select(m => new DesktopMonitorInfo(m.Name, m.X, m.Y, m.Width, m.Height, m.ScaleFactor, m.IsPrimary))
            .ToArray() ?? [];
    }

    public DesktopMonitorInfo? GetCurrentMonitor()
    {
        var m = _window?.GetCurrentMonitor();
        return m == null ? null : new DesktopMonitorInfo(m.Name, m.X, m.Y, m.Width, m.Height, m.ScaleFactor, m.IsPrimary);
    }

    // ── Notifications (static) ───────────────────────────────────────────

    public static bool ShowNotification(string title, string body, string? iconPath = null, string? appId = null)
        => RustinoWindow.ShowNotification(title, body, iconPath, appId);

    // ── Run ──────────────────────────────────────────────────────────────

    public int Run()
    {
        if (OperatingSystem.IsWindows() &&
            Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            var result = 0;
            var thread = new Thread(() => result = RunCore());
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return result;
        }
        return RunCore();
    }

    private int RunCore()
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

        // Force HTTPS/TLS on Windows and macOS if not explicitly set
        var ivyTlsEnv = Environment.GetEnvironmentVariable("IVY_TLS");
        if (string.IsNullOrEmpty(ivyTlsEnv) && (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()))
        {
            Environment.SetEnvironmentVariable("IVY_TLS", "true");
            ivyTlsEnv = "true";
        }

        // Configure Kestrel HTTPS defaults using UseWebApplicationBuilder
        server.UseWebApplicationBuilder(builder =>
        {
            var currentIvyTls = Environment.GetEnvironmentVariable("IVY_TLS");
            var useTls = !string.IsNullOrEmpty(currentIvyTls) && currentIvyTls.ToLowerInvariant() is "1" or "true" or "yes" or "on";
            if (useTls)
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        httpsOptions.ServerCertificate = CertificateHelper.GetOrCreateCertificate();
                    });
                });
            }
        });

        server.Services.AddSingleton(this);

        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts);

        // Read port AFTER RunAsync returns — the synchronous portion (including
        // FindAvailablePort) completes before the first await, so Args.Port is
        // already updated to the actual port the server will bind to.
        var port = server.Args.Port;
        var useTls = !string.IsNullOrEmpty(ivyTlsEnv)
            ? ivyTlsEnv.ToLowerInvariant() is "1" or "true" or "yes" or "on"
            : OperatingSystem.IsWindows() || OperatingSystem.IsMacOS(); // Default to true on Windows and macOS
        var url = $"{(useTls ? "https" : "http")}://localhost:{port}";

        // Show splash while the server starts
        RustinoSplashscreen? splash = null;
        try { splash = CreateSplashscreen(); } catch { }

        // Wait for the server to become ready (or detect early failure).
        WaitForServerReady(serverTask, url);

        splash?.Dispose();
        splash = null;

        string? tempLoadingPath = null;
        try
        {
            _window = CreateWindow();
            WireEvents();

            var loadingHtml = GetLoadingHtml(url);
            tempLoadingPath = Path.Combine(Path.GetTempPath(), $"ivy_loading_{Guid.NewGuid():N}.html");
            File.WriteAllText(tempLoadingPath, loadingHtml);
            _window.Load(new Uri(tempLoadingPath));

            if (_menu != null) _window.SetMenu(_menu.ToRustinoMenu());

            _onReady?.Invoke(this);

            _window.WaitForClose();
        }
        finally
        {
            _window = null;

            if (tempLoadingPath != null)
                try { File.Delete(tempLoadingPath); } catch { }

            cts.Cancel();
            serverTask.GetAwaiter().GetResult();
        }

        return 0;
    }

    private RustinoWindow CreateWindow()
    {
        var windowWidth = _width;
        var windowHeight = _height;

        if (_useDpiScaling)
        {
            var scalingFactor = DpiDetector.GetSystemScalingFactor();
            windowWidth = (int)(_width * scalingFactor);
            windowHeight = (int)(_height * scalingFactor);
        }

        var window = new RustinoWindow() { LogVerbosity = 0 };
        window
            .SetUseOsDefaultSize(false)
            .SetSize(windowWidth, windowHeight)
            .SetTitle(_title)
            .SetResizable(_resizable)
            .SetTopMost(_topMost)
            .SetJavascriptClipboardAccessEnabled(true)
            .SetDevToolsEnabled(_devTools)
            .SetIgnoreCertificateErrorsEnabled(true)
            .SetWebSecurityEnabled(false);

        if (_position is { } pos) window.SetPosition(pos.X, pos.Y);
        if (_minSize is { } min) window.SetMinSize(min.W, min.H);
        if (_maxSize is { } max) window.SetMaxSize(max.W, max.H);
        if (_chromeless) window.SetChromeless(true);
        if (_transparent) window.SetTransparent(true);
        if (_maximized) window.SetMaximized(true);
        if (_fullscreen) window.SetFullscreen(true);
        if (_backgroundColor is { } bg) window.SetBackgroundColor(bg.R, bg.G, bg.B, bg.A);
        if (_userAgent != null) window.SetUserAgent(_userAgent);
        if (_zoomHotkeys) window.SetZoomHotkeysEnabled(true);
        if (!_mediaAutoplay) window.SetMediaAutoplayEnabled(false);
        foreach (var script in _initScripts) window.AddInitScript(script);

        if (_appId != null) window.SetApplicationId(_appId);

        if (_iconFilePath != null)
        {
            window.SetIconFile(_iconFilePath);
        }
        else if (_iconSet && _iconAssembly != null && _iconResourceName != null)
        {
            var iconPath = ExtractEmbeddedIcon(_iconAssembly, _iconResourceName);
            if (iconPath != null) window.SetIconFile(iconPath);
        }
        else if (!_iconSet)
        {
            var iconPath = ExtractEmbeddedIcon(
                typeof(DesktopWindow).Assembly, "Ivy.Desktop.ivy.ico");
            if (iconPath != null) window.SetIconFile(iconPath);
        }

        if (_center) window.Center();

        return window;
    }

    private void WireEvents()
    {
        if (_window == null) return;

        _window.WindowClosing += (_, e) => WindowClosing?.Invoke(this, e);
        _window.WindowClosed += (_, _) => WindowClosed?.Invoke(this, EventArgs.Empty);
        _window.SizeChanged += (_, e) =>
        {
            var size = NormalizeSizeFromNative(e.Width, e.Height);
            SizeChanged?.Invoke(this, new DesktopSizeEventArgs(size.Width, size.Height));
        };
        _window.LocationChanged += (_, e) => LocationChanged?.Invoke(this, new DesktopPointEventArgs(e.X, e.Y));
        _window.FocusChanged += (_, focused) => FocusChanged?.Invoke(this, focused);
        _window.WebMessageReceived += (_, msg) => WebMessageReceived?.Invoke(this, msg);
        _window.PageLoaded += (_, e) => PageLoaded?.Invoke(this, new DesktopPageLoadEventArgs(e.IsStarted, e.Url));
        _window.Navigating += (_, e) =>
        {
            var args = new DesktopNavigationEventArgs(e.Url);
            Navigating?.Invoke(this, args);

            if (!args.Cancel && Uri.TryCreate(e.Url, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    if (uri.Host != "localhost" && uri.Host != "127.0.0.1")
                    {
                        args.Cancel = true;
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = e.Url,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[Ivy.Desktop] Failed to open external link: {ex.Message}");
                        }
                    }
                }
            }

            e.Cancel = args.Cancel;
        };
        _window.MenuItemClicked += (_, id) => MenuItemClicked?.Invoke(this, id);
        _window.TrayIconClicked += (_, _) => TrayIconClicked?.Invoke(this, EventArgs.Empty);
    }

    private static string GetLoadingHtml(string url)
    {
        return """
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="utf-8"><title>Loading</title>
              <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/geist@1/dist/fonts/geist-sans/style.min.css">
            </head>
            <body style="margin:0;background:#ffffff;color:#000000;font-family:'Geist',system-ui,sans-serif;display:flex;align-items:center;justify-content:center;height:100vh;flex-direction:column">
              <div id="loader" style="display:none;align-items:center;flex-direction:column">
                <div style="width:40px;height:40px;border:3px solid #d1d5db;border-top-color:#00cc92;border-radius:50%;animation:spin 0.8s linear infinite;margin-bottom:1.5rem"></div>
                <p id="status" style="color:#8f8f8f;font-size:0.95rem">Connecting to server...</p>
              </div>
              <style>@keyframes spin{to{transform:rotate(360deg)}}</style>
              <script>
                var serverUrl = '__SERVER_URL__';
                var elapsed = 0;
                var shown = false;
                function poll() {
                  fetch(serverUrl, { mode: 'no-cors' }).then(function() {
                    window.location.href = serverUrl;
                  }).catch(function() {
                    elapsed += 500;
                    if (!shown && elapsed >= 4000) {
                      shown = true;
                      document.getElementById('loader').style.display = 'flex';
                    }
                    if (elapsed >= 30000) {
                      document.getElementById('status').textContent = 'Unable to connect to the server. It may have failed to start.';
                      document.getElementById('status').style.color = '#dd5860';
                      return;
                    }
                    setTimeout(poll, 500);
                  });
                }
                poll();
              </script>
            </body>
            </html>
            """.Replace("__SERVER_URL__", url);
    }

    private void ShowErrorDialog(Exception ex)
    {
        try
        {
            var errorHtml = $"""
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset="utf-8"><title>Error</title>
                  <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/geist@1/dist/fonts/geist-sans/style.min.css">
                </head>
                <body style="font-family:'Geist',system-ui,sans-serif;padding:2rem;background:#ffffff;color:#000000">
                <h2 style="color:#dd5860">Application Error</h2>
                <p>{WebUtility.HtmlEncode(ex.Message)}</p>
                <pre style="background:#f8f8f8;padding:1rem;border-radius:8px;overflow:auto;font-size:0.85rem;color:#8f8f8f;border:1px solid #d1d5db">{WebUtility.HtmlEncode(ex.ToString())}</pre>
                </body></html>
                """;

            var tempHtmlPath = Path.Combine(Path.GetTempPath(), $"ivy_error_{Guid.NewGuid():N}.html");
            File.WriteAllText(tempHtmlPath, errorHtml);

            var errorWindow = new RustinoWindow() { LogVerbosity = 0 };
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

    /// <summary>
    /// Blocks until the server responds to HTTP requests, or throws if the server
    /// task faults, completes early (e.g. missing secrets), or times out.
    /// </summary>
    private static void WaitForServerReady(Task serverTask, string url, int timeoutMs = 30_000)
    {
        var healthUrl = $"{url}/ivy/health";
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) };
        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (serverTask.IsFaulted)
                throw serverTask.Exception!.GetBaseException();

            if (serverTask.IsCompleted)
                throw new InvalidOperationException(
                    "The Ivy server exited before it started accepting requests. " +
                    "Check the console output for errors (missing secrets, port conflicts, etc.).");

            try
            {
                using var response = http.GetAsync(healthUrl).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // Server not ready yet — keep polling.
            }

            Thread.Sleep(250);
        }

        throw new TimeoutException(
            $"The Ivy server did not respond within {timeoutMs / 1000} seconds at {healthUrl}.");
    }

    private RustinoSplashscreen? CreateSplashscreen()
    {
        if (_splashImagePath != null)
            return new RustinoSplashscreen(_splashImagePath, _splashWidth, _splashHeight);

        if (_splashImageAssembly != null && _splashImageResourceName != null)
        {
            using var stream = _splashImageAssembly.GetManifestResourceStream(_splashImageResourceName);
            if (stream != null)
                return RustinoSplashscreen.FromImage(stream, _splashWidth, _splashHeight);
        }

        return null;
    }

    private static string? ExtractEmbeddedIcon(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            Console.Error.WriteLine($"[Ivy.Desktop] Manifest resource '{resourceName}' was NOT found in assembly {assembly.FullName}!");
            return null;
        }

        var tempPath = Path.Combine(Path.GetTempPath(), $"ivy_icon_{Path.GetFileName(resourceName)}");
        using var fileStream = File.Create(tempPath);
        stream.CopyTo(fileStream);
        return tempPath;
    }

    private (int Width, int Height) NormalizeSizeFromNative(int width, int height)
    {
        if (!OperatingSystem.IsMacOS() || !_useDpiScaling)
            return (width, height);

        var scale = DpiDetector.GetSystemScalingFactor();
        if (scale <= 1.001)
            return (width, height);

        var logicalWidth = (int)Math.Round(width / scale, MidpointRounding.AwayFromZero);
        var logicalHeight = (int)Math.Round(height / scale, MidpointRounding.AwayFromZero);
        return (logicalWidth, logicalHeight);
    }
}
