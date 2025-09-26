using Ivy.Chrome;

namespace Ivy.Docs.Shared.Helpers;

public static class Hooks
{
    public static Action<string> UseLinks(this IView view)
    {
        var navigator = view.UseNavigation();
        return uri =>
        {
            navigator.Navigate(uri);
        };
    }
}