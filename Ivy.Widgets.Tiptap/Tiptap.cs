using System.Runtime.CompilerServices;
using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;
using Ivy.Widgets.Inputs;

namespace Ivy.Widgets.Tiptap;

[ExternalWidget("frontend/dist/TiptapWidget.js", ExportName = "TiptapWidget")]
public record TiptapInput : WidgetBase<TiptapInput>
{
    public TiptapInput(string? content = null)
    {
        Content = content;
    }

    internal TiptapInput() { }

    [Prop] public string? Content { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Editable { get; set; } = true;

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public int? MinHeight { get; set; }

    [Prop] public int? MaxHeight { get; set; }

    [Prop] public bool ShowToolbar { get; set; } = true;

    [Event] public Func<Event<TiptapInput, string>, ValueTask>? OnChange { get; set; }

    [Event] public Func<Event<TiptapInput>, ValueTask>? OnFocus { get; set; }

    [Event] public Func<Event<TiptapInput>, ValueTask>? OnBlur { get; set; }
}

public static class TiptapInputExtensions
{
    public static TiptapInput Content(this TiptapInput editor, string? content) => editor with { Content = content };

    public static TiptapInput Placeholder(this TiptapInput editor, string placeholder) =>
        editor with { Placeholder = placeholder };

    public static TiptapInput Editable(this TiptapInput editor, bool editable = true) => editor with { Editable = editable };

    public static TiptapInput ReadOnly(this TiptapInput editor) => editor with { Editable = false };

    public static TiptapInput AutoFocus(this TiptapInput editor, bool autoFocus = true) => editor with { AutoFocus = autoFocus };

    public static TiptapInput MinHeight(this TiptapInput editor, int pixels) => editor with { MinHeight = pixels };

    public static TiptapInput MaxHeight(this TiptapInput editor, int pixels) => editor with { MaxHeight = pixels };

    public static TiptapInput ShowToolbar(this TiptapInput editor, bool show = true) => editor with { ShowToolbar = show };

    public static TiptapInput HideToolbar(this TiptapInput editor) => editor with { ShowToolbar = false };

    [OverloadResolutionPriority(1)]
    public static TiptapInput HandleChange(this TiptapInput editor, Func<Event<TiptapInput, string>, ValueTask> onChange) =>
        editor with { OnChange = onChange };

    public static TiptapInput HandleChange(this TiptapInput editor, Action<Event<TiptapInput, string>> onChange) =>
        editor with { OnChange = onChange.ToValueTask() };

    public static TiptapInput HandleChange(this TiptapInput editor, Action<string> handler) => editor with
    {
        OnChange = e =>
        {
            handler(e.Value);
            return ValueTask.CompletedTask;
        }
    };

    [OverloadResolutionPriority(1)]
    public static TiptapInput HandleFocus(this TiptapInput editor, Func<Event<TiptapInput>, ValueTask> onFocus) =>
        editor with { OnFocus = onFocus };

    public static TiptapInput HandleFocus(this TiptapInput editor, Action<Event<TiptapInput>> onFocus) =>
        editor with { OnFocus = onFocus.ToValueTask() };

    public static TiptapInput HandleFocus(this TiptapInput editor, Action handler) => editor with
    {
        OnFocus = _ =>
        {
            handler();
            return ValueTask.CompletedTask;
        }
    };

    [OverloadResolutionPriority(1)]
    public static TiptapInput HandleBlur(this TiptapInput editor, Func<Event<TiptapInput>, ValueTask> onBlur) =>
        editor with { OnBlur = onBlur };

    public static TiptapInput HandleBlur(this TiptapInput editor, Action<Event<TiptapInput>> onBlur) =>
        editor with { OnBlur = onBlur.ToValueTask() };

    public static TiptapInput HandleBlur(this TiptapInput editor, Action handler) => editor with
    {
        OnBlur = _ =>
        {
            handler();
            return ValueTask.CompletedTask;
        }
    };
}
