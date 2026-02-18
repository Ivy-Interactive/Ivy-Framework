using System.Reflection;
using System.Runtime.CompilerServices;
using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Widgets.Tiptap;

public interface IAnyTiptapInput : IAnyInput
{
}

public abstract record TiptapInputBase : WidgetBase<TiptapInputBase>, IAnyTiptapInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public bool Editable { get; set; } = true;

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public bool ShowToolbar { get; set; } = true;

    [Prop] public bool Nullable { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnFocus { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [];
}

[ExternalWidget("frontend/dist/TiptapInput.js", ExportName = "TiptapInput")]
public record TiptapInput<TString> : TiptapInputBase, IInput<TString>
{
    public TiptapInput(IAnyState state, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public TiptapInput(TString value, Func<Event<IInput<TString>, TString>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        OnChange = onChange;
        Value = value;
    }

    public TiptapInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        OnChange = onChange?.ToValueTask();
        Value = value;
    }

    public TiptapInput(string? placeholder = null, bool disabled = false) : this()
    {
        Placeholder = placeholder;
        Disabled = disabled;
    }

    internal TiptapInput()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public TString Value { get; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TString).IsNullableType();

    [Event] public Func<Event<IInput<TString>, TString>, ValueTask>? OnChange { get; }
}

[ExternalWidget("frontend/dist/TiptapInput.js", ExportName = "TiptapInput")]
public record TiptapInput : TiptapInput<string>
{
    public TiptapInput(IAnyState state, string? placeholder = null, bool disabled = false)
        : base(state, placeholder, disabled)
    {
    }

    [OverloadResolutionPriority(1)]
    public TiptapInput(string value, Func<Event<IInput<string>, string>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false)
        : base(value, onChange, placeholder, disabled)
    {
    }

    public TiptapInput(string value, Action<Event<IInput<string>, string>>? onChange = null, string? placeholder = null, bool disabled = false)
        : base(value, onChange?.ToValueTask(), placeholder, disabled)
    {
    }

    public TiptapInput(string? placeholder = null, bool disabled = false)
        : base(placeholder, disabled)
    {
    }
}

public static class TiptapInputExtensions
{
    public static TiptapInputBase ToTiptapInput(this IAnyState state, string? placeholder = null, bool disabled = false)
    {
        var type = state.GetStateType();
        Type genericType = typeof(TiptapInput<>).MakeGenericType(type);
        TiptapInputBase input = (TiptapInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled)!;
        input.Nullable = type.IsNullableType();
        return input;
    }

    public static TiptapInputBase Placeholder(this TiptapInputBase widget, string placeholder) =>
        widget with { Placeholder = placeholder };

    public static TiptapInputBase Disabled(this TiptapInputBase widget, bool disabled = true) =>
        widget with { Disabled = disabled };

    public static TiptapInputBase Invalid(this TiptapInputBase widget, string? invalid) =>
        widget with { Invalid = invalid };

    public static TiptapInputBase Editable(this TiptapInputBase widget, bool editable = true) =>
        widget with { Editable = editable };

    public static TiptapInputBase ReadOnly(this TiptapInputBase widget) =>
        widget with { Editable = false };

    public static TiptapInputBase AutoFocus(this TiptapInputBase widget, bool autoFocus = true) =>
        widget with { AutoFocus = autoFocus };

    public static TiptapInputBase ShowToolbar(this TiptapInputBase widget, bool show = true) =>
        widget with { ShowToolbar = show };

    public static TiptapInputBase HideToolbar(this TiptapInputBase widget) =>
        widget with { ShowToolbar = false };

    public static TiptapInputBase Nullable(this TiptapInputBase widget, bool? nullable = true)
    {
        var property = widget.GetType().GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (property != null && property.CanWrite)
        {
            property.SetValue(widget, nullable ?? true);
            return widget;
        }
        return widget with { Nullable = nullable ?? true };
    }

    [OverloadResolutionPriority(1)]
    public static TiptapInputBase HandleFocus(this TiptapInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus) =>
        widget with { OnFocus = onFocus };

    public static TiptapInputBase HandleFocus(this TiptapInputBase widget, Action<Event<IAnyInput>> onFocus) =>
        widget.HandleFocus(onFocus.ToValueTask());

    public static TiptapInputBase HandleFocus(this TiptapInputBase widget, Action handler) =>
        widget.HandleFocus(_ => { handler(); return ValueTask.CompletedTask; });

    [OverloadResolutionPriority(1)]
    public static TiptapInputBase HandleBlur(this TiptapInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur) =>
        widget with { OnBlur = onBlur };

    public static TiptapInputBase HandleBlur(this TiptapInputBase widget, Action<Event<IAnyInput>> onBlur) =>
        widget.OnBlur(onBlur.ToValueTask());

    public static TiptapInputBase HandleBlur(this TiptapInputBase widget, Action handler) =>
        widget.OnBlur(_ => { handler(); return ValueTask.CompletedTask; });
}
