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
    public void GetLoadedPluginIds_ReturnsPluginManagerList()
    {
        var fakeManager = new FakePluginManager();
        fakeManager.LoadedPluginIds = new List<string> { "plugin1", "plugin2" };

        var service = new PluginStateService(fakeManager);

        var result = service.GetLoadedPluginIds();

        Assert.Equal(2, result.Count);
        Assert.Contains("plugin1", result);
        Assert.Contains("plugin2", result);
    }

    private class FakePluginManager : IPluginManager
    {
        public List<string> LoadedPluginIds { get; set; } = [];

        public event Action<string>? PluginLoaded;
        public event Action<string>? PluginUnloaded;
        public event Action<string>? PluginReloaded;

        public IReadOnlyList<string> GetLoadedPluginIds() => LoadedPluginIds;

        public IReadOnlyList<PluginCandidate> GetUnloadedPlugins() => [];

        public bool UnloadPlugin(string pluginId) => throw new NotImplementedException();

        public bool LoadPlugin(string pluginPath) => throw new NotImplementedException();

        public bool ReloadPlugin(string pluginId) => throw new NotImplementedException();

        public void RaisePluginLoaded(string pluginId) => PluginLoaded?.Invoke(pluginId);
        public void RaisePluginUnloaded(string pluginId) => PluginUnloaded?.Invoke(pluginId);
        public void RaisePluginReloaded(string pluginId) => PluginReloaded?.Invoke(pluginId);
    }
}
