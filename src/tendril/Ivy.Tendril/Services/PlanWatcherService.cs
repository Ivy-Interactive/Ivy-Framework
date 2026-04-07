using Timer = System.Timers.Timer;

namespace Ivy.Tendril.Services;

public class PlanWatcherService : IPlanWatcherService, IDisposable
{
    private static readonly HashSet<string> WatchedFiles = new(StringComparer.OrdinalIgnoreCase)
        { "plan.yaml" };

    private static readonly HashSet<string> WatchedFolders = new(StringComparer.OrdinalIgnoreCase)
        { "revisions", "logs", "verification", "artifacts" };

    private readonly Timer _debounceTimer;
    private readonly FileSystemWatcher? _watcher;
    private string? _pendingPlanFolder;

    public PlanWatcherService(IConfigService config)
    {
        _debounceTimer = new Timer(500);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += (_, _) =>
        {
            var folder = _pendingPlanFolder;
            _pendingPlanFolder = null;
            PlansChanged?.Invoke(folder);
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
        _watcher.Renamed += (_, _) => ScheduleDebounce(null);
    }

    public event Action<string?>? PlansChanged;

    public void NotifyChanged(string? changedPlanFolder = null)
    {
        ScheduleDebounce(changedPlanFolder);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _debounceTimer.Dispose();
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        // Only react to plan-relevant changes, not worktree/temp/artifact churn
        var fileName = Path.GetFileName(e.FullPath);
        var parentFolder = Path.GetFileName(Path.GetDirectoryName(e.FullPath) ?? "");

        // New plan folder created at top level
        if (e.ChangeType == WatcherChangeTypes.Created && parentFolder == Path.GetFileName(_watcher!.Path))
        {
            ScheduleDebounce(null); // full rescan for new plan
            return;
        }

        // plan.yaml changed, or files in watched folders (revisions/logs/verification/artifacts)
        if (WatchedFiles.Contains(fileName) || WatchedFolders.Contains(parentFolder))
        {
            var planFolder = ResolvePlanFolder(e.FullPath);
            ScheduleDebounce(planFolder);
        }
    }

    /// <summary>
    ///     Walk up from the changed file path to find the plan folder (direct child of the plans directory).
    /// </summary>
    private string? ResolvePlanFolder(string changedPath)
    {
        if (_watcher == null) return null;

        var plansRoot = _watcher.Path;
        var dir = Path.GetDirectoryName(changedPath);
        while (dir != null && !string.Equals(Path.GetDirectoryName(dir), plansRoot, StringComparison.OrdinalIgnoreCase))
            dir = Path.GetDirectoryName(dir);

        return dir;
    }

    private void ScheduleDebounce(string? planFolder)
    {
        // If we already have a pending folder and a different one arrives, escalate to full rescan
        if (_pendingPlanFolder != null && planFolder != null
                                       && !string.Equals(_pendingPlanFolder, planFolder,
                                           StringComparison.OrdinalIgnoreCase))
            _pendingPlanFolder = null; // null = full rescan
        else if (_pendingPlanFolder == null && planFolder != null && !_debounceTimer.Enabled)
            _pendingPlanFolder = planFolder;
        // If planFolder is null (full rescan requested), override any specific folder
        else if (planFolder == null) _pendingPlanFolder = null;

        _debounceTimer.Stop();
        _debounceTimer.Start();
    }
}