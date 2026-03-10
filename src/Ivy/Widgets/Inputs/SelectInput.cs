using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum SearchMode
{
    CaseInsensitive,
    CaseSensitive,
    Fuzzy
}

public enum SelectInputVariant
{
    Select,
    List,
    Toggle
}

public interface IAnySelectInput : IAnyInput
{
    public SelectInputVariant Variant { get; set; }
}

public abstract record SelectInputBase : WidgetBase<SelectInputBase>, IAnySelectInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public SelectInputVariant Variant { get; set; } = SelectInputVariant.Select;

    [Prop] public bool SelectMany { get; set; } = false;

    [Prop] public char Separator { get; set; } = ';';

    [Prop] public bool Nullable { get; set; }

    [Prop] public int? MaxSelections { get; set; }

    [Prop] public int? MinSelections { get; set; }

    [Prop] public bool Searchable { get; set; }

    [Prop] public SearchMode SearchMode { get; set; } = SearchMode.CaseInsensitive;

    [Prop] public string? EmptyMessage { get; set; }

    [Prop] public bool Loading { get; set; }

    [Prop] public bool Ghost { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [];
}

/// <summary>
/// A dropdown list for selecting one or more options.
/// </summary>
public record SelectInput<TValue> : SelectInputBase, IInput<TValue>
{
    [OverloadResolutionPriority(1)]
    public SelectInput(IAnyState state, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputVariant variant = SelectInputVariant.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public SelectInput(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputVariant variant = SelectInputVariant.Select, bool selectMany = false)
    : this(options, placeholder, disabled, variant, selectMany)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    public SelectInput(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputVariant variant = SelectInputVariant.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    public SelectInput(IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputVariant variant = SelectInputVariant.Select, bool selectMany = false)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
        Options = [.. options];
        SelectMany = selectMany;
    }

    internal SelectInput() { }

    [Prop(AlwaysSerialize = true)] public TValue Value { get; init; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    [Prop] public IAnyOption[] Options { get; set; } = [];

    [Event] public EventHandler<Event<IInput<TValue>, TValue>>? OnChange { get; }
}

public static class SelectInputExtensions
{
    public static SelectInputBase ToSelectInput(this IAnyState state, IEnumerable<IAnyOption>? options = null, string? placeholder = null, bool disabled = false, SelectInputVariant variant = SelectInputVariant.Select)
    {
        var type = state.GetStateType();
        bool selectMany = type.IsCollectionType();
        Type genericType = typeof(SelectInput<>).MakeGenericType(type);

        if (options == null)
        {
            var nonNullableType = System.Nullable.GetUnderlyingType(type) ?? type;
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
        input.Nullable = type.IsNullableType();

        return input;
    }

    public static SelectInputBase Placeholder(this SelectInputBase widget, string title) => widget with { Placeholder = title };

    public static SelectInputBase Disabled(this SelectInputBase widget, bool disabled = true) => widget with { Disabled = disabled };

    public static SelectInputBase Variant(this SelectInputBase widget, SelectInputVariant variant) => widget with { Variant = variant };

    public static SelectInputBase Invalid(this SelectInputBase widget, string? invalid) => widget with { Invalid = invalid };

    public static SelectInputBase Nullable(this SelectInputBase widget, bool? nullable = true)
    {
        var property = widget.GetType().GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (property != null && property.CanWrite)
        {
            property.SetValue(widget, nullable ?? true);
            return widget;
        }
        // Fallback to 'with' if reflection doesn't work (shouldn't happen, but safe fallback)
        return widget with { Nullable = nullable ?? true };
    }

    public static SelectInputBase Separator(this SelectInputBase widget, char separator) => widget with { Separator = separator };

    public static SelectInputBase MaxSelections(this SelectInputBase widget, int max) => widget with { MaxSelections = max };

    public static SelectInputBase MinSelections(this SelectInputBase widget, int min) => widget with { MinSelections = min };

    public static SelectInputBase Searchable(this SelectInputBase widget, bool searchable = true) => widget with { Searchable = searchable };

    public static SelectInputBase SearchMode(this SelectInputBase widget, SearchMode mode) => widget with { SearchMode = mode };

    public static SelectInputBase EmptyMessage(this SelectInputBase widget, string message) => widget with { EmptyMessage = message };

    public static SelectInputBase Loading(this SelectInputBase widget, bool loading = true) => widget with { Loading = loading };

    public static SelectInputBase Ghost(this SelectInputBase widget, bool ghost = true) => widget with { Ghost = ghost };

    public static SelectInputBase List(this SelectInputBase widget) => widget with { Variant = SelectInputVariant.List };

    [OverloadResolutionPriority(1)]
    public static SelectInputBase OnBlur(this SelectInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static SelectInputBase OnBlur(this SelectInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget with { OnBlur = new(onBlur.ToValueTask()) };
    }

    public static SelectInputBase OnBlur(this SelectInputBase widget, Action onBlur)
    {
        return widget with { OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; }) };
    }

    public static SelectInput<string> Options(this SelectInput<string> widget, IEnumerable<string> options)
    {
        return widget with { Options = options.ToOptions().Cast<IAnyOption>().ToArray() };
    }

    public static SelectInput<string[]> Options(this SelectInput<string[]> widget, IEnumerable<string> options)
    {
        return widget with { Options = options.ToOptions().Cast<IAnyOption>().ToArray() };
    }

    [OverloadResolutionPriority(2)]
    public static SelectInput<string> ToSelectInput(this IState<string> state)
    {
        return new SelectInput<string>(state, [], null, false, SelectInputVariant.Select, false);
    }

    [OverloadResolutionPriority(1)]
    public static SelectInput<string> ToSelectInput(this IState<string> state, IEnumerable<string> options, string? placeholder = null, bool disabled = false, SelectInputVariant variant = SelectInputVariant.Select)
    {
        return new SelectInput<string>(state, options.ToOptions(), placeholder, disabled, variant, false);
    }

    [OverloadResolutionPriority(2)]
    public static SelectInput<string[]> ToSelectInput(this IState<string[]> state)
    {
        return new SelectInput<string[]>(state, [], "Select options...", false, SelectInputVariant.Select, true);
    }

    [OverloadResolutionPriority(1)]
    public static SelectInput<string[]> ToSelectInput(this IState<string[]> state, IEnumerable<string> options, string? placeholder = null, bool disabled = false, SelectInputVariant variant = SelectInputVariant.Select)
    {
        return new SelectInput<string[]>(state, options.ToOptions(), placeholder ?? "Select options...", disabled, variant, true);
    }

    public static SelectInputBase Value<T>(this SelectInputBase widget, T value)
    {
        if (widget is SelectInput<T> typedWidget)
        {
            return typedWidget with { Value = value };
        }
        throw new InvalidOperationException($"Cannot set Value: widget is not SelectInput<{typeof(T).Name}>");
    }
}