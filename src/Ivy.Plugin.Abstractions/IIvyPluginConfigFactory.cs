namespace Ivy.Plugins;

public interface IIvyPluginConfigFactory
{
    IIvyPluginConfig Create(string pluginId);
    void SetPluginManager(IPluginManager pluginManager) { }
}
