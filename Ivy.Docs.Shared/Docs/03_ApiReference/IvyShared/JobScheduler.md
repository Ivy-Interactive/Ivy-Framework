---
searchHints:
  - job scheduler
  - background tasks
  - async workflows
  - dependency graph
  - progress tracking
---

# JobScheduler

<!-- markdownlint-disable-next-line MD033 -->
<Ingress>
Coordinate complex async work with declarative job graphs, dependency-aware scheduling, and built-in UI status reporting.
</Ingress>

The `JobScheduler` in `Ivy.Helpers` orchestrates asynchronous jobs, resolves dependencies, and exposes real-time state via reactive updates. Pair it with `JobSchedulerExtensions.ToView()` to render a hierarchical job monitor.

## Basic Usage

Create a scheduler, define jobs, and use a button to trigger execution:

```csharp demo-tabs
public class JobsDashboard : ViewBase
{
    public override object? Build()
    {
        var scheduler = this.UseStatic(BuildScheduler);
        var refresh = this.UseRefreshToken();

        UseEffect(() => scheduler.Subscribe(_ => refresh.Refresh()));

        return Layout.Vertical()
            | new Button("Start Jobs", onClick: async _ => await scheduler.RunAsync())
            | scheduler.ToView();
    }

    private static JobScheduler BuildScheduler()
    {
        var scheduler = new JobScheduler(maxParallelJobs: 2);

        var initialize = scheduler.CreateJob("Initialize")
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(300, token);
                progress.Report(1);
            })
            .Build();

        scheduler.CreateJob("Load Data")
            .DependsOn(initialize)
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(500, token);
                progress.Report(1);
            })
            .Build();

        scheduler.CreateJob("Refresh Cache")
            .WithAction(async (_, _, progress, token) =>
            {
                for (var i = 0; i < 5; i++)
                {
                    await Task.Delay(100, token);
                    progress.Report((i + 1) / 5.0);
                }
            })
            .Build();

        return scheduler;
    }
}
```

## Job States

Jobs transition through these states automatically:

```mermaid
graph LR
    A[Waiting] --> B[Running]
    B --> C[Finished]
    B --> D[Failed]
    A --> E[Cancelled]
    B --> E
    
```

## Creating Jobs

Use `CreateJob()` to obtain a `JobBuilder`, configure the job, then call `Build()` to register it.

```csharp demo-tabs
public class ImportJobDemo : ViewBase
{
    public override object? Build()
    {
        var scheduler = this.UseStatic(BuildScheduler);
        var refresh = this.UseRefreshToken();

        UseEffect(() => scheduler.Subscribe(_ => refresh.Refresh()));

        return Layout.Vertical()
            | new Button("Import Data", onClick: async _ => await scheduler.RunAsync())
            | scheduler.ToView();
    }

    private static JobScheduler BuildScheduler()
    {
        var scheduler = new JobScheduler(maxParallelJobs: 1);

        scheduler.CreateJob("Import Customers")
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(250, token);
                progress.Report(0.25);

                await Task.Delay(400, token);
                progress.Report(0.75);

                await Task.Delay(150, token);
                progress.Report(1);
            })
            .Build();

        return scheduler;
    }
}
```

`WithAction` overloads accept signatures ranging from `Func<Task>` to full access with `(Job, IJobScheduler, IProgress<double>, CancellationToken)`.

### Reporting Progress

Inside the action, call `progress.Report(0..1)`. The scheduler clamps values and the built-in view renders a progress bar.

### Setting Display Content

Call `job.SetDisplay(object)` within the action to attach custom status UI. The view renders any display object between the job header and its children.

## Dependencies

### Linear Dependencies

`DependsOn` enforces that a job waits for its prerequisites to finish before entering the queue.

```csharp demo-tabs
public class DependencyGraphDemo : ViewBase
{
    public override object? Build()
    {
        var scheduler = this.UseStatic(BuildScheduler);
        var refresh = this.UseRefreshToken();

        UseEffect(() => scheduler.Subscribe(_ => refresh.Refresh()));

        return Layout.Vertical()
            | new Button("Run Pipeline", onClick: async _ => await scheduler.RunAsync())
            | scheduler.ToView();
    }

    private static JobScheduler BuildScheduler()
    {
        var scheduler = new JobScheduler(maxParallelJobs: 1);

        var extract = scheduler.CreateJob("Extract Data")
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(300, token);
                progress.Report(1);
            })
            .Build();

        var transform = scheduler.CreateJob("Transform Data")
            .DependsOn(extract)
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(300, token);
                progress.Report(1);
            })
            .Build();

        scheduler.CreateJob("Load Data")
            .DependsOn(extract, transform)
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(300, token);
                progress.Report(1);
            })
            .Build();

        return scheduler;
    }
}
```

### Dynamically Adding Child Jobs

Children can be attached to a parent at build time or while the parent is running. When added during execution, the scheduler defers them until the parent finishes its action.

```csharp demo-tabs
public class DynamicChildrenDemo : ViewBase
{
    public override object? Build()
    {
        var scheduler = this.UseStatic(BuildScheduler);
        var refresh = this.UseRefreshToken();

        UseEffect(() => scheduler.Subscribe(_ => refresh.Refresh()));

        return Layout.Vertical()
            | new Button("Generate Reports", onClick: async _ => await scheduler.RunAsync())
            | scheduler.ToView();
    }

    private static JobScheduler BuildScheduler()
    {
        var scheduler = new JobScheduler(maxParallelJobs: 2);

        scheduler.CreateJob("Generate Reports")
            .WithAction(async (job, sched, _, token) =>
            {
                for (int i = 1; i <= 3; i++)
                {
                    var child = sched.CreateJob($"Report {i}")
                        .WithAction(async (_, _, progress, childToken) =>
                        {
                            await Task.Delay(200, childToken);
                            progress.Report(1);
                        })
                        .Build();

                    sched.AddChild(job, child);
                }

                await Task.Delay(600, token);
            })
            .Build();

        return scheduler;
    }
}
```

### Handling Failures

- Exceptions inside a job transition it to `Failed` and propagate through `CompletionSource`.
- By default, `CancelAll()` is invoked when a job fails. Opt in to continue by calling `WithContinueOnChildFailure()` on the parent job builder.
- Cancellation requests set state to `Cancelled` and complete the job's task with `TrySetCanceled()`.

## UI Rendering

`JobSchedulerExtensions.ToView()` renders a collapsible tree showing each job's title, icon by state, progress, custom display, and failures.

```csharp demo-tabs
public class JobMonitorDemo : ViewBase
{
    public override object? Build()
    {
        var scheduler = this.UseStatic(() => BuildScheduler());
        var refresh = this.UseRefreshToken();

        UseEffect(() => scheduler.Subscribe(_ => refresh.Refresh()));

        return Layout.Vertical()
            | Text.H2("Background Jobs")
            | new Button("Start Jobs", onClick: async _ => await scheduler.RunAsync())
            | scheduler.ToView();
    }

    private static JobScheduler BuildScheduler()
    {
        var scheduler = new JobScheduler(maxParallelJobs: 4);

        var plan = scheduler.CreateJob("Plan")
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(200, token);
                progress.Report(1);
            })
            .Build();

        scheduler.CreateJob("Execute")
            .DependsOn(plan)
            .WithAction(async (_, _, progress, token) =>
            {
                for (var i = 0; i < 4; i++)
                {
                    await Task.Delay(150, token);
                    progress.Report((i + 1) / 4.0);
                }
            })
            .Build();

        scheduler.CreateJob("Review")
            .DependsOn(plan)
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(400, token);
                progress.Report(1);
            })
            .Build();

        return scheduler;
    }
}
```

The view automatically:

- Chooses icons for waiting, running, finished, failed, and cancelled jobs.
- Animates running jobs and shows percentage bars when progress is between 0% and 100%.
- Displays only relevant error details (parent errors hide when a failing child already shows them).
- Nests child jobs with separators and hides completed branches to reduce noise.

## Running and Monitoring

- `RunAsync(token)` starts scheduling and waits until all jobs settle. Remaining `Waiting` jobs are cancelled when scheduling completes.
- `CancelAll()` requests cancellation on all running jobs and prevents new scheduling.
- `AllCompleted()` returns `true` when every job is `Finished`.
- `Subscribe(observer)` yields each job update; use it to refresh state or log progress.

## Reference

| Method | Description |
|--------|-------------|
| `JobScheduler(int maxParallelJobs)` | Constructor that controls concurrency and lifecycle. |
| `JobScheduler.CreateJob(string title)` | Creates a new job builder. Returns `JobBuilder`. |
| `JobBuilder.WithAction(...)` | Registers job logic. Supports multiple overloads. |
| `JobBuilder.DependsOn(params Job[] jobs)` | Sets job prerequisites. |
| `JobBuilder.WithContinueOnChildFailure(bool)` | Keeps parents alive when children fail. |
| `JobBuilder.Then(...)` | Chains dependent jobs fluently. |
| `JobScheduler.AddChild(Job parent, Job child)` | Links dynamic child work to a parent job. |
| `JobSchedulerExtensions.ToView()` | Renders the scheduler state using Ivy components. |
| `JobScheduler.RunAsync(CancellationToken?)` | Starts scheduling and waits until all jobs settle. |
| `JobScheduler.CancelAll()` | Requests cancellation on all running jobs. |
| `JobScheduler.AllCompleted()` | Returns `true` when every job is `Finished`. |
| `JobScheduler.Subscribe(IObserver<Job> observer)` | Yields each job update for state refresh or logging. |
