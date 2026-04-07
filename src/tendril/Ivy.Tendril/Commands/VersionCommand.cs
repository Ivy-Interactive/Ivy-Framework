using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ivy.Tendril.Commands;

public class VersionCommand : AsyncCommand<VersionCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var assembly = typeof(Program).Assembly;
        var version = assembly.GetName().Version;
        var versionString = version?.ToString(3) ?? "0.0.0";

        AnsiConsole.MarkupLine($"[blue]Ivy Tendril[/] v{versionString}");
        return Task.FromResult(0);
    }
}
