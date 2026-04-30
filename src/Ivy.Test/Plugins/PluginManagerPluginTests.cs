using Ivy.Apps;

namespace Ivy.Test.Plugins;

public class PluginManagerAppTests
{
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
            server.UsePlugins(tempPluginsDir, enableHotReload: false);

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
