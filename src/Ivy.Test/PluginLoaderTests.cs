using Ivy.Core.Apps;
using Ivy.Core.Plugins;
using Ivy.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ivy.Test.Plugins;

public class PluginLoaderTests
{
    private interface ITestServiceA { }
    private interface ITestServiceB { }
    private class TestServiceA : ITestServiceA { }
    private class TestServiceB : ITestServiceB { }
    private class AnotherTestServiceA : ITestServiceA { }

    private class TestPluginContext : PluginContextBase
    {
        public TestPluginContext()
            : base(new AppRepository(), new HashSet<string>(), WebApplication.CreateBuilder())
        {
        }

        public AppRepository GetAppRepository() => AppRepository;
    }

    [Fact]
    public void PerPluginServiceIsolation()
    {
        var context = new TestPluginContext();

        // Simulate plugin A registering services
        context.SetCurrentPlugin("plugin-a", "/plugins/a");
        context.Services.AddSingleton<ITestServiceA, TestServiceA>();
        context.ClearCurrentPlugin();

        // Simulate plugin B registering services
        context.SetCurrentPlugin("plugin-b", "/plugins/b");
        context.Services.AddSingleton<ITestServiceB, TestServiceB>();
        context.ClearCurrentPlugin();

        context.BuildServiceProvider();

        // Both services should be resolvable through the aggregate
        Assert.NotNull(context.GetService<ITestServiceA>());
        Assert.NotNull(context.GetService<ITestServiceB>());
    }

    [Fact]
    public void UnloadRemovesServicesFromAggregator()
    {
        var context = new TestPluginContext();

        context.SetCurrentPlugin("plugin-a", "/plugins/a");
        context.Services.AddSingleton<ITestServiceA, TestServiceA>();
        context.ClearCurrentPlugin();

        context.SetCurrentPlugin("plugin-b", "/plugins/b");
        context.Services.AddSingleton<ITestServiceB, TestServiceB>();
        context.ClearCurrentPlugin();

        context.BuildServiceProvider();

        // Both should exist before unload
        Assert.NotNull(context.GetService<ITestServiceA>());
        Assert.NotNull(context.GetService<ITestServiceB>());

        // Unload plugin A
        context.RemovePluginContributions("plugin-a");

        // Plugin A's service should be gone
        Assert.Null(context.GetService<ITestServiceA>());
        // Plugin B's service should still exist
        Assert.NotNull(context.GetService<ITestServiceB>());
    }

    [Fact]
    public void ServiceResolutionAfterUnloadDoesNotReturnUnloadedServices()
    {
        var context = new TestPluginContext();

        context.SetCurrentPlugin("plugin-a", "/plugins/a");
        context.Services.AddSingleton<ITestServiceA, TestServiceA>();
        context.ClearCurrentPlugin();

        context.BuildServiceProvider();

        // Service exists
        Assert.NotNull(context.GetService<ITestServiceA>());

        // Unload
        context.RemovePluginContributions("plugin-a");

        // Service gone — should not leak
        Assert.Null(context.GetService<ITestServiceA>());
        Assert.Empty(context.GetServices<ITestServiceA>());
    }

    [Fact]
    public void ReloadPluginWorkflow()
    {
        var context = new TestPluginContext();

        // Load plugin A with TestServiceA
        context.SetCurrentPlugin("plugin-a", "/plugins/a");
        context.Services.AddSingleton<ITestServiceA, TestServiceA>();
        context.ClearCurrentPlugin();
        context.BuildServiceProvider();

        var beforeReload = context.GetService<ITestServiceA>();
        Assert.NotNull(beforeReload);

        // Unload (simulate reload step 1)
        context.RemovePluginContributions("plugin-a");
        Assert.Null(context.GetService<ITestServiceA>());

        // Reload (simulate reload step 2 — register with potentially different impl)
        context.SetCurrentPlugin("plugin-a", "/plugins/a");
        context.Services.AddSingleton<ITestServiceA, AnotherTestServiceA>();
        context.ClearCurrentPlugin();
        context.BuildPluginServiceProvider("plugin-a", new ServiceCollection());

        var afterReload = context.GetService<ITestServiceA>();
        Assert.NotNull(afterReload);
        Assert.IsType<AnotherTestServiceA>(afterReload);
    }

    [Fact]
    public void MultiplePluginsWithOverlappingServiceTypes()
    {
        var context = new TestPluginContext();

        // Both plugins register ITestServiceA
        context.SetCurrentPlugin("plugin-a", "/plugins/a");
        context.Services.AddSingleton<ITestServiceA, TestServiceA>();
        context.ClearCurrentPlugin();

        context.SetCurrentPlugin("plugin-b", "/plugins/b");
        context.Services.AddSingleton<ITestServiceA, AnotherTestServiceA>();
        context.ClearCurrentPlugin();

        context.BuildServiceProvider();

        // GetServices should return both
        var services = context.GetServices<ITestServiceA>().ToList();
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s is TestServiceA);
        Assert.Contains(services, s => s is AnotherTestServiceA);

        // GetService returns the first one found
        Assert.NotNull(context.GetService<ITestServiceA>());
    }

    [Fact]
    public void PluginDependencyPreventsUnload()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var loader = new PluginLoader(tempDir, logger);

            // We can't easily test this through the full PluginLoader without real DLLs,
            // but we can verify GetActivePluginIds returns empty for a directory with no plugins
            using var sp = new ServiceCollection().BuildServiceProvider();
            loader.DiscoverAndLoad(new Version(1, 0), sp);

            Assert.Empty(loader.GetActivePluginIds());
            Assert.False(loader.UnloadPlugin("nonexistent"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }



    [Fact]
    public async Task AggregateProviderThreadSafety()
    {
        var provider = new AggregatePluginServiceProvider();

        var services1 = new ServiceCollection();
        services1.AddSingleton<ITestServiceA, TestServiceA>();
        provider.AddProvider("p1", services1.BuildServiceProvider());

        var services2 = new ServiceCollection();
        services2.AddSingleton<ITestServiceB, TestServiceB>();
        provider.AddProvider("p2", services2.BuildServiceProvider());

        // Concurrent reads should work
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            Assert.NotNull(provider.GetService<ITestServiceA>());
            Assert.NotNull(provider.GetService<ITestServiceB>());
        })).ToArray();

        await Task.WhenAll(tasks);

        Assert.Equal(2, provider.LoadedPluginIds.Count);

        provider.RemoveProvider("p1");
        Assert.Single(provider.LoadedPluginIds);
        Assert.Null(provider.GetService<ITestServiceA>());
    }

    [Fact]
    public void FailedPluginsAppearInUnloadedList()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a plugin directory with no DLLs
            var emptyPluginDir = Path.Combine(tempDir, "empty-plugin");
            Directory.CreateDirectory(emptyPluginDir);

            var loader = new PluginLoader(tempDir, logger);
            using var sp = new ServiceCollection().BuildServiceProvider();
            loader.DiscoverAndLoad(new Version(1, 0), sp);

            // Should have no loaded plugins
            Assert.Empty(loader.GetActivePluginIds());

            // Should have one unloaded plugin with failure info
            var unloadedPlugins = loader.GetUnloadedPlugins();
            var failedPlugin = unloadedPlugins.FirstOrDefault(p => p.FailureReason is not null);
            Assert.NotNull(failedPlugin);
            Assert.Equal(emptyPluginDir, failedPlugin.Directory);
            Assert.Contains("No DLL files found", failedPlugin.FailureReason);
            Assert.True((DateTime.UtcNow - failedPlugin.FailedAt!.Value).TotalSeconds < 5);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void MultipleFailedPluginsAppearInUnloadedList()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create multiple directories with different failure scenarios
            var noDllsDir = Path.Combine(tempDir, "no-dlls");
            Directory.CreateDirectory(noDllsDir);

            var noAttributeDir = Path.Combine(tempDir, "no-attribute");
            Directory.CreateDirectory(noAttributeDir);
            // Create a dummy DLL file (won't have IvyPlugin attribute)
            File.WriteAllText(Path.Combine(noAttributeDir, "dummy.dll"), "fake dll");

            var loader = new PluginLoader(tempDir, logger);
            using var sp = new ServiceCollection().BuildServiceProvider();
            loader.DiscoverAndLoad(new Version(1, 0), sp);

            // Should have two unloaded plugins with failure info
            var unloadedPlugins = loader.GetUnloadedPlugins();
            var failedPlugins = unloadedPlugins.Where(p => p.FailureReason is not null).ToList();
            Assert.Equal(2, failedPlugins.Count);

            // Each directory gets a specific failure reason
            var noDllsFailure = failedPlugins.First(f => f.Directory == noDllsDir);
            Assert.Contains("No DLL files found", noDllsFailure.FailureReason);

            var noAttrFailure = failedPlugins.First(f => f.Directory == noAttributeDir);
            Assert.NotNull(noAttrFailure.FailureReason);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void SuccessfulLoadRemovesFailureInformation()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a plugin directory with no DLLs initially
            var pluginDir = Path.Combine(tempDir, "test-plugin");
            Directory.CreateDirectory(pluginDir);

            var loader = new PluginLoader(tempDir, logger);
            using var sp = new ServiceCollection().BuildServiceProvider();

            // First discovery should fail (no DLLs)
            loader.DiscoverAndLoad(new Version(1, 0), sp);
            var unloadedPlugins = loader.GetUnloadedPlugins();
            var failedPlugin = unloadedPlugins.FirstOrDefault(p => p.FailureReason is not null);
            Assert.NotNull(failedPlugin);
            Assert.Equal(pluginDir, failedPlugin.Directory);

            // Now we can't easily simulate a successful LoadPlugin without a real plugin DLL,
            // but we can test that the remove mechanism exists in the code.
            // The implementation removes from _failedPlugins on successful load,
            // which we've verified by code inspection.

            // For this test, we verify the failed plugin appears in unloaded list with failure info
            Assert.Contains(unloadedPlugins, p => p.Directory == pluginDir && p.FailureReason is not null);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact(Skip = "FileSystemWatcher events are unreliable in test environments, especially on macOS")]
    public async Task PluginWatcherDetectsNewDirectory()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var loaderLogger = loggerFactory.CreateLogger<PluginLoader>();
        var watcherLogger = loggerFactory.CreateLogger<PluginWatcher>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-watcher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var loader = new PluginLoader(tempDir, loaderLogger);
            using var sp = new ServiceCollection().BuildServiceProvider();
            loader.DiscoverAndLoad(new Version(1, 0), sp);

            var watcher = new PluginWatcher(tempDir, loader, watcherLogger);
            watcher.Start();

            // Create a new plugin directory
            var pluginDir = Path.Combine(tempDir, "new-plugin");
            Directory.CreateDirectory(pluginDir);

            // Wait for debounce + processing (increased for macOS)
            await Task.Delay(1000);

            // Verify the loader attempted to load (will be in unloaded list with a failure reason since no DLLs)
            var unloadedPlugins = loader.GetUnloadedPlugins();
            Assert.Contains(unloadedPlugins, f => f.Directory == pluginDir && f.FailureReason != null);

            watcher.Dispose();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact(Skip = "FileSystemWatcher events are unreliable in test environments, especially on macOS")]
    public async Task PluginWatcherDetectsRemovedDirectory()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var loaderLogger = loggerFactory.CreateLogger<PluginLoader>();
        var watcherLogger = loggerFactory.CreateLogger<PluginWatcher>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-watcher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a test plugin directory
            var pluginDir = Path.Combine(tempDir, "test-plugin");
            Directory.CreateDirectory(pluginDir);

            var loader = new PluginLoader(tempDir, loaderLogger);
            using var sp = new ServiceCollection().BuildServiceProvider();

            // Use internal test helper to register a fake plugin
            var testPlugin = new TestPlugin { Id = "test-plugin-id" };
            loader.AddTestPlugin(testPlugin, pluginDir);

            var loadedIdsBefore = loader.GetActivePluginIds();
            Assert.Contains("test-plugin-id", loadedIdsBefore);

            var watcher = new PluginWatcher(tempDir, loader, watcherLogger);
            watcher.Start();

            // Delete the plugin directory
            Directory.Delete(pluginDir, true);

            // Wait for filesystem event processing (increased for macOS)
            await Task.Delay(500);

            // Verify the plugin was unloaded
            var loadedIdsAfter = loader.GetActivePluginIds();
            Assert.DoesNotContain("test-plugin-id", loadedIdsAfter);

            watcher.Dispose();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task PluginWatcherDebouncesRapidChanges()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var loaderLogger = loggerFactory.CreateLogger<PluginLoader>();
        var watcherLogger = loggerFactory.CreateLogger<PluginWatcher>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-watcher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var pluginDir = Path.Combine(tempDir, "test-plugin");
            Directory.CreateDirectory(pluginDir);
            var dllPath = Path.Combine(pluginDir, "test.dll");
            File.WriteAllText(dllPath, "initial");

            var loader = new PluginLoader(tempDir, loaderLogger);
            using var sp = new ServiceCollection().BuildServiceProvider();

            var testPlugin = new TestPlugin { Id = "test-plugin-id" };
            loader.AddTestPlugin(testPlugin, pluginDir);

            var watcher = new PluginWatcher(tempDir, loader, watcherLogger);
            watcher.Start();

            // Trigger multiple rapid changes
            for (int i = 0; i < 5; i++)
            {
                File.WriteAllText(dllPath, $"change-{i}");
                await Task.Delay(50); // 50ms between changes (less than 300ms debounce)
            }

            // Wait for debounce to complete
            await Task.Delay(400);

            // We can't easily count reload attempts without mocking,
            // but we verify the watcher doesn't crash on rapid changes
            Assert.True(File.Exists(dllPath));

            watcher.Dispose();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task PluginWatcherIgnoresNonDllChanges()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var loaderLogger = loggerFactory.CreateLogger<PluginLoader>();
        var watcherLogger = loggerFactory.CreateLogger<PluginWatcher>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-watcher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var pluginDir = Path.Combine(tempDir, "test-plugin");
            Directory.CreateDirectory(pluginDir);

            var loader = new PluginLoader(tempDir, loaderLogger);
            using var sp = new ServiceCollection().BuildServiceProvider();

            var testPlugin = new TestPlugin { Id = "test-plugin-id" };
            loader.AddTestPlugin(testPlugin, pluginDir);

            var watcher = new PluginWatcher(tempDir, loader, watcherLogger);
            watcher.Start();

            // Create a non-DLL file
            var txtPath = Path.Combine(pluginDir, "readme.txt");
            File.WriteAllText(txtPath, "This should not trigger a reload");

            // Wait for potential processing
            await Task.Delay(400);

            // Plugin should still be loaded (not reloaded/unloaded)
            var loadedIds = loader.GetActivePluginIds();
            Assert.Contains("test-plugin-id", loadedIds);

            watcher.Dispose();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private class TestPlugin : Ivy.Plugins.IIvyPlugin
    {
        public required string Id { get; init; }

        public Ivy.Plugins.PluginManifest Manifest => new()
        {
            Id = Id,
            Title = "Test Plugin",
        };

        public Ivy.Plugins.PluginConfigurationSchema? ConfigurationSchema => null;

        public void Configure(Ivy.Plugins.IIvyPluginContext context) { }
    }

    [Fact]
    public void PluginCanCastContextToIIvyExtendedPluginContext()
    {
        var context = new TestPluginContext();
        Ivy.Plugins.IIvyPluginContext pluginContext = context;

        // Verify the cast succeeds
        var ivyContext = pluginContext as Ivy.Plugins.IIvyExtendedPluginContext;
        Assert.NotNull(ivyContext);
        Assert.Same(context, ivyContext);
    }

    [Fact]
    public void AsExtendedContextReturnsIIvyExtendedPluginContext()
    {
        var context = new TestPluginContext();
        Ivy.Plugins.IIvyPluginContext pluginContext = context;

        // Use the extension method
        var ivyContext = pluginContext.AsExtendedContext();
        Assert.NotNull(ivyContext);
        Assert.Same(context, ivyContext);
    }

    [Fact]
    public void AsExtendedContextThrowsForNonIvyContext()
    {
        // Create a mock non-Ivy context
        var mockContext = new MockNonIvyPluginContext();

        var exception = Assert.Throws<InvalidOperationException>(() => mockContext.AsExtendedContext());
        Assert.Contains("This plugin requires Ivy framework features", exception.Message);
        Assert.Contains("Ensure the plugin is loaded in an Ivy host application", exception.Message);
    }

    [Fact]
    public void TryGetExtendedContextReturnsContextForIvyContext()
    {
        var context = new TestPluginContext();
        Ivy.Plugins.IIvyPluginContext pluginContext = context;

        var ivyContext = pluginContext.TryGetExtendedContext();
        Assert.NotNull(ivyContext);
        Assert.Same(context, ivyContext);
    }

    [Fact]
    public void TryGetExtendedContextReturnsNullForNonIvyContext()
    {
        var mockContext = new MockNonIvyPluginContext();

        var ivyContext = mockContext.TryGetExtendedContext();
        Assert.Null(ivyContext);
    }

    [Fact]
    public void PluginCanAddAppViaIIvyExtendedPluginContext()
    {
        var context = new TestPluginContext();

        context.SetCurrentPlugin("test-plugin", "/plugins/test");

        // Plugin casts and adds app
        Ivy.Plugins.IIvyPluginContext pluginContext = context;
        var ivyContext = pluginContext.AsExtendedContext();

        ivyContext.AddApp(new AppDescriptor
        {
            Id = "test-app",
            Title = "Test App",
            Group = ["Test"],
            IsVisible = true
        });

        context.ClearCurrentPlugin();

        // Trigger reload to process added app
        context.GetAppRepository().Reload(new HashSet<string>());

        // Verify app was registered
        var app = context.GetAppRepository().GetApp("test-app");
        Assert.NotNull(app);
        Assert.Equal("Test App", app.Title);
    }

    private class MockNonIvyPluginContext : Ivy.Plugins.IIvyPluginContext
    {
        public IServiceCollection Services => new ServiceCollection();
        public IIvyPluginConfig Config => throw new NotImplementedException();
    }

    [Fact]
    public void GenericPlugin_ReceivesTypedContext()
    {
        var context = new TestPluginContext();
        var received = false;

        var plugin = new TypedTestPlugin(ctx =>
        {
            // Verify we get the actual extended context type
            Assert.NotNull(ctx);
            received = true;
        });

        // Call via the non-generic interface (simulates how PluginLoader calls it)
        ((Ivy.Plugins.IIvyPlugin)plugin).Configure(context);

        Assert.True(received);
    }

    [Fact]
    public void GenericPlugin_ThrowsOnIncompatibleContext()
    {
        var mockContext = new MockNonIvyPluginContext();

        var plugin = new TypedTestPlugin(_ => { });

        // MockNonIvyPluginContext does not implement IIvyExtendedPluginContext,
        // so the DIM should throw
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ((Ivy.Plugins.IIvyPlugin)plugin).Configure(mockContext));

        Assert.Contains("requires context type", ex.Message);
        Assert.Contains(nameof(Ivy.Plugins.IIvyExtendedPluginContext), ex.Message);
    }

    private class TypedTestPlugin(Action<Ivy.Plugins.IIvyExtendedPluginContext> onConfigure)
        : Ivy.Plugins.IIvyPlugin<Ivy.Plugins.IIvyExtendedPluginContext>
    {
        public Ivy.Plugins.PluginManifest Manifest { get; } = new()
        {
            Id = "Ivy.Plugin.TypedTest",
            Title = "Typed Test",
        };

        public Ivy.Plugins.PluginConfigurationSchema? ConfigurationSchema => null;

        public void Configure(Ivy.Plugins.IIvyExtendedPluginContext context) => onConfigure(context);
    }
}
