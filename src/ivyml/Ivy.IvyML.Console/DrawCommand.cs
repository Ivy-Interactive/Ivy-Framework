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
        public required string IvyML { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(settings.IvyML))
        {
            System.Console.Error.WriteLine("Error: IvyML input (-i) is required.");
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
        var options = new ScreenshotOptions(settings.Width, settings.Height, outputPath);
        var result = await service.CaptureAsync(settings.IvyML, options, ct);

        if (result.Success)
        {
            System.Console.WriteLine(result.OutputPath);
            return 0;
        }

        System.Console.Error.WriteLine($"Error: {result.ErrorMessage}");
        return 1;
    }
}
