using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyReadOnlyInput : IAnyInput
{
}

public record ReadOnlyInput<TValue> : WidgetBase<ReadOnlyInput<TValue>>, IInput<TValue>, IAnyReadOnlyInput
{
    [OverloadResolutionPriority(1)]
    public ReadOnlyInput(IAnyState state)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public ReadOnlyInput(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange = null)
    {
        OnChange = onChange;
        Value = value;
    }

    public ReadOnlyInput(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange = null)
    {
        OnChange = onChange == null ? null : e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    [Prop] public TValue Value { get; }

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public bool ShowCopyButton { get; set; } = true;

    [Prop] public string? Placeholder { get; set; } //not really used but included to consistency with IAnyInput    

    [Prop] public new Scale? Scale { get; set; }

    [Event] public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange { get; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [typeof(object)];
}

public static class ReadOnlyInputExtensions
{
    public static IAnyReadOnlyInput ToReadOnlyInput(this IAnyState state)
    {
        var type = state.GetStateType();
        Type genericType = typeof(ReadOnlyInput<>).MakeGenericType(type);
        IAnyReadOnlyInput input = (IAnyReadOnlyInput)Activator.CreateInstance(genericType, state)!;
        return input;
    }

    [OverloadResolutionPriority(1)]
    public static IAnyReadOnlyInput HandleBlur<T>(this IAnyReadOnlyInput widget, Func<Event<IAnyInput>, ValueTask> onBlur) where T : notnull
    {
        if (widget is ReadOnlyInput<T> typedWidget)
        {
            return typedWidget with { OnBlur = onBlur };
        }
        throw new InvalidOperationException($"Widget is not of expected type ReadOnlyInput<{typeof(T).Name}>");
    }

    public static IAnyReadOnlyInput HandleBlur<T>(this IAnyReadOnlyInput widget, Action<Event<IAnyInput>> onBlur) where T : notnull
    {
        return widget.HandleBlur<T>(onBlur.ToValueTask());
    }

    public static IAnyReadOnlyInput HandleBlur<T>(this IAnyReadOnlyInput widget, Action onBlur) where T : notnull
    {
        return widget.HandleBlur<T>(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}