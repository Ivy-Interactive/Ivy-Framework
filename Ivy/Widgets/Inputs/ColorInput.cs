using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual variants available for color input controls.
/// </summary>
public enum ColorInputs
{
    /// <summary>Text-only input for entering color values as hex codes, RGB values, or color names.</summary>
    Text,
    /// <summary>Visual color picker interface for selecting colors through a graphical color palette.</summary>
    Picker,
    /// <summary>Combined interface with both text input and visual color picker for maximum flexibility.</summary>
    TextAndPicker
}

/// <summary>
/// Interface for color input controls that extends IAnyInput with color-specific properties.
/// </summary>
public interface IAnyColorInput : IAnyInput
{
    /// <summary>Gets or sets the placeholder text displayed when the color input is empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual variant of the color input.</summary>
    public ColorInputs Variant { get; set; }
}

/// <summary>
/// Abstract base class for color input controls.
/// </summary>
public abstract record ColorInputBase : WidgetBase<ColorInputBase>, IAnyColorInput
{
    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    [Prop] public string? Placeholder { get; set; }

    /// <summary>Gets or sets whether the input accepts null values.</summary>
    [Prop] public bool Nullable { get; set; }

    /// <summary>Gets or sets the visual variant of the color input.</summary>
    [Prop] public ColorInputs Variant { get; set; } = ColorInputs.TextAndPicker;

    /// <summary>Gets or sets the size of the color input.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns the types that this color input can bind to and work with.
    /// </summary>
    public Type[] SupportedStateTypes() => [
        typeof(string),
        typeof(Colors), typeof(Colors?)
        ];
}

/// <summary>
/// Generic color input control.
/// </summary>
/// <typeparam name="TColor">The type of the color value (string, Colors, or Colors?).</typeparam>
public record ColorInput<TColor> : ColorInputBase, IInput<TColor>
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    [OverloadResolutionPriority(1)]
    public ColorInput(IAnyState state, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TColor>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new instance with an explicit value and async change handler.
    /// </summary>
    /// <param name="value">The initial color value.</param>
    /// <param name="onChange">Event handler called when the color value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    [OverloadResolutionPriority(1)]
    public ColorInput(TColor value, Func<Event<IInput<TColor>, TColor>, ValueTask> onChange, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial color value.</param>
    /// <param name="onChange">Event handler called when the color value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    public ColorInput(TColor value, Action<Event<IInput<TColor>, TColor>> onChange, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : this(placeholder, disabled, variant)
    {
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    public ColorInput(string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
    {
        Disabled = disabled;
        Placeholder = placeholder;
        Variant = variant;
    }

    /// <summary>Gets the current color value.</summary>
    [Prop] public TColor Value { get; } = default!;

    /// <summary>Gets the event handler called when the color value changes.</summary>
    [Event] public Func<Event<IInput<TColor>, TColor>, ValueTask>? OnChange { get; }
}

/// <summary>
/// Concrete color input control for string-based color values.
/// </summary>
public record ColorInput : ColorInput<string>
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    [OverloadResolutionPriority(1)]
    public ColorInput(IAnyState state, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : base(state, placeholder, disabled, variant)
    {
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial color value as a string (hex code, RGB, or color name).</param>
    /// <param name="onChange">Event handler called when the color value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    [OverloadResolutionPriority(1)]
    public ColorInput(string value, Func<Event<IInput<string>, string>, ValueTask> onChange, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial color value as a string (hex code, RGB, or color name).</param>
    /// <param name="onChange">Event handler called when the color value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    public ColorInput(string value, Action<Event<IInput<string>, string>> onChange, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    public ColorInput(string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
        : base(placeholder, disabled, variant)
    {
    }
}

/// <summary>
/// Provides extension methods for creating and configuring color inputs with fluent syntax.
/// </summary>
public static class ColorInputExtensions
{
    /// <summary>
    /// Creates a color input from a state object with automatic type binding and nullable detection.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the color input.</param>
    public static ColorInputBase ToColorInput(this IAnyState state, string? placeholder = null, bool disabled = false, ColorInputs variant = ColorInputs.TextAndPicker)
    {
        var type = state.GetStateType();
        Type genericType = typeof(ColorInput<>).MakeGenericType(type);
        ColorInputBase input = (ColorInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        input.Nullable = type.IsNullableType();
        return input;
    }

    /// <summary>Sets the disabled state of the color input.</summary>
    /// <param name="widget">The color input to configure.</param>
    /// <param name="disabled">Whether the input should be disabled.</param>
    public static ColorInputBase Disabled(this ColorInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    /// <summary>Sets the placeholder text for the color input.</summary>
    /// <param name="widget">The color input to configure.</param>
    /// <param name="placeholder">The placeholder text to display when the input is empty.</param>
    public static ColorInputBase Placeholder(this ColorInputBase widget, string? placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    /// <summary>Sets the validation error message for the color input.</summary>
    /// <param name="widget">The color input to configure.</param>
    /// <param name="invalid">The validation error message to display.</param>
    public static ColorInputBase Invalid(this ColorInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    /// <summary>Sets the visual variant of the color input.</summary>
    /// <param name="widget">The color input to configure.</param>
    /// <param name="variant">The visual variant (Text, Picker, or TextAndPicker).</param>
    public static ColorInputBase Variant(this ColorInputBase widget, ColorInputs variant)
    {
        return widget with { Variant = variant };
    }

    /// <summary>Sets the size of the color input.</summary>
    /// <param name="widget">The color input to configure.</param>
    /// <param name="size">The size of the color input.</param>
    public static ColorInputBase Size(this ColorInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    /// <summary>Sets the color input size to large for prominent display.</summary>
    /// <param name="widget">The color input to configure.</param>
    /// <returns>A new ColorInputBase instance with large size applied.</returns>
    public static ColorInputBase Large(this ColorInputBase widget)
    {
        return widget.Size(Sizes.Large);
    }

    /// <summary>Sets the color input size to small for compact display.</summary>
    /// <param name="widget">The color input to configure.</param>
    /// <returns>A new ColorInputBase instance with small size applied.</returns>
    public static ColorInputBase Small(this ColorInputBase widget)
    {
        return widget.Size(Sizes.Small);
    }


    /// <summary>
    /// Sets the blur event handler for the color input.
    /// </summary>
    /// <param name="widget">The color input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static ColorInputBase HandleBlur(this ColorInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary>
    /// Sets the blur event handler for the color input.
    /// </summary>
    /// <param name="widget">The color input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static ColorInputBase HandleBlur(this ColorInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the color input.
    /// </summary>
    /// <param name="widget">The color input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static ColorInputBase HandleBlur(this ColorInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}