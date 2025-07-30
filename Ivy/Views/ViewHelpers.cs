using Ivy.Client;
using Ivy.Core;

namespace Ivy.Views;

public static class ViewHelpers
{
    public static ViewBase WithLabel(this IWidget widget, string label)
    {
        return Layout.Vertical()
            | Text.Label(label)
            | widget;
    }

    [Obsolete("Not needed anymore.")]
    public static Action HandleError(this Action action, IView view)
    {
        var client = view.Context.UseService<IClientProvider>();
        return () =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
#if DEBUG
                Utils.PrintDetailedException(ex);
#endif
                client.Toast(ex);
            }
        };
    }

    [Obsolete("Not needed anymore.")]
    public static Action<T> HandleError<T>(this Action<T> action, IView view)
    {
        var client = view.Context.UseService<IClientProvider>();
        return e =>
        {
            try
            {
                action(e);
            }
            catch (Exception ex)
            {
#if DEBUG
                Utils.PrintDetailedException(ex);
#endif
                client.Toast(ex);
            }
        };
    }

    [Obsolete("Not needed anymore.")]
    public static Action<T> HandleError<T>(this Func<T, Task> action, IView view)
    {
        var client = view.Context.UseService<IClientProvider>();
        return async void (e) =>
        {
            try
            {
                await action(e);
            }
            catch (Exception ex)
            {
#if DEBUG
                Utils.PrintDetailedException(ex);
#endif
                client.Toast(ex);
            }
        };
    }

    [Obsolete("Not needed anymore.")]
    public static Action HandleError(this Func<Task> action, IView view)
    {
        var client = view.Context.UseService<IClientProvider>();
        return async void () =>
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
#if DEBUG
                Utils.PrintDetailedException(ex);
#endif
                client.Toast(ex);
            }
        };
    }
}