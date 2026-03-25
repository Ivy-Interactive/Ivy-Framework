namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Loader, group: ["Widgets", "Primitives"], searchHints: ["spinner", "loader", "waiting", "progress", "loading", "busy", "UseLoading", "cancellation"])]
public class LoadingApp : SampleBase
{
    protected override object? BuildSample()
    {
        var log = UseState("");
        var (loadingView, showLoading) = UseLoading();

        static string Ts() => DateTime.Now.ToString("HH:mm:ss.fff");

        void Append(string line) => log.Set(log.Value + (log.Value.Length > 0 ? "\n" : "") + $"[{Ts()}] {line}");

        return new Fragment(
            loadingView,
            Layout.Vertical()
            | Text.H2("Loading")
            | Text.P("The Loading widget is a static indeterminate progress bar for inline use.")
            | new Loading()
            | Text.H2("UseLoading")
            | Text.P(
                "Opens a modal while async work runs. The handler is void and schedules work in the background so cancel/close events are not blocked by the serial UI event queue. Pass cancellable: true and observe ILoadingContext.CancellationToken (e.g. Task.Delay(..., ct)). Optional third argument: LoadingOptions — set CancellingDisplayDuration to control how long the \"Cancelling…\" state is shown before the dialog closes (default 800ms)."
            )
            | new Card(
                Layout.Vertical()
                | Text.H3("Cancellable")
                | Text.P("Default options. After cancel, \"Cancelling…\" shows for 800ms (see LoadingOptions.CancellingDisplayDuration).")
                | new Button("Run (~12s, cancel anytime)", () =>
                {
                    Append("Started: cancellable (default)");
                    showLoading(async ctx =>
                    {
                        try
                        {
                            ctx.Message("Working…");
                            ctx.Status("Close control or overlay cancels");
                            for (var i = 0; i < 12; i++)
                            {
                                ctx.CancellationToken.ThrowIfCancellationRequested();
                                ctx.Message($"Step {i + 1} / 12");
                                ctx.Progress(i * 100 / 12);
                                await Task.Delay(1000, ctx.CancellationToken);
                            }
                        }
                        finally
                        {
                            Append("Ended: cancellable (default)");
                        }
                    }, cancellable: true);
                })
            ).Title("Default cancellable")
            | new Card(
                Layout.Vertical()
                | Text.H3("Custom cancelling delay")
                | Text.P("CancellingDisplayDuration is 300ms — the closing message is brief.")
                | new Button("Run with 300ms cancelling delay", () =>
                {
                    Append("Started: custom CancellingDisplayDuration");
                    showLoading(
                        async ctx =>
                        {
                            try
                            {
                                ctx.Message("Working…");
                                ctx.Status("Cancel to see a short \"Cancelling…\" phase");
                                for (var i = 0; i < 30; i++)
                                {
                                    ctx.CancellationToken.ThrowIfCancellationRequested();
                                    ctx.Message($"Step {i + 1} / 30");
                                    ctx.Progress(i * 100 / 30);
                                    await Task.Delay(1000, ctx.CancellationToken);
                                }
                            }
                            finally
                            {
                                Append("Ended: custom delay");
                            }
                        },
                        cancellable: true,
                        options: new LoadingOptions { CancellingDisplayDuration = TimeSpan.FromMilliseconds(300) }
                    );
                })
            ).Title("Configurable LoadingOptions")
            | new Card(
                Layout.Vertical()
                | Text.H3("Non-cancellable")
                | Text.P("Close control is hidden; overlay/Escape do not cancel the token.")
                | new Button("Run (8s)", () =>
                {
                    Append("Started: non-cancellable");
                    showLoading(async ctx =>
                    {
                        try
                        {
                            ctx.Message("Please wait");
                            ctx.Status("No close control");
                            ctx.Progress(null);
                            await Task.Delay(8000);
                        }
                        finally
                        {
                            Append("Ended: non-cancellable");
                        }
                    }, cancellable: false);
                })
            ).Title("No cancel")
            | (log.Value.Length > 0 ? new Card(Text.Monospaced(log.Value)).Title("Log") : null)
        );
    }
}
