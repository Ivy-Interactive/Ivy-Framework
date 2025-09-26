using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual variants available for number input controls.
/// </summary>
public enum NumberInputs
{
    /// <summary>Standard number input field for direct numeric entry with validation and formatting.</summary>
    Number,
    /// <summary>Slider control for visual numeric selection within a defined range.</summary>
    Slider
}

/// <summary>
/// Defines the formatting styles available for displaying numeric values.
/// </summary>
public enum NumberFormatStyle
{
    /// <summary>Standard decimal number formatting (e.g., 123.45).</summary>
    Decimal,
    /// <summary>Currency formatting with currency symbols and appropriate decimal places (e.g., $123.45).</summary>
    Currency,
    /// <summary>Percentage formatting with percent symbol and scaling (e.g., 12.34%).</summary>
    Percent
}

/// <summary>
/// Interface for number input controls that extends IAnyInput with numeric-specific properties.
/// </summary>
public interface IAnyNumberInput : IAnyInput
{
    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets the minimum allowed value for the numeric input.</summary>
    public double? Min { get; set; }

    /// <summary>Gets or sets the maximum allowed value for the numeric input.</summary>
    public double? Max { get; set; }

    /// <summary>Gets or sets the step increment for the numeric input (used for sliders and increment/decrement buttons).</summary>
    public double? Step { get; set; }

    /// <summary>Gets or sets the number of decimal places to display and accept.</summary>
    public int? Precision { get; set; }

    /// <summary>Gets or sets the visual variant of the number input.</summary>
    public NumberInputs Variant { get; set; }

    /// <summary>Gets or sets the formatting style for displaying numeric values.</summary>
    public NumberFormatStyle FormatStyle { get; set; }

    /// <summary>Gets or sets the currency code for currency formatting (e.g., "USD", "EUR").</summary>
    public string? Currency { get; set; }

    /// <summary>Gets or sets the target type name for frontend validation and formatting.</summary>
    public string? TargetType { get; set; }

    /// <summary>Gets or sets the size of the number input(Medium, Small, Large)</summary>
    [Prop] public Sizes Size { get; set; }
}

/// <summary>
/// Abstract base class for number input controls.
/// </summary>
public abstract record NumberInputBase : WidgetBase<NumberInputBase>, IAnyNumberInput
{
    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    [Prop] public string? Placeholder { get; set; }

    /// <summary>Gets or sets the minimum allowed value for the numeric input.</summary>
    [Prop] public double? Min { get; set; }

    /// <summary>Gets or sets the maximum allowed value for the numeric input.</summary>
    [Prop] public double? Max { get; set; }

    /// <summary>Gets or sets the step increment for the numeric input.</summary>
    [Prop] public double? Step { get; set; }

    /// <summary>Gets or sets the number of decimal places to display and accept.</summary>
    [Prop] public int? Precision { get; set; }

    /// <summary>Gets or sets the visual variant of the number input.</summary>
    [Prop] public NumberInputs Variant { get; set; }

    /// <summary>Gets or sets the formatting style for displaying numeric values.</summary>
    [Prop] public NumberFormatStyle FormatStyle { get; set; }

    /// <summary>Gets or sets the currency code for currency formatting.</summary>
    [Prop] public string? Currency { get; set; }

    /// <summary>Gets or sets the target type name for frontend validation and formatting.</summary>
    [Prop] public string? TargetType { get; set; }

    /// <summary>Gets or sets the size of the number input(Medium, Small, Large)</summary>
    [Prop] public Sizes Size { get; set; }

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns the types that this number input can bind to and work with.
    /// Supports all standard .NET numeric types including signed, unsigned, and floating-point types.
    /// </summary>
    public Type[] SupportedStateTypes() => [
        // Signed numeric types
        typeof(short), typeof(short?),
        typeof(int), typeof(int?),
        typeof(long), typeof(long?),
        typeof(float), typeof(float?),
        typeof(double), typeof(double?),
        typeof(decimal), typeof(decimal?),

        // Unsigned integer types
        typeof(byte), typeof(byte?)
    ];
}

/// <summary>
/// Generic number input control that provides type-safe numeric input functionality for various .NET numeric types.
/// </summary>
/// <typeparam name="TNumber">The type of the numeric value (int, double, decimal, float, long, short, byte, or their nullable variants).</typeparam>
public record NumberInput<TNumber> : NumberInputBase, IInput<TNumber>, IAnyNumberInput
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the number input.</param>
    /// <param name="formatStyle">The formatting style for displaying numeric values.</param>
    [OverloadResolutionPriority(1)]
    public NumberInput(IAnyState state, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(placeholder, disabled, variant, formatStyle)
    {
        var typedState = state.As<TNumber>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new instance with an explicit value and change handler.
    /// </summary>
    /// <param name="value">The initial numeric value.</param>
    /// <param name="onChange">Async function to handle numeric value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the number input.</param>
    /// <param name="formatStyle">The formatting style for displaying numeric values.</param>
    [OverloadResolutionPriority(1)]
    public NumberInput(TNumber value, Func<Event<IInput<TNumber>, TNumber>, ValueTask> onChange, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(placeholder, disabled, variant, formatStyle)
    {
        OnChange = onChange;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with an explicit value and state setter function.
    /// </summary>
    /// <param name="value">The initial numeric value.</param>
    /// <param name="state">Function to update the state when the numeric value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the number input.</param>
    /// <param name="formatStyle">The formatting style for displaying numeric values.</param>
    public NumberInput(TNumber value, Action<TNumber> state, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(placeholder, disabled, variant, formatStyle)
    {
        OnChange = e => { state(e.Value); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the number input.</param>
    /// <param name="formatStyle">The formatting style for displaying numeric values.</param>
    public NumberInput(string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        Placeholder = placeholder;
        Disabled = disabled;
        Variant = variant;
        FormatStyle = formatStyle;
    }

    /// <summary>Gets the current numeric value.</summary>
    [Prop] public TNumber Value { get; } = default!;

    /// <summary>Gets or sets whether the input accepts null values.</summary>
    [Prop] public bool Nullable { get; set; } = typeof(TNumber).IsNullableType();

    /// <summary>Gets the event handler called when the numeric value changes.</summary>
    [Event] public Func<Event<IInput<TNumber>, TNumber>, ValueTask>? OnChange { get; }
}

/// <summary>
/// Provides extension methods for creating and configuring number inputs with fluent syntax.
/// </summary>
public static class NumberInputExtensions
{
    /// <summary>
    /// Creates a slider-style number input from a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="formatStyle">The formatting style for displaying numeric values.</param>
    public static NumberInputBase ToSliderInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        return state.ToNumberInput(placeholder, disabled, NumberInputs.Slider, formatStyle);
    }

    /// <summary>
    /// Creates a number input from a state object with automatic type detection and intelligent defaults.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the number input.</param>
    /// <param name="formatStyle">The formatting style for displaying numeric values.</param>
    public static NumberInputBase ToNumberInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        var type = state.GetStateType();
        Type genericType = typeof(NumberInput<>).MakeGenericType(type);
        NumberInputBase input = (NumberInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant, formatStyle)!;
        input.ScaffoldDefaults(null, type);
        return input;
    }

    /// <summary>
    /// Creates a currency-formatted number input from a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the number input.</param>
    /// <param name="currency">The currency code for formatting (e.g., "USD", "EUR").</param>
    public static NumberInputBase ToMoneyInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, string currency = "USD")
        => state.ToNumberInput(placeholder, disabled, variant, NumberFormatStyle.Currency).Currency(currency);

    /// <summary>
    /// Internal method for setting intelligent defaults based on the numeric type.
    /// </summary>
    /// <param name="input">The number input to configure.</param>
    /// <param name="name">The property name (currently unused, reserved for future label generation).</param>
    /// <param name="type">The numeric type to base defaults on.</param>
    internal static IAnyNumberInput ScaffoldDefaults(this IAnyNumberInput input, string? name, Type type)
    {
        input.Precision ??= type.SuggestPrecision();
        input.Step ??= type.SuggestStep();
        input.Min ??= type.SuggestMin();
        input.Max ??= type.SuggestMax();

        // Set target type for frontend validation
        input.TargetType = GetTargetTypeName(type);

        // Add default currency for Currency style inputs
        if (input.FormatStyle == NumberFormatStyle.Currency && string.IsNullOrEmpty(input.Currency))
        {
            input.Currency = "USD";
        }

        return input;
    }

    /// <summary>
    /// Gets the target type name for frontend validation and formatting.
    /// </summary>
    /// <param name="type">The .NET type to get the name for.</param>
    private static string GetTargetTypeName(Type type)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type);
        var actualType = underlyingType ?? type;

        return actualType.Name.ToLowerInvariant();
    }

    /// <summary>Sets the placeholder text for the number input.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="placeholder">The placeholder text to display when the input is empty.</param>
    public static NumberInputBase Placeholder(this NumberInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    /// <summary>Sets the disabled state of the number input.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="enabled">Whether the input should be disabled (note: parameter name suggests enabled but sets disabled state).</param>
    public static NumberInputBase Disabled(this NumberInputBase widget, bool enabled = true)
    {
        return widget with { Disabled = enabled };
    }

    /// <summary>Sets the minimum allowed value for the number input.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="min">The minimum value that can be entered.</param>
    public static NumberInputBase Min(this NumberInputBase widget, double min)
    {
        return widget with { Min = min };
    }

    /// <summary>Sets the maximum allowed value for the number input.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="max">The maximum value that can be entered.</param>
    public static NumberInputBase Max(this NumberInputBase widget, double max)
    {
        return widget with { Max = max };
    }

    /// <summary>Sets the step increment for the number input (used for sliders and increment/decrement buttons).</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="step">The step value for incremental changes.</param>
    public static NumberInputBase Step(this NumberInputBase widget, double step)
    {
        return widget with { Step = step };
    }

    /// <summary>Sets the visual variant of the number input.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="variant">The visual variant (Number or Slider).</param>
    public static NumberInputBase Variant(this NumberInputBase widget, NumberInputs variant)
    {
        return widget with { Variant = variant };
    }

    /// <summary>Sets the number of decimal places to display and accept.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="precision">The number of decimal places for precision control.</param>
    public static NumberInputBase Precision(this NumberInputBase widget, int precision)
    {
        return widget with { Precision = precision };
    }

    /// <summary>Sets the formatting style for displaying numeric values.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="formatStyle">The format style (Decimal, Currency, or Percent).</param>
    public static NumberInputBase FormatStyle(this NumberInputBase widget, NumberFormatStyle formatStyle)
    {
        return widget with { FormatStyle = formatStyle };
    }

    /// <summary>Sets the currency code for currency formatting.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="currency">The currency code (e.g., "USD", "EUR", "GBP").</param>
    public static NumberInputBase Currency(this NumberInputBase widget, string currency)
    {
        return widget with { Currency = currency };
    }

    /// <summary>Sets the validation error message for the number input.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="invalid">The validation error message to display.</param>
    public static NumberInputBase Invalid(this NumberInputBase widget, string invalid)
    {
        return widget with { Invalid = invalid };
    }

    /// <summary>Sets the size of the number input. </summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="size">The size to apply to the input.</param>
    public static NumberInputBase Size(this NumberInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    /// <summary>Sets the number input size to large for prominent display.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <returns>A new NumberInputBase instance with large size applied.</returns>
    [RelatedTo(nameof(NumberInputBase.Size))]
    public static NumberInputBase Large(this NumberInputBase widget)
    {
        return widget.Size(Sizes.Large);
    }

    /// <summary>Sets the number input size to small for compact display.</summary>
    /// <param name="widget">The number input to configure.</param>
    /// <returns>A new NumberInputBase instance with small size applied.</returns>
    [RelatedTo(nameof(NumberInputBase.Size))]
    public static NumberInputBase Small(this NumberInputBase widget)
    {
        return widget.Size(Sizes.Small);
    }


    /// <summary>
    /// Sets the blur event handler for the number input.
    /// This method allows you to configure the number input's blur behavior,
    /// enabling it to perform custom actions when the input loses focus.
    /// </summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static NumberInputBase HandleBlur(this NumberInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary>
    /// Sets the blur event handler for the number input.
    /// Compatibility overload for Action-based event handlers.
    /// </summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static NumberInputBase HandleBlur(this NumberInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the number input.
    /// This method allows you to configure the number input's blur behavior with
    /// a simple action that doesn't require the input event context.
    /// </summary>
    /// <param name="widget">The number input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static NumberInputBase HandleBlur(this NumberInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}