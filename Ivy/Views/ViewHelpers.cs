using Ivy.Client;
using Ivy.Core;

namespace Ivy.Views;

/// <summary>
/// Provides utility methods for common view operations including widget labeling
/// and error handling.
/// </summary>
public static class ViewHelpers
{
    /// <summary>
    /// Adds a label above a widget using a vertical layout. This extension method
    /// creates a labeled widget by combining a Text.Label with the specified widget
    /// in a vertical arrangement.
    /// </summary>
    /// <param name="widget">The widget to add a label to.</param>
    /// <param name="label">The label text to display above the widget.</param>
    /// <returns>A ViewBase containing the labeled widget in a vertical layout.</returns>
    public static ViewBase WithLabel(this IWidget widget, string label)
    {
        return Layout.Vertical()
            | Text.Label(label)
            | widget;
    }

    /// <summary>
    /// Wraps an action with error handling that displays exceptions as toast messages.
    /// </summary>
    /// <param name="action">The action to wrap with error handling.</param>
    /// <param name="view">The view context for accessing client services.</param>
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

    /// <summary>
    /// Wraps an action with error handling that displays exceptions as toast messages.
    /// </summary>
    /// <typeparam name="T">The type of parameter for the action.</typeparam>
    /// <param name="action">The action to wrap with error handling.</param>
    /// <param name="view">The view context for accessing client services.</param>
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

    /// <summary>
    /// Wraps an async function with error handling that displays exceptions as toast messages.
    /// </summary>
    /// <typeparam name="T">The type of parameter for the function.</typeparam>
    /// <param name="action">The async function to wrap with error handling.</param>
    /// <param name="view">The view context for accessing client services.</param>
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

    /// <summary>
    /// Wraps an async function with error handling that displays exceptions as toast messages.
    /// </summary>
    /// <param name="action">The async function to wrap with error handling.</param>
    /// <param name="view">The view context for accessing client services.</param>
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