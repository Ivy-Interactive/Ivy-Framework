// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseCallbackExtensions
{
    /// <summary>
    /// Memoizes a callback function, returning a stable delegate reference that only changes when dependencies change.
    /// This is syntactic sugar over UseMemo for callback memoization.
    /// </summary>
    public static Action UseCallback(this IViewContext context, Action callback, params object?[] deps)
        => context.UseMemo(() => callback, deps);

    /// <summary>
    /// Memoizes a callback function with one parameter.
    /// </summary>
    public static Action<T> UseCallback<T>(this IViewContext context, Action<T> callback, params object?[] deps)
        => context.UseMemo(() => callback, deps);

    /// <summary>
    /// Memoizes a callback function with a return value.
    /// </summary>
    public static Func<TResult> UseCallback<TResult>(this IViewContext context, Func<TResult> callback, params object?[] deps)
        => context.UseMemo(() => callback, deps);

    /// <summary>
    /// Memoizes a callback function with one parameter and a return value.
    /// </summary>
    public static Func<T, TResult> UseCallback<T, TResult>(this IViewContext context, Func<T, TResult> callback, params object?[] deps)
        => context.UseMemo(() => callback, deps);

    /// <summary>
    /// Memoizes a callback function with two parameters.
    /// </summary>
    public static Action<T1, T2> UseCallback<T1, T2>(this IViewContext context, Action<T1, T2> callback, params object?[] deps)
        => context.UseMemo(() => callback, deps);

    /// <summary>
    /// Memoizes a callback function with two parameters and a return value.
    /// </summary>
    public static Func<T1, T2, TResult> UseCallback<T1, T2, TResult>(this IViewContext context, Func<T1, T2, TResult> callback, params object?[] deps)
        => context.UseMemo(() => callback, deps);
}
