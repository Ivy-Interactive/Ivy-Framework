using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public delegate void ShowAlertDelegate(string message, Action<AlertResult> callback, string? title = null, AlertButtonSet buttonSet = AlertButtonSet.OkCancel);

public static class UseAlertExtensions
{
    public static (IView? alertView, ShowAlertDelegate showAlert) UseAlert(this IViewContext context)
    {
        var open = context.UseRef(false);
        var alertResult = context.UseRef(AlertResult.Undecided);
        var alertOptions = context.UseRef<AlertOptions?>();
        var alertCallback = context.UseRef<Action<AlertResult>?>();

        context.UseEffect(() =>
        {
            if (alertResult.Value != AlertResult.Undecided && alertCallback.Value != null)
            {
                alertCallback.Value(alertResult.Value);
            }
        }, [alertResult, alertCallback]);

        var view = new FuncView(context2 =>
        {
            var openInternal = context2.UseState(false);

            context2.UseEffect(() =>
            {
                openInternal.Set(open.Value);
            }, open);

            return openInternal.Value && alertOptions.Value != null ? new AlertView(alertResult, open, alertOptions.Value) : null;
        });

        var showAlert = new ShowAlertDelegate((message, callback, title, buttonSet) =>
        {
            alertOptions.Set(new AlertOptions(title, message, buttonSet));
            alertResult.Set(AlertResult.Undecided);
            alertCallback.Set(callback);
            open.Set(true);
        });

        return (view, showAlert);
    }
}
