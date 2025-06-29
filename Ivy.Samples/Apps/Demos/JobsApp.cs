using Ivy.Hooks;
using Ivy.Shared;

namespace Ivy.Samples.Apps.Demos;

[App(icon: Icons.Workflow)]
public class JobsApp : ViewBase
{
    public override object? Build()
    {
        var scheduler = this.UseStatic(CreateJobs);
        var refreshToken = this.UseRefreshToken();

        UseEffect(() => scheduler.Subscribe(_ => refreshToken.Refresh()));
        UseEffect(async () => await scheduler.RunAsync());

        return Layout.Horizontal(scheduler.ToView()).Width(100);
    }

    private JobScheduler CreateJobs()
    {
        var scheduler = new JobScheduler(maxParallelJobs: 3);

        Job initializeJob = scheduler.CreateJob("Initialize")
            .WithAction((Job j) =>
            {
                var folderJob = scheduler.CreateJob("Create Folders")
                    .WithAction(async () =>
                    {
                        await Task.Delay(1000);
                    })
                    .Build();

                scheduler.AddChild(j, folderJob);

                return Task.CompletedTask;
            })
            .Build();

        //Declare jobA variable
        Job jobA = null!;

        // Job A
        jobA = scheduler.CreateJob("Job A")
            .WithAction(async (j, _, progress, token) =>
            {
                for (int i = 1; i <= 5; i++)
                {
                    var child = scheduler.CreateJob($"Child A-{i}")
                        .WithAction(async (_, s, p, t) =>
                        {
                            for (int j = 0; j <= 100; j++)
                            {
                                await Task.Delay(50, t);
                                p.Report(j / 100.0);
                            }
                        })
                        .Build();

                    scheduler.AddChild(j, child);
                }

                await Task.Delay(1000, token);
                progress.Report(1);
            })
            .Build();

        // Job B, depends on A
        scheduler.CreateJob("Job B")
            .DependsOn(jobA)
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(1000, token);
                progress.Report(1);
            })
            .Build();

        // Job C, independent   
        scheduler.CreateJob("Job C")
            .WithAction(async (_, _, progress, token) =>
            {
                await Task.Delay(800, token);
                progress.Report(1);
            })
            .Build();

        return scheduler;
    }
}