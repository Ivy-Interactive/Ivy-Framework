using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Interface for read-only input controls that extends IAnyInput for non-editable value display.
/// </summary>
public interface IAnyReadOnlyInput : IAnyInput
{
}

/// <summary>
/// Generic read-only input control that displays values without allowing user modification.
/// </summary>
/// <typeparam name="TValue">The type of the value to display (can be any type with string representation).</typeparam>
public record ReadOnlyInput<TValue> : WidgetBase<ReadOnlyInput<TValue>>, IInput<TValue>, IAnyReadOnlyInput
{
    /// <summary>
    /// Initializes a new read-only input bound to a state object for automatic value synchronization.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public ReadOnlyInput(IAnyState state)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new read-only input with an explicit value and optional async change handler.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public ReadOnlyInput(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange = null)
    {
        OnChange = onChange;
        Value = value;
    }

    /// <summary>
    /// Initializes a new read-only input with an explicit value and optional synchronous change handler.
    /// </summary>
    public ReadOnlyInput(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange = null)
    {
        OnChange = onChange == null ? null : e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>Gets the current value displayed in the read-only input.</summary>
    [Prop] public TValue Value { get; }

    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message to display.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the size of the read-only input.</summary>
    [Prop] public Sizes Size { get; set; }

    /// <summary>Gets or sets whether to show a copy button for the displayed value.</summary>
    [Prop] public bool ShowCopyButton { get; set; } = true;

    /// <summary>Gets the event handler called when the value would change.</summary>
    [Event] public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange { get; }

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns the types that this read-only input can bind to and work with.
    /// </summary>
    public Type[] SupportedStateTypes() => [typeof(object)];
}

/// <summary>
/// Provides extension methods for creating read-only input controls with automatic type detection.
/// </summary>
public static class ReadOnlyInputExtensions
{
    /// <summary>
    /// Creates a read-only input control from a state object with automatic type detection.
    /// </summary>
    public static IAnyReadOnlyInput ToReadOnlyInput(this IAnyState state)
    {
        var type = state.GetStateType();
        Type genericType = typeof(ReadOnlyInput<>).MakeGenericType(type);
        IAnyReadOnlyInput input = (IAnyReadOnlyInput)Activator.CreateInstance(genericType, state)!;
        return input;
    }

    /// <summary>
    /// Sets the blur event handler for the read-only input.
    /// This method allows you to configure the read-only input's blur behavior,
    /// enabling it to perform custom actions when the input loses focus.
    /// </summary>
    /// <param name="widget">The read-only input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static IAnyReadOnlyInput HandleBlur<T>(this IAnyReadOnlyInput widget, Func<Event<IAnyInput>, ValueTask> onBlur) where T : notnull
    {
        if (widget is ReadOnlyInput<T> typedWidget)
        {
            return typedWidget with { OnBlur = onBlur };
        }
        throw new InvalidOperationException($"Widget is not of expected type ReadOnlyInput<{typeof(T).Name}>");
    }

    /// <summary>
    /// Sets the blur event handler for the read-only input.
    /// Compatibility overload for Action-based event handlers.
    /// </summary>
    /// <param name="widget">The read-only input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static IAnyReadOnlyInput HandleBlur<T>(this IAnyReadOnlyInput widget, Action<Event<IAnyInput>> onBlur) where T : notnull
    {
        return widget.HandleBlur<T>(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the read-only input.
    /// This method allows you to configure the read-only input's blur behavior with
    /// a simple action that doesn't require the input event context.
    /// </summary>
    /// <param name="widget">The read-only input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static IAnyReadOnlyInput HandleBlur<T>(this IAnyReadOnlyInput widget, Action onBlur) where T : notnull
    {
        return widget.HandleBlur<T>(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}