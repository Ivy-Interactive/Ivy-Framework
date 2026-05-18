namespace Ivy.Core.Plugins;

public interface IPluginStateService
{
    IReadOnlyList<string> GetActivePluginIds();
    event Action? PluginStateChanged;
}
