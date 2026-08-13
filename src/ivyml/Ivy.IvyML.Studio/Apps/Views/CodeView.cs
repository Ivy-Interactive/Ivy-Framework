using System.Reactive.Disposables;
using Ivy.IvyML.Studio.Helpers;

namespace Ivy.IvyML.Studio.Apps.Views;

public class CodeView : ViewBase
{
    public override object Build()
    {
        var latest = Context.UseLatestWireframe();
        var code = UseState(() => latest.Value.Content ?? "");

        // Push new file contents into the editor whenever the latest wireframe changes.
        UseEffect(() => code.Set(latest.Value.Content ?? ""), EffectTrigger.OnStateChange(latest));

        // Auto-save edits to the current file, debounced 1000ms after the last change. The returned
        // disposable cancels the pending save whenever a new edit arrives, so only the final value
        // is written. Skipped when the editor already matches disk (e.g. content pushed in above).
        UseEffect(() =>
        {
            var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                try { await Task.Delay(1000, cts.Token); }
                catch (TaskCanceledException) { return; }

                if (latest.Value.Path is { } path && code.Value != latest.Value.Content)
                    WireframeLibrary.Save(path, code.Value);
            });
            return Disposable.Create(() => cts.Cancel());
        }, EffectTrigger.OnStateChange(code));

        var snapshot = latest.Value;

        var header = Layout.Horizontal().Gap(2)
            | (snapshot.Name is { } name ? Text.Muted(name) : Text.Muted("no wireframes yet"));

        object body = snapshot is { Content.Length: > 0, Path: { } path }
            ? code.ToCodeInput(language: Languages.Xml)
                .Ghost()
                .ShowCopyButton()
                // Also save immediately on blur, so a quick edit-then-leave isn't lost waiting for
                // the 500ms debounce above; the watcher then refreshes the preview.
                .OnBlur(() => WireframeLibrary.Save(path, code.Value))
                .Width(Size.Full())
                .Height(Size.Full())
                .WithLayout()
                .RemoveParentPadding()
            : Layout.Center().Height(Size.Full()).Width(Size.Full())
                | Text.Muted("Ask the agent to create a wireframe.");

        return new HeaderLayout(header, body);
    }
}
