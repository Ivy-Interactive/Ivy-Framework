using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Ivy.Core;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ColorInputVariant
{
    Text,
    Picker,
    TextAndPicker,
    Swatch,
    SwatchPicker
}

public interface IAnyColorInput : IAnyInput
{
    public ColorInputVariant Variant { get; set; }
}

[Slot("Prefix")]
[Slot("Suffix")]
public abstract record ColorInputBase : WidgetBase<ColorInputBase>, IAnyColorInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public bool? Foreground { get; set; }

    [Prop] public bool Ghost { get; set; }
    [Prop] public bool AllowAlpha { get; set; }

    [Prop] public ColorInputVariant Variant { get; set; } = ColorInputVariant.TextAndPicker;

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public Type[] SupportedStateTypes() => [
        typeof(string),
        typeof(Colors), typeof(Colors?)
    ];
}

public record ColorInput<TColor> : ColorInputBase, IInput<TColor>
{
    [OverloadResolutionPriority(1)]
    internal ColorInput(IAnyState state, string? placeholder = null, bool disabled = false, ColorInputVariant variant = ColorInputVariant.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TColor>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    internal ColorInput(TColor value, Func<Event<IInput<TColor>, TColor>, ValueTask> onChange, string? placeholder = null, bool disabled = false, ColorInputVariant variant = ColorInputVariant.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    internal ColorInput(TColor value, Action<Event<IInput<TColor>, TColor>> onChange, string? placeholder = null, bool disabled = false, ColorInputVariant variant = ColorInputVariant.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        OnChange = new(e => { onChange(e); return ValueTask.CompletedTask; });
        Value = value;
    }

    internal ColorInput(string? placeholder = null, bool disabled = false, ColorInputVariant variant = ColorInputVariant.TextAndPicker)
    {
        Disabled = disabled;
        Placeholder = placeholder;
        Variant = variant;
    }

    internal ColorInput() { }

    [Prop(AlwaysSerialize = true)] public TColor Value { get; init; } = default!;

    [Event] public EventHandler<Event<IInput<TColor>, TColor>>? OnChange { get; }
}

/// <summary>
/// An input field for selecting colors.
/// </summary>
public record ColorInput : ColorInput<string>
{
    [OverloadResolutionPriority(1)]
    internal ColorInput(IAnyState state, string? placeholder = null, bool disabled = false, ColorInputVariant variant = ColorInputVariant.TextAndPicker)
        : base(state, placeholder, disabled, variant)
    {
    }

    [OverloadResolutionPriority(1)]
    internal ColorInput(string value, Func<Event<IInput<string>, string>, ValueTask> onChange, string? placeholder = null, bool disabled = false, ColorInputVariant variant = ColorInputVariant.TextAndPicker)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    internal ColorInput(string value, Action<Event<IInput<string>, string>> onChange, string? placeholder = null, bool disabled = false, ColorInputVariant variant = ColorInputVariant.TextAndPicker)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    internal ColorInput(string? placeholder = null, bool disabled = false, ColorInputVariant variant = ColorInputVariant.TextAndPicker)
        : base(placeholder, disabled, variant)
    {
    }
}

public static class ColorInputExtensions
{
    private static readonly Regex HexColorPattern = new(
        @"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$",
        RegexOptions.Compiled);
    private static readonly Regex FunctionalColorPattern = new(
        @"^(rgb|rgba|hsl|hsla|hwb|lab|lch|oklab|oklch|color)\s*\(.+\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static string? ValidateColorFormat(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.StartsWith("#"))
            return HexColorPattern.IsMatch(normalized) ? null : "Invalid color format";

        if (FunctionalColorPattern.IsMatch(normalized))
            return null;

        if (Enum.TryParse<Colors>(normalized, ignoreCase: true, out _))
            return null;

        return "Invalid color format";
    }

    public static ColorInputBase ToColorInput(this IAnyState state, string? placeholder = null, bool disabled = false, ColorInputVariant? variant = null)
    {
        var type = state.GetStateType();
        var underlyingType = System.Nullable.GetUnderlyingType(type) ?? type;
        var effectiveVariant = variant ?? (underlyingType == typeof(Colors) ? ColorInputVariant.Swatch : ColorInputVariant.TextAndPicker);

        Type genericType = typeof(ColorInput<>).MakeGenericType(type);
        ColorInputBase input = (ColorInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, placeholder, disabled, effectiveVariant }, null)!;
        var nullableProperty = genericType.GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        nullableProperty?.SetValue(input, type.IsNullableType());

        var currentValue = state.As<object>().Value?.ToString();
        var validationError = ValidateColorFormat(currentValue);
        if (validationError != null)
        {
            return input with { Invalid = validationError };
        }

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

    public static ColorInputBase Nullable(this ColorInputBase widget, bool? nullable = true)
    {
        return widget with { Nullable = nullable ?? true };
    }

    public static ColorInputBase Variant(this ColorInputBase widget, ColorInputVariant variant)
    {
        return widget with { Variant = variant };
    }

    public static ColorInputBase Foreground(this ColorInputBase widget, bool? foreground = true)
    {
        return widget with { Foreground = foreground };
    }

    public static ColorInputBase Ghost(this ColorInputBase widget, bool ghost = true)
    {
        return widget with { Ghost = ghost };
    }

    public static ColorInputBase AllowAlpha(this ColorInputBase widget, bool allowAlpha = true)
    {
        return widget with { AllowAlpha = allowAlpha };
    }

    [OverloadResolutionPriority(1)]
    public static ColorInputBase OnBlur(this ColorInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static ColorInputBase OnBlur(this ColorInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.OnBlur(onBlur.ToValueTask());
    }

    public static ColorInputBase OnBlur(this ColorInputBase widget, Action onBlur)
    {
        return widget.OnBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public static ColorInputBase OnFocus(this ColorInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        return widget with { OnFocus = new(onFocus) };
    }

    public static ColorInputBase OnFocus(this ColorInputBase widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget.OnFocus(onFocus.ToValueTask());
    }

    public static ColorInputBase OnFocus(this ColorInputBase widget, Action onFocus)
    {
        return widget.OnFocus(_ => { onFocus(); return ValueTask.CompletedTask; });
    }

    public static ColorInputBase Prefix(this ColorInputBase widget, object prefix)
        => widget with { Children = widget.WithSlot("Prefix", prefix) };

    public static ColorInputBase Suffix(this ColorInputBase widget, object suffix)
        => widget with { Children = widget.WithSlot("Suffix", suffix) };
}