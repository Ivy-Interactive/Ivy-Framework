namespace Ivy.Core.Plugins;

public interface IPluginStateService
{
    IReadOnlyList<string> GetLoadedPluginIds();
    event Action? PluginStateChanged;
}
