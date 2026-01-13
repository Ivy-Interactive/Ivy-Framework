namespace Ivy.Core;

public static class WidgetExtensions
{
    public static T Key<T>(this T widget, object key) where T : IWidget
    {
        widget.Key = key.ToString();
        return widget;
    }

    public static T? FindDescendant<T>(this IWidget widget, string? key = null) where T : IWidget
    {
        Type type = typeof(T);
        return FindDescendant<T>(widget, type, key);
    }

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