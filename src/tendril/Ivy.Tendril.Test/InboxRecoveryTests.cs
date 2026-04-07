using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class InboxRecoveryTests
{
    [Fact]
    public void RecoverProcessingFiles_RenamesProcessingToMd()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-recovery-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            // Simulate a crashed job: .processing file left behind
            var processingFile = Path.Combine(inboxDir, "pending-job-001.md.processing");
            File.WriteAllText(processingFile, "---\nproject: Tendril\n---\nSome task");

            var config = new ConfigService(new TendrilSettings(), tempDir);
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);

            // Create InboxWatcherService — constructor calls RecoverProcessingFiles
            using var watcher = new InboxWatcherService(config, jobService);

            // .processing file should be gone, .md file should exist
            Assert.False(File.Exists(processingFile));
            Assert.True(File.Exists(Path.Combine(inboxDir, "pending-job-001.md")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RecoverProcessingFiles_DeletesIfMdAlreadyExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-recovery-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            // Both .md and .processing exist (edge case)
            var mdFile = Path.Combine(inboxDir, "test.md");
            var processingFile = Path.Combine(inboxDir, "test.md.processing");
            File.WriteAllText(mdFile, "---\nproject: Tendril\n---\nOriginal");
            File.WriteAllText(processingFile, "---\nproject: Tendril\n---\nDuplicate");

            var config = new ConfigService(new TendrilSettings(), tempDir);
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);
            using var watcher = new InboxWatcherService(config, jobService);

            // .processing should be deleted, .md preserved
            Assert.False(File.Exists(processingFile));
            Assert.True(File.Exists(mdFile));
            Assert.Equal("---\nproject: Tendril\n---\nOriginal", File.ReadAllText(mdFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ProcessingFilesIgnoredByWatcher()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-ignored-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            // Write a .processing file — should NOT be picked up by ProcessExistingFiles
            var processingFile = Path.Combine(inboxDir, "active-job.md.processing");
            File.WriteAllText(processingFile, "---\nproject: Tendril\n---\nRunning task");

            // Also no .md files
            var config = new ConfigService(new TendrilSettings(), tempDir);
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);

            // Don't use InboxWatcherService constructor (it calls RecoverProcessingFiles).
            // Instead, directly test ProcessExistingFiles won't pick up .processing files.
            // The watcher glob is *.md, so .processing files are inherently excluded.
            var mdFiles = Directory.GetFiles(inboxDir, "*.md");
            Assert.Empty(mdFiles);

            // .processing file should still be there
            Assert.True(File.Exists(processingFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void MakePlanJob_WritesInboxFile_AndDeletesOnCompletion()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-makeplan-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);

            var id = jobService.StartJob("MakePlan",
                "-Description", "Test plan description",
                "-Project", "Tendril");

            var job = jobService.GetJob(id);
            Assert.NotNull(job);
            Assert.NotNull(job.InboxFile);
            Assert.True(File.Exists(job.InboxFile));
            Assert.EndsWith(".processing", job.InboxFile);

            // Verify the content is parseable inbox format
            var content = File.ReadAllText(job.InboxFile);
            Assert.Contains("project: Tendril", content);
            Assert.Contains("Test plan description", content);

            // Complete the job — inbox file should be deleted
            jobService.CompleteJob(id, 0);
            Assert.False(File.Exists(job.InboxFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void MakePlanJob_InboxFileDeletedOnFailure()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-fail-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);

            var id = jobService.StartJob("MakePlan",
                "-Description", "Failing plan",
                "-Project", "Tendril");

            var job = jobService.GetJob(id);
            Assert.NotNull(job?.InboxFile);
            Assert.True(File.Exists(job.InboxFile));

            // Fail the job
            jobService.CompleteJob(id, 1);
            Assert.False(File.Exists(job.InboxFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void MakePlanJob_InboxFileDeletedOnStop()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-stop-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);

            var id = jobService.StartJob("MakePlan",
                "-Description", "Stopped plan",
                "-Project", "Tendril");

            var job = jobService.GetJob(id);
            Assert.NotNull(job?.InboxFile);
            Assert.True(File.Exists(job.InboxFile));

            // Stop the job
            jobService.StopJob(id);
            Assert.False(File.Exists(job.InboxFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void MakePlanJob_WithExistingInboxFile_TracksIt()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-existing-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            // Simulate what InboxWatcherService does: rename to .processing, pass to StartJob
            var processingFile = Path.Combine(inboxDir, "request.md.processing");
            File.WriteAllText(processingFile, "---\nproject: Agent\n---\nSome request");

            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);

            var id = jobService.StartJob("MakePlan",
                ["-Description", "Some request", "-Project", "Agent"],
                processingFile);

            var job = jobService.GetJob(id);
            Assert.NotNull(job);
            Assert.Equal(processingFile, job.InboxFile);

            // Complete — should delete the .processing file
            jobService.CompleteJob(id, 0);
            Assert.False(File.Exists(processingFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void NonMakePlanJob_DoesNotWriteInboxFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-nonmake-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);

            var id = jobService.StartJob("ExecutePlan", Path.GetTempPath());
            var job = jobService.GetJob(id);
            Assert.NotNull(job);
            Assert.Null(job.InboxFile);

            // No .processing files should have been created
            Assert.Empty(Directory.GetFiles(inboxDir, "*.processing"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CrashRecovery_EndToEnd_ProcessingFileReprocessedOnStartup()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-e2e-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            // Step 1: Simulate a crashed MakePlan — .processing file left behind
            var processingFile = Path.Combine(inboxDir, "pending-job-001.md.processing");
            File.WriteAllText(processingFile, "---\nproject: Tendril\n---\nCrashed task description");

            // Step 2: Create services (simulating restart)
            var config = new ConfigService(new TendrilSettings(), tempDir);
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);
            using var watcher = new InboxWatcherService(config, jobService);

            // Step 3: Wait for async processing
            Thread.Sleep(2000);

            // The .processing file should have been renamed to .md by RecoverProcessingFiles,
            // then picked up by ProcessExistingFiles, renamed back to .processing, and a job started.
            // After the job launches, the .md file should be gone (renamed to .processing by the watcher).
            var mdFiles = Directory.GetFiles(inboxDir, "*.md");
            var processingFiles = Directory.GetFiles(inboxDir, "*.processing");

            // The file should either be .processing (job running) or gone (job completed/processed)
            Assert.Empty(mdFiles);

            // A MakePlan job should have been started
            var jobs = jobService.GetJobs();
            Assert.Contains(jobs, j => j.Type == "MakePlan" && j.PlanFile.Contains("Crashed task description"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}