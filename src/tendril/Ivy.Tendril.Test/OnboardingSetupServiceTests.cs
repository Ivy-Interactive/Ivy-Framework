using Ivy.Tendril.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class OnboardingSetupServiceTests : IAsyncLifetime
{
    private readonly string _tempDir;
    private ServiceProvider? _serviceProvider;

    public OnboardingSetupServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-onboarding-test-{Guid.NewGuid()}");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            // Dispose services to release SQLite connections
            await _serviceProvider.DisposeAsync();
            // Give background tasks a moment to complete
            await Task.Delay(100);
        }

        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch
        {
            // Best effort cleanup
        }
    }

    private (OnboardingSetupService service, ConfigService config) CreateService()
    {
        var settings = new TendrilSettings();
        var config = new ConfigService(settings, "");
        config.SetPendingTendrilHome(_tempDir);

        var services = new ServiceCollection();
        services.AddSingleton<IConfigService>(config);
        services.AddSingleton<ConfigService>(config);
        services.AddSingleton<IPlanWatcherService>(new PlanWatcherService(config));
        services.AddSingleton<IInboxWatcherService>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfigService>();
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
            return new InboxWatcherService(cfg, jobService);
        });
        services.AddSingleton<WorktreeCleanupService>(sp =>
            new WorktreeCleanupService(Path.Combine(_tempDir, "Plans"), NullLogger<WorktreeCleanupService>.Instance));
        services.AddSingleton<IPlanDatabaseService>(sp =>
        {
            var dbPath = Path.Combine(_tempDir, "tendril.db");
            return new PlanDatabaseService(dbPath, NullLogger<PlanDatabaseService>.Instance);
        });
        services.AddSingleton<PlanDatabaseSyncService>(sp =>
        {
            var planReader = new PlanReaderService(config, NullLogger<PlanReaderService>.Instance);
            var database = sp.GetRequiredService<IPlanDatabaseService>();
            var watcher = sp.GetRequiredService<IPlanWatcherService>();
            return new PlanDatabaseSyncService(planReader, database, watcher,
                NullLogger<PlanDatabaseSyncService>.Instance);
        });

        _serviceProvider = services.BuildServiceProvider();
        var service = new OnboardingSetupService(config, _serviceProvider);

        return (service, config);
    }

    [Fact]
    public async Task CompleteSetupAsync_CreatesDirectoryStructure()
    {
        var (service, _) = CreateService();

        await service.CompleteSetupAsync(_tempDir);

        Assert.True(Directory.Exists(Path.Combine(_tempDir, "Inbox")));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "Plans")));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "Trash")));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "Promptwares")));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "Hooks")));
    }

    [Fact]
    public async Task CompleteSetupAsync_CopiesExampleConfig_WhenExists()
    {
        var (service, _) = CreateService();

        // Create example config next to AppContext.BaseDirectory
        var baseDir = System.AppContext.BaseDirectory;
        var examplePath = Path.Combine(baseDir, "example.config.yaml");
        var exampleContent = "codingAgent: test\nprojects: []\nverifications: []\n";
        await File.WriteAllTextAsync(examplePath, exampleContent);

        try
        {
            await service.CompleteSetupAsync(_tempDir);

            var configPath = Path.Combine(_tempDir, "config.yaml");
            Assert.True(File.Exists(configPath));
            var content = await File.ReadAllTextAsync(configPath);
            Assert.Contains("codingAgent: test", content);
        }
        finally
        {
            File.Delete(examplePath);
        }
    }

    [Fact]
    public async Task CompleteSetupAsync_CreatesBasicConfig_WhenNoExampleExists()
    {
        var (service, _) = CreateService();

        // Ensure no example config exists anywhere up from BaseDirectory
        var baseDir = System.AppContext.BaseDirectory;
        var checkDir = baseDir;
        while (checkDir != null)
        {
            var examplePath = Path.Combine(checkDir, "example.config.yaml");
            if (File.Exists(examplePath))
                File.Delete(examplePath);
            checkDir = Path.GetDirectoryName(checkDir);
        }

        await service.CompleteSetupAsync(_tempDir);

        var configPath = Path.Combine(_tempDir, "config.yaml");
        Assert.True(File.Exists(configPath));
        var content = await File.ReadAllTextAsync(configPath);
        Assert.Contains("codingAgent: claude", content);
        Assert.Contains("projects: []", content);
        Assert.Contains("codingAgents:", content);
        Assert.Contains("ClaudeCode", content);
        Assert.Contains("Codex", content);
        Assert.Contains("Gemini", content);
    }

    [Fact]
    public async Task CompleteSetupAsync_PreservesPendingCodingAgent()
    {
        var (service, config) = CreateService();

        var baseDir = System.AppContext.BaseDirectory;
        var examplePath = Path.Combine(baseDir, "example.config.yaml");
        var exampleContent = "codingAgent: claude\nprojects: []\nverifications: []\n";
        await File.WriteAllTextAsync(examplePath, exampleContent);

        try
        {
            config.SetPendingCodingAgent("codex");

            await service.CompleteSetupAsync(_tempDir);

            Assert.Equal("codex", config.Settings.CodingAgent);
            var savedContent = await File.ReadAllTextAsync(Path.Combine(_tempDir, "config.yaml"));
            Assert.Contains("codingAgent: codex", savedContent);
        }
        finally
        {
            File.Delete(examplePath);
        }
    }

    [Fact]
    public async Task CompleteSetupAsync_CallsCompleteOnboarding()
    {
        var (service, config) = CreateService();

        await service.CompleteSetupAsync(_tempDir);

        Assert.False(config.NeedsOnboarding);
        Assert.Equal(_tempDir, config.TendrilHome);
    }

    [Fact]
    public async Task CompleteSetupAsync_InitializesPlanCounter()
    {
        var (service, _) = CreateService();

        await service.CompleteSetupAsync(_tempDir);

        var counterPath = Path.Combine(_tempDir, "Plans", ".counter");
        Assert.True(File.Exists(counterPath));
        var content = (await File.ReadAllTextAsync(counterPath)).Trim();
        Assert.Equal("1", content);
    }

    [Fact]
    public async Task CompleteSetupAsync_PersistsEnvironmentVariable_UnixShellRc()
    {
        if (OperatingSystem.IsWindows())
            return;

        var (service, _) = CreateService();

        await service.CompleteSetupAsync(_tempDir);

        var shell = Environment.GetEnvironmentVariable("SHELL") ?? "";
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var rcFile = shell.EndsWith("/zsh") ? Path.Combine(home, ".zshrc")
                   : shell.EndsWith("/bash") ? Path.Combine(home, ".bashrc")
                   : Path.Combine(home, ".profile");

        var content = await File.ReadAllTextAsync(rcFile);
        Assert.Contains($"export TENDRIL_HOME=\"{_tempDir}\"", content);
    }

    [Fact]
    public async Task CompleteSetupAsync_ExampleConfigContainsVerifications()
    {
        var (service, _) = CreateService();

        var baseDir = System.AppContext.BaseDirectory;
        var examplePath = Path.Combine(baseDir, "example.config.yaml");
        var exampleContent = File.Exists(examplePath) ? await File.ReadAllTextAsync(examplePath) : "";

        if (!exampleContent.Contains("DotnetBuild"))
        {
            exampleContent =
                "codingAgent: claude\nprojects: []\nverifications:\n- name: DotnetBuild\n  prompt: Build\n";
            await File.WriteAllTextAsync(examplePath, exampleContent);
        }

        try
        {
            await service.CompleteSetupAsync(_tempDir);

            var configPath = Path.Combine(_tempDir, "config.yaml");
            var content = await File.ReadAllTextAsync(configPath);
            Assert.Contains("DotnetBuild", content);
        }
        finally
        {
            // Don't delete if it was already there
        }
    }
}
