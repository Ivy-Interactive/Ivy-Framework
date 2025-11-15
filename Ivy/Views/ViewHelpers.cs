using Ivy.Client;
using Ivy.Core;

namespace Ivy.Views;

/// <summary>Provides utility methods for common view operations including widget labeling and error handling.</summary>
public static class ViewHelpers
{
    /// <returns>A ViewBase containing the labeled widget in a vertical layout.</returns>
    public static ViewBase WithLabel(this IWidget widget, string label)
    {
        return Layout.Vertical()
            | Text.Label(label)
            | widget;
    }

    /// <returns>An action wrapped with error handling.</returns>
    /// <obsolete>This method is obsolete and should not be used in new code.</obsolete>
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

    /// <returns>An action wrapped with error handling.</returns>
    /// <obsolete>This method is obsolete and should not be used in new code.</obsolete>
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

    /// <returns>An action wrapped with error handling.</returns>
    /// <obsolete>This method is obsolete and should not be used in new code.</obsolete>
    [Obsolete("Not needed anymore.")]
    public static Action<T> HandleError<T>(this Func<T, Task> action, IView view)
    {
        var client = view.Context.UseService<IClientProvider>();
        return async e =>
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

    /// <returns>An action wrapped with error handling.</returns>
    /// <obsolete>This method is obsolete and should not be used in new code.</obsolete>
    [Obsolete("Not needed anymore.")]
    public static Action HandleError(this Func<Task> action, IView view)
    {
        var client = view.Context.UseService<IClientProvider>();
        return async () =>
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