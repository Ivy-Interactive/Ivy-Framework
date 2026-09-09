using System.Reactive.Disposables;

namespace Ivy.IvyML.Studio.Helpers;

public static class PreviewHooks
{
    /// <summary>
    /// Subscribes to the shared out-of-process preview server, starting its build-output watchers on
    /// first use. The server itself is process-wide, so several browser sessions share one preview
    /// process and each gets its own state to render from.
    /// </summary>
    public static IState<PreviewStatus> UsePreviewServer(this IViewContext context)
    {
        var state = context.UseState(() => PreviewServer.Instance.Status);

        context.UseEffect(() =>
        {
            void OnStatusChanged(PreviewStatus status) => state.Set(status);

            PreviewServer.Instance.StatusChanged += OnStatusChanged;
            PreviewServer.Instance.Start();
            // Catch up on anything that happened between the initial read above and subscribing.
            state.Set(PreviewServer.Instance.Status);

            return Disposable.Create(() => PreviewServer.Instance.StatusChanged -= OnStatusChanged);
        }, EffectTrigger.OnMount());

        return state;
    }
}
