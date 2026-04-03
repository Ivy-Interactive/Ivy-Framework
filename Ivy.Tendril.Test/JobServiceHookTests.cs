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
            Assert.Equal("Running", job.Status);

            service.CompleteJob(id, exitCode: 0);
            Assert.Equal("Completed", job.Status);
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
    public void RunHooks_AfterHooksReceiveJobStatus()
    {
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
        Assert.Equal("Running", job.Status);

        service.CompleteJob(id, exitCode: 0);
    }

    [Fact]
    public void WriteHookLog_CreatesLogFile()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new() { Name = "TestHook", When = "before", Action = "Write-Host test" },
        };
        var (service, config) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();
        var planReaderService = new PlanReaderService(config);
        service.SetPlanReaderService(planReaderService);

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            var job = service.GetJob(id)!;

            // Check that a log file was created
            var logsDir = Path.Combine(planFolder, "logs");
            Assert.True(Directory.Exists(logsDir), "Logs directory should exist");

            var logFiles = Directory.GetFiles(logsDir, "*.md");
            Assert.NotEmpty(logFiles);

            var logFile = logFiles.First(f => Path.GetFileName(f).Contains("TestHook"));
            Assert.True(File.Exists(logFile), "Log file for TestHook should exist");

            service.CompleteJob(id, exitCode: 0);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void WriteHookLog_ContainsMetadata()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new() { Name = "MetaHook", When = "after", Action = "Write-Host metadata" },
        };
        var (service, config) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();
        var planReaderService = new PlanReaderService(config);
        service.SetPlanReaderService(planReaderService);

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            service.CompleteJob(id, exitCode: 0);

            var logsDir = Path.Combine(planFolder, "logs");
            var logFiles = Directory.GetFiles(logsDir, "*.md");
            var logFile = logFiles.First(f => Path.GetFileName(f).Contains("MetaHook"));
            var content = File.ReadAllText(logFile);

            // Verify metadata fields
            Assert.Contains("**Status:** Completed", content);
            Assert.Contains("**When:** after", content);
            Assert.Contains("**Job Type:** ExecutePlan", content);
            Assert.Contains("**Started:**", content);
            Assert.Contains("**Completed:**", content);
            Assert.Contains("**Duration:**", content);
            Assert.Contains("**Exit Code:** 0", content);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void WriteHookLog_CapturesOutput()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "OutputHook",
                When = "before",
                Action = "Write-Host 'stdout-message'; Write-Error 'stderr-message'",
            },
        };
        var (service, config) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();
        var planReaderService = new PlanReaderService(config);
        service.SetPlanReaderService(planReaderService);

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            service.CompleteJob(id, exitCode: 0);

            var logsDir = Path.Combine(planFolder, "logs");
            var logFiles = Directory.GetFiles(logsDir, "*.md");
            var logFile = logFiles.First(f => Path.GetFileName(f).Contains("OutputHook"));
            var content = File.ReadAllText(logFile);

            // Verify stdout and stderr are captured
            Assert.Contains("## Output", content);
            Assert.Contains("stdout-message", content);
            Assert.Contains("## Errors", content);
            Assert.Contains("stderr-message", content);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void WriteHookLog_SkippedHook_LogsAsSkipped()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "SkippedHook",
                When = "before",
                Condition = "$false",
                Action = "Write-Host should-not-run",
            },
        };
        var (service, config) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();
        var planReaderService = new PlanReaderService(config);
        service.SetPlanReaderService(planReaderService);

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            service.CompleteJob(id, exitCode: 0);

            var logsDir = Path.Combine(planFolder, "logs");
            var logFiles = Directory.GetFiles(logsDir, "*.md");
            var logFile = logFiles.First(f => Path.GetFileName(f).Contains("SkippedHook"));
            var content = File.ReadAllText(logFile);

            // Verify skipped status and condition
            Assert.Contains("**Status:** Skipped", content);
            Assert.Contains("**Condition:** $false (result: not met)", content);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void WriteHookLog_FailedHook_LogsError()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new()
            {
                Name = "FailedHook",
                When = "before",
                Action = "exit 42",
            },
        };
        var (service, config) = CreateServiceWithHooks(hooks);
        var planFolder = CreateTempPlanFolder();
        var planReaderService = new PlanReaderService(config);
        service.SetPlanReaderService(planReaderService);

        try
        {
            var id = service.StartJob("ExecutePlan", planFolder);
            service.CompleteJob(id, exitCode: 0);

            var logsDir = Path.Combine(planFolder, "logs");
            var logFiles = Directory.GetFiles(logsDir, "*.md");
            var logFile = logFiles.First(f => Path.GetFileName(f).Contains("FailedHook"));
            var content = File.ReadAllText(logFile);

            // Verify failed status and exit code
            Assert.Contains("**Status:** Failed", content);
            Assert.Contains("**Exit Code:** 42", content);
        }
        finally
        {
            Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void WriteHookLog_MakePlanBeforeHook_NoLog()
    {
        var hooks = new List<PromptwareHookConfig>
        {
            new() { Name = "MakePlanHook", When = "before", Action = "Write-Host makeplan" },
        };
        var (service, config) = CreateServiceWithHooks(hooks);
        var planReaderService = new PlanReaderService(config);
        service.SetPlanReaderService(planReaderService);

        // Start a job with empty plan folder (simulating MakePlan before-hook)
        var id = service.StartJob("MakePlan", "");

        // No plan folder exists, so no log should be written
        // Just verify the job runs without throwing
        var job = service.GetJob(id)!;
        Assert.Equal("Running", job.Status);

        service.CompleteJob(id, exitCode: 0);
    }
}
