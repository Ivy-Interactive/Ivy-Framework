namespace Ivy.Core;

public static class ViewExtensions
{
    public static T Key<T>(this T view, string key) where T : IView
    {
        return view.Key([key]);
    }

    public static T Key<T>(this T view, params object?[] keys) where T : IView
    {
        view.Key = Utils.StableHash(keys).ToString();
        return view;
    }
}