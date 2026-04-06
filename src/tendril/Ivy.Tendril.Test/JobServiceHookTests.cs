using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceHookTests
{
    private static (JobService Service, ConfigService Config) CreateServiceWithHooks(
        List<PromptwareHookConfig> hooks, string projectName = "TestProject")
    {
        var settings = new TendrilSettings
        {
            JobTimeout = 30,
            StaleOutputTimeout = 10,
            Projects = new List<ProjectConfig>
            {
                new()
                {
                    Name = projectName,
                    Hooks = hooks,
                }
            }
        };
        var config = new ConfigService(settings);
        var service = new JobService(config);
        return (service, config);
    }

    private static string CreateTempPlanFolder(string projectName = "TestProject")
    {
        var dir = Path.Combine(Path.GetTempPath(), $"ivy-hook-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"), $"state: Executing\nproject: {projectName}\n");
        return dir;
    }

    [Fact]
    public void RunHooks_FiltersBy_When()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new() { Name = "Before Hook", When = "before", Action = "Write-Host before" },
            new() { Name = "After Hook", When = "after", Action = "Write-Host after" },
        };
        var (service, _) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            var job = service.GetJob(id)!;

            // Before hooks should have run during StartJob
            Assert.Contains(job.OutputLines, l => l.Contains("[hook:Before Hook]"));
            Assert.DoesNotContain(job.OutputLines, l => l.Contains("[hook:After Hook]"));

            service.CompleteJob(id, exitCode: 0);

            // After hooks should now have run
            Assert.Contains(job.OutputLines, l => l.Contains("[hook:After Hook]"));
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void RunHooks_EmptyPromptwaresMatchesAll()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new() { Name = "Global Hook", When = "before", Promptwares = new(), Action = "Write-Host global" },
        };
        var (service, _) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();

        try
        {
            var id = service.StartJob("MakePr", planFolder);
            var job = service.GetJob(id)!;

            Assert.Contains(job.OutputLines, l => l.Contains("[hook:Global Hook]"));

            service.CompleteJob(id, exitCode: 0);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void RunHooks_FiltersByPromptwareType()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "Execute Only",
                When = "before",
                Promptwares = new List<string> { "ExecutePlan" },
                Action = "Write-Host execute-only",
            },
        };
        var (service, _) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();

        try
        {
            // Start a MakePr job — the hook should NOT match
            var id = service.StartJob("MakePr", planFolder);
            var job = service.GetJob(id)!;

            Assert.DoesNotContain(job.OutputLines, l => l.Contains("[hook:Execute Only]"));

            service.CompleteJob(id, exitCode: 0);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void RunHooks_FailingHookDoesNotBlockJob()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "Bad Hook",
                When = "before",
                Action = "exit 1",
            },
        };
        var (service, _) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            var job = service.GetJob(id)!;

            // Job should still be running despite hook failure
            Assert.Equal(JobStatus.Running, job.Status);

            service.CompleteJob(id, exitCode: 0);
            Assert.Equal(JobStatus.Completed, job.Status);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void RunHooks_ConditionFalse_SkipsHook()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "Conditional Hook",
                When = "before",
                Condition = "$false",
                Action = "Write-Host should-not-run",
            },
        };
        var (service, _) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            var job = service.GetJob(id)!;

            Assert.Contains(job.OutputLines, l => l.Contains("[hook:Conditional Hook]") && l.Contains("Condition not met"));
            Assert.DoesNotContain(job.OutputLines, l => l.Contains("should-not-run"));

            service.CompleteJob(id, exitCode: 0);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void RunHooks_AllJobStatusesProduceExpectedStrings()
    {
        // Verify that all JobStatus enum values produce their name as a string
        // This ensures hook scripts can safely perform string comparisons
        var expected = new Dictionary<JobStatus, string>
        {
            [JobStatus.Pending] = "Pending",
            [JobStatus.Queued] = "Queued",
            [JobStatus.Running] = "Running",
            [JobStatus.Completed] = "Completed",
            [JobStatus.Failed] = "Failed",
            [JobStatus.Timeout] = "Timeout",
            [JobStatus.Stopped] = "Stopped",
            [JobStatus.Blocked] = "Blocked",
        };

        foreach (var (status, expectedString) in expected)
        {
            var actual = status.ToString();
            Assert.Equal(expectedString, actual);
        }
    }

    [Fact]
    public void RunHooks_HookCanPerformStringComparison()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "StatusCheck",
                When = "after",
                Action = @"
                    if ($env:TENDRIL_JOB_STATUS -eq 'Completed') {
                        Write-Host 'Job completed successfully'
                    } elseif ($env:TENDRIL_JOB_STATUS -eq 'Failed') {
                        Write-Host 'Job failed'
                    } else {
                        Write-Host ""Job status: $env:TENDRIL_JOB_STATUS""
                    }
                ",
            },
        };
        var (service, _) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            service.CompleteJob(id, exitCode: 0);

            var job = service.GetJob(id)!;
            Assert.Contains(job.OutputLines, l => l.Contains("Job completed successfully"));
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void RunHooks_HookCanPerformStringComparison_Failed()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "StatusCheck",
                When = "after",
                Action = @"
                    if ($env:TENDRIL_JOB_STATUS -eq 'Completed') {
                        Write-Host 'Job completed successfully'
                    } elseif ($env:TENDRIL_JOB_STATUS -eq 'Failed') {
                        Write-Host 'Job failed'
                    } else {
                        Write-Host ""Job status: $env:TENDRIL_JOB_STATUS""
                    }
                ",
            },
        };
        var (service, _) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            service.CompleteJob(id, exitCode: 1);

            var job = service.GetJob(id)!;
            Assert.Contains(job.OutputLines, l => l.Contains("Job failed"));
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void RunHooks_AfterHooksReceiveJobStatus()
    {
        // Basic smoke test — see RunHooks_AllJobStatusesProduceExpectedStrings
        // for comprehensive enum-to-string conversion verification
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "Status Hook",
                When = "after",
                Action = "Write-Host $env:TENDRIL_JOB_STATUS",
            },
        };
        var (service, _) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            service.CompleteJob(id, exitCode: 0);

            var job = service.GetJob(id)!;
            Assert.Contains(job.OutputLines, l => l.Contains("[hook:Status Hook]") && l.Contains("Completed"));
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void RunHooks_NoConfigService_DoesNothing()
    {
        // Use the constructor that doesn't take ConfigService
        var service = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id)!;

        // Should not throw, just silently skip hooks
        Assert.Equal(JobStatus.Running, job.Status);

        service.CompleteJob(id, exitCode: 0);
    }
}
