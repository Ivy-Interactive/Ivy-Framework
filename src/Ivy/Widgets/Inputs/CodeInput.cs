using System.Reflection;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum CodeInputVariant
{
    Default
}

public interface IAnyCodeInput : IAnyInput
{
    public CodeInputVariant Variant { get; set; }
}

public abstract record CodeInputBase : WidgetBase<CodeInputBase>, IAnyCodeInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public CodeInputVariant Variant { get; set; } = CodeInputVariant.Default;

    [Prop] public Languages? Language { get; set; } = null;

    [Prop] public bool ShowCopyButton { get; set; } = false;

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public Type[] SupportedStateTypes() => [typeof(string)];
}

/// <summary>
/// An editor input optimized for writing code snippets.
/// </summary>
public record CodeInput<TString> : CodeInputBase, IInput<TString>
{
    [OverloadResolutionPriority(1)]
    internal CodeInput(IAnyState state, string? placeholder = null, bool disabled = false, CodeInputVariant variant = CodeInputVariant.Default)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    internal CodeInput(TString value, Func<Event<IInput<TString>, TString>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, CodeInputVariant variant = CodeInputVariant.Default)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange?.ToEventHandler();
        Value = value;
    }

    internal CodeInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false, CodeInputVariant variant = CodeInputVariant.Default)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange == null ? null : new(e => { onChange(e); return ValueTask.CompletedTask; });
        Value = value;
    }

    internal CodeInput(string? placeholder = null, bool disabled = false, CodeInputVariant variant = CodeInputVariant.Default) : this()
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
    }

    internal CodeInput()
    {
        Width = Size.Full();
        Height = Size.Units(25);
    }

    [Prop(AlwaysSerialize = true)] public TString Value { get; init; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TString).IsNullableType();

    [Event] public EventHandler<Event<IInput<TString>, TString>>? OnChange { get; }
}

public static class CodeInputExtensions
{
    public static CodeInputBase ToCodeInput(this IAnyState state, string? placeholder = null, bool disabled = false, CodeInputVariant variant = CodeInputVariant.Default, Languages language = Languages.Json)
    {
        var type = state.GetStateType();
        Type genericType = typeof(CodeInput<>).MakeGenericType(type);
        CodeInputBase input = (CodeInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, placeholder, disabled, variant }, null)!;
        var nullableProperty = genericType.GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        nullableProperty?.SetValue(input, type.IsNullableType());
        return input;
    }

    public static CodeInputBase Placeholder(this CodeInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    public static CodeInputBase Nullable(this CodeInputBase widget, bool? nullable = true)
    {
        var property = widget.GetType().GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (property != null && property.CanWrite)
        {
            property.SetValue(widget, nullable ?? true);
            return widget;
        }
        return widget with { Nullable = nullable ?? true };
    }

    public static CodeInputBase Disabled(this CodeInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static CodeInputBase Variant(this CodeInputBase widget, CodeInputVariant variant)
    {
        return widget with { Variant = variant };
    }

    public static CodeInputBase Invalid(this CodeInputBase widget, string invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static CodeInputBase Language(this CodeInputBase widget, Languages language)
    {
        return widget with { Language = language };
    }

    public static CodeInputBase ShowCopyButton(this CodeInputBase widget, bool showCopyButton = true)
    {
        return widget with { ShowCopyButton = showCopyButton };
    }

    [OverloadResolutionPriority(1)]
    public static CodeInputBase OnBlur(this CodeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static CodeInputBase OnBlur(this CodeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.OnBlur(onBlur.ToValueTask());
    }

    public static CodeInputBase OnBlur(this CodeInputBase widget, Action onBlur)
    {
        return widget.OnBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public static CodeInputBase OnFocus(this CodeInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        return widget with { OnFocus = new(onFocus) };
    }

    public static CodeInputBase OnFocus(this CodeInputBase widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget.OnFocus(onFocus.ToValueTask());
    }

    public static CodeInputBase OnFocus(this CodeInputBase widget, Action onFocus)
    {
        return widget.OnFocus(_ => { onFocus(); return ValueTask.CompletedTask; });
    }


}