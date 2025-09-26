namespace Ivy.Core;

/// <summary>
/// Provides extension methods for IWidget objects to add key-based functionality
/// and descendant search capabilities.
/// </summary>
public static class WidgetExtensions
{
    /// <summary>
    /// Assigns a key to a widget for identification and search purposes.
    /// </summary>
    /// <typeparam name="T">The type of the widget, which must implement IWidget.</typeparam>
    /// <param name="widget">The widget to assign a key to.</param>
    /// <param name="key">The object to use as the widget's key (converted to string).</param>
    /// <returns>The widget instance for method chaining.</returns>
    public static T Key<T>(this T widget, object key) where T : IWidget
    {
        widget.Key = key.ToString();
        return widget;
    }

    /// <summary>
    /// Searches for a descendant widget of the specified type within the widget hierarchy.
    /// </summary>
    /// <typeparam name="T">The type of descendant widget to search for.</typeparam>
    /// <param name="widget">The root widget to start the search from.</param>
    /// <param name="key">Optional key to match against the found widget (null to ignore key matching).</param>
    /// <returns>The found descendant widget of type T, or null if no matching widget is found.</returns>
    public static T? FindDescendant<T>(this IWidget widget, string? key = null) where T : IWidget
    {
        Type type = typeof(T);
        return FindDescendant<T>(widget, type, key);
    }

    /// <summary>
    /// Searches for a descendant widget of the specified type within the widget hierarchy.
    /// </summary>
    /// <typeparam name="T">The type of descendant widget to search for.</typeparam>
    /// <param name="widget">The root widget to start the search from.</param>
    /// <param name="type">The specific type to search for within the widget hierarchy.</param>
    /// <param name="key">Optional key to match against the found widget (null to ignore key matching).</param>
    /// <returns>The found descendant widget of type T, or null if no matching widget is found.</returns>
    /// <exception cref="NotSupportedException">Thrown when the widget tree is not fully built and contains non-widget children.</exception>
    public static T? FindDescendant<T>(this IWidget widget, Type type, string? key = null) where T : IWidget
    {
        if (widget.GetType() == type && (key == null || widget.Key == key))
            return (T)widget;

        foreach (var child in widget.Children)
        {
            if (child is IWidget childWidget)
            {
                var found = FindDescendant<T>(childWidget, type, key);
                if (found != null)
                    return found;
            }
            else
            {
                throw new NotSupportedException("This widget tree seems not to be a fully built.");
            }
        }

        return default(T);
    }
}