using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Alerts;

public delegate void ShowAlertDelegate(string message, Action<AlertResult> callback, string? title = null, AlertButtonSet buttonSet = AlertButtonSet.OkCancel);

public static class UseAlertExtensions
{
    public static (IView? alertView, ShowAlertDelegate showAlert) UseAlert<TW>(this TW view) where TW : ViewBase =>
        view.Context.UseAlert();

    public static (IView? alertView, ShowAlertDelegate showAlert) UseAlert(this IViewContext context)
    {
        var alertResult = context.UseState(AlertResult.Undecided);
        var isOpen = context.UseState(false);
        var alertOptions = context.UseState<AlertOptions?>();
        var alertCallback = context.UseState<Action<AlertResult>?>();
        var client = context.UseService<IClientProvider>();

        context.UseEffect(() =>
        {
            if (alertResult.Value != AlertResult.Undecided && alertCallback.Value != null)
            {
                try
                {
                    alertCallback.Value(alertResult.Value);
                }
                catch (Exception ex)
                {
                    client.Toast(ex);
                }
            }
        }, [alertResult, alertCallback]);

        var view = isOpen.Value && alertOptions.Value != null ? new AlertView(alertResult, isOpen, alertOptions.Value) : null;

        var showAlert = new ShowAlertDelegate((message, callback, title, buttonSet) =>
        {
            alertOptions.Set(new AlertOptions(title, message, buttonSet));
            alertResult.Set(AlertResult.Undecided);
            alertCallback.Set(callback);
            isOpen.Set(true);
        });

        return (view, showAlert);
    }
}