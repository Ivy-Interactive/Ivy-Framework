namespace Ivy.Tendril.Services;

public class PlanWatcherService : IPlanWatcherService, IDisposable
{
    private readonly FileSystemWatcher? _watcher;
    private readonly System.Timers.Timer _debounceTimer;

    public event Action? PlansChanged;

    public PlanWatcherService(IConfigService config)
    {
        _debounceTimer = new System.Timers.Timer(500);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += (_, _) =>
        {
            PlansChanged?.Invoke();
        };

        var planFolder = config.PlanFolder;
        if (!Directory.Exists(planFolder))
            return;

        _watcher = new FileSystemWatcher(planFolder)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileEvent;
        _watcher.Created += OnFileEvent;
        _watcher.Deleted += OnFileEvent;
        _watcher.Renamed += (_, _) => ScheduleDebounce();
    }

    private static readonly HashSet<string> WatchedFiles = new(StringComparer.OrdinalIgnoreCase)
        { "plan.yaml" };

    private static readonly HashSet<string> WatchedFolders = new(StringComparer.OrdinalIgnoreCase)
        { "revisions", "logs", "verification" };

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        // Only react to plan-relevant changes, not worktree/temp/artifact churn
        var fileName = Path.GetFileName(e.FullPath);
        var parentFolder = Path.GetFileName(Path.GetDirectoryName(e.FullPath) ?? "");

        // New plan folder created at top level
        if (e.ChangeType == WatcherChangeTypes.Created && parentFolder == Path.GetFileName(_watcher!.Path))
        {
            ScheduleDebounce();
            return;
        }

        // plan.yaml changed, or files in revisions/logs/verification
        if (WatchedFiles.Contains(fileName) || WatchedFolders.Contains(parentFolder))
        {
            ScheduleDebounce();
        }
    }

    private void ScheduleDebounce()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _debounceTimer.Dispose();
    }
}
