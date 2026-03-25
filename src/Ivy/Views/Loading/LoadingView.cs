using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class LoadingView(LoadingOptions options, CancellationTokenSource? cts) : ViewBase
{
    public override object? Build()
    {
        void OnClose(Event<Dialog> _)
        {
            if (options.Cancellable && !options.IsCancelling)
            {
                cts?.Cancel();
            }
        }

        var progressWidget = options.Indeterminate
            ? new Progress().Indeterminate(true)
            : new Progress(options.Progress ?? 0);

        return new Dialog(
            OnClose,
            new DialogHeader(options.Message) { HideCloseButton = !options.Cancellable || options.IsCancelling },
            new DialogBody(
                Layout.Vertical()
                | (!options.IsCancelling && options.Status != null ? Text.P(options.Status).Muted() : null)!
                | progressWidget
            )
        );
    }
}
