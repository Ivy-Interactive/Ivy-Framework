using Ivy.Core.Apps;
using Xunit.Abstractions;

namespace Ivy.Test;

/// <summary>
/// Reload runs concurrently in practice: plugin load/unload, hot reload and the deferred plugin loader
/// all call it from threadpool threads while the main thread is still building the app. These tests pin
/// the invariants that concurrency previously broke.
/// </summary>
public class AppRepositoryConcurrencyTests(ITestOutputHelper output)
{
    private static AppDescriptor[] MakeApps(string prefix, int count) =>
        Enumerable.Range(0, count).Select(i => new AppDescriptor
        {
            Id = $"{prefix}-app-{i}",
            Title = $"{prefix} App {i}",
            // Mix of root-level apps and grouped apps so the group-lookup path is covered too.
            Group = i % 3 == 0 ? [] : [$"Group{i % 3}"],
            IsVisible = true,
        }).ToArray();

    private static List<string> LeafLabels(MenuItem[] items)
    {
        var labels = new List<string>();

        void Walk(MenuItem item)
        {
            if (item.Children is { Length: > 0 })
            {
                foreach (var child in item.Children) Walk(child);
            }
            else
            {
                labels.Add(item.Label ?? "<null>");
            }
        }

        foreach (var item in items) Walk(item);
        return labels;
    }

    [Fact]
    public void ConcurrentReloads_DoNotDuplicateMenuItems()
    {
        // Two reloads that overlapped used to build into one shared root, so both loops appended their
        // full app set to the same tree and every app showed up twice in the sidebar.
        var repository = new AppRepository();
        repository.AddFactory(() => MakeApps("core", 20));
        repository.AddFactory(() => MakeApps("plugin", 20));

        var reservedPaths = new HashSet<string>();
        var problems = new List<string>();
        var problemLock = new object();

        void Check(string who)
        {
            string? problem = null;
            try
            {
                var labels = LeafLabels(repository.GetMenuItems());
                var duplicates = labels.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => $"{g.Key} x{g.Count()}").ToArray();
                if (duplicates.Length > 0)
                {
                    problem = $"{who}: {duplicates.Length} duplicated label(s) out of {labels.Count} leaves, e.g. {string.Join(", ", duplicates.Take(3))}";
                }
            }
            catch (Exception ex)
            {
                problem = $"{who}: threw {ex.GetType().Name}: {ex.Message}";
            }

            if (problem == null) return;
            lock (problemLock)
            {
                if (problems.Count < 10) problems.Add(problem);
            }
        }

        const int iterations = 300;
        using var start = new Barrier(3);

        Thread Reloader(string name) => new(() =>
        {
            start.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                try
                {
                    repository.Reload(reservedPaths);
                }
                catch (Exception ex)
                {
                    lock (problemLock)
                    {
                        if (problems.Count < 10) problems.Add($"{name} threw {ex.GetType().Name}: {ex.Message}");
                    }
                }

                Check(name);
            }
        });

        var reloaderA = Reloader("reloadA");
        var reloaderB = Reloader("reloadB");
        var reader = new Thread(() =>
        {
            start.SignalAndWait();
            for (var i = 0; i < iterations * 20; i++) Check("reader");
        });

        foreach (var thread in new[] { reloaderA, reloaderB, reader }) thread.Start();
        foreach (var thread in new[] { reloaderA, reloaderB, reader }) thread.Join();

        foreach (var problem in problems) output.WriteLine(problem);
        Assert.Empty(problems);
    }

    [Fact]
    public void Reload_DoesNotPublishStaleTree_WhenAnotherReloadStartsLater()
    {
        // Models startup: the main thread is midway through Reload when the deferred plugin loader
        // registers a plugin's factory and reloads. The plugin's apps must survive, even though the
        // in-flight reload snapshotted its factories before that registration and finishes last.
        var repository = new AppRepository();
        var startupReloadEnteredFactory = new ManualResetEventSlim(false);
        var releaseStartupReload = new ManualResetEventSlim(false);

        var isFirstCall = true;
        repository.AddFactory(() =>
        {
            // Only the startup reload's invocation blocks; later reloads run straight through.
            if (isFirstCall)
            {
                isFirstCall = false;
                startupReloadEnteredFactory.Set();
                Assert.True(releaseStartupReload.Wait(TimeSpan.FromSeconds(10)));
            }

            return MakeApps("core", 3);
        });

        var startupReload = new Thread(() => repository.Reload(new HashSet<string>()));
        startupReload.Start();
        Assert.True(startupReloadEnteredFactory.Wait(TimeSpan.FromSeconds(10)));

        // Plugin loads on another thread: registers its factory, then reloads.
        repository.AddFactory(() => MakeApps("plugin", 3));
        var pluginReload = new Thread(() => repository.Reload(new HashSet<string>()));
        pluginReload.Start();

        releaseStartupReload.Set();
        Assert.True(startupReload.Join(TimeSpan.FromSeconds(10)));
        Assert.True(pluginReload.Join(TimeSpan.FromSeconds(10)));

        var appIds = repository.All().Select(a => a.Id).ToArray();
        output.WriteLine("Final app ids: " + string.Join(", ", appIds));

        Assert.Contains("plugin-app-0", appIds);
        Assert.Contains("core-app-0", appIds);
        Assert.Contains("plugin App 0", LeafLabels(repository.GetMenuItems()));
    }

    [Fact]
    public void DuplicateAppId_AcrossFactories_YieldsOneMenuItem_AndLastRegistrationWins()
    {
        // Two factories claiming the same id must not each get a menu item. The dictionary has always
        // been last-wins, and callers depend on that to override built-in apps, so the tree follows it.
        var repository = new AppRepository();
        repository.AddFactory(() => [
            new AppDescriptor { Id = "shared", Title = "First", Group = [], IsVisible = true }
        ]);
        repository.AddFactory(() => [
            new AppDescriptor { Id = "shared", Title = "Second", Group = [], IsVisible = true }
        ]);

        repository.Reload(new HashSet<string>());

        var labels = LeafLabels(repository.GetMenuItems());
        output.WriteLine("Labels: " + string.Join(", ", labels));

        Assert.Single(labels);
        Assert.Equal("Second", labels[0]);
        Assert.Equal("Second", repository.GetApp("shared")?.Title);
    }
}
