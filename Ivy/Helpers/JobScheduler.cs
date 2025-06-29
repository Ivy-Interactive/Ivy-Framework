using System.Collections.ObjectModel;
using System.Reactive.Subjects;

namespace Ivy.Helpers;

public enum JobState
{
    Waiting,
    Running,
    Failed,
    Finished,
    Cancelled
}

public interface IJobScheduler
{
    JobBuilder CreateJob(string title);
    void NotifyJobUpdated(Job job);
    void CancelAll();
    void AddChild(Job parent, Job child);
}

public class Job(string title, Func<Job, IJobScheduler, IProgress<double>, CancellationToken, Task> action)
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; } = title;
    public JobState State { get; internal set; } = JobState.Waiting;
    public List<Guid> DependsOn { get; } = new();
    public List<Job> Children { get; } = new();
    public TaskCompletionSource<bool> CompletionSource { get; } = new();
    public double Progress { get; private set; }
    private readonly List<Job> _pendingChildrenToSchedule = new();

    public void AddChildToSchedule(Job child)
    {
        _pendingChildrenToSchedule.Add(child);
    }

    internal async Task ExecuteAsync(IJobScheduler scheduler, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            State = JobState.Cancelled;
            CompletionSource.TrySetCanceled();
            scheduler.NotifyJobUpdated(this);
            return;
        }

        State = JobState.Running;
        scheduler.NotifyJobUpdated(this);

        var progressReporter = new Progress<double>(p =>
        {
            Progress = Math.Clamp(p, 0, 1);
            scheduler.NotifyJobUpdated(this);
        });

        try
        {
            await action(this, scheduler, progressReporter, token);

            // Schedule any children that were added during execution
            var jobScheduler = scheduler as JobScheduler;
            foreach (var child in _pendingChildrenToSchedule)
            {
                jobScheduler?.ScheduleJob(child, token);
            }

            foreach (var child in Children)
                await child.CompletionSource.Task.WaitAsync(token);

            Progress = 1.0;
            State = JobState.Finished;
            CompletionSource.TrySetResult(true);
        }
        catch (OperationCanceledException)
        {
            State = JobState.Cancelled;
            CompletionSource.TrySetCanceled();
        }
        catch (Exception ex)
        {
            State = JobState.Failed;
            CompletionSource.TrySetException(ex);
            scheduler.CancelAll();
        }

        scheduler.NotifyJobUpdated(this);
    }
}

public class JobBuilder
{
    private string _title;
    private readonly List<Job> _dependencies = new();
    private Func<Job, IJobScheduler, IProgress<double>, CancellationToken, Task>? _action;
    private readonly JobScheduler _scheduler;

    private JobBuilder(string title, JobScheduler scheduler)
    {
        _title = title;
        _scheduler = scheduler;
    }

    public static JobBuilder Create(string title, JobScheduler scheduler) => new JobBuilder(title, scheduler);

    public JobBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public JobBuilder WithAction(Func<Job, IJobScheduler, IProgress<double>, CancellationToken, Task> action)
    {
        _action = action;
        return this;
    }

    public JobBuilder WithAction(Func<Task> action)
    {
        _action = (_, _, _, _) => action();
        return this;
    }

    public JobBuilder WithAction(Func<CancellationToken, Task> action)
    {
        _action = (_, _, _, token) => action(token);
        return this;
    }

    public JobBuilder WithAction(Func<Job, Task> action)
    {
        _action = (job, _, _, _) => action(job);
        return this;
    }

    public JobBuilder DependsOn(params Job[] jobs)
    {
        _dependencies.AddRange(jobs);
        return this;
    }

    public JobBuilder Then(string nextTitle, Func<Job, IJobScheduler, IProgress<double>, CancellationToken, Task> nextAction)
    {
        var previousJob = Build();
        return _scheduler.CreateJob(nextTitle)
            .WithAction(nextAction)
            .DependsOn(previousJob);
    }

    public JobBuilder Then(JobBuilder nextBuilder)
    {
        var previousJob = Build();
        return nextBuilder.DependsOn(previousJob);
    }

    public Job Build()
    {
        if (_action == null)
            throw new InvalidOperationException("Action must be set for the job.");

        var job = new Job(_title, _action);
        foreach (var dep in _dependencies)
            job.DependsOn.Add(dep.Id);

        _scheduler.AddJob(job);
        return job;
    }
}

public class JobScheduler(int maxParallelJobs) : IJobScheduler, IObservable<Job>, IDisposable
{
    private readonly Dictionary<Guid, Job> _jobs = new();
    private readonly SemaphoreSlim _concurrency = new(maxParallelJobs);
    private readonly Lock _lock = new();
    private readonly Subject<Job> _jobSubject = new();
    private readonly CancellationTokenSource _internalCts = new();
    private readonly HashSet<Guid> _startedJobs = new();

    public IDisposable Subscribe(IObserver<Job> observer) => _jobSubject.Subscribe(observer);

    public void NotifyJobUpdated(Job job) => _jobSubject.OnNext(job);

    public void AddJob(Job job)
    {
        lock (_lock)
        {
            _jobs[job.Id] = job;
        }
    }

    public void AddChild(Job parent, Job child)
    {
        parent.Children.Add(child);
        AddJob(child);

        // If parent is currently running, add child to pending schedule
        if (parent.State == JobState.Running)
        {
            parent.AddChildToSchedule(child);
        }
    }

    public JobBuilder CreateJob(string title) => JobBuilder.Create(title, this);

    public void CancelAll() => _internalCts.Cancel();

    public async Task RunAsync(CancellationToken token = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _internalCts.Token);

        var completedSignal = new TaskCompletionSource();
        var activeJobCount = 0;

        async Task TryScheduleJobs()
        {
            while (true)
            {
                List<Job> readyJobs;
                lock (_lock)
                {
                    readyJobs = _jobs.Values.Where(j =>
                        j.State == JobState.Waiting &&
                        j.DependsOn.All(d => _jobs[d].State == JobState.Finished) &&
                        !_startedJobs.Contains(j.Id)).ToList();

                    foreach (var job in readyJobs)
                        _startedJobs.Add(job.Id);
                }

                if (readyJobs.Count == 0)
                    break;

                foreach (var job in readyJobs)
                {
                    var isRoot = job.DependsOn.Count == 0;
                    if (isRoot)
                        await _concurrency.WaitAsync(linkedCts.Token);

                    Interlocked.Increment(ref activeJobCount);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await job.ExecuteAsync(this, linkedCts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            job.State = job.State == JobState.Waiting ? JobState.Cancelled : JobState.Failed;
                            job.CompletionSource.TrySetCanceled();
                            NotifyJobUpdated(job);
                        }
                        finally
                        {
                            if (isRoot)
                                _concurrency.Release();

                            await TryScheduleJobs();

                            if (Interlocked.Decrement(ref activeJobCount) == 0)
                                completedSignal.TrySetResult();
                        }
                    }, linkedCts.Token);
                }
            }
        }

        await TryScheduleJobs();
        await completedSignal.Task;

        foreach (var job in _jobs.Values.Where(j => j.State == JobState.Waiting))
        {
            job.State = JobState.Cancelled;
            job.CompletionSource.TrySetCanceled();
            NotifyJobUpdated(job);
        }
    }

    public List<Job> GetRootJobs()
    {
        var allReferenced = new HashSet<Guid>(_jobs.Values.SelectMany(j => j.Children.Select(c => c.Id)));
        var roots = _jobs.Values.Where(j => !allReferenced.Contains(j.Id)).ToList();

        var visited = new HashSet<Guid>();
        var result = new List<Job>();

        void Visit(Job job)
        {
            if (!visited.Add(job.Id)) return;
            foreach (var depId in job.DependsOn)
            {
                if (_jobs.TryGetValue(depId, out var dep))
                    Visit(dep);
            }
            result.Add(job);
        }

        foreach (var root in roots)
            Visit(root);

        return result;
    }

    public void Dispose()
    {
        _concurrency.Dispose();
        _internalCts.Dispose();
        _jobSubject.Dispose();
    }

    internal Task ScheduleJob(Job job, CancellationToken token)
    {
        return Task.Run(async () =>
        {
            try
            {
                // Make sure all dependencies are finished before executing
                foreach (var depId in job.DependsOn)
                {
                    if (_jobs.TryGetValue(depId, out var depJob))
                    {
                        await depJob.CompletionSource.Task.WaitAsync(token);
                    }
                }

                await job.ExecuteAsync(this, token);
            }
            catch (OperationCanceledException)
            {
                job.State = job.State == JobState.Waiting ? JobState.Cancelled : JobState.Failed;
                job.CompletionSource.TrySetCanceled();
                NotifyJobUpdated(job);
            }
        }, token);
    }

    public bool AllCompleted()
    {
        lock (_lock)
        {
            return _jobs.Count > 0 && _jobs.Values.All(job => job.State == JobState.Finished);
        }
    }
}
