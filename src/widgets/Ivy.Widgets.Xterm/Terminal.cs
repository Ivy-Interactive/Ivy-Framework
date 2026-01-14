using Ivy;
using Ivy.Core;
using Ivy.Shared;
using Ivy.Core.ExternalWidgets;

namespace Ivy.Widgets.Xterm;

public record TerminalSize(int Cols, int Rows);

public enum CursorStyle
{
    Block,
    Underline,
    Bar
}

public record TerminalTheme
{
    public string? Background { get; init; }
    public string? Foreground { get; init; }
    public string? Cursor { get; init; }
    public string? CursorAccent { get; init; }
    public string? Selection { get; init; }
    public string? Black { get; init; }
    public string? Red { get; init; }
    public string? Green { get; init; }
    public string? Yellow { get; init; }
    public string? Blue { get; init; }
    public string? Magenta { get; init; }
    public string? Cyan { get; init; }
    public string? White { get; init; }
    public string? BrightBlack { get; init; }
    public string? BrightRed { get; init; }
    public string? BrightGreen { get; init; }
    public string? BrightYellow { get; init; }
    public string? BrightBlue { get; init; }
    public string? BrightMagenta { get; init; }
    public string? BrightCyan { get; init; }
    public string? BrightWhite { get; init; }

    public static TerminalTheme Dark => new()
    {
        Background = "#1e1e1e",
        Foreground = "#d4d4d4",
        Cursor = "#aeafad",
        CursorAccent = "#000000",
        Selection = "rgba(255, 255, 255, 0.3)",
        Black = "#000000",
        Red = "#cd3131",
        Green = "#0dbc79",
        Yellow = "#e5e510",
        Blue = "#2472c8",
        Magenta = "#bc3fbc",
        Cyan = "#11a8cd",
        White = "#e5e5e5",
        BrightBlack = "#666666",
        BrightRed = "#f14c4c",
        BrightGreen = "#23d18b",
        BrightYellow = "#f5f543",
        BrightBlue = "#3b8eea",
        BrightMagenta = "#d670d6",
        BrightCyan = "#29b8db",
        BrightWhite = "#ffffff"
    };

    public static TerminalTheme Light => new()
    {
        Background = "#ffffff",
        Foreground = "#383a42",
        Cursor = "#526eff",
        CursorAccent = "#ffffff",
        Selection = "rgba(0, 0, 0, 0.15)",
        Black = "#000000",
        Red = "#e45649",
        Green = "#50a14f",
        Yellow = "#c18401",
        Blue = "#4078f2",
        Magenta = "#a626a4",
        Cyan = "#0184bc",
        White = "#a0a1a7",
        BrightBlack = "#5c6370",
        BrightRed = "#e06c75",
        BrightGreen = "#98c379",
        BrightYellow = "#d19a66",
        BrightBlue = "#61afef",
        BrightMagenta = "#c678dd",
        BrightCyan = "#56b6c2",
        BrightWhite = "#ffffff"
    };
}

[ExternalWidget("frontend/dist/Ivy_Widgets_Xterm.js", ExportName = "Terminal")]
public record Terminal : WidgetBase<Terminal>
{
    public Terminal()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public int? Cols { get; init; }
    [Prop] public int? Rows { get; init; }
    [Prop] public int FontSize { get; init; } = 14;
    [Prop] public string FontFamily { get; init; } = "Menlo, Monaco, 'Courier New', monospace";
    [Prop] public double LineHeight { get; init; } = 1.0;
    [Prop] public bool CursorBlink { get; init; } = true;
    [Prop] public CursorStyle CursorStyle { get; init; } = CursorStyle.Block;
    [Prop] public int Scrollback { get; init; } = 1000;
    [Prop] public TerminalTheme? Theme { get; init; }
    [Prop] public string? InitialContent { get; init; }

    [Event] public Func<Event<Terminal, string>, ValueTask>? OnData { get; init; }
    [Event] public Func<Event<Terminal, TerminalSize>, ValueTask>? OnResize { get; init; }
    [Event] public Func<Event<Terminal, string>, ValueTask>? OnTitleChange { get; init; }
}

public static class TerminalExtensions
{
    public static Terminal Cols(this Terminal widget, int cols) =>
        widget with { Cols = cols };

    public static Terminal Rows(this Terminal widget, int rows) =>
        widget with { Rows = rows };

    public static Terminal FontSize(this Terminal widget, int fontSize) =>
        widget with { FontSize = fontSize };

    public static Terminal FontFamily(this Terminal widget, string fontFamily) =>
        widget with { FontFamily = fontFamily };

    public static Terminal LineHeight(this Terminal widget, double lineHeight) =>
        widget with { LineHeight = lineHeight };

    public static Terminal CursorBlink(this Terminal widget, bool cursorBlink) =>
        widget with { CursorBlink = cursorBlink };

    public static Terminal CursorStyle(this Terminal widget, CursorStyle cursorStyle) =>
        widget with { CursorStyle = cursorStyle };

    public static Terminal Scrollback(this Terminal widget, int scrollback) =>
        widget with { Scrollback = scrollback };

    public static Terminal Theme(this Terminal widget, TerminalTheme theme) =>
        widget with { Theme = theme };

    public static Terminal DarkTheme(this Terminal widget) =>
        widget with { Theme = TerminalTheme.Dark };

    public static Terminal LightTheme(this Terminal widget) =>
        widget with { Theme = TerminalTheme.Light };

    public static Terminal InitialContent(this Terminal widget, string content) =>
        widget with { InitialContent = content };

    public static Terminal HandleData(this Terminal widget, Func<Event<Terminal, string>, ValueTask> handler) =>
        widget with { OnData = handler };

    public static Terminal HandleData(this Terminal widget, Action<string> handler) =>
        widget with { OnData = e => { handler(e.Value); return ValueTask.CompletedTask; } };

    public static Terminal HandleResize(this Terminal widget, Func<Event<Terminal, TerminalSize>, ValueTask> handler) =>
        widget with { OnResize = handler };

    public static Terminal HandleResize(this Terminal widget, Action<TerminalSize> handler) =>
        widget with { OnResize = e => { handler(e.Value); return ValueTask.CompletedTask; } };

    public static Terminal HandleResize(this Terminal widget, Action<int, int> handler) =>
        widget with { OnResize = e => { handler(e.Value.Cols, e.Value.Rows); return ValueTask.CompletedTask; } };

    public static Terminal HandleTitleChange(this Terminal widget, Func<Event<Terminal, string>, ValueTask> handler) =>
        widget with { OnTitleChange = handler };

    public static Terminal HandleTitleChange(this Terminal widget, Action<string> handler) =>
        widget with { OnTitleChange = e => { handler(e.Value); return ValueTask.CompletedTask; } };
}
