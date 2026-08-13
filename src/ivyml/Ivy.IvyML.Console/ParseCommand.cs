using System.ComponentModel;
using Spectre.Console.Cli;

namespace Ivy.IvyML.Console;

/// <summary>
/// Validates that an IvyML document parses into a widget tree, without rendering it. Accepts the
/// same input arguments as <see cref="DrawCommand"/> (<c>-i</c> markup string or <c>-f</c> file).
/// </summary>
public sealed class ParseCommand : AsyncCommand<ParseCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-i|--input <IVYML>")]
        [Description("IvyML markup string.")]
        public string? IvyML { get; init; }

        [CommandOption("-f|--file <PATH>")]
        [Description("Path to an IvyML file.")]
        public string? FilePath { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(settings.IvyML) && !string.IsNullOrWhiteSpace(settings.FilePath))
        {
            System.Console.Error.WriteLine("Error: Specify either -i or -f, not both.");
            return 1;
        }

        string? ivyml;

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

        var result = IvyMLValidator.Validate(ivyml);

        if (result.IsValid)
        {
            System.Console.WriteLine($"OK: parsed <{result.Widget!.GetType().Name}>.");
            return 0;
        }

        System.Console.Error.WriteLine($"Error: {result.ErrorMessage}");
        return 1;
    }
}
