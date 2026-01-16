using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyAsyncSelectInputBase : IAnyInput
{
}

public delegate QueryResult<Option<T>[]> AsyncSelectSearchDelegate<T>(IViewContext context, string query);

public delegate QueryResult<Option<T>?> AsyncSelectLookupDelegate<T>(IViewContext context, T id);

/// <summary>
/// A selection input that fetches options asynchronously.
/// </summary>
public class AsyncSelectInputView<TValue> : ViewBase, IAnyAsyncSelectInputBase, IInput<TValue>
{
    public Type[] SupportedStateTypes() => [];

    public AsyncSelectInputView(IAnyState state, AsyncSelectSearchDelegate<TValue> search, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(search, lookup, placeholder, disabled)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public AsyncSelectInputView(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange, AsyncSelectSearchDelegate<TValue> search, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(search, lookup, placeholder, disabled)
    {
        OnChange = onChange;
        Value = value;
    }

    public AsyncSelectInputView(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, AsyncSelectSearchDelegate<TValue> search, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(search, lookup, placeholder, disabled)
    {
        OnChange = onChange == null ? null : e => { onChange(e); return ValueTask.CompletedTask; };
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

    public TValue Value { get; private set; } = typeof(TValue).IsValueType ? Activator.CreateInstance<TValue>() : default!;

    public bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange { get; }

    public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public bool Disabled { get; set; }

    public string? Invalid { get; set; }

    public string? Placeholder { get; set; }

    public Scale Scale { get; set; } = Scale.Medium;

    public override object? Build()
    {
        var open = UseState(false);
        var refreshToken = UseRefreshToken();
        var currentValue = UseState(() => Value);

        UseEffect(() =>
        {
            if (refreshToken.IsRefreshed)
            {
                open.Set(false);
                currentValue.Set((TValue)refreshToken.ReturnValue!);
                if (OnChange != null)
                {
                    _ = OnChange(new Event<IInput<TValue>, TValue>("OnChange", this, currentValue.Value));
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
                DisplayValue = displayValue,
                OnSelect = HandleSelect,
                Loading = loading,
                Scale = Scale
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

        var content = Layout.Vertical().Gap(2)
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
    public static IAnyAsyncSelectInputBase HandleBlur(this IAnyAsyncSelectInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        if (widget is AsyncSelectInputView<object> typedWidget)
        {
            typedWidget.OnBlur = onBlur;
            return typedWidget;
        }

        var widgetType = widget.GetType();
        if (widgetType.IsGenericType && widgetType.GetGenericTypeDefinition() == typeof(AsyncSelectInputView<>))
        {
            var onBlurProperty = widgetType.GetProperty("OnBlur");
            if (onBlurProperty != null)
            {
                onBlurProperty.SetValue(widget, onBlur);
                return widget;
            }
        }

        throw new InvalidOperationException("Unable to set blur handler on async select input");
    }

    public static IAnyAsyncSelectInputBase HandleBlur(this IAnyAsyncSelectInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static IAnyAsyncSelectInputBase HandleBlur(this IAnyAsyncSelectInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}

internal record AsyncSelectInput : WidgetBase<AsyncSelectInput>
{
    [Prop] public string? Placeholder { get; init; }

    [Prop] public bool Disabled { get; init; }

    [Prop] public string? Invalid { get; init; }

    [Prop] public string? DisplayValue { get; init; }

    [Prop] public bool Loading { get; init; }

    [Event] public Func<Event<AsyncSelectInput>, ValueTask>? OnSelect { get; init; }
}