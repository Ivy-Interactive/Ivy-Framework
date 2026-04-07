using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ivy.Tendril.Commands;

public class VersionCommand : Command<VersionCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var assembly = typeof(Program).Assembly;
        var version = assembly.GetName().Version;
        var versionString = version?.ToString(3) ?? "0.0.0";

        AnsiConsole.MarkupLine($"[blue]Ivy Tendril[/] v{versionString}");
        return 0;
    }
}
