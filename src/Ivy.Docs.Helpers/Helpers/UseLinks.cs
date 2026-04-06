using Ivy.Core;

namespace Ivy.Docs.Helpers;

public static class Hooks
{
    public static Action<string> UseLinks(this IView view)
    {
        var navigator = view.Context.UseNavigation();
        return uri =>
        {
            navigator.Navigate(uri);
        };
    }
}
