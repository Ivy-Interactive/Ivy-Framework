using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;

namespace Ivy.Widgets.Tiptap;

/// <summary>
/// Tiptap rich text editor widget.
/// A headless, framework-agnostic rich text editor built on ProseMirror.
/// </summary>
[ExternalWidget("frontend/dist/TiptapWidget.js", ExportName = "TiptapWidget")]
public record Tiptap : WidgetBase<Tiptap>
{
    /// <summary>
    /// Creates a new Tiptap editor with optional initial content.
    /// </summary>
    /// <param name="content">Initial HTML content.</param>
    public Tiptap(string? content = null)
    {
        Content = content;
    }

    internal Tiptap() { }

    [Prop]
    public string? Content { get; set; }

    [Prop]
    public string? Placeholder { get; set; }

    [Prop] public bool Editable { get; set; } = true;

    /// <summary>
    /// Whether to auto-focus the editor on mount.
    /// </summary>
    [Prop]
    public bool AutoFocus { get; set; }

    /// <summary>
    /// Minimum height of the editor in pixels.
    /// </summary>
    [Prop]
    public int? MinHeight { get; set; }

    /// <summary>
    /// Maximum height of the editor in pixels. If set, content will scroll.
    /// </summary>
    [Prop]
    public int? MaxHeight { get; set; }

    /// <summary>
    /// Whether to show the toolbar. Defaults to true.
    /// </summary>
    [Prop]
    public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Event triggered when the editor content changes.
    /// The event value is the HTML content.
    /// </summary>
    [Event]
    public Func<Event<Tiptap, string>, ValueTask>? OnChange { get; set; }

    /// <summary>
    /// Event triggered when the editor gains focus.
    /// </summary>
    [Event]
    public Func<Event<Tiptap>, ValueTask>? OnFocus { get; set; }

    /// <summary>
    /// Event triggered when the editor loses focus.
    /// </summary>
    [Event]
    public Func<Event<Tiptap>, ValueTask>? OnBlur { get; set; }
}

/// <summary>
/// Extension methods for Tiptap widget.
/// </summary>
public static class TiptapExtensions
{
    /// <summary>
    /// Sets the HTML content of the editor.
    /// </summary>
    public static Tiptap WithContent(this Tiptap editor, string? content) =>
        editor with { Content = content };

    /// <summary>
    /// Sets the placeholder text.
    /// </summary>
    public static Tiptap WithPlaceholder(this Tiptap editor, string placeholder) =>
        editor with { Placeholder = placeholder };

    /// <summary>
    /// Sets whether the editor is editable.
    /// </summary>
    public static Tiptap Editable(this Tiptap editor, bool editable = true) =>
        editor with { Editable = editable };

    /// <summary>
    /// Makes the editor read-only.
    /// </summary>
    public static Tiptap ReadOnly(this Tiptap editor) =>
        editor with { Editable = false };

    /// <summary>
    /// Enables auto-focus on mount.
    /// </summary>
    public static Tiptap AutoFocus(this Tiptap editor, bool autoFocus = true) =>
        editor with { AutoFocus = autoFocus };

    /// <summary>
    /// Sets the minimum height of the editor.
    /// </summary>
    public static Tiptap MinHeight(this Tiptap editor, int pixels) =>
        editor with { MinHeight = pixels };

    /// <summary>
    /// Sets the maximum height of the editor (enables scrolling).
    /// </summary>
    public static Tiptap MaxHeight(this Tiptap editor, int pixels) =>
        editor with { MaxHeight = pixels };

    /// <summary>
    /// Shows or hides the toolbar.
    /// </summary>
    public static Tiptap ShowToolbar(this Tiptap editor, bool show = true) =>
        editor with { ShowToolbar = show };

    /// <summary>
    /// Hides the toolbar.
    /// </summary>
    public static Tiptap HideToolbar(this Tiptap editor) =>
        editor with { ShowToolbar = false };

    /// <summary>
    /// Handles the content change event.
    /// </summary>
    public static Tiptap HandleChange(this Tiptap editor, Action<string> handler) =>
        editor with { OnChange = e => { handler(e.Value); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles the content change event asynchronously.
    /// </summary>
    public static Tiptap OnChange(this Tiptap editor, Func<Event<Tiptap, string>, ValueTask> handler) =>
        editor with { OnChange = handler };

    /// <summary>
    /// Handles the focus event.
    /// </summary>
    public static Tiptap HandleFocus(this Tiptap editor, Action handler) =>
        editor with { OnFocus = _ => { handler(); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles the blur event.
    /// </summary>
    public static Tiptap HandleBlur(this Tiptap editor, Action handler) =>
        editor with { OnBlur = _ => { handler(); return ValueTask.CompletedTask; } };
}
