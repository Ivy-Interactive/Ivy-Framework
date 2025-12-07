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

public delegate Task<Option<T>[]> AsyncSelectQueryDelegate<T>(string query);

public delegate Task<Option<T>?> AsyncSelectLookupDelegate<T>(T id);

public class AsyncSelectInputView<TValue> : ViewBase, IAnyAsyncSelectInputBase, IInput<TValue>
{
    public Type[] SupportedStateTypes() => [];

    public AsyncSelectInputView(IAnyState state, AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(query, lookup, placeholder, disabled)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public AsyncSelectInputView(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange, AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(query, lookup, placeholder, disabled)
    {
        OnChange = onChange;
        Value = value;
    }

    public AsyncSelectInputView(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(query, lookup, placeholder, disabled)
    {
        OnChange = onChange == null ? null : e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    public AsyncSelectInputView(AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
    {
        Query = query;
        Lookup = lookup;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    public AsyncSelectQueryDelegate<TValue> Query { get; }

    public AsyncSelectLookupDelegate<TValue> Lookup { get; }

    public TValue Value { get; private set; } = typeof(TValue).IsValueType ? Activator.CreateInstance<TValue>() : default!;

    public bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange { get; }

    public Scale? Scale { get; set; }

    public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public bool Disabled { get; set; }

    public string? Invalid { get; set; }

    public string? Placeholder { get; set; }

    public override object? Build()
    {
        IState<string?> displayValue = UseState<string?>();
        var open = UseState(false);
        var loading = UseState(false);
        var refreshToken = this.UseRefreshToken();

        UseEffect(async () =>
        {
            open.Set(false);

            if (refreshToken.IsRefreshed)
            {
                Value = (TValue)refreshToken.ReturnValue!;
                if (OnChange != null)
                    await OnChange(new Event<IInput<TValue>, TValue>("OnChange", this, Value));
            }

            if (!(Value?.Equals(typeof(TValue).IsValueType ? Activator.CreateInstance<TValue>() : default!) ?? true))
            {
                loading.Set(true);
                try
                {
                    if ((await Lookup(Value)) is { } option)
                    {
                        displayValue.Set(option.Label);
                        return;
                    }
                }
                finally
                {
                    loading.Set(false);
                }
            }
            displayValue.Set((string?)null!);
        }, [EffectTrigger.AfterInit(), refreshToken]);

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
                DisplayValue = displayValue.Value,
                OnSelect = HandleSelect,
                Loading = loading.Value,
                Scale = Scale
            },
            open.Value ? new Sheet(
                OnClose,
                new AsyncSelectListSheet<TValue>(refreshToken, Query, Scale),
                title: Placeholder
                ) : null
        );
    }
}

public class AsyncSelectListSheet<T>(RefreshToken refreshToken, AsyncSelectQueryDelegate<T> query, Scale? scale = null) : ViewBase
{
    public override object? Build()
    {
        var records = UseState(Array.Empty<Option<T>>);
        var filter = UseState("");
        var loading = UseState(true);

        UseEffect(async () =>
        {
            loading.Set(true);
            records.Set(await query(filter.Value));
            loading.Set(false);
        }, [filter.Throttle(TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var option = (Option<T>)e.Sender.Tag!;
            refreshToken.Refresh(option.TypedValue);
        });

        var items = records.Value.Select(option =>
            new ListItem(title: option.Label, subtitle: option.Description, onClick: onItemClicked, tag: option)).ToArray();

        var searchInput = filter.ToSearchInput().Placeholder("Search").Width(Size.Grow());
        if (scale.HasValue)
        {
            searchInput.Scale = scale.Value;
        }

        var header = Layout.Vertical().Gap(2)
            | searchInput;

        var content = Layout.Vertical().Gap(2)
            | (loading.Value ? Text.Block("Loading...") : new List(items));

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
        AsyncSelectQueryDelegate<TValue> query,
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
                .CreateInstance(genericType, state, query, lookup, placeholder, disabled)!;
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