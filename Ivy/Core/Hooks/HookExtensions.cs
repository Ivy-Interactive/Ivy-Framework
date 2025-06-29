namespace Ivy.Core.Hooks;


public static class HookExtensions
{
    public static (T value, Func<string, T> dispatch) UseReducer<TView, T>(this TView view, Func<T, string, T> reducer, T initialState) where TView : ViewBase
    {
        var state = view.Context.UseState(initialState);

        T Dispatch(string action) =>
            state.Set((prevState) => reducer(prevState, action));

        return (state.Value, Dispatch);
    }

    public static T UseStatic<TView, T>(this TView view, T initialValue) where TView : ViewBase
    {
        var state = view.Context.UseState(initialValue, buildOnChange: false);
        return state.Value;
    }

    public static T UseStatic<TView, T>(this TView view, Func<T> buildInitialValue) where TView : ViewBase
    {
        var state = view.Context.UseState(buildInitialValue, buildOnChange: false);
        return state.Value;
    }

    private class CallbackRef<T>(Func<T>? callback = null, object[]? dependencies = null)
    {
        public readonly object[]? Dependencies = dependencies;
        public readonly Func<T>? Callback = callback;
    }

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

    private class MemoRef<T>(T value, object[] dependencies)
    {
        public readonly T Value = value;
        public readonly object[] Dependencies = dependencies;
    }

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