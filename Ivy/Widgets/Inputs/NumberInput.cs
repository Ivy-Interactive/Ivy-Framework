using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum NumberInputs
{
    Number,
    Slider
}

public enum NumberFormatStyle
{
    Decimal,
    Currency,
    Percent
}

public interface IAnyNumberInput : IAnyInput
{
    public string? Placeholder { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public double? Step { get; set; }
    public int? Precision { get; set; }
    public NumberInputs Variant { get; set; }
    public NumberFormatStyle FormatStyle { get; set; }
    public string? Currency { get; set; }
}

public abstract record NumberInputBase : WidgetBase<NumberInputBase>, IAnyNumberInput
{
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Prop] public string? Placeholder { get; set; }
    [Prop] public double? Min { get; set; }
    [Prop] public double? Max { get; set; }
    [Prop] public double? Step { get; set; }
    [Prop] public int? Precision { get; set; }
    [Prop] public NumberInputs Variant { get; set; }
    [Prop] public NumberFormatStyle FormatStyle { get; set; }
    [Prop] public string? Currency { get; set; }
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }
    public Type[] SupportedStateTypes() => [];
}

public record NumberInput<TNumber> : NumberInputBase, IInput<TNumber>, IAnyNumberInput
{
    public NumberInput(IAnyState state, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(placeholder, disabled, variant, formatStyle)
    {
        var typedState = state.As<TNumber>();
        Value = typedState.Value;
        OnChange = e => typedState.Set(e.Value);
    }

    public NumberInput(TNumber value, Action<TNumber> state, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(placeholder, disabled, variant, formatStyle)
    {
        OnChange = e => state(e.Value);
        Value = value;
    }

    public NumberInput(string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        Placeholder = placeholder;
        Disabled = disabled;
        Variant = variant;
        FormatStyle = formatStyle;
    }

    [Prop] public TNumber Value { get; } = default!;
    [Prop] public bool Nullable { get; set; } = typeof(TNumber).IsNullableType();
    [Event] public Action<Event<IInput<TNumber>, TNumber>>? OnChange { get; }
}

public static class NumberInputExtensions
{
    public static NumberInputBase ToSliderInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        return state.ToNumberInput(placeholder, disabled, NumberInputs.Slider, formatStyle);
    }

    public static NumberInputBase ToNumberInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        var type = state.GetStateType();
        Type genericType = typeof(NumberInput<>).MakeGenericType(type);
        NumberInputBase input = (NumberInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant, formatStyle)!;
        input.ScaffoldDefaults(null, type);
        return input;
    }

    public static NumberInputBase ToMoneyInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberInputs variant = NumberInputs.Number, string currency = "USD")
        => state.ToNumberInput(placeholder, disabled, variant, NumberFormatStyle.Currency).Currency(currency);

    internal static IAnyNumberInput ScaffoldDefaults(this IAnyNumberInput input, string? name, Type type)
    {
        input.Precision ??= type.SuggestPrecision();
        input.Step ??= type.SuggestStep();
        input.Min ??= type.SuggestMin();
        input.Max ??= type.SuggestMax();

        // Add default currency for Currency style inputs
        if (input.FormatStyle == NumberFormatStyle.Currency && string.IsNullOrEmpty(input.Currency))
        {
            input.Currency = "USD";
        }

        return input;
    }

    public static NumberInputBase Placeholder(this NumberInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    public static NumberInputBase Disabled(this NumberInputBase widget, bool enabled = true)
    {
        return widget with { Disabled = enabled };
    }

    public static NumberInputBase Min(this NumberInputBase widget, double min)
    {
        return widget with { Min = min };
    }

    public static NumberInputBase Max(this NumberInputBase widget, double max)
    {
        return widget with { Max = max };
    }

    public static NumberInputBase Step(this NumberInputBase widget, double step)
    {
        return widget with { Step = step };
    }

    public static NumberInputBase Variant(this NumberInputBase widget, NumberInputs variant)
    {
        return widget with { Variant = variant };
    }

    public static NumberInputBase Precision(this NumberInputBase widget, int precision)
    {
        return widget with { Precision = precision };
    }

    public static NumberInputBase FormatStyle(this NumberInputBase widget, NumberFormatStyle formatStyle)
    {
        return widget with { FormatStyle = formatStyle };
    }

    public static NumberInputBase Currency(this NumberInputBase widget, string currency)
    {
        return widget with { Currency = currency };
    }

    public static NumberInputBase Invalid(this NumberInputBase widget, string invalid)
    {
        return widget with { Invalid = invalid };
    }
}