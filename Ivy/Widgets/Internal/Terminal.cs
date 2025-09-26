using Ivy.Core;

namespace Ivy.Widgets.Internal;

/// <summary>
/// Represents a single line of terminal output or input, including the content text, command status, and optional custom prompt.
/// </summary>
/// <param name="Content">The text content of the terminal line.</param>
/// <param name="IsCommand">Whether this line represents a user command (true) or system output (false).</param>
/// <param name="Prompt">The prompt symbol to display before the line content.</param>
public record TerminalLine(string Content, bool IsCommand = false, string Prompt = ">");

/// <summary>
/// Represents a terminal widget that displays command-line interface interactions with support for command input, output display, and terminal-like formatting.
/// </summary>
public record Terminal : WidgetBase<Terminal>
{
    /// <summary>
    /// Gets or sets the array of terminal lines that make up the terminal content.
    /// Each line can represent either a user command or system output, with the TerminalLine record providing the content, command status, and prompt information for proper formatting.
    /// </summary>
    [Prop] public TerminalLine[] Lines { get; init; } = [];

    /// <summary>
    /// Gets or sets the optional title displayed at the top of the terminal widget.
    /// This title provides context about the terminal's purpose, such as "System Console", "Command History", or "Debug Terminal", helping users understand what the terminal is displaying or controlling.
    /// </summary>
    [Prop] public string? Title { get; init; }

    /// <summary>
    /// Gets or sets whether the terminal header (including title and terminal controls) is displayed.
    /// When true, the terminal shows a header section that may include the title, close buttons, or other terminal-specific controls. When false, the terminal displays only the content lines without any header elements.
    /// </summary>
    [Prop] public bool ShowHeader { get; init; } = true;
}

/// <summary>
/// Provides extension methods for the Terminal widget that enable a fluent API for building terminal content and configuring terminal behavior.
/// </summary>
public static class TerminalExtensions
{
    private static Terminal AddLine(this Terminal terminal, string content, bool isCommand = false, string prompt = ">")
    {
        return terminal with { Lines = terminal.Lines.Append(new TerminalLine(content, isCommand, prompt)).ToArray() };
    }

    /// <summary>
    /// Adds a user command line to the terminal with the specified command text.
    /// </summary>
    /// <param name="terminal">The terminal to add the command to.</param>
    /// <param name="command">The command text to display in the terminal.</param>
    public static Terminal AddCommand(this Terminal terminal, string command)
    {
        return terminal.AddLine(command, true);
    }

    /// <summary>
    /// Adds a system output line to the terminal with the specified output text.
    /// </summary>
    /// <param name="terminal">The terminal to add the output to.</param>
    /// <param name="output">The output text to display in the terminal.</param>
    public static Terminal AddOutput(this Terminal terminal, string output)
    {
        return terminal.AddLine(output);
    }

    /// <summary>
    /// Sets the title of the terminal widget to the specified text.
    /// </summary>
    /// <param name="terminal">The terminal to set the title for.</param>
    /// <param name="title">The title text to display in the terminal header.</param>
    public static Terminal Title(this Terminal terminal, string title)
    {
        return terminal with { Title = title };
    }
}