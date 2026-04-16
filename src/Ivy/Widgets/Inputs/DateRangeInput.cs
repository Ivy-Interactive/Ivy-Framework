using System.Runtime.CompilerServices;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyDateRangeInput : IAnyInput
{
    public string? Format { get; set; }
}

public abstract record DateRangeInputBase : WidgetBase<DateRangeInputBase>, IAnyDateRangeInput
{
    [Prop] public string? Placeholder { get; set; }

    [Prop] public string? StartPlaceholder { get; set; }

    [Prop] public string? EndPlaceholder { get; set; }

    [Prop] public string? Format { get; set; }

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public DayOfWeek? FirstDayOfWeek { get; set; }

    [Prop] public DateOnly? Min { get; set; }

    [Prop] public DateOnly? Max { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public Type[] SupportedStateTypes() =>
[
    typeof((DateOnly, DateOnly)), typeof((DateOnly?, DateOnly?)),
    ];
}

/// <summary>
/// An input for selecting a start and end date.
/// </summary>
public record DateRangeInput<TDateRange> : DateRangeInputBase, IInput<TDateRange>
{
    [OverloadResolutionPriority(1)]
    internal DateRangeInput(IAnyState state, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        var typedState = state.As<TDateRange>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    [OverloadResolutionPriority(1)]
    internal DateRangeInput(TDateRange value, Func<Event<IInput<TDateRange>, TDateRange>, ValueTask> onChange, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    internal DateRangeInput(TDateRange value, Action<Event<IInput<TDateRange>, TDateRange>> onChange, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        OnChange = new(e => { onChange(e); return ValueTask.CompletedTask; });
        Value = value;
        Nullable = typeof(TDateRange) == typeof((DateOnly?, DateOnly?));
    }

    internal DateRangeInput(string? placeholder = null, bool disabled = false)
    {
        Placeholder = placeholder;
        Disabled = disabled;
    }

    internal DateRangeInput() { }

    [Prop(AlwaysSerialize = true)] public TDateRange Value { get; init; } = default!;

    [Event] public EventHandler<Event<IInput<TDateRange>, TDateRange>>? OnChange { get; set; }
}

public static class DateRangeInputExtensions
{
    public static DateRangeInputBase ToDateRangeInput(this IAnyState state, string? placeholder = null, bool disabled = false)
    {
        var type = state.GetStateType();

        if (!type.IsGenericType || type.GetGenericArguments().Length != 2)
        {
            throw new Exception("DateRangeInput can only be used with a tuple of two elements");
        }

        Type genericType = typeof(DateRangeInput<>).MakeGenericType(type);
        DateRangeInputBase input = (DateRangeInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, placeholder, disabled }, null)!;
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

    public static DateRangeInputBase StartPlaceholder(this DateRangeInputBase widget, string placeholder)
    {
        return widget with { StartPlaceholder = placeholder };
    }

    public static DateRangeInputBase EndPlaceholder(this DateRangeInputBase widget, string placeholder)
    {
        return widget with { EndPlaceholder = placeholder };
    }

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

    public static DateRangeInputBase FirstDayOfWeek(this DateRangeInputBase widget, DayOfWeek day)
    {
        return widget with { FirstDayOfWeek = day };
    }

    public static DateRangeInputBase Min(this DateRangeInputBase widget, DateOnly min)
    {
        return widget with { Min = min };
    }

    public static DateRangeInputBase Max(this DateRangeInputBase widget, DateOnly max)
    {
        return widget with { Max = max };
    }

    [OverloadResolutionPriority(1)]
    public static DateRangeInputBase OnBlur(this DateRangeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static DateRangeInputBase OnBlur(this DateRangeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.OnBlur(onBlur.ToValueTask());
    }

    public static DateRangeInputBase OnBlur(this DateRangeInputBase widget, Action onBlur)
    {
        return widget.OnBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public static DateRangeInputBase OnFocus(this DateRangeInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        return widget with { OnFocus = new(onFocus) };
    }

    public static DateRangeInputBase OnFocus(this DateRangeInputBase widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget.OnFocus(onFocus.ToValueTask());
    }

    public static DateRangeInputBase OnFocus(this DateRangeInputBase widget, Action onFocus)
    {
        return widget.OnFocus(_ => { onFocus(); return ValueTask.CompletedTask; });
    }

    public static DateRangeInputBase Prefix(this DateRangeInputBase widget, object prefix)
        => widget with { Children = widget.WithSlot("Prefix", prefix) };

    public static DateRangeInputBase Suffix(this DateRangeInputBase widget, object suffix)
        => widget with { Children = widget.WithSlot("Suffix", suffix) };

}