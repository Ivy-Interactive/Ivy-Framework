using System.Reflection;
using Ivy.Core.Server;
using Microsoft.Playwright;
using SkiaSharp;

namespace Ivy.IvyML;

public class IvyScreenshotService
{
    private static readonly Dictionary<string, SKEncodedImageFormat> FormatMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = SKEncodedImageFormat.Png,
        [".jpg"] = SKEncodedImageFormat.Jpeg,
        [".jpeg"] = SKEncodedImageFormat.Jpeg,
        [".webp"] = SKEncodedImageFormat.Webp,
    };

    private static readonly Lazy<string> DebugOverlayScript = new(() =>
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Ivy.IvyML.DebugOverlay.js");
        if (stream is null) return "";
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    });

    public static bool IsSupportedFormat(string ext) => FormatMap.ContainsKey(ext);

    private static readonly BrowserTypeLaunchOptions LaunchOptions = new() { Headless = true };

    /// <summary>
    /// Launches headless Chromium, downloading it first if Playwright has not been provisioned on
    /// this machine. Without this the very first <c>ivyml draw</c> on a clean checkout fails with
    /// Playwright's "Executable doesn't exist" message and a shell command to run by hand.
    /// </summary>
    private static async Task<IBrowser> LaunchChromiumAsync(IPlaywright playwright, CancellationToken ct)
    {
        try
        {
            return await playwright.Chromium.LaunchAsync(LaunchOptions);
        }
        catch (PlaywrightException ex) when (IsBrowserMissing(ex))
        {
            Console.Error.WriteLine("Chromium is not installed for Playwright. Downloading it now (one-time setup)...");

            var exitCode = await Task.Run(() => Microsoft.Playwright.Program.Main(["install", "chromium"]), ct);
            if (exitCode != 0)
                throw new PlaywrightException(
                    $"Automatic Chromium download failed (exit code {exitCode}). "
                    + "Install it manually with: pwsh playwright.ps1 install chromium", ex);

            return await playwright.Chromium.LaunchAsync(LaunchOptions);
        }
    }

    /// <summary>
    /// Playwright reports a missing browser as a generic <see cref="PlaywrightException"/>, so the
    /// message is the only thing that distinguishes it from a real launch failure.
    /// </summary>
    private static bool IsBrowserMissing(PlaywrightException ex) =>
        ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("playwright.ps1 install", StringComparison.OrdinalIgnoreCase);

    public async Task<ScreenshotResult> CaptureAsync(
        string ivyml,
        ScreenshotOptions options,
        CancellationToken ct = default)
    {
        var validation = IvyMLValidator.Validate(ivyml);
        if (!validation.IsValid)
            return ScreenshotResult.Failed(validation.ErrorMessage!);

        var ext = Path.GetExtension(options.OutputPath).ToLowerInvariant();
        if (!FormatMap.TryGetValue(ext, out var skFormat))
            return ScreenshotResult.Failed($"Unsupported format '{ext}'. Supported: png, jpg, webp.");

        var widget = validation.Widget!;
        var sessionStore = new AppSessionStore();
        var server = new Server(new ServerArgs
        {
            Port = 0,
            Silent = true,
            Host = "127.0.0.1"
        });

        if (options.Theme is { } theme)
            server.UseTheme(theme);

        server.AddApp(new AppDescriptor
        {
            Id = AppIds.Default,
            Title = "IvyML Preview",
            ViewFunc = _ => widget,
            Group = [],
            IsVisible = true
        });

        var app = server.BuildWebApplication(sessionStore);
        if (app is null)
            return ScreenshotResult.Failed("Failed to build the Ivy application.");

        await app.StartAsync(ct);
        var baseUrl = app.Urls.First();

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await LaunchChromiumAsync(playwright, ct);

            var page = await browser.NewPageAsync(new BrowserNewPageOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = options.Width,
                    Height = options.Height
                }
            });

            await page.GotoAsync(baseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            // Ivy streams the widget tree to the client over a WebSocket *after* the initial page
            // load, so NetworkIdle fires while the DOM is still just an empty app shell. Wait for the
            // root element to actually contain rendered content before capturing, otherwise the
            // screenshot is a blank white page.
            try
            {
                await page.WaitForFunctionAsync(
                    "() => { const r = document.getElementById('root'); return r != null && r.innerText.trim().length > 0; }",
                    null,
                    new PageWaitForFunctionOptions { Timeout = options.TimeoutMs });
            }
            catch (PlaywrightException)
            {
                // WaitForFunctionAsync only fails here by exceeding the timeout.
                return ScreenshotResult.Failed(
                    $"Timed out after {options.TimeoutMs}ms waiting for the widget tree to render. " +
                    "Increase the timeout (draw --timeout) if the widget is slow to render.");
            }

            if (options.Debug)
            {
                await page.EvaluateAsync(DebugOverlayScript.Value);
            }

            var fullPath = Path.GetFullPath(options.OutputPath);
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var pngBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Type = ScreenshotType.Png,
                FullPage = false
            });

            using var bitmap = SKBitmap.Decode(pngBytes);
            await using var fs = File.Create(fullPath);
            using var wrapperStream = new SKManagedWStream(fs);
            bitmap.Encode(wrapperStream, skFormat, 90);

            return ScreenshotResult.Succeeded(fullPath);
        }
        catch (PlaywrightException ex)
        {
            return ScreenshotResult.Failed($"Browser error: {ex.Message}");
        }
        finally
        {
            await app.StopAsync(ct);
            await app.DisposeAsync();
            sessionStore.Dispose();
        }
    }
}
