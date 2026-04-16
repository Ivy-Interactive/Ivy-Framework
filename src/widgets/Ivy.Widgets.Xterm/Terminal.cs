namespace Ivy.Widgets.Xterm;

public record TerminalSize(int Cols, int Rows);

public enum CursorStyle
{
    Block,
    Underline,
    Bar
}

[ExternalWidget("frontend/dist/Ivy_Widgets_Xterm.js", ExportName = "Terminal")]
public record Terminal : WidgetBase<Terminal>
{

    [Prop] public int? Cols { get; init; }
    [Prop] public int? Rows { get; init; }
    [Prop] public bool CursorBlink { get; init; } = true;
    [Prop] public CursorStyle CursorStyle { get; init; } = CursorStyle.Block;
    [Prop] public int Scrollback { get; init; } = 1000;
    [Prop] public string? InitialContent { get; init; }
    [Prop] public bool Closed { get; init; }
    [Prop] public bool AllowClipboard { get; init; } = true;
    [Prop] public Colors? Background { get; init; }
    [Prop] public Colors? Foreground { get; init; }

    [Prop] public IWriteStream<byte[]>? Stream { get; init; }

    [Event] public Func<Event<Terminal, string>, ValueTask>? OnInput { get; init; }
    [Event] public Func<Event<Terminal, TerminalSize>, ValueTask>? OnResize { get; init; }
    [Event] public Func<Event<Terminal, string>, ValueTask>? OnLinkClick { get; init; }
}

public static class TerminalExtensions
{
    public static Terminal Cols(this Terminal widget, int cols) =>
        widget with { Cols = cols };

    public static Terminal Rows(this Terminal widget, int rows) =>
        widget with { Rows = rows };

    public static Terminal CursorBlink(this Terminal widget, bool cursorBlink) =>
        widget with { CursorBlink = cursorBlink };

    public static Terminal CursorStyle(this Terminal widget, CursorStyle cursorStyle) =>
        widget with { CursorStyle = cursorStyle };

    public static Terminal Scrollback(this Terminal widget, int scrollback) =>
        widget with { Scrollback = scrollback };

    public static Terminal InitialContent(this Terminal widget, string content) =>
        widget with { InitialContent = content };

    public static Terminal Closed(this Terminal widget, bool closed = true) =>
        widget with { Closed = closed };

    public static Terminal AllowClipboard(this Terminal widget, bool allowClipboard = true) =>
        widget with { AllowClipboard = allowClipboard };

    public static Terminal Background(this Terminal widget, Colors color) =>
        widget with { Background = color };

    public static Terminal Foreground(this Terminal widget, Colors color) =>
        widget with { Foreground = color };

    public static Terminal Stream(this Terminal widget, IWriteStream<byte[]> stream) =>
        widget with { Stream = stream };

    public static Terminal OnInput(this Terminal widget, Func<Event<Terminal, string>, ValueTask> handler) =>
        widget with { OnInput = handler };

    public static Terminal OnInput(this Terminal widget, Action<string> handler) =>
        widget with { OnInput = e => { handler(e.Value); return ValueTask.CompletedTask; } };

    public static Terminal OnResize(this Terminal widget, Func<Event<Terminal, TerminalSize>, ValueTask> handler) =>
        widget with { OnResize = handler };

    public static Terminal OnResize(this Terminal widget, Action<TerminalSize> handler) =>
        widget with { OnResize = e => { handler(e.Value); return ValueTask.CompletedTask; } };

    public static Terminal OnResize(this Terminal widget, Action<int, int> handler) =>
        widget with { OnResize = e => { handler(e.Value.Cols, e.Value.Rows); return ValueTask.CompletedTask; } };

    public static Terminal OnLinkClick(this Terminal widget, Func<Event<Terminal, string>, ValueTask> handler) =>
        widget with { OnLinkClick = handler };

    public static Terminal OnLinkClick(this Terminal widget, Action<string> handler) =>
        widget with { OnLinkClick = e => { handler(e.Value); return ValueTask.CompletedTask; } };
}
