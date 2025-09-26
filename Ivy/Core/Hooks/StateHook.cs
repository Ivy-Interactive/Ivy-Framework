namespace Ivy.Core.Hooks;

/// <summary>
/// Wrapper for state objects that provides identity tracking and render behavior configuration for the hooks system.
/// </summary>
/// <param name="identity">Unique identifier for this state hook within its view context.</param>
/// <param name="lazyState">Lazy-initialized state instance to defer creation until first access.</param>
/// <param name="renderOnChange">Whether changes to this state should trigger view re-rendering.</param>
public class StateHook(int identity, Lazy<IAnyState> lazyState, bool renderOnChange)
{
    /// <summary>Gets the unique identifier for this state hook.</summary>
    public int Identity { get; } = identity;

    /// <summary>Gets the state instance, creating it if not already initialized.</summary>
    public IAnyState State => lazyState.Value;

    /// <summary>Gets whether changes to this state should trigger view re-rendering.</summary>
    public bool RenderOnChange { get; } = renderOnChange;

    /// <summary>
    /// Creates a new state hook with lazy initialization.
    /// </summary>
    /// <param name="identity">Unique identifier for this state hook.</param>
    /// <param name="stateFactory">Factory function to create the state instance.</param>
    /// <param name="renderOnChange">Whether changes should trigger re-rendering.</param>
    /// <returns>A new state hook instance.</returns>
    public static StateHook Create(int identity, Func<IAnyState> stateFactory, bool renderOnChange) =>
        new(identity, new Lazy<IAnyState>(stateFactory), renderOnChange);
}