using Ivy.Tendril.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class BackgroundServiceActivatorTests : IAsyncLifetime
{
    private readonly string _tempDir;
    private ServiceProvider? _serviceProvider;

    public BackgroundServiceActivatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-activator-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        Directory.CreateDirectory(Path.Combine(_tempDir, "Plans"));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
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

    private ServiceProvider BuildServiceProvider()
    {
        var settings = new TendrilSettings();
        var config = new ConfigService(settings, _tempDir);

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
        services.AddSingleton<IStartable>(sp => sp.GetRequiredService<WorktreeCleanupService>());
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
        return _serviceProvider;
    }

    [Fact]
    public void Start_ResolvesAllExpectedServices()
    {
        var sp = BuildServiceProvider();

        // Should not throw — all services are registered and resolvable
        BackgroundServiceActivator.Start(sp);

        // Verify the services were resolved by checking they exist in the container
        var planWatcher = sp.GetRequiredService<IPlanWatcherService>();
        var inboxWatcher = sp.GetRequiredService<IInboxWatcherService>();
        var worktreeCleanup = sp.GetRequiredService<WorktreeCleanupService>();
        var syncService = sp.GetRequiredService<PlanDatabaseSyncService>();

        Assert.NotNull(planWatcher);
        Assert.NotNull(inboxWatcher);
        Assert.NotNull(worktreeCleanup);
        Assert.NotNull(syncService);
    }

    [Fact]
    public void Start_ThrowsWhenServiceMissing()
    {
        // Register only some services — omit IPlanWatcherService
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => BackgroundServiceActivator.Start(sp));
    }

    [Fact]
    public void Start_CallsStartOnAllRegisteredIStartables()
    {
        var startable1 = new MockStartable();
        var startable2 = new MockStartable();

        var settings = new TendrilSettings();
        var config = new ConfigService(settings, _tempDir);

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
        services.AddSingleton<IStartable>(startable1);
        services.AddSingleton<IStartable>(startable2);
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

        BackgroundServiceActivator.Start(_serviceProvider);

        Assert.True(startable1.Started);
        Assert.True(startable2.Started);
    }

    private class MockStartable : IStartable
    {
        public bool Started { get; private set; }
        public void Start() => Started = true;
    }
}
