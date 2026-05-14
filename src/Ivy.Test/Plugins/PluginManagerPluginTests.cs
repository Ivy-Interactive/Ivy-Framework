using Ivy.Apps;
using Ivy.Plugins;

namespace Ivy.Test.Plugins;

public class PluginManagerAppTests
{
    private class NullPluginConfigFactory : IIvyPluginConfigFactory
    {
        public IIvyPluginConfig Create(string pluginId) => new NullPluginConfig();
    }

    private class NullPluginConfig : IIvyPluginConfig
    {
        public string? GetValue(string key) => null;
        public void SetValue(string key, string value) { }
        public void RemoveValue(string key) { }
        public void Save() { }
    }

    [Fact]
    public void UsePlugins_RegistersPluginManagerApp()
    {
        // Arrange
        var tempPluginsDir = Path.Combine(Path.GetTempPath(), "ivy-test-plugins-" + Guid.NewGuid());
        Directory.CreateDirectory(tempPluginsDir);

        try
        {
            var args = new ServerArgs();
            var server = new Server(args);
            server.UsePlugins(tempPluginsDir, new NullPluginConfigFactory(), enableHotReload: false);

            // Act
            var app = server.AppRepository.GetApp("plugin-manager");

            // Assert
            Assert.NotNull(app);
            Assert.Equal("Plugin Manager", app.Title);
            Assert.Equal(typeof(PluginManagerApp), app.Type);
        }
        finally
        {
            if (Directory.Exists(tempPluginsDir))
                Directory.Delete(tempPluginsDir, true);
        }
    }
}
