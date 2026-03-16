using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class LoadingView(LoadingOptions options) : ViewBase
{
    public override object? Build()
    {
        void OnCancel(Event<Dialog> _)
        {
            // Loading dialogs are non-dismissable
        }

        var progressWidget = options.Indeterminate
            ? new Progress().Indeterminate(true)
            : new Progress(options.Progress ?? 0);

        return new Dialog(
            OnCancel,
            new DialogHeader(options.Message),
            new DialogBody(
                Layout.Vertical()
                | (options.Status != null ? Text.P(options.Status).Muted() : null)!
                | progressWidget
            ),
            new DialogFooter()
        );
    }
}
