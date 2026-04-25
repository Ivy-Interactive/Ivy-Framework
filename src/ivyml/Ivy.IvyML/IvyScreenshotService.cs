using System.Reflection;
using Ivy.Core.Apps;
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
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

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
