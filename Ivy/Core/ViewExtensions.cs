namespace Ivy.Core;

/// <summary>
/// Provides extension methods for IView objects to add key-based functionality.
/// These extensions enable views to be uniquely identified and memoized
/// through key assignment and hash code generation.
/// </summary>
public static class ViewExtensions
{
    /// <summary>
    /// Assigns a string key to a view for identification and memoization purposes.
    /// This method enables views to be uniquely identified and tracked within
    /// the widget tree system.
    /// </summary>
    /// <typeparam name="T">The type of the view, which must implement IView.</typeparam>
    /// <param name="view">The view to assign a key to.</param>
    /// <param name="key">The string key to assign to the view.</param>
    /// <returns>The view instance for method chaining.</returns>
    public static T Key<T>(this T view, string key) where T : IView
    {
        view.Key = key;
        return view;
    }

    /// <summary>
    /// Assigns a computed key to a view based on multiple key values.
    /// This method generates a memoized hash code from the provided keys
    /// and assigns it as the view's key for identification and memoization.
    /// </summary>
    /// <typeparam name="T">The type of the view, which must implement IView.</typeparam>
    /// <param name="view">The view to assign a computed key to.</param>
    /// <param name="keys">The array of objects to use for hash code generation.</param>
    /// <returns>The view instance for method chaining.</returns>
    public static T Key<T>(this T view, params object[] keys) where T : IView
    {
        view.Key = WidgetTree.CalculateMemoizedHashCode("key", keys).ToString();
        return view;
    }
}