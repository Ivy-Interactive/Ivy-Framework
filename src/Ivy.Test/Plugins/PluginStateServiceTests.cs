using Ivy.Core.Plugins;
using Ivy.Plugins;

namespace Ivy.Test.Plugins;

public class PluginStateServiceTests
{
    [Fact]
    public void PluginStateChanged_FiresWhenPluginLoaded()
    {
        var fakeManager = new FakePluginManager();
        var service = new PluginStateService(fakeManager);

        var eventFired = false;
        service.PluginStateChanged += () => eventFired = true;

        fakeManager.RaisePluginLoaded("test-plugin");

        Assert.True(eventFired);
    }

    [Fact]
    public void PluginStateChanged_FiresWhenPluginUnloaded()
    {
        var fakeManager = new FakePluginManager();
        var service = new PluginStateService(fakeManager);

        var eventFired = false;
        service.PluginStateChanged += () => eventFired = true;

        fakeManager.RaisePluginUnloaded("test-plugin");

        Assert.True(eventFired);
    }

    [Fact]
    public void PluginStateChanged_FiresWhenPluginReloaded()
    {
        var fakeManager = new FakePluginManager();
        var service = new PluginStateService(fakeManager);

        var eventFired = false;
        service.PluginStateChanged += () => eventFired = true;

        fakeManager.RaisePluginReloaded("test-plugin");

        Assert.True(eventFired);
    }

    [Fact]
    public void PluginStateChanged_FiresWhenPluginActivated()
    {
        var fakeManager = new FakePluginManager();
        var service = new PluginStateService(fakeManager);

        var eventFired = false;
        service.PluginStateChanged += () => eventFired = true;

        fakeManager.RaisePluginActivated("test-plugin");

        Assert.True(eventFired);
    }

    [Fact]
    public void PluginStateChanged_FiresWhenPluginDeactivated()
    {
        var fakeManager = new FakePluginManager();
        var service = new PluginStateService(fakeManager);

        var eventFired = false;
        service.PluginStateChanged += () => eventFired = true;

        fakeManager.RaisePluginDeactivated("test-plugin");

        Assert.True(eventFired);
    }

    [Fact]
    public void GetActivePluginIds_ReturnsPluginManagerList()
    {
        var fakeManager = new FakePluginManager();
        fakeManager.ActivePluginIds = new List<string> { "plugin1", "plugin2" };

        var service = new PluginStateService(fakeManager);

        var result = service.GetActivePluginIds();

        Assert.Equal(2, result.Count);
        Assert.Contains("plugin1", result);
        Assert.Contains("plugin2", result);
    }

    private class FakePluginManager : IPluginManager
    {
        public List<string> ActivePluginIds { get; set; } = [];

        public event Action<string>? PluginLoaded;
        public event Action<string>? PluginLoadFailed;
        public event Action<string>? PluginUnloaded;
        public event Action<string>? PluginRemoved;
        public event Action<string>? PluginReloaded;
        public event Action<string>? PluginActivated;
        public event Action<string>? PluginDeactivated;

        public IReadOnlyList<string> GetActivePluginIds() => ActivePluginIds;

        public PluginManifest? GetPluginManifest(string pluginId) => null;

        public IReadOnlyList<UnconfiguredPlugin> GetUnconfiguredPlugins() => [];

        public IReadOnlyList<PluginCandidate> GetUnloadedPlugins() => [];

        public bool UnloadPlugin(string pluginId) => throw new NotImplementedException();

        public bool LoadPlugin(string pluginPath) => throw new NotImplementedException();

        public bool ReloadPlugin(string pluginId) => throw new NotImplementedException();

        public bool ReconfigurePlugin(string pluginId) => throw new NotImplementedException();

        public PluginConfigurationSchema? GetPluginSchema(string pluginId) => null;

        public object? BuildPluginConfigurationView(string pluginId, IIvyPluginConfig config) => null;

        public void RaisePluginLoaded(string pluginId) => PluginLoaded?.Invoke(pluginId);
        public void RaisePluginLoadFailed(string pluginId) => PluginLoadFailed?.Invoke(pluginId);
        public void RaisePluginUnloaded(string pluginId) => PluginUnloaded?.Invoke(pluginId);
        public void RaisePluginRemoved(string pluginId) => PluginRemoved?.Invoke(pluginId);
        public void RaisePluginReloaded(string pluginId) => PluginReloaded?.Invoke(pluginId);
        public void RaisePluginActivated(string pluginId) => PluginActivated?.Invoke(pluginId);
        public void RaisePluginDeactivated(string pluginId) => PluginDeactivated?.Invoke(pluginId);
    }
}
