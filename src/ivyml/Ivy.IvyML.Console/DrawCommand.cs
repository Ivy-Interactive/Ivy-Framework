using System.ComponentModel;
using Ivy.IvyML;
using Spectre.Console.Cli;

namespace Ivy.IvyML.Console;

public sealed class DrawCommand : AsyncCommand<DrawCommand.Settings>
{
    private static readonly string[] SupportedExtensions = [".png", ".jpg", ".jpeg", ".webp"];

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-w|--width")]
        [Description("Viewport width in pixels.")]
        [DefaultValue(300)]
        public int Width { get; init; }

        [CommandOption("-h|--height")]
        [Description("Viewport height in pixels.")]
        [DefaultValue(200)]
        public int Height { get; init; }

        [CommandOption("-o|--output <PATH>")]
        [Description("Output file path. Supported formats: png, jpg, webp. If omitted a temp file is used.")]
        public string? OutputPath { get; init; }

        [CommandOption("-i|--input <IVYML>")]
        [Description("IvyML markup string.")]
        public string? IvyML { get; init; }

        [CommandOption("-f|--file <PATH>")]
        [Description("Path to an IvyML file.")]
        public string? FilePath { get; init; }

        [CommandOption("-d|--debug")]
        [Description("Draw debug overlays showing widget bounds, sizes, and padding.")]
        [DefaultValue(false)]
        public bool Debug { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(settings.IvyML) && !string.IsNullOrWhiteSpace(settings.FilePath))
        {
            System.Console.Error.WriteLine("Error: Specify either -i or -f, not both.");
            return 1;
        }

        string? ivyml = null;

        if (!string.IsNullOrWhiteSpace(settings.FilePath))
        {
            if (!File.Exists(settings.FilePath))
            {
                System.Console.Error.WriteLine($"Error: File not found: {settings.FilePath}");
                return 1;
            }
            ivyml = await File.ReadAllTextAsync(settings.FilePath, ct);
        }
        else
        {
            ivyml = settings.IvyML;
        }

        if (string.IsNullOrWhiteSpace(ivyml))
        {
            System.Console.Error.WriteLine("Error: Provide IvyML markup via -i or -f.");
            return 1;
        }

        if (settings.Width <= 0 || settings.Height <= 0)
        {
            System.Console.Error.WriteLine("Error: Width and height must be positive integers.");
            return 1;
        }

        var outputPath = settings.OutputPath;
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            outputPath = Path.Combine(Path.GetTempPath(), $"ivyml_{Guid.NewGuid():N}.png");
        }

        var ext = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!SupportedExtensions.Contains(ext))
        {
            System.Console.Error.WriteLine($"Error: Unsupported format '{ext}'. Supported: {string.Join(", ", SupportedExtensions)}");
            return 1;
        }

        var service = new IvyScreenshotService();
        var options = new ScreenshotOptions(settings.Width, settings.Height, outputPath, settings.Debug);
        var result = await service.CaptureAsync(ivyml, options, ct);

        if (result.Success)
        {
            System.Console.WriteLine(result.OutputPath);
            return 0;
        }

        System.Console.Error.WriteLine($"Error: {result.ErrorMessage}");
        return 1;
    }
}
