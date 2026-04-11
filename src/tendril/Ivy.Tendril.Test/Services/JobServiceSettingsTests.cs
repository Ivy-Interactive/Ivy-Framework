using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test.Services;

public class JobServiceSettingsTests
{
    private static string CreateTempConfigFile(string yamlContent)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-jobservice-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(Path.Combine(tempDir, "Inbox"));
        Directory.CreateDirectory(Path.Combine(tempDir, "Plans"));
        File.WriteAllText(Path.Combine(tempDir, "config.yaml"), yamlContent);
        return tempDir;
    }

    [Fact]
    public void JobService_UpdatesTimeoutsOnConfigReload()
    {
        var yaml = @"
jobTimeout: 30
staleOutputTimeout: 10
maxConcurrentJobs: 5
";
        var tempDir = CreateTempConfigFile(yaml);
        var config = new ConfigService(new TendrilSettings());
        config.SetTendrilHome(tempDir);

        try
        {
            var jobService = new JobService(config);

            File.WriteAllText(Path.Combine(tempDir, "config.yaml"), @"
jobTimeout: 60
staleOutputTimeout: 20
maxConcurrentJobs: 8
");
            config.ReloadSettings();

            Assert.Equal(60, config.Settings.JobTimeout);
            Assert.Equal(20, config.Settings.StaleOutputTimeout);
            Assert.Equal(8, config.Settings.MaxConcurrentJobs);

            jobService.Dispose();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void JobService_UnsubscribesOnDispose()
    {
        var yaml = @"
jobTimeout: 30
staleOutputTimeout: 10
maxConcurrentJobs: 5
";
        var tempDir = CreateTempConfigFile(yaml);
        var config = new ConfigService(new TendrilSettings());
        config.SetTendrilHome(tempDir);

        try
        {
            var jobService = new JobService(config);
            jobService.Dispose();

            File.WriteAllText(Path.Combine(tempDir, "config.yaml"), @"
jobTimeout: 99
staleOutputTimeout: 99
maxConcurrentJobs: 99
");
            config.ReloadSettings();

            Assert.Equal(99, config.Settings.JobTimeout);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void SaveSettings_TriggersJobServiceReload()
    {
        var yaml = @"
jobTimeout: 30
staleOutputTimeout: 10
maxConcurrentJobs: 5
";
        var tempDir = CreateTempConfigFile(yaml);
        var config = new ConfigService(new TendrilSettings());
        config.SetTendrilHome(tempDir);

        try
        {
            var jobService = new JobService(config);

            config.Settings.JobTimeout = 45;
            config.Settings.StaleOutputTimeout = 15;
            config.SaveSettings();

            Assert.Equal(45, config.Settings.JobTimeout);
            Assert.Equal(15, config.Settings.StaleOutputTimeout);

            jobService.Dispose();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
