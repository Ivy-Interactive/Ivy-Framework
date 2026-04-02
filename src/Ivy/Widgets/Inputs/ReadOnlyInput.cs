using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyReadOnlyInput : IAnyInput
{
}

public record ReadOnlyInput<TValue> : WidgetBase<ReadOnlyInput<TValue>>, IInput<TValue>, IAnyReadOnlyInput
{
    [OverloadResolutionPriority(1)]
    internal ReadOnlyInput(IAnyState state)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    internal ReadOnlyInput(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange = null)
    {
        OnChange = onChange?.ToEventHandler();
        Value = value;
    }

    internal ReadOnlyInput(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange = null)
    {
        OnChange = onChange == null ? null : new(e => { onChange(e); return ValueTask.CompletedTask; });
        Value = value;
    }

    internal ReadOnlyInput()
    {
        Value = default!;
    }

    [Prop(AlwaysSerialize = true)] public TValue Value { get; init; }

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public bool ShowCopyButton { get; set; } = true;

    [Prop] public string? Placeholder { get; set; } //not really used but included to consistency with IAnyInput    
    [Prop] public bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    [Prop] public bool AutoFocus { get; set; }

    [Event] public EventHandler<Event<IInput<TValue>, TValue>>? OnChange { get; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public Type[] SupportedStateTypes() => [typeof(object)];
}

/// <summary>
/// Displays a value that cannot be edited by the user.
/// </summary>
public record ReadOnlyInput : ReadOnlyInput<string>
{
    internal ReadOnlyInput(IAnyState state) : base(state)
    {
    }

    [OverloadResolutionPriority(1)]
    internal ReadOnlyInput(string value, Func<Event<IInput<string>, string>, ValueTask>? onChange = null) : base(value, onChange)
    {
    }

    internal ReadOnlyInput(string value, Action<Event<IInput<string>, string>>? onChange = null) : base(value, onChange)
    {
    }

    internal ReadOnlyInput() : base()
    {
    }
}

public static class ReadOnlyInputExtensions
{
    public static IAnyReadOnlyInput ToReadOnlyInput(this IAnyState state)
    {
        var type = state.GetStateType();
        Type genericType = typeof(ReadOnlyInput<>).MakeGenericType(type);
        IAnyReadOnlyInput input = (IAnyReadOnlyInput)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state }, null)!;
        return input;
    }

    [OverloadResolutionPriority(1)]
    public static IAnyReadOnlyInput OnBlur<T>(this IAnyReadOnlyInput widget, Func<Event<IAnyInput>, ValueTask> onBlur) where T : notnull
    {
        if (widget is ReadOnlyInput<T> typedWidget)
        {
            return typedWidget with { OnBlur = new(onBlur) };
        }
        throw new InvalidOperationException($"Widget is not of expected type ReadOnlyInput<{typeof(T).Name}>");
    }

    [OverloadResolutionPriority(1)]
    public static IAnyReadOnlyInput OnFocus<T>(this IAnyReadOnlyInput widget, Func<Event<IAnyInput>, ValueTask> onFocus) where T : notnull
    {
        if (widget is ReadOnlyInput<T> typedWidget)
        {
            return typedWidget with { OnFocus = new(onFocus) };
        }
        throw new InvalidOperationException($"Widget is not of expected type ReadOnlyInput<{typeof(T).Name}>");
    }

    public static IAnyReadOnlyInput Nullable(this IAnyReadOnlyInput widget, bool? nullable = true)
    {
        // Use reflection to set the property since we don't know the generic type at compile time
        var property = widget.GetType().GetProperty("Nullable");
        if (property != null && property.CanWrite)
        {
            property.SetValue(widget, nullable ?? true);
        }
        return widget;
    }

    public static IAnyReadOnlyInput ShowCopyButton(this IAnyReadOnlyInput widget, bool show = true)
    {
        var property = widget.GetType().GetProperty("ShowCopyButton");
        if (property != null && property.CanWrite)
        {
            property.SetValue(widget, show);
        }
        return widget;
    }

    public static IAnyReadOnlyInput Placeholder(this IAnyReadOnlyInput widget, string? placeholder)
    {
        var property = widget.GetType().GetProperty("Placeholder");
        if (property != null && property.CanWrite)
        {
            property.SetValue(widget, placeholder);
        }
        return widget;
    }

    public static IAnyReadOnlyInput OnBlur<T>(this IAnyReadOnlyInput widget, Action<Event<IAnyInput>> onBlur) where T : notnull
    {
        return widget.OnBlur<T>(onBlur.ToValueTask());
    }

    public static IAnyReadOnlyInput OnBlur<T>(this IAnyReadOnlyInput widget, Action onBlur) where T : notnull
    {
        return widget.OnBlur<T>(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    public static IAnyReadOnlyInput OnFocus<T>(this IAnyReadOnlyInput widget, Action<Event<IAnyInput>> onFocus) where T : notnull
    {
        return widget.OnFocus<T>(onFocus.ToValueTask());
    }

    public static IAnyReadOnlyInput OnFocus<T>(this IAnyReadOnlyInput widget, Action onFocus) where T : notnull
    {
        return widget.OnFocus<T>(_ => { onFocus(); return ValueTask.CompletedTask; });
    }


}