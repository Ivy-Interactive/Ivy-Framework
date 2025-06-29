using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

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
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }
    public Type[] SupportedStateTypes() => [];
}

public record DateRangeInput<TDateRange> : DateRangeInputBase, IInput<TDateRange>
{
    public DateRangeInput(IAnyState state, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        var typedState = state.As<TDateRange>();
        Value = typedState.Value;
        OnChange = e => typedState.Set(e.Value);
    }

    public DateRangeInput(TDateRange value, Action<Event<IInput<TDateRange>, TDateRange>> onChange, string? placeholder = null, bool disabled = false) : this(placeholder, disabled)
    {
        OnChange = onChange;
        Value = value;
    }

    public DateRangeInput(string? placeholder = null, bool disabled = false)
    {
        Placeholder = placeholder;
        Disabled = disabled;
    }

    [Prop] public TDateRange Value { get; set; } = default!;
    [Event] public Action<Event<IInput<TDateRange>, TDateRange>>? OnChange { get; set; }
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

    public static DateRangeInputBase Format(this DateRangeInputBase widget, string format)
    {
        return widget with { Format = format };
    }

    public static DateRangeInputBase Invalid(this DateRangeInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }
}