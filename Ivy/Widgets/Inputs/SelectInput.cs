using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum SelectInputs
{
    Select,
    List,
    Toggle
}

public interface IAnySelectInput : IAnyInput
{
    public string? Placeholder { get; set; }
    public SelectInputs Variant { get; set; }
}

public abstract record SelectInputBase : WidgetBase<SelectInputBase>, IAnySelectInput
{
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Prop] public string? Placeholder { get; set; }
    [Prop] public SelectInputs Variant { get; set; }
    [Prop] public bool SelectMany { get; set; } = false;
    [Prop] public char Separator { get; set; } = ';';
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }
    public Type[] SupportedStateTypes() => [];
}

public record SelectInput<TValue> : SelectInputBase, IInput<TValue>, IAnySelectInput
{
    public SelectInput(IAnyState state, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => typedState.Set(e.Value);
    }

    public SelectInput(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        OnChange = onChange;
        Value = value;
    }

    public SelectInput(IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
        Options = [.. options];
        SelectMany = selectMany;
    }

    [Prop] public TValue Value { get; } = default!;

    [Prop] public bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    [Prop] public IAnyOption[] Options { get; set; }

    [Event] public Action<Event<IInput<TValue>, TValue>>? OnChange { get; }
}

public static class SelectInputExtensions
{
    public static SelectInputBase ToSelectInput(this IAnyState state, IEnumerable<IAnyOption>? options = null, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select)
    {
        var type = state.GetStateType();
        bool selectMany = type.IsCollectionType();
        Type genericType = typeof(SelectInput<>).MakeGenericType(type);

        if (options == null)
        {
            if (type.IsEnum)
            {
                options = type.ToOptions();
            }
            else if (selectMany && type.GetCollectionTypeParameter() is { } itemType)
            {
                options = itemType.ToOptions();
            }
            else
            {
                throw new ArgumentException("Options must be provided for non-enum types.", nameof(options));
            }
        }

        // Set a default placeholder for multi-selects if not provided
        if (selectMany && string.IsNullOrWhiteSpace(placeholder))
        {
            placeholder = "Select options...";
        }

        SelectInputBase input = (SelectInputBase)Activator.CreateInstance(genericType, state, options, placeholder, disabled, variant, selectMany)!;
        return input;
    }

    public static SelectInputBase Placeholder(this SelectInputBase widget, string title)
    {
        return widget with { Placeholder = title };
    }

    public static SelectInputBase Disabled(this SelectInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static SelectInputBase Variant(this SelectInputBase widget, SelectInputs variant)
    {
        return widget with { Variant = variant };
    }

    public static SelectInputBase Invalid(this SelectInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static SelectInputBase Separator(this SelectInputBase widget, char separator)
    {
        return widget with { Separator = separator };
    }

    public static SelectInputBase List(this SelectInputBase widget)
    {
        return widget with { Variant = SelectInputs.List };
    }
}