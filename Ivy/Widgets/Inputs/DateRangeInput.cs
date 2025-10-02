using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Interface for date range input controls.
/// </summary>
public interface IAnyDateRangeInput : IAnyInput
{
    /// <summary>Gets or sets the placeholder text displayed when the date range input is empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets the date format string for displaying and parsing dates.</summary>
    public string? Format { get; set; }
}

/// <summary>
/// Abstract base class for date range input controls.
/// </summary>
public abstract record DateRangeInputBase : WidgetBase<DateRangeInputBase>, IAnyDateRangeInput
{
    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    [Prop] public string? Placeholder { get; set; }

    /// <summary>Gets or sets the date format string for displaying and parsing dates.</summary>
    [Prop] public string? Format { get; set; }

    /// <summary>Gets or sets the size of the date range input.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets whether the input accepts null values.</summary>
    [Prop] public bool Nullable { get; set; }

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns the types that this date range input can bind to and work with.
    /// </summary>
    public Type[] SupportedStateTypes() =>
    [
        // Only DateOnly tuple types
        typeof((DateOnly, DateOnly)), typeof((DateOnly?, DateOnly?)),
    ];
}

/// <summary>
/// Generic date range input control.
/// </summary>
/// <typeparam name="TDateRange">The type of the date range tuple ((DateOnly, DateOnly) or (DateOnly?, DateOnly?)).</typeparam>
public record DateRangeInput<TDateRange> : DateRangeInputBase, IInput<TDateRange>
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    [OverloadResolutionPriority(1)]
    public DateRangeInput(IAnyState state, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        var typedState = state.As<TDateRange>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial date range value as a tuple.</param>
    /// <param name="onChange">Event handler called when the date range value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    [OverloadResolutionPriority(1)]
    public DateRangeInput(TDateRange value, Func<Event<IInput<TDateRange>, TDateRange>, ValueTask> onChange, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        OnChange = onChange;
        Value = value;
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial date range value as a tuple.</param>
    /// <param name="onChange">Event handler called when the date range value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public DateRangeInput(TDateRange value, Action<Event<IInput<TDateRange>, TDateRange>> onChange, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public DateRangeInput(string? placeholder = null, bool disabled = false)
    {
        Placeholder = placeholder;
        Disabled = disabled;
    }

    /// <summary>Gets or sets the current date range value as a tuple.</summary>
    [Prop] public TDateRange Value { get; set; } = default!;

    /// <summary>Gets or sets the event handler called when the date range value changes.</summary>
    [Event] public Func<Event<IInput<TDateRange>, TDateRange>, ValueTask>? OnChange { get; set; }
}

/// <summary>
/// Provides extension methods for creating and configuring date range inputs with fluent syntax.
/// </summary>
public static class DateRangeInputExtensions
{
    /// <summary>
    /// Creates a date range input from a state object with automatic tuple type validation.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public static DateRangeInputBase ToDateRangeInput(this IAnyState state, string? placeholder = null, bool disabled = false)
    {
        var type = state.GetStateType();

        // Check if it's a tuple type with 2 elements
        if (!type.IsGenericType || type.GetGenericArguments().Length != 2)
        {
            throw new Exception("DateRangeInput can only be used with a tuple of two elements");
        }

        Type genericType = typeof(DateRangeInput<>).MakeGenericType(type);
        DateRangeInputBase input = (DateRangeInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled)!;
        return input;
    }

    /// <summary>Sets the disabled state of the date range input.</summary>
    /// <param name="widget">The date range input to configure.</param>
    /// <param name="disabled">Whether the input should be disabled.</param>
    public static DateRangeInputBase Disabled(this DateRangeInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    /// <summary>Sets the placeholder text for the date range input.</summary>
    /// <param name="widget">The date range input to configure.</param>
    /// <param name="placeholder">The placeholder text to display when the input is empty.</param>
    public static DateRangeInputBase Placeholder(this DateRangeInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    /// <summary>Sets the date format string for the date range input.</summary>
    /// <param name="widget">The date range input to configure.</param>
    /// <param name="format">The date format string (e.g., "yyyy-MM-dd", "MM/dd/yyyy") for displaying and parsing dates.</param>
    public static DateRangeInputBase Format(this DateRangeInputBase widget, string format)
    {
        return widget with { Format = format };
    }

    /// <summary>Sets the validation error message for the date range input.</summary>
    /// <param name="widget">The date range input to configure.</param>
    /// <param name="invalid">The validation error message to display.</param>
    public static DateRangeInputBase Invalid(this DateRangeInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    /// <summary>Sets whether the date range input accepts null values.</summary>
    /// <param name="widget">The date range input to configure.</param>
    /// <param name="nullable">Whether null date ranges should be allowed.</param>
    public static DateRangeInputBase Nullable(this DateRangeInputBase widget, bool nullable = true)
    {
        return widget with { Nullable = nullable };
    }


    /// <summary>
    /// Sets the blur event handler for the date range input.
    /// </summary>
    /// <param name="widget">The date range input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static DateRangeInputBase HandleBlur(this DateRangeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary>
    /// Sets the blur event handler for the date range input.
    /// </summary>
    /// <param name="widget">The date range input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static DateRangeInputBase HandleBlur(this DateRangeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the date range input.
    /// </summary>
    /// <param name="widget">The date range input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    /// <returns>A new date range input instance with the updated blur handler.</returns>
    public static DateRangeInputBase HandleBlur(this DateRangeInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    /// <summary>Sets the size of the date range input.</summary>
    public static DateRangeInputBase Size(this DateRangeInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    /// <summary>Sets the date range input size to large for prominent display.</summary>
    public static DateRangeInputBase Large(this DateRangeInputBase widget)
    {
        return widget.Size(Sizes.Large);
    }

    /// <summary>Sets the date range input size to small for compact display.</summary>
    public static DateRangeInputBase Small(this DateRangeInputBase widget)
    {
        return widget.Size(Sizes.Small);
    }
}