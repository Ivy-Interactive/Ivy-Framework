using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ColorInputs
{
    Text,
    Picker,
    TextAndPicker
}

public interface IAnyColorInput : IAnyInput
{
    public ColorInputs Variant { get; set; }
}

public abstract record ColorInputBase : WidgetBase<ColorInputBase>, IAnyColorInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public new Scale? Scale { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public ColorInputs Variant { get; set; } = ColorInputs.TextAndPicker;

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [
        typeof(string),
        typeof(Colors), typeof(Colors?)
        ];
}

public record ColorInput<TColor> : ColorInputBase, IInput<TColor>
{
    [OverloadResolutionPriority(1)]
    public ColorInput(IAnyState state, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TColor>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public ColorInput(TColor value, Func<Event<IInput<TColor>, TColor>, ValueTask> onChange, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public ColorInput(TColor value, Action<Event<IInput<TColor>, TColor>> onChange, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    public ColorInput(string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
    {
        Disabled = disabled;
        Placeholder = placeholder;
        Variant = variant;
    }

    [Prop] public TColor Value { get; } = default!;

    [Event] public Func<Event<IInput<TColor>, TColor>, ValueTask>? OnChange { get; }
}

public record ColorInput : ColorInput<string>
{
    [OverloadResolutionPriority(1)]
    public ColorInput(IAnyState state, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : base(state, placeholder, disabled, variant)
    {
    }

    [OverloadResolutionPriority(1)]
    public ColorInput(string value, Func<Event<IInput<string>, string>, ValueTask> onChange, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    public ColorInput(string value, Action<Event<IInput<string>, string>> onChange, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    public ColorInput(string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : base(placeholder, disabled, variant)
    {
    }
}

public static class ColorInputExtensions
{
    public static ColorInputBase ToColorInput(this IAnyState state, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
    {
        var type = state.GetStateType();
        Type genericType = typeof(ColorInput<>).MakeGenericType(type);
        ColorInputBase input = (ColorInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        input.Nullable = type.IsNullableType();
        return input;
    }

    public static ColorInputBase Disabled(this ColorInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static ColorInputBase Placeholder(this ColorInputBase widget, string? placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    public static ColorInputBase Invalid(this ColorInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static ColorInputBase Variant(this ColorInputBase widget, ColorInputs variant)
    {
        return widget with { Variant = variant };
    }

    [OverloadResolutionPriority(1)]
    public static ColorInputBase HandleBlur(this ColorInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    public static ColorInputBase HandleBlur(this ColorInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static ColorInputBase HandleBlur(this ColorInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}