using System.ComponentModel;
using Ivy.IvyML;
using Spectre.Console.Cli;

namespace Ivy.IvyML.Console;

public sealed class DocsCommand : Command<DocsCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[widget]")]
        [Description("Widget name to show detailed props for.")]
        public string? Widget { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(settings.Widget))
        {
            System.Console.WriteLine(DocsProvider.GetDocsOutput());
        }
        else
        {
            System.Console.WriteLine(DocsProvider.GetWidgetDocs(settings.Widget));
        }

        return 0;
    }
}
