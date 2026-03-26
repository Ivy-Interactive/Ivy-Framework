using System.Collections.Concurrent;
using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Tendril.Apps.Tasks;

namespace Ivy.Tendril.Services;

public class TaskService
{
    private readonly ConcurrentDictionary<string, TaskItem> _tasks = new();
    private int _counter;

    private static readonly string PromptsRoot =
        Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", ".promptwares"));

    private static readonly Dictionary<string, string> ScriptPaths = new()
    {
        ["MakePlan"] = Path.Combine(PromptsRoot, "MakePlan.ps1"),
        ["UpdatePlan"] = Path.Combine(PromptsRoot, "UpdatePlan.ps1"),
        ["SplitPlan"] = Path.Combine(PromptsRoot, "SplitPlan.ps1"),
        ["ExpandPlan"] = Path.Combine(PromptsRoot, "ExpandPlan.ps1"),
    };

    public string StartTask(string type, params string[] args)
    {
        var id = $"task-{Interlocked.Increment(ref _counter):D3}";
        var scriptPath = ScriptPaths.GetValueOrDefault(type, "");
        var planFile = args.Length > 0 ? Path.GetFileName(args[0]) : "";

        var task = new TaskItem
        {
            Id = id,
            Type = type,
            PlanFile = planFile,
            Queue = "Tendril",
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            ScriptPath = scriptPath,
            Args = args,
        };

        _tasks[id] = task;
        return id;
    }

    public void CompleteTask(string id, int? exitCode)
    {
        if (!_tasks.TryGetValue(id, out var task)) return;

        task.Status = exitCode == 0 ? "Completed" : "Failed";
        task.CompletedAt = DateTime.UtcNow;
        if (task.StartedAt.HasValue)
            task.DurationSeconds = (int)(task.CompletedAt.Value - task.StartedAt.Value).TotalSeconds;
    }

    public void StopTask(string id)
    {
        if (!_tasks.TryGetValue(id, out var task)) return;

        task.CancellationRequested = true;
        task.Status = "Stopped";
        task.CompletedAt = DateTime.UtcNow;
        if (task.StartedAt.HasValue)
            task.DurationSeconds = (int)(task.CompletedAt.Value - task.StartedAt.Value).TotalSeconds;
    }

    public void DeleteTask(string id)
    {
        _tasks.TryRemove(id, out _);
    }

    public List<TaskItem> GetTasks()
    {
        return _tasks.Values.OrderByDescending(t => t.StartedAt ?? DateTime.MinValue).ToList();
    }

    public TaskItem? GetTask(string id)
    {
        return _tasks.GetValueOrDefault(id);
    }

    public void InitializePty(string id, PtyHandle ptyHandle)
    {
        if (!_tasks.TryGetValue(id, out var task)) return;

        task.PtyStream = ptyHandle.Stream;
        task.PtyHandleInput = ptyHandle.HandleInput;
        task.PtyHandleResize = ptyHandle.HandleResize;
        task.PtyKill = ptyHandle.Kill;
        task.PtyClosed = ptyHandle.Closed;
        task.PtyExitCode = ptyHandle.ExitCode;
    }

    public void UpdatePtyState(string id, bool closed, int? exitCode)
    {
        if (!_tasks.TryGetValue(id, out var task)) return;

        task.PtyClosed = closed;
        task.PtyExitCode = exitCode;
    }
}
