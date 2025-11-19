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

    [Prop] public Sizes Size { get; set; }

    [Prop] public bool SelectMany { get; set; } = false;

    /// <summary>Character used to separate multiple selected values in display and serialization.</summary>
    [Prop] public char Separator { get; set; } = ';';

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [];
}

/// <typeparam name="TValue">Can be single values or collections for multi-select.</typeparam>
public record SelectInput<TValue> : SelectInputBase, IInput<TValue>, IAnySelectInput
{
    [OverloadResolutionPriority(1)]
    public SelectInput(IAnyState state, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>Useful for manual state management or when custom change handling is required.</summary>
    [OverloadResolutionPriority(1)]
    public SelectInput(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        OnChange = onChange;
        Value = value;
    }

    public SelectInput(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        OnChange = onChange == null ? null : e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>Requires separate value and change handler assignment for functionality.</summary>
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

    [Event] public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange { get; }
}

public static class SelectInputExtensions
{
    /// <summary>If null, attempts automatic generation for enums and collections.</summary>
    /// <exception cref="ArgumentException">Thrown when options are null and cannot be automatically generated for the state type.</exception>
    public static SelectInputBase ToSelectInput(this IAnyState state, IEnumerable<IAnyOption>? options = null, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select)
    {
        var type = state.GetStateType();
        bool selectMany = type.IsCollectionType();
        Type genericType = typeof(SelectInput<>).MakeGenericType(type);

        if (options == null)
        {
            var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
            if (nonNullableType.IsEnum)
            {
                options = nonNullableType.ToOptions();
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

    public static SelectInputBase Size(this SelectInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    [RelatedTo(nameof(SelectInputBase.Size))]
    public static SelectInputBase Large(this SelectInputBase widget)
    {
        return widget.Size(Sizes.Large);
    }

    [RelatedTo(nameof(SelectInputBase.Size))]
    public static SelectInputBase Small(this SelectInputBase widget)
    {
        return widget.Size(Sizes.Small);
    }

    /// <summary>Or null to clear the error.</summary>
    public static SelectInputBase Invalid(this SelectInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static SelectInputBase Separator(this SelectInputBase widget, char separator)
    {
        return widget with { Separator = separator };
    }

    /// <summary>Convenience method that sets the variant to SelectInputs.List.</summary>
    public static SelectInputBase List(this SelectInputBase widget)
    {
        return widget with { Variant = SelectInputs.List };
    }

    [OverloadResolutionPriority(1)]
    public static SelectInputBase HandleBlur(this SelectInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    public static SelectInputBase HandleBlur(this SelectInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static SelectInputBase HandleBlur(this SelectInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}