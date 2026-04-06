using System.Reflection;
using System.Runtime.CompilerServices;
using System.Reactive.Linq;
using Ivy.Core;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyAsyncSelectInputBase : IAnyInput
{
}

public delegate QueryResult<Option<T>[]> AsyncSelectSearchDelegate<T>(IViewContext context, string query);

public delegate QueryResult<Option<T>?> AsyncSelectLookupDelegate<T>(IViewContext context, T id);

/// <summary>
/// A selection input that fetches options asynchronously. Ideal for large datasets,
/// foreign key lookups, or any scenario where options are loaded on-demand.
/// </summary>
/// <remarks>
/// Requires two delegates: a <c>search</c> delegate that returns matching options for a query string,
/// and a <c>lookup</c> delegate that resolves a single value back to its display option.
/// <para>
/// Options are created using <see cref="Option{TValue}"/> with the parameter order <c>(label, value)</c>:
/// the label is displayed to the user, and the value is stored when selected.
/// </para>
/// <example>
/// <code>
/// var country = UseState&lt;string?&gt;(default(string));
///
/// QueryResult&lt;Option&lt;string&gt;[]&gt; SearchCountries(IViewContext ctx, string query) =&gt;
///     ctx.UseQuery&lt;Option&lt;string&gt;[], (string, string)&gt;(
///         key: ("countries", query),
///         fetcher: ct =&gt; Task.FromResult(countries
///             .Where(c =&gt; c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
///             .Select(c =&gt; new Option&lt;string&gt;(c.Name, c.Code))  // label: Name, value: Code
///             .ToArray()));
///
/// country.ToAsyncSelectInput(SearchCountries, LookupCountry, "Search countries...");
/// </code>
/// </example>
/// </remarks>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
public class AsyncSelectInputView<TValue> : ViewBase, IAnyAsyncSelectInputBase, IInput<TValue>
{
    public Type[] SupportedStateTypes() => [];

    public AsyncSelectInputView(IAnyState state, AsyncSelectSearchDelegate<TValue> search, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(search, lookup, placeholder, disabled)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public AsyncSelectInputView(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange, AsyncSelectSearchDelegate<TValue> search, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(search, lookup, placeholder, disabled)
    {
        OnChange = onChange?.ToEventHandler();
        Value = value;
    }

    public AsyncSelectInputView(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, AsyncSelectSearchDelegate<TValue> search, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(search, lookup, placeholder, disabled)
    {
        OnChange = onChange == null ? null : new(e => { onChange(e); return ValueTask.CompletedTask; });
        Value = value;
    }

    public AsyncSelectInputView(AsyncSelectSearchDelegate<TValue> search, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
    {
        Search = search;
        Lookup = lookup;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    public AsyncSelectSearchDelegate<TValue> Search { get; }

    public AsyncSelectLookupDelegate<TValue> Lookup { get; }

    public TValue Value { get; init; } = typeof(TValue).IsValueType ? Activator.CreateInstance<TValue>() : default!;

    public bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    public bool AutoFocus { get; set; }

    public EventHandler<Event<IInput<TValue>, TValue>>? OnChange { get; init; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public bool Disabled { get; set; }

    public string? Invalid { get; set; }

    public string? Placeholder { get; set; }

    public Density Density { get; set; } = Density.Medium;

    public bool Ghost { get; set; }

    public override object? Build()
    {
        var open = UseState(false);
        var refreshToken = UseRefreshToken();
        var currentValue = UseState(() => Value);

        UseEffect(() =>
        {
            if (refreshToken.IsRefreshed)
            {
                var newValue = (TValue)refreshToken.ReturnValue!;
                open.Set(false);
                currentValue.Set(newValue);
                if (OnChange != null)
                {
                    _ = OnChange.Invoke(new Event<IInput<TValue>, TValue>("OnChange", this, newValue));
                }
            }
        }, [refreshToken]);

        var lookupResult = Lookup(Context, currentValue.Value);
        var displayValue = lookupResult.Value?.Label ?? (lookupResult.Loading ? "Loading..." : null);
        var loading = lookupResult.Loading;

        ValueTask HandleSelect(Event<AsyncSelectInput> _)
        {
            open.Set(true);
            return ValueTask.CompletedTask;
        }

        void OnClose(Event<Sheet> _)
        {
            open.Set(false);
        }

        return new Fragment(
            new AsyncSelectInput()
            {
                Placeholder = Placeholder,
                Disabled = Disabled,
                Invalid = Invalid,
                Nullable = Nullable,
                DisplayValue = displayValue,
                OnSelect = HandleSelect,
                Loading = loading,
                Density = Density,
                Ghost = Ghost,
                AutoFocus = AutoFocus,
                OnBlur = OnBlur,
                OnFocus = OnFocus
            },
            open.Value ? new Sheet(
                OnClose,
                new AsyncSelectListSheet<TValue>(refreshToken, Search),
                title: Placeholder
                ) : null
        );
    }
}

public class AsyncSelectListSheet<T>(RefreshToken refreshToken, AsyncSelectSearchDelegate<T> search) : ViewBase
{
    public override object? Build()
    {
        var filter = UseState("");
        var throttledFilter = UseState("");

        UseEffect(() =>
        {
            throttledFilter.Set(filter.Value);
        }, [filter.Throttle(TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        // Use the search delegate which now returns a QueryResult
        var searchResult = search(Context, throttledFilter.Value);
        var records = searchResult.Value ?? [];
        var loading = searchResult.Loading;

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var option = (Option<T>)e.Sender.Tag!;
            refreshToken.Refresh(option.TypedValue);
        });

        var items = records.Select(option =>
            new ListItem(title: option.Label, subtitle: option.Description, onClick: onItemClicked, tag: option)).ToArray();

        var searchInput = filter.ToSearchInput().Placeholder("Search").Width(Size.Grow());

        var header = Layout.Vertical().Gap(2)
            | searchInput;

        var content = Layout.Vertical().Gap(2).RemoveParentPadding()
            | (loading ? Text.Block("Loading...") : new List(items));

        return new HeaderLayout(header, content)
        {
            ShowHeaderDivider = false
        };
    }
}

public static class AsyncSelectInputViewExtensions
{
    public static IAnyAsyncSelectInputBase ToAsyncSelectInput<TValue>(
        this IAnyState state,
        AsyncSelectSearchDelegate<TValue> search,
        AsyncSelectLookupDelegate<TValue> lookup,
        string? placeholder = null,
        bool disabled = false
        )
    {
        var targetValueType = typeof(TValue);
        var stateType = state.GetStateType();

        // If the state is nullable but TValue is a non-nullable value type
        if (stateType.IsNullableType() &&
            targetValueType.IsValueType &&
            !targetValueType.IsNullableType() &&
            Nullable.GetUnderlyingType(stateType) == targetValueType)
        {
            var method = typeof(AsyncSelectInputViewExtensions).GetMethod(nameof(CreateNullableAsyncSelectInput), BindingFlags.NonPublic | BindingFlags.Static);
            if (method != null)
            {
                var genericMethod = method.MakeGenericMethod(targetValueType);
                return (IAnyAsyncSelectInputBase)genericMethod.Invoke(null, [state, search, lookup, placeholder, disabled])!;
            }
        }

        var type = typeof(TValue);
        Type genericType = typeof(AsyncSelectInputView<>).MakeGenericType(type);

        try
        {
            IAnyAsyncSelectInputBase input = (IAnyAsyncSelectInputBase)Activator
                .CreateInstance(genericType, state, search, lookup, placeholder, disabled)!;
            return input;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }


    [OverloadResolutionPriority(1)]
    public static IAnyAsyncSelectInputBase OnBlur(this IAnyAsyncSelectInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        if (widget is AsyncSelectInputView<object> typedWidget)
        {
            typedWidget.OnBlur = new(onBlur);
            return typedWidget;
        }

        var widgetType = widget.GetType();
        if (widgetType.IsGenericType && widgetType.GetGenericTypeDefinition() == typeof(AsyncSelectInputView<>))
        {
            var onBlurProperty = widgetType.GetProperty("OnBlur");
            if (onBlurProperty != null)
            {
                onBlurProperty.SetValue(widget, new EventHandler<Event<IAnyInput>>(onBlur));
                return widget;
            }
        }

        throw new InvalidOperationException("Unable to set blur handler on async select input");
    }

    public static IAnyAsyncSelectInputBase OnBlur(this IAnyAsyncSelectInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.OnBlur(onBlur.ToValueTask());
    }

    public static IAnyAsyncSelectInputBase OnBlur(this IAnyAsyncSelectInputBase widget, Action onBlur)
    {
        return widget.OnBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public static IAnyAsyncSelectInputBase OnFocus(this IAnyAsyncSelectInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        if (widget is AsyncSelectInputView<object> typedWidget)
        {
            typedWidget.OnFocus = new(onFocus);
            return typedWidget;
        }

        var widgetType = widget.GetType();
        if (widgetType.IsGenericType && widgetType.GetGenericTypeDefinition() == typeof(AsyncSelectInputView<>))
        {
            var onFocusProperty = widgetType.GetProperty("OnFocus");
            if (onFocusProperty != null)
            {
                onFocusProperty.SetValue(widget, new EventHandler<Event<IAnyInput>>(onFocus));
                return widget;
            }
        }

        throw new InvalidOperationException("Unable to set focus handler on async select input");
    }

    public static IAnyAsyncSelectInputBase OnFocus(this IAnyAsyncSelectInputBase widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget.OnFocus(onFocus.ToValueTask());
    }

    public static IAnyAsyncSelectInputBase OnFocus(this IAnyAsyncSelectInputBase widget, Action onFocus)
    {
        return widget.OnFocus(_ => { onFocus(); return ValueTask.CompletedTask; });
    }

    public static IAnyAsyncSelectInputBase Ghost(this IAnyAsyncSelectInputBase widget, bool ghost = true)
    {
        var widgetType = widget.GetType();
        if (widgetType.IsGenericType && widgetType.GetGenericTypeDefinition() == typeof(AsyncSelectInputView<>))
        {
            var ghostProperty = widgetType.GetProperty("Ghost");
            if (ghostProperty != null)
            {
                ghostProperty.SetValue(widget, ghost);
                return widget;
            }
        }

        throw new InvalidOperationException("Unable to set ghost on async select input");
    }

    private static IAnyAsyncSelectInputBase CreateNullableAsyncSelectInput<TValue>(
        IAnyState state,
        AsyncSelectSearchDelegate<TValue> search,
        AsyncSelectLookupDelegate<TValue> lookup,
        string? placeholder,
        bool disabled) where TValue : struct
    {
        AsyncSelectSearchDelegate<TValue?> nullableSearch = (ctx, query) =>
        {
            var res = search(ctx, query);

            var options = res.Value?.Select(opt => new Option<TValue?>(opt.Label, opt.TypedValue, opt.Group, opt.Description, opt.Icon, opt.Disabled)).ToArray();

            var newMutator = new QueryMutator<Option<TValue?>[]>(
                (_, _) => { },
                res.Mutator.Revalidate,
                res.Mutator.Invalidate);

            return new QueryResult<Option<TValue?>[]>(options, res.Loading, res.Validating, res.Previous, newMutator, res.Error);
        };

        AsyncSelectLookupDelegate<TValue?> nullableLookup = (ctx, id) =>
        {
            if (!id.HasValue)
            {
                var emptyMutator = new QueryMutator<Option<TValue?>?>(
                    (_, _) => { }, () => { }, () => { });
                return new QueryResult<Option<TValue?>?>(null, false, false, false, emptyMutator);
            }

            var res = lookup(ctx, id.Value);

            Option<TValue?>? mapped = null;
            if (res.Value != null)
            {
                var opt = res.Value;
                mapped = new Option<TValue?>(opt.Label, opt.TypedValue, opt.Group, opt.Description, opt.Icon, opt.Disabled);
            }

            var newMutator = new QueryMutator<Option<TValue?>?>(
                (_, _) => { },
                res.Mutator.Revalidate,
                res.Mutator.Invalidate);

            return new QueryResult<Option<TValue?>?>(mapped, res.Loading, res.Validating, res.Previous, newMutator, res.Error);
        };

        return new AsyncSelectInputView<TValue?>(state, nullableSearch, nullableLookup, placeholder, disabled);
    }
}


internal record AsyncSelectInput : WidgetBase<AsyncSelectInput>, IAnyInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public string? DisplayValue { get; set; }

    [Prop] public bool Loading { get; set; }

    [Prop] public bool Ghost { get; set; }

    [Event] public Func<Event<AsyncSelectInput>, ValueTask>? OnSelect { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public Type[] SupportedStateTypes() => [];
}