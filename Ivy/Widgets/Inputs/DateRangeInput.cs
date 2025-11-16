using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyDateRangeInput : IAnyInput
{
    public string? Placeholder { get; set; }

    public string? Format { get; set; }
}

public abstract record DateRangeInputBase : WidgetBase<DateRangeInputBase>, IAnyDateRangeInput
{
    [Prop] public string? Placeholder { get; set; }

    [Prop] public string? Format { get; set; }

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>Only DateOnly tuple types.</summary>
    public Type[] SupportedStateTypes() =>
    [
        typeof((DateOnly, DateOnly)), typeof((DateOnly?, DateOnly?)),
    ];
}

/// <typeparam name="TDateRange">(DateOnly, DateOnly) or (DateOnly?, DateOnly?).</typeparam>
public record DateRangeInput<TDateRange> : DateRangeInputBase, IInput<TDateRange>
{
    [OverloadResolutionPriority(1)]
    public DateRangeInput(IAnyState state, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        var typedState = state.As<TDateRange>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    [OverloadResolutionPriority(1)]
    public DateRangeInput(TDateRange value, Func<Event<IInput<TDateRange>, TDateRange>, ValueTask> onChange, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        OnChange = onChange;
        Value = value;
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    public DateRangeInput(TDateRange value, Action<Event<IInput<TDateRange>, TDateRange>> onChange, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    public DateRangeInput(string? placeholder = null, bool disabled = false)
    {
        Placeholder = placeholder;
        Disabled = disabled;
    }

    [Prop] public TDateRange Value { get; set; } = default!;

    [Event] public Func<Event<IInput<TDateRange>, TDateRange>, ValueTask>? OnChange { get; set; }
}

public static class DateRangeInputExtensions
{
    /// <summary>Creates a date range input from a state object with automatic tuple type validation.</summary>
    public static DateRangeInputBase ToDateRangeInput(this IAnyState state, string? placeholder = null, bool disabled = false)
    {
        var type = state.GetStateType();

        if (!type.IsGenericType || type.GetGenericArguments().Length != 2)
        {
            throw new Exception("DateRangeInput can only be used with a tuple of two elements");
        }

        Type genericType = typeof(DateRangeInput<>).MakeGenericType(type);
        DateRangeInputBase input = (DateRangeInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled)!;
        return input;
    }

    public static DateRangeInputBase Disabled(this DateRangeInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static DateRangeInputBase Placeholder(this DateRangeInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    /// <summary>Format string (e.g., "yyyy-MM-dd", "MM/dd/yyyy") for displaying and parsing dates.</summary>
    public static DateRangeInputBase Format(this DateRangeInputBase widget, string format)
    {
        return widget with { Format = format };
    }

    public static DateRangeInputBase Invalid(this DateRangeInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static DateRangeInputBase Nullable(this DateRangeInputBase widget, bool nullable = true)
    {
        return widget with { Nullable = nullable };
    }

    [OverloadResolutionPriority(1)]
    public static DateRangeInputBase HandleBlur(this DateRangeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    public static DateRangeInputBase HandleBlur(this DateRangeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static DateRangeInputBase HandleBlur(this DateRangeInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    public static DateRangeInputBase Size(this DateRangeInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    public static DateRangeInputBase Large(this DateRangeInputBase widget)
    {
        return widget.Size(Sizes.Large);
    }

    public static DateRangeInputBase Small(this DateRangeInputBase widget)
    {
        return widget.Size(Sizes.Small);
    }
}