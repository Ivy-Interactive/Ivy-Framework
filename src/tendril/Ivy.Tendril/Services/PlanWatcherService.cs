using Timer = System.Timers.Timer;

namespace Ivy.Tendril.Services;

public class PlanWatcherService : IPlanWatcherService
{
    private readonly Timer _debounceTimer;
    private readonly FileSystemWatcher? _watcher;
    private readonly System.Threading.Timer? _pollTimer;
    private string? _pendingPlanFolder;

    public PlanWatcherService(IConfigService config)
    {
        _debounceTimer = new Timer(500);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += (_, _) =>
        {
            try
            {
                var folder = _pendingPlanFolder;
                _pendingPlanFolder = null;
                PlansChanged?.Invoke(folder);
            }
            catch
            {
                // Swallow to prevent unhandled exceptions on the timer's thread-pool
                // thread from terminating the process.
            }
        };

        var planFolder = config.PlanFolder;
        if (!Directory.Exists(planFolder))
            return;

        // Only watch the top-level Plans directory (no subdirectories) to detect
        // new/deleted plan folders. This avoids the massive file event storm from
        // worktree operations (git checkout, npm install) that was overflowing
        // the FSW buffer and destabilizing explorer.exe.
        _watcher = new FileSystemWatcher(planFolder)
        {
            NotifyFilter = NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };

        _watcher.Created += (_, _) => ScheduleDebounce(null);
        _watcher.Deleted += (_, _) => ScheduleDebounce(null);
        _watcher.Renamed += (_, _) => ScheduleDebounce(null);
        _watcher.Error += (_, e) =>
            Program.WriteCrashLog($"[{DateTime.UtcNow:O}] PlanWatcher FSW error: {e.GetException()}");

        // Poll as a safety net for external edits to plan.yaml or metadata files
        // that aren't covered by explicit NotifyChanged() calls from JobService.
        _pollTimer = new System.Threading.Timer(_ =>
        {
            try
            {
                ScheduleDebounce(null);
            }
            catch
            {
                // Best-effort polling
            }
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public event Action<string?>? PlansChanged;

    public void NotifyChanged(string? changedPlanFolder = null)
    {
        ScheduleDebounce(changedPlanFolder);
    }

    public void Dispose()
    {
        _pollTimer?.Dispose();
        _watcher?.Dispose();
        _debounceTimer.Dispose();
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
