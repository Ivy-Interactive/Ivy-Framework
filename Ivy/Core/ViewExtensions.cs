namespace Ivy.Core;

public static class ViewExtensions
{
    public static T Key<T>(this T view, string key) where T : IView
    {
        view.Key = key;
        return view;
    }

    public static T Key<T>(this T view, params object[] keys) where T : IView
    {
        view.Key = WidgetTree.CalculateMemoizedHashCode("key", keys).ToString();
        return view;
    }

}