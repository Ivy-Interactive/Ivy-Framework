using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum DateTimeInputs
{
    Date,
    DateTime,
    Time
}

public interface IAnyDateTimeInput : IAnyInput
{
    public DateTimeInputs Variant { get; set; }
    public string? Placeholder { get; set; }
    public string? Format { get; set; }
}

public abstract record DateTimeInputBase : WidgetBase<DateTimeInputBase>, IAnyDateTimeInput
{
    [Prop] public DateTimeInputs Variant { get; set; }
    [Prop] public string? Placeholder { get; set; }
    [Prop] public string? Format { get; set; }
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }
    public Type[] SupportedStateTypes() => [];
}

public record DateTimeInput<TDate> : DateTimeInputBase, IInput<TDate>
{
    public DateTimeInput(IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TDate>();
        Value = typedState.Value;
        OnChange = e => typedState.Set(e.Value);
    }

    public DateTimeInput(TDate value, Action<Event<IInput<TDate>, TDate>> onChange, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public DateTimeInput(string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date)
    {
        Variant = variant;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    [Prop] public TDate Value { get; set; } = default!;
    [Prop] public bool Nullable { get; set; } = typeof(TDate) == typeof(DateTime?) || typeof(TDate) == typeof(DateTimeOffset?) || typeof(TDate) == typeof(DateOnly?);
    [Event] public Action<Event<IInput<TDate>, TDate>>? OnChange { get; set; }
}

public static class DateTimeInputExtensions
{
    public static DateTimeInputBase ToDateTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date)
    {
        var type = state.GetStateType();
        Type genericType = typeof(DateTimeInput<>).MakeGenericType(type);
        DateTimeInputBase input = (DateTimeInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        return input;
    }

    public static DateTimeInputBase Title(this DateTimeInputBase widget, string title) => widget with { Placeholder = title };
    public static DateTimeInputBase Disabled(this DateTimeInputBase widget, bool disabled = true) => widget with { Disabled = disabled };
    public static DateTimeInputBase Variant(this DateTimeInputBase widget, DateTimeInputs variant) => widget with { Variant = variant };
    public static DateTimeInputBase Placeholder(this DateTimeInputBase widget, string placeholder) => widget with { Placeholder = placeholder };
    public static DateTimeInputBase Format(this DateTimeInputBase widget, string format) => widget with { Format = format };
    public static DateTimeInputBase Invalid(this DateTimeInputBase widget, string? invalid) => widget with { Invalid = invalid };
}