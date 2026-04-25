namespace Ivy.IvyML;

public record ScreenshotOptions(int Width, int Height, string OutputPath);

public record ScreenshotResult(bool Success, string? OutputPath, string? ErrorMessage)
{
    public static ScreenshotResult Succeeded(string outputPath) => new(true, outputPath, null);
    public static ScreenshotResult Failed(string error) => new(false, null, error);
}
