// ReSharper disable once CheckNamespace
namespace Ivy;

public record TerminalLine(string Content, bool IsCommand = false, string Prompt = ">");

/// <summary>
/// Displays a command-line interface or log output.
/// </summary>
public record Terminal : WidgetBase<Terminal>
{
    public Terminal() { }

    [Prop] public TerminalLine[] Lines { get; init; } = [];

    [Prop] public string? Title { get; init; }

    [Prop] public bool ShowHeader { get; init; } = true;

    [Prop] public bool ShowCopyButton { get; init; } = true;
}

public static class TerminalExtensions
{
    private static Terminal AddLine(this Terminal terminal, string content, bool isCommand = false, string prompt = ">")
    {
        return terminal with { Lines = terminal.Lines.Append(new TerminalLine(content, isCommand, prompt)).ToArray() };
    }

    public static Terminal AddCommand(this Terminal terminal, string command)
    {
        return terminal.AddLine(command, true);
    }

    public static Terminal AddOutput(this Terminal terminal, string output)
    {
        return terminal.AddLine(output);
    }

    public static Terminal Title(this Terminal terminal, string title)
    {
        return terminal with { Title = title };
    }

    public static Terminal ShowCopyButton(this Terminal terminal, bool show = true)
    {
        return terminal with { ShowCopyButton = show };
    }
}
