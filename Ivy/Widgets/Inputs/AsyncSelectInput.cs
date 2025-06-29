using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Views;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyAsyncSelectInputBase : IAnyInput
{
    public string? Placeholder { get; set; }
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
        OnChange = e => typedState.Set(e.Value);
    }

    public AsyncSelectInputView(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(query, lookup, placeholder, disabled)
    {
        OnChange = onChange;
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
    public Action<Event<IInput<TValue>, TValue>>? OnChange { get; }
    public Action<Event<IAnyInput>>? OnBlur { get; set; }
    public bool Disabled { get; set; }
    public string? Invalid { get; set; }
    public string? Placeholder { get; set; }

    public override object? Build()
    {
        IState<string?> displayValue = UseState<string?>((string?)null!);
        var open = UseState(false);
        var loading = UseState(false);
        var refreshToken = this.UseRefreshToken();
        var client = UseService<IClientProvider>();

        UseEffect(async () =>
        {
            open.Set(false);

            if (refreshToken.IsRefreshed)
            {
                Value = (TValue)refreshToken.ReturnValue!;
                OnChange?.Invoke(new Event<IInput<TValue>, TValue>("OnChange", this, Value));
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

        void OnSelect(Event<AsyncSelectInput> _)
        {
            open.Set(true);
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
                OnSelect = OnSelect,
                Loading = loading.Value
            },
            open.Value ? new Sheet(
                OnClose,
                new AsyncSelectListSheet<TValue>(refreshToken, Query),
                title: Placeholder
                ) : null
        );
    }
}

public class AsyncSelectListSheet<T>(RefreshToken refreshToken, AsyncSelectQueryDelegate<T> query) : ViewBase
{
    public override object? Build()
    {
        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var option = (Option<T>)e.Sender.Tag!;
            refreshToken.Refresh(option.TypedValue);
        });

        ListItem CreateItem(Option<T> option) =>
            new(title: option.Label, onClick: onItemClicked, tag: option);

        return new FilteredListView<Option<T>>(filter => query(filter), CreateItem);
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
}

internal record AsyncSelectInput : WidgetBase<AsyncSelectInput>
{
    [Prop] public string? Placeholder { get; init; }
    [Prop] public bool Disabled { get; init; }
    [Prop] public string? Invalid { get; init; }
    [Prop] public string? DisplayValue { get; init; }
    [Prop] public bool Loading { get; init; }
    [Event] public Action<Event<AsyncSelectInput>>? OnSelect { get; init; }
}