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
            | Text.H2("Loading widget")
            | Text.P("Static indeterminate progress bar.")
            | new Loading()
            | Text.H2("UseLoading")
            | Text.P("Programmatic modal with optional cancellation. Pass cancellable: true and use ILoadingContext.CancellationToken in async work (e.g. Task.Delay(..., ct)). showLoading is non-blocking so the dialog can receive close/cancel while work runs.")
            | Layout.Horizontal().Gap(2)
            | new Button("Cancellable (30s, respects cancel)", () =>
            {
                Append("Started cancellable job");
                showLoading(async ctx =>
                {
                    try
                    {
                        ctx.Message("Working…");
                        ctx.Status("Try the header close control or overlay");
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
                        Append("Cancellable job ended (completed or cancelled)");
                    }
                }, cancellable: true);
            })
            | new Button("Non-cancellable (10s)", () =>
            {
                Append("Started non-cancellable job");
                showLoading(async ctx =>
                {
                    try
                    {
                        ctx.Message("Please wait");
                        ctx.Status("Close control is hidden");
                        ctx.Progress(null);
                        await Task.Delay(10000);
                    }
                    finally
                    {
                        Append("Non-cancellable job ended");
                    }
                }, cancellable: false);
            })
            | (log.Value.Length > 0 ? new Card(Text.Monospaced(log.Value)) : null)
        );
    }
}
