namespace Ivy.Core.Hooks;

/// <summary>
/// Extension methods providing additional hook patterns like UseReducer, UseStatic, UseCallback, and UseMemo.
/// </summary>
public static class HookExtensions
{
    /// <summary>
    /// Implements a reducer pattern for state management with action-based updates.
    /// </summary>
    /// <typeparam name="TView">The view type that owns the context.</typeparam>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="view">The view that owns the context.</param>
    /// <param name="reducer">Function that takes current state and action, returns new state.</param>
    /// <param name="initialState">The initial state value.</param>
    /// <returns>Tuple containing current state value and dispatch function.</returns>
    public static (T value, Func<string, T> dispatch) UseReducer<TView, T>(this TView view, Func<T, string, T> reducer, T initialState) where TView : ViewBase
    {
        var state = view.Context.UseState(initialState);

        T Dispatch(string action) =>
            state.Set((prevState) => reducer(prevState, action));

        return (state.Value, Dispatch);
    }

    /// <summary>
    /// Creates static state that doesn't trigger re-renders when changed.
    /// </summary>
    /// <typeparam name="TView">The view type that owns the context.</typeparam>
    /// <typeparam name="T">The type of the static value.</typeparam>
    /// <param name="view">The view that owns the context.</param>
    /// <param name="initialValue">The initial static value.</param>
    /// <returns>The static value.</returns>
    public static T UseStatic<TView, T>(this TView view, T initialValue) where TView : ViewBase
    {
        var state = view.Context.UseState(initialValue, buildOnChange: false);
        return state.Value;
    }

    /// <summary>
    /// Creates static state using a factory function that doesn't trigger re-renders when changed.
    /// </summary>
    /// <typeparam name="TView">The view type that owns the context.</typeparam>
    /// <typeparam name="T">The type of the static value.</typeparam>
    /// <param name="view">The view that owns the context.</param>
    /// <param name="buildInitialValue">Factory function to create the initial value.</param>
    /// <returns>The static value.</returns>
    public static T UseStatic<TView, T>(this TView view, Func<T> buildInitialValue) where TView : ViewBase
    {
        var state = view.Context.UseState(buildInitialValue, buildOnChange: false);
        return state.Value;
    }

    /// <summary>
    /// Internal wrapper for callback references with dependency tracking.
    /// </summary>
    private class CallbackRef<T>(Func<T>? callback = null, object[]? dependencies = null)
    {
        public readonly object[]? Dependencies = dependencies;
        public readonly Func<T>? Callback = callback;
    }

    /// <summary>
    /// Memoizes a callback function, only recreating it when dependencies change.
    /// </summary>
    /// <typeparam name="TView">The view type that owns the context.</typeparam>
    /// <typeparam name="T">The return type of the callback.</typeparam>
    /// <param name="view">The view that owns the context.</param>
    /// <param name="callback">The callback function to memoize.</param>
    /// <param name="dependencies">Dependencies that trigger callback recreation when changed.</param>
    /// <returns>The memoized callback function.</returns>
    public static Func<T> UseCallback<TView, T>(
        this TView view,
        Func<T> callback,
        params object[] dependencies) where TView : ViewBase
    {
        var refState = view.Context.UseState(new CallbackRef<T>(callback, dependencies), buildOnChange: false);

        if (!AreDependenciesEqual(refState.Value.Dependencies!, dependencies))
        {
            refState.Value = new CallbackRef<T>(callback, dependencies);
        }

        return refState.Value.Callback!;
    }

    /// <summary>
    /// Internal wrapper for memoized values with dependency tracking.
    /// </summary>
    private class MemoRef<T>(T value, object[] dependencies)
    {
        public readonly T Value = value;
        public readonly object[] Dependencies = dependencies;
    }

    /// <summary>
    /// Memoizes a computed value, only recalculating when dependencies change.
    /// </summary>
    /// <typeparam name="TView">The view type that owns the context.</typeparam>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    /// <param name="view">The view that owns the context.</param>
    /// <param name="factory">Factory function to compute the value.</param>
    /// <param name="dependencies">Dependencies that trigger recomputation when changed.</param>
    /// <returns>The memoized computed value.</returns>
    public static T UseMemo<TView, T>(this TView view, Func<T> factory, params object[] dependencies) where TView : ViewBase
    {
        var memo = view.Context.UseState(() => new MemoRef<T>(factory(), dependencies), buildOnChange: false);
        if (!AreDependenciesEqual(memo.Value.Dependencies, dependencies))
        {
            memo.Value = new MemoRef<T>(factory(), dependencies);
        }
        return memo.Value.Value;
    }

    private static bool AreDependenciesEqual(object[]? oldDeps, object[]? newDeps)
    {
        if (oldDeps == null || newDeps == null || oldDeps.Length != newDeps.Length)
        {
            return false;
        }
        for (int i = 0; i < oldDeps.Length; i++)
        {
            if (!Equals(oldDeps[i], newDeps[i]))
            {
                return false;
            }
        }

        return true;
    }
}