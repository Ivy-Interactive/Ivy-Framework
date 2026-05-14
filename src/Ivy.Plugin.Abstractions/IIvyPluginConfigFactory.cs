namespace Ivy.Plugins;

public interface IIvyPluginConfigFactory
{
    IIvyPluginConfig Create(string pluginId);
}
