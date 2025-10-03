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
/// Defines the visual variants available for boolean input controls.
/// </summary>
public enum BoolInputs
{
    /// <summary>Traditional checkbox with checkbox indicator for true/false selection.</summary>
    Checkbox,
    /// <summary>Toggle switch with sliding indicator for on/off states.</summary>
    Switch,
    /// <summary>Toggle button that can display custom icons and labels.</summary>
    Toggle
}

/// <summary>
/// Interface for boolean input controls.
/// </summary>
public interface IAnyBoolInput : IAnyInput
{
    /// <summary>Gets or sets the label text displayed alongside the boolean input.</summary>
    public string? Label { get; set; }

    /// <summary>Gets or sets the description or help text for the boolean input.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the visual variant of the boolean input.</summary>
    public BoolInputs Variant { get; set; }

    /// <summary>Gets or sets the icon displayed with the boolean input (primarily for Toggle variant).</summary>
    public Icons Icon { get; set; }
}

/// <summary>
/// Abstract base class for boolean input controls that provides common functionality
/// and properties shared across all boolean input variants. Supports automatic conversion
/// between boolean values and various numeric types for flexible state binding.
/// </summary>
public abstract record BoolInputBase : WidgetBase<BoolInputBase>, IAnyBoolInput
{
    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the label text displayed alongside the input.</summary>
    [Prop] public string? Label { get; set; }

    /// <summary>Gets or sets the description or help text for the input.</summary>
    [Prop] public string? Description { get; set; }

    /// <summary>Gets or sets the visual variant of the boolean input.</summary>
    [Prop] public BoolInputs Variant { get; set; }

    /// <summary>Gets or sets the icon displayed with the input.</summary>
    [Prop] public Icons Icon { get; set; }

    /// <summary>Gets or sets the size of the boolean input.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns the types that this boolean input can bind to.
    /// </summary>
    /// <returns>An array of supported types.</returns>
    public Type[] SupportedStateTypes() =>
    [
        // Boolean types
        typeof(bool), typeof(bool?),
        // Signed integer types
        typeof(short), typeof(short?),
        typeof(int), typeof(int?),
        typeof(long), typeof(long?),
        // Unsigned integer types
        typeof(byte), typeof(byte?),
        // Floating-point types
        typeof(float), typeof(float?),
        typeof(double), typeof(double?),
        typeof(decimal), typeof(decimal?)
    ];
}

/// <summary>
/// Generic boolean input control.
/// </summary>
/// <typeparam name="TBool">The type of the boolean value.</typeparam>
public record BoolInput<TBool> : BoolInputBase, IInput<TBool>
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    [OverloadResolutionPriority(1)]
    public BoolInput(IAnyState state, string? label = null, bool disabled = false,
        BoolInputs variant = BoolInputs.Checkbox)
        : this(label, disabled, variant)
    {
        var typedState = state.As<TBool>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial boolean value.</param>
    /// <param name="onChange">Event handler called when the value changes.</param>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    [OverloadResolutionPriority(1)]
    public BoolInput(TBool value, Func<Event<IInput<TBool>, TBool>, ValueTask> onChange, string? label = null,
        bool disabled = false, BoolInputs variant = BoolInputs.Checkbox) : this(label, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial boolean value.</param>
    /// <param name="onChange">Event handler called when the value changes.</param>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    public BoolInput(TBool value, Action<Event<IInput<TBool>, TBool>> onChange, string? label = null,
        bool disabled = false, BoolInputs variant = BoolInputs.Checkbox) : this(label, disabled, variant)
    {
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    public BoolInput(string? label = null, bool disabled = false, BoolInputs variant = BoolInputs.Checkbox)
    {
        Label = label;
        Disabled = disabled;
        Variant = variant;
    }

    /// <summary>Gets the current boolean value.</summary>
    [Prop] public TBool Value { get; } = default!;

    /// <summary>Gets or sets whether the input accepts null values.</summary>
    [Prop] public bool Nullable { get; set; } = typeof(TBool) == typeof(bool?);

    /// <summary>Gets the event handler called when the boolean value changes.</summary>
    [Event] public Func<Event<IInput<TBool>, TBool>, ValueTask>? OnChange { get; }
}

/// <summary>
/// Concrete boolean input control for standard boolean values.
/// </summary>
public record BoolInput : BoolInput<bool>
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    [OverloadResolutionPriority(1)]
    public BoolInput(IAnyState state, string? label = null, bool disabled = false,
        BoolInputs variant = BoolInputs.Checkbox)
        : base(state, label, disabled, variant)
    {
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial boolean value.</param>
    /// <param name="onChange">Event handler called when the value changes.</param>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    [OverloadResolutionPriority(1)]
    public BoolInput(bool value, Func<Event<IInput<bool>, bool>, ValueTask> onChange, string? label = null,
        bool disabled = false, BoolInputs variant = BoolInputs.Checkbox)
        : base(value, onChange, label, disabled, variant)
    {
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial boolean value.</param>
    /// <param name="onChange">Event handler called when the value changes.</param>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    public BoolInput(bool value, Action<Event<IInput<bool>, bool>> onChange, string? label = null,
        bool disabled = false, BoolInputs variant = BoolInputs.Checkbox)
        : base(value, onChange, label, disabled, variant)
    {
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    public BoolInput(string? label = null, bool disabled = false, BoolInputs variant = BoolInputs.Checkbox)
        : base(label, disabled, variant)
    {
    }
}

/// <summary>
/// Provides extension methods for creating and configuring boolean inputs with fluent syntax.
/// </summary>
public static class BoolInputExtensions
{
    /// <summary>
    /// Creates a boolean input from a state object with automatic type conversion.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="label">Optional label text displayed alongside the input.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the boolean input.</param>
    /// <returns>A boolean input bound to the state object with automatic type conversion.</returns>
    public static BoolInputBase ToBoolInput(this IAnyState state, string? label = null, bool disabled = false,
        BoolInputs variant = BoolInputs.Checkbox)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        if (isNullable)
        {
            var boolValue = ConvertToBoolValue<bool?>(state);
            var input = new BoolInput<bool?>(boolValue, e => SetStateValue(state, e.Value), label, disabled, variant);
            input.ScaffoldDefaults(null!, stateType);
            return input;
        }
        else
        {
            var boolValue = ConvertToBoolValue<bool>(state);
            var input = new BoolInput<bool>(boolValue, e => SetStateValue(state, e.Value), label, disabled, variant);
            input.ScaffoldDefaults(null!, stateType);
            return input;
        }
    }

    private static T ConvertToBoolValue<T>(IAnyState state)
    {
        var stateType = state.GetStateType();
        var value = state.As<object>().Value;

        // Convert to boolean based on type
        var boolValue = stateType switch
        {
            // Boolean types - direct conversion
            _ when stateType == typeof(bool) => (bool)value,
            _ when stateType == typeof(bool?) => (bool?)value,

            // Numeric types - convert to boolean (0 = false, non-zero = true)
            // Expression value==null should always be null (suggestion by IntelliJ), but in this case it is a valid check.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            _ when stateType.IsNumeric() && stateType.IsNullableType() => value == null ? null : Convert.ToBoolean(value),
            _ when stateType.IsNumeric() => Convert.ToBoolean(value),

            // Other types - try BestGuessConvert, fallback to false
            _ => Core.Utils.BestGuessConvert(value, typeof(bool)) is true
        };

        // Handle the return type T appropriately
        if (typeof(T) == typeof(bool?))
        {
            return (T)(object)boolValue!;
        }

        // For non-nullable bool, convert null to false
        var nonNullableBool = boolValue ?? false;
        return (T)(object)nonNullableBool;
    }

    private static void SetStateValue(IAnyState state, bool boolValue)
    {
        SetStateValue(state, (bool?)boolValue);
    }

    private static void SetStateValue(IAnyState state, bool? boolValue)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        // Convert boolean back to the original state type
        var convertedValue = stateType switch
        {
            // Boolean types - direct conversion
            _ when stateType == typeof(bool) => boolValue ?? false,
            _ when stateType == typeof(bool?) => boolValue,

            // Numeric types - convert boolean to numeric
            _ when stateType.IsNumeric() => ConvertBoolToNumeric(boolValue, stateType, isNullable),

            // Other types - use BestGuessConvert
            _ => Core.Utils.BestGuessConvert(boolValue ?? false, stateType) ?? false
        };

        // Set the state value
        state.As<object>().Set(convertedValue!);
    }

    private static object ConvertBoolToNumeric(bool? boolValue, Type targetType, bool isNullable)
    {
        if (isNullable)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (boolValue == null)
            {
                return null!;
            }

            var numericValue = boolValue == true ? 1 : 0;
            return Convert.ChangeType(numericValue, underlyingType!);
        }
        else
        {
            var numericValue = boolValue == true ? 1 : 0;
            return Convert.ChangeType(numericValue, targetType);
        }
    }

    /// <summary>
    /// Creates a switch-style boolean input.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="label">Optional label text displayed alongside the switch.</param>
    /// <param name="disabled">Whether the switch should be disabled initially.</param>
    /// <returns>A switch-style boolean input bound to the state object.</returns>
    public static BoolInputBase ToSwitchInput(this IAnyState state, string? label = null, bool disabled = false)
        => state.ToBoolInput(label, disabled, BoolInputs.Switch);

    /// <summary>
    /// Creates a toggle-style boolean input.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="icon">Optional icon to display on the toggle button.</param>
    /// <param name="label">Optional label text displayed alongside the toggle.</param>
    /// <param name="disabled">Whether the toggle should be disabled initially.</param>
    /// <returns>A toggle-style boolean input bound to the state object.</returns>
    public static BoolInputBase ToToggleInput(this IAnyState state, Icons? icon = null, string? label = null,
        bool disabled = false)
    {
        var input = state.ToBoolInput(label, disabled, BoolInputs.Toggle);
        if (icon != null)
        {
            input.Icon = icon.Value;
        }

        return input;
    }

    internal static IAnyBoolInput ScaffoldDefaults(this IAnyBoolInput input, string? name, Type type)
    {
        if (string.IsNullOrEmpty(input.Label))
        {
            input.Label = Utils.SplitPascalCase(name) ?? name;
        }

        return input;
    }

    /// <summary>Sets the label text.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="label">The label text to display.</param>
    public static BoolInputBase Label(this BoolInputBase widget, string label) => widget with { Label = label };

    /// <summary>Sets the disabled state.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="disabled">Whether the input should be disabled.</param>
    public static BoolInputBase Disabled(this BoolInputBase widget, bool disabled = true) =>
        widget with { Disabled = disabled };

    /// <summary>Sets the visual variant.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="variant">The visual variant (Checkbox, Switch, or Toggle).</param>
    public static BoolInputBase Variant(this BoolInputBase widget, BoolInputs variant) =>
        widget with { Variant = variant };

    /// <summary>Sets the icon.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="icon">The icon to display.</param>
    public static BoolInputBase Icon(this BoolInputBase widget, Icons icon) => widget with { Icon = icon };

    /// <summary>Sets the description text.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="description">The description or help text to display.</param>
    public static BoolInputBase Description(this BoolInputBase widget, string description) =>
        widget with { Description = description };

    /// <summary>Sets the size of the boolean input.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="size">The size of the boolean input.</param>
    public static BoolInputBase Size(this BoolInputBase widget, Sizes size) =>
        widget with { Size = size };

    /// <summary>Sets the boolean input size to large for prominent display.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <returns>A new BoolInputBase instance with large size applied.</returns>
    public static BoolInputBase Large(this BoolInputBase widget) =>
        widget.Size(Sizes.Large);

    /// <summary>Sets the boolean input size to small for compact display.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <returns>A new BoolInputBase instance with small size applied.</returns>
    public static BoolInputBase Small(this BoolInputBase widget) =>
        widget.Size(Sizes.Small);

    /// <summary>Sets the validation error message.</summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="invalid">The validation error message, or null to clear the error.</param>
    public static BoolInputBase Invalid(this BoolInputBase widget, string? invalid) =>
        widget with { Invalid = invalid };

    /// <summary>
    /// Sets the blur event handler.
    /// </summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static BoolInputBase HandleBlur(this BoolInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary>
    /// Sets the blur event handler for the boolean input.
    /// </summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static BoolInputBase HandleBlur(this BoolInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the boolean input.
    /// </summary>
    /// <param name="widget">The boolean input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static BoolInputBase HandleBlur(this BoolInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}