using System.Reactive.Disposables;

namespace Ivy.IvyML.Studio.Helpers;

public static class WireframeHooks
{
    /// <summary>
    /// Returns a state that always holds the latest wireframe in <see cref="WireframeLibrary"/>.
    /// A <see cref="FileSystemWatcher"/> reloads it whenever the agent writes a new design, so the
    /// code and preview panels stay in sync without polling.
    /// </summary>
    public static IState<WireframeSnapshot> UseLatestWireframe(this IViewContext context)
    {
        var state = context.UseState(WireframeLibrary.LoadLatest);

        context.UseEffect(() =>
        {
            var watcher = new FileSystemWatcher(WireframeLibrary.Directory, "*.ivyml")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true,
            };

            void Reload()
            {
                // A short delay lets a just-created file finish flushing before we read it.
                _ = Task.Run(async () =>
                {
                    await Task.Delay(100);
                    state.Set(WireframeLibrary.LoadLatest());
                });
            }

            watcher.Created += (_, _) => Reload();
            watcher.Changed += (_, _) => Reload();
            watcher.Deleted += (_, _) => Reload();
            watcher.Renamed += (_, _) => Reload();

            return Disposable.Create(() =>
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            });
        }, EffectTrigger.OnMount());

        return state;
    }
}
