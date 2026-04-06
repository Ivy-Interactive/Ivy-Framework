using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum SignatureInputVariants
{
    Default
}

public interface IAnySignatureInput : IAnyInput
{
    public SignatureInputVariants Variant { get; set; }
}

public abstract record SignatureInputBase : WidgetBase<SignatureInputBase>, IAnySignatureInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public SignatureInputVariants Variant { get; set; } = SignatureInputVariants.Default;

    [Prop] public Colors? Pen { get; set; }

    [Prop] public Colors? Background { get; set; }

    [Prop] public int PenThickness { get; set; } = 2;

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public Type[] SupportedStateTypes() => [];

    public ValidationResult ValidateValue(object? value)
    {
        if (value == null) return ValidationResult.Success();

        if (value is byte[])
            return ValidationResult.Success();

        return ValidationResult.Error("Value must be a byte array (PNG image data).");
    }
}

/// <summary>
/// A canvas-based signature capture widget that allows users to draw signatures directly in the browser.
/// </summary>
public record SignatureInput<TValue> : SignatureInputBase, IInput<TValue>, IAnySignatureInput
{
    [OverloadResolutionPriority(1)]
    public SignatureInput(IAnyState state, string? placeholder = null, bool disabled = false, SignatureInputVariants variant = SignatureInputVariants.Default)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public SignatureInput(TValue value, string? placeholder = null, bool disabled = false, SignatureInputVariants variant = SignatureInputVariants.Default)
        : this(placeholder, disabled, variant)
    {
        Value = value;
    }

    public SignatureInput(string? placeholder = null, bool disabled = false, SignatureInputVariants variant = SignatureInputVariants.Default) : this()
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
    }

    internal SignatureInput()
    {
        Width = Size.Units(100);
        Height = Size.Units(40);
    }

    [Prop(AlwaysSerialize = true)] public TValue Value { get; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    [Event] public EventHandler<Event<IInput<TValue>, TValue>>? OnChange { get; }
}

public static class SignatureInputExtensions
{
    public static SignatureInputBase ToSignatureInput(this IAnyState state, string? placeholder = null, bool disabled = false, SignatureInputVariants variant = SignatureInputVariants.Default)
    {
        var stateType = state.GetStateType();

        Type genericType = typeof(SignatureInput<>).MakeGenericType(stateType);
        SignatureInputBase input = (SignatureInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        input.Nullable = stateType.IsNullableType();

        return input;
    }

    public static SignatureInputBase Placeholder(this SignatureInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    public static SignatureInputBase Disabled(this SignatureInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static SignatureInputBase Variant(this SignatureInputBase widget, SignatureInputVariants variant)
    {
        return widget with { Variant = variant };
    }

    public static SignatureInputBase Invalid(this SignatureInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static SignatureInputBase Nullable(this SignatureInputBase widget, bool? nullable = true)
    {
        return widget with { Nullable = nullable ?? true };
    }

    public static SignatureInputBase Pen(this SignatureInputBase widget, Colors? color)
    {
        return widget with { Pen = color };
    }

    public static SignatureInputBase Background(this SignatureInputBase widget, Colors? color)
    {
        return widget with { Background = color };
    }

    public static SignatureInputBase PenThickness(this SignatureInputBase widget, int thickness)
    {
        return widget with { PenThickness = thickness };
    }

    [OverloadResolutionPriority(1)]
    public static SignatureInputBase OnBlur(this SignatureInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static SignatureInputBase OnBlur(this SignatureInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.OnBlur(onBlur.ToValueTask());
    }

    public static SignatureInputBase OnBlur(this SignatureInputBase widget, Action onBlur)
    {
        return widget.OnBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public static SignatureInputBase OnFocus(this SignatureInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        return widget with { OnFocus = new(onFocus) };
    }

    public static SignatureInputBase OnFocus(this SignatureInputBase widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget.OnFocus(onFocus.ToValueTask());
    }

    public static SignatureInputBase OnFocus(this SignatureInputBase widget, Action onFocus)
    {
        return widget.OnFocus(_ => { onFocus(); return ValueTask.CompletedTask; });
    }
}
