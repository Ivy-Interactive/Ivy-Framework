using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseMemoExtensions
{
    /// <summary>
    /// Memoizes a computed value, only recomputing when dependencies change.
    /// Uses value equality (Equals) to compare dependencies.
    /// </summary>
    /// <example>
    /// var filteredItems = UseMemo(
    ///     () => items.Value.Where(i => i.Name.Contains(filter.Value)).ToList(),
    ///     items.Value, filter.Value);
    /// </example>
    public static T UseMemo<T>(this IViewContext context, Func<T> factory, params object?[] deps)
    {
        var valueRef = context.UseRef<T>((T?)default);
        var prevDepsRef = context.UseRef<object?[]?>((object?[]?)null);
        var hasComputedRef = context.UseRef<bool>(false);

        var unwrapped = UnwrapDeps(deps);

        var depsChanged = !hasComputedRef.Value ||
                          prevDepsRef.Value is null ||
                          !DepsEqual(prevDepsRef.Value, unwrapped);

        if (depsChanged)
        {
            valueRef.Value = factory();
            prevDepsRef.Value = unwrapped;
            hasComputedRef.Value = true;
        }

        return valueRef.Value;
    }

    /// <summary>
    /// Memoizes a computed value, computing it only once on first render.
    /// Useful for expensive one-time computations that don't depend on changing state.
    /// </summary>
    /// <example>
    /// var expensiveResult = UseMemo(() => ComputeExpensiveValue());
    /// </example>
    public static T UseMemo<T>(this IViewContext context, Func<T> factory)
    {
        var valueRef = context.UseRef<T?>((T?)default);
        var hasComputedRef = context.UseRef<bool>(false);

        if (!hasComputedRef.Value)
        {
            valueRef.Value = factory();
            hasComputedRef.Value = true;
        }

        return valueRef.Value!;
    }

    private static object?[] UnwrapDeps(object?[] deps)
    {
        var unwrapped = new object?[deps.Length];
        for (var i = 0; i < deps.Length; i++)
        {
            unwrapped[i] = deps[i] is IAnyState state ? state.GetValueAsObject() : deps[i];
        }
        return unwrapped;
    }

    private static bool DepsEqual(object?[] prev, object?[] current)
    {
        if (prev.Length != current.Length) return false;
        for (var i = 0; i < prev.Length; i++)
        {
            if (!Equals(prev[i], current[i])) return false;
        }
        return true;
    }
}
