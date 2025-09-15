using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual variants available for date and time input controls.
/// Each variant provides a different interface for selecting dates, times, or both.
/// </summary>
public enum DateTimeInputs
{
    /// <summary>Date-only input for selecting calendar dates without time components.</summary>
    Date,
    /// <summary>Combined date and time input for selecting both date and time values.</summary>
    DateTime,
    /// <summary>Time-only input for selecting time values without date components.</summary>
    Time
}

/// <summary>
/// Interface for date and time input controls.
/// </summary>
public interface IAnyDateTimeInput : IAnyInput
{
    /// <summary>Gets or sets the visual variant of the date/time input.</summary>
    public DateTimeInputs Variant { get; set; }

    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets the date/time format string for displaying and parsing values.</summary>
    public string? Format { get; set; }
}

/// <summary>
/// Abstract base class for date and time input controls.
/// </summary>
public abstract record DateTimeInputBase : WidgetBase<DateTimeInputBase>, IAnyDateTimeInput
{
    /// <summary>Gets or sets the visual variant of the date/time input.</summary>
    [Prop] public DateTimeInputs Variant { get; set; }

    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    [Prop] public string? Placeholder { get; set; }

    /// <summary>Gets or sets the date/time format string for displaying and parsing values.</summary>
    [Prop] public string? Format { get; set; }

    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns the types that this date/time input can bind to and work with.
    /// Supports various .NET date and time types with automatic conversion between them.
    /// </summary>
    public Type[] SupportedStateTypes() =>
    [
        // DateTime types
        typeof(DateTime), typeof(DateTime?),
        typeof(DateTimeOffset), typeof(DateTimeOffset?),
        typeof(DateOnly), typeof(DateOnly?),
        typeof(TimeOnly), typeof(TimeOnly?),
    ];
}

/// <summary>
/// Generic date and time input control.
/// </summary>
/// <typeparam name="TDate">The type of the date/time value.</typeparam>
public record DateTimeInput<TDate> : DateTimeInputBase, IInput<TDate>
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the date/time input.</param>
    [OverloadResolutionPriority(1)]
    public DateTimeInput(IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TDate>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial date/time value.</param>
    /// <param name="onChange">Event handler called when the date/time value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the date/time input.</param>
    [OverloadResolutionPriority(1)]
    public DateTimeInput(TDate value, Func<Event<IInput<TDate>, TDate>, ValueTask> onChange, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial date/time value.</param>
    /// <param name="onChange">Event handler called when the date/time value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the date/time input.</param>
    public DateTimeInput(TDate value, Action<Event<IInput<TDate>, TDate>> onChange, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the date/time input.</param>
    public DateTimeInput(string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date)
    {
        Variant = variant;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    /// <summary>Gets or sets the current date/time value.</summary>
    [Prop] public TDate Value { get; set; } = default!;

    /// <summary>Gets or sets whether the input accepts null values.</summary>
    [Prop] public bool Nullable { get; set; } = typeof(TDate) == typeof(DateTime?) || typeof(TDate) == typeof(DateTimeOffset?) || typeof(TDate) == typeof(DateOnly?) || typeof(TDate) == typeof(TimeOnly?);

    /// <summary>Gets or sets the event handler called when the date/time value changes.</summary>
    [Event] public Func<Event<IInput<TDate>, TDate>, ValueTask>? OnChange { get; set; }
}

/// <summary>
/// Provides extension methods for creating and configuring date/time inputs with fluent syntax.
/// </summary>
public static class DateTimeInputExtensions
{
    /// <summary>
    /// Creates a date-only input from a state object with automatic type conversion.
    /// Convenience method that creates a date input with the Date variant.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the date input.</param>
    public static DateTimeInputBase ToDateInput(this IAnyState state, string? placeholder = null, bool disabled = false,
        DateTimeInputs variant = DateTimeInputs.Date)
        => ToDateTimeInput(state, placeholder, disabled, variant);

    /// <summary>
    /// Creates a date/time input from a state object with comprehensive type conversion and nullable handling.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the date/time input.</param>
    public static DateTimeInputBase ToDateTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.DateTime)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        // Create the appropriate DateTimeInput
        if (isNullable)
        {
            var dateValue = ConvertToDateValue<object?>(state);
            var input = new DateTimeInput<object?>(dateValue, e => SetStateValue(state, e.Value), placeholder, disabled, variant);
            input.ScaffoldDefaults(null!, stateType);
            input.Nullable = true;
            return input;
        }
        else
        {
            var dateValue = ConvertToDateValue<object>(state);
            var input = new DateTimeInput<object>(dateValue, e => SetStateValue(state, e.Value), placeholder, disabled, variant);
            input.ScaffoldDefaults(null!, stateType);
            return input;
        }
    }

    private static T ConvertToDateValue<T>(IAnyState state)
    {
        var stateType = state.GetStateType();
        var value = state.As<object>().Value;

        // Convert to appropriate date value
        var dateValue = stateType switch
        {
            // DateTime types - direct conversion
            _ when stateType == typeof(DateTime) => value,
            _ when stateType == typeof(DateTime?) => value,
            _ when stateType == typeof(DateTimeOffset) => value,
            _ when stateType == typeof(DateTimeOffset?) => value,
            _ when stateType == typeof(DateOnly) => value,
            _ when stateType == typeof(DateOnly?) => value,
            _ when stateType == typeof(TimeOnly) => value,
            _ when stateType == typeof(TimeOnly?) => value,

            // Other types - try BestGuessConvert
            _ => Core.Utils.BestGuessConvert(value, typeof(DateTime)) ?? DateTime.Now
        };

        return (T)dateValue!;
    }

    private static void SetStateValue(IAnyState state, object? dateValue)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        // Convert date value back to the original state type.
        var convertedValue = stateType switch
        {
            // DateTime types - direct conversion
            _ when stateType == typeof(DateTime) =>
                dateValue is DateTime dt ? dt :
                dateValue is string s ? DateTime.Parse(s) :
                DateTime.Now,
            _ when stateType == typeof(DateTime?) =>
                dateValue is null ? null :
                dateValue is DateTime dt ? dt :
                dateValue is string s ? DateTime.Parse(s) :
                (DateTime?)DateTime.Now,
            _ when stateType == typeof(DateTimeOffset) => dateValue ?? DateTimeOffset.Now,
            _ when stateType == typeof(DateTimeOffset?) => dateValue,
            _ when stateType == typeof(DateOnly) =>
                dateValue is DateOnly d ? d :
                dateValue is string s ? DateOnly.FromDateTime(DateTime.Parse(s)) :
                dateValue is DateTime dt ? DateOnly.FromDateTime(dt) :
                DateOnly.FromDateTime(DateTime.Now),
            _ when stateType == typeof(DateOnly?) =>
                dateValue is null ? null :
                dateValue is DateOnly d ? d :
                dateValue is string s ? DateOnly.FromDateTime(DateTime.Parse(s)) :
                dateValue is DateTime dt ? DateOnly.FromDateTime(dt) :
                (DateOnly?)DateOnly.FromDateTime(DateTime.Now),
            _ when stateType == typeof(TimeOnly) =>
                dateValue is TimeOnly t ? t :
                dateValue is string s ? ParseTimeOnly(s) :
                dateValue is DateTime dt ? TimeOnly.FromDateTime(dt) :
                TimeOnly.FromDateTime(DateTime.Now),
            _ when stateType == typeof(TimeOnly?) =>
                dateValue is null ? null :
                dateValue is string s && string.IsNullOrWhiteSpace(s) ? null :
                dateValue is TimeOnly t ? t :
                dateValue is string s2 ? ParseTimeOnly(s2) :
                dateValue is DateTime dt ? TimeOnly.FromDateTime(dt) :
                (TimeOnly?)TimeOnly.FromDateTime(DateTime.Now),
            _ when stateType == typeof(string) => dateValue?.ToString() ?? DateTime.Now.ToString("O"),

            // Other types - use BestGuessConvert
            _ => Core.Utils.BestGuessConvert(dateValue, stateType) ?? DateTime.Now
        };

        // Set the state value.
        state.As<object>().Set(convertedValue!);
    }

    private static TimeOnly ParseTimeOnly(string timeString)
    {
        // Try different time formats
        var formats = new[] { "HH:mm:ss", "HH:mm", "H:mm:ss", "H:mm" };

        foreach (var format in formats)
        {
            if (TimeOnly.TryParseExact(timeString, format, null, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        // If all parsing fails, return current time.
        return TimeOnly.FromDateTime(DateTime.Now);
    }

    /// <summary>
    /// Creates a time-only input from a state object with automatic type conversion.
    /// This is the recommended way to create a time input.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public static DateTimeInputBase ToTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false)
        => state.ToDateTimeInput(placeholder, disabled, DateTimeInputs.Time);

    /// <summary>
    /// Internal method for setting default values during scaffolding operations.
    /// </summary>
    /// <param name="input">The date/time input to configure.</param>
    /// <param name="name">The property name to use for generating placeholder text.</param>
    /// <param name="type">The property type for additional context.</param>
    internal static IAnyDateTimeInput ScaffoldDefaults(this IAnyDateTimeInput input, string? name, Type type)
    {
        if (string.IsNullOrEmpty(input.Placeholder)
            && !string.IsNullOrEmpty(name))
        {
            input.Placeholder = Utils.LabelFor(name, type);
        }

        return input;
    }

    /// <summary>Sets the disabled state of the date/time input.</summary>
    /// <param name="widget">The date/time input to configure.</param>
    /// <param name="disabled">Whether the input should be disabled.</param>
    public static DateTimeInputBase Disabled(this DateTimeInputBase widget, bool disabled = true) => widget with { Disabled = disabled };

    /// <summary>Sets the visual variant of the date/time input.</summary>
    /// <param name="widget">The date/time input to configure.</param>
    /// <param name="variant">The visual variant (Date, DateTime, or Time).</param>
    public static DateTimeInputBase Variant(this DateTimeInputBase widget, DateTimeInputs variant) => widget with { Variant = variant };

    /// <summary>Sets the placeholder text for the date/time input.</summary>
    /// <param name="widget">The date/time input to configure.</param>
    /// <param name="placeholder">The placeholder text to display when the input is empty.</param>
    public static DateTimeInputBase Placeholder(this DateTimeInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    /// <summary>Sets the date/time format string for the input.</summary>
    /// <param name="widget">The date/time input to configure.</param>
    /// <param name="format">The format string (e.g., "yyyy-MM-dd", "HH:mm:ss") for displaying and parsing values.</param>
    public static DateTimeInputBase Format(this DateTimeInputBase widget, string format) => widget with { Format = format };

    /// <summary>Sets the validation error message for the date/time input.</summary>
    /// <param name="widget">The date/time input to configure.</param>
    /// <param name="invalid">The validation error message to display.</param>
    public static DateTimeInputBase Invalid(this DateTimeInputBase widget, string? invalid) => widget with { Invalid = invalid };


    /// <summary>
    /// Sets the blur event handler for the date/time input.
    /// </summary>
    /// <param name="widget">The date/time input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static DateTimeInputBase HandleBlur(this DateTimeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary>
    /// Sets the blur event handler for the date/time input.
    /// </summary>
    /// <param name="widget">The date/time input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static DateTimeInputBase HandleBlur(this DateTimeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the date/time input.
    /// </summary>
    /// <param name="widget">The date/time input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static DateTimeInputBase HandleBlur(this DateTimeInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}