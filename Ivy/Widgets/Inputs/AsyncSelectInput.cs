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

/// <summary>
/// Base interface for asynchronous select input controls.
/// </summary>
public interface IAnyAsyncSelectInputBase : IAnyInput
{
    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    public string? Placeholder { get; set; }
}

/// <summary>
/// Delegate for asynchronously querying and filtering options.
/// </summary>
/// <typeparam name="T">The type of the option values.</typeparam>
/// <param name="query">The search query string entered by the user.</param>
/// <returns>A task that resolves to an array of matching options.</returns>
public delegate Task<Option<T>[]> AsyncSelectQueryDelegate<T>(string query);

/// <summary>
/// Delegate for asynchronously looking up a specific option by its value.
/// </summary>
/// <typeparam name="T">The type of the option value.</typeparam>
/// <param name="id">The value to look up.</param>
/// <returns>A task that resolves to the option, or null if not found.</returns>
public delegate Task<Option<T>?> AsyncSelectLookupDelegate<T>(T id);

/// <summary>
/// Asynchronous select input control.
/// </summary>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
public class AsyncSelectInputView<TValue> : ViewBase, IAnyAsyncSelectInputBase, IInput<TValue>
{
    /// <summary>
    /// Returns an empty array.
    /// </summary>
    public Type[] SupportedStateTypes() => [];

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="state">The state object.</param>
    /// <param name="query">Delegate for querying options.</param>
    /// <param name="lookup">Delegate for looking up option by value.</param>
    /// <param name="placeholder">Optional placeholder text.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public AsyncSelectInputView(IAnyState state, AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(query, lookup, placeholder, disabled)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="value">The initial selected value.</param>
    /// <param name="onChange">Event handler called when the selection changes.</param> 
    /// <param name="query">Delegate for querying options.</param>
    /// <param name="lookup">Delegate for looking up option by value.</param>
    /// <param name="placeholder">Optional placeholder text.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    [OverloadResolutionPriority(1)]
    public AsyncSelectInputView(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange, AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(query, lookup, placeholder, disabled)
    {
        OnChange = onChange;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="value">The initial selected value.</param>
    /// <param name="onChange">Event handler called when the selection changes.</param>
    /// <param name="query">Delegate for querying options.</param>
    /// <param name="lookup">Delegate for looking up option by value.</param>
    /// <param name="placeholder">Optional placeholder text.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public AsyncSelectInputView(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
        : this(query, lookup, placeholder, disabled)
    {
        OnChange = onChange == null ? null : e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="query">Delegate for querying options.</param>
    /// <param name="lookup">Delegate for looking up option by value.</param>
    /// <param name="placeholder">Optional placeholder text.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public AsyncSelectInputView(AsyncSelectQueryDelegate<TValue> query, AsyncSelectLookupDelegate<TValue> lookup, string? placeholder = null, bool disabled = false)
    {
        Query = query;
        Lookup = lookup;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    /// <summary>
    /// Gets the delegate used for querying options based on user search input.
    /// </summary>
    public AsyncSelectQueryDelegate<TValue> Query { get; }

    /// <summary>
    /// Gets the delegate used for looking up option display information by value.
    /// </summary>
    public AsyncSelectLookupDelegate<TValue> Lookup { get; }

    /// <summary>
    /// Gets the currently selected value.
    /// </summary>
    public TValue Value { get; private set; } = typeof(TValue).IsValueType ? Activator.CreateInstance<TValue>() : default!;

    /// <summary>
    /// Gets or sets whether the input accepts null values.
    /// </summary>
    public bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    /// <summary>
    /// Gets the event handler called when the selected value changes.
    /// </summary>
    public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange { get; }

    /// <summary>
    /// Gets or sets the event handler called when the input loses focus.
    /// </summary>
    public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Gets or sets whether the input is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string? Invalid { get; set; }

    /// <summary> Gets or sets the size of the async select input. </summary>
    public Sizes Size { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed when no option is selected.
    /// </summary>
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

/// <summary>
/// Sheet view that displays a searchable list of options.
/// </summary>
/// <typeparam name="T">The type of the option values.</typeparam>
/// <param name="refreshToken">Token used to communicate selection back to the parent input.</param>
/// <param name="query">Delegate for querying options based on search input.</param>
public class AsyncSelectListSheet<T>(RefreshToken refreshToken, AsyncSelectQueryDelegate<T> query) : ViewBase
{
    /// <summary>
    /// Builds the UI.
    /// </summary>
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

        return Layout.Vertical().Gap(2)
            | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
            | (loading.Value ? Text.Block("Loading...") : new List(items));
    }
}

/// <summary>
/// Provides extension methods for creating async select inputs.
/// </summary>
public static class AsyncSelectInputViewExtensions
{
    /// <summary>
    /// Creates an async select input bound to the specified state object.
    /// </summary>
    /// <typeparam name="TValue">The type of the selected value.</typeparam>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="query">Delegate for querying options.</param>
    /// <param name="lookup">Delegate for looking up option by value.</param>
    /// <param name="placeholder">Optional placeholder text.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <returns>An async select input.</returns>
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


    /// <summary>
    /// Sets the blur event handler for the async select input.
    /// </summary>
    /// <param name="widget">The async select input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    /// <returns>A new async select input.</returns>
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

    /// <summary>
    /// Sets the blur event handler for the async select input.
    /// </summary>
    /// <param name="widget">The async select input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    /// <returns>A new async select input.</returns>
    public static IAnyAsyncSelectInputBase HandleBlur(this IAnyAsyncSelectInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the async select input.
    /// </summary>
    /// <param name="widget">The async select input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    /// <returns>A new async select input.</returns>
    public static IAnyAsyncSelectInputBase HandleBlur(this IAnyAsyncSelectInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}

/// <summary>
/// Internal widget that represents the visual async select input control.
/// </summary>
internal record AsyncSelectInput : WidgetBase<AsyncSelectInput>
{
    /// <summary>Gets the placeholder text displayed when no option is selected.</summary>
    [Prop] public string? Placeholder { get; init; }

    /// <summary>Gets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; init; }

    /// <summary>Gets the validation error message.</summary>
    [Prop] public string? Invalid { get; init; }

    /// <summary>Gets the display text for the currently selected option.</summary>
    [Prop] public string? DisplayValue { get; init; }

    /// <summary>Gets whether the input is currently loading option data.</summary>
    [Prop] public bool Loading { get; init; }

    /// <summary>Gets the event handler called when the user triggers option selection.</summary>
    [Event] public Func<Event<AsyncSelectInput>, ValueTask>? OnSelect { get; init; }
}