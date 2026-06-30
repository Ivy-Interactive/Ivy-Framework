using Ivy.Plugins;

namespace Ivy.Core.Plugins;

internal class PluginStateService : IPluginStateService, IDisposable
{
    private readonly IPluginManager _pluginManager;

    public event Action? PluginStateChanged;

    public PluginStateService(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;

        _pluginManager.PluginLoaded += OnPluginChanged;
        _pluginManager.PluginLoadFailed += OnPluginChanged;
        _pluginManager.PluginUnloaded += OnPluginChanged;
        _pluginManager.PluginRemoved += OnPluginChanged;
        _pluginManager.PluginReloaded += OnPluginChanged;
        _pluginManager.PluginActivated += OnPluginChanged;
        _pluginManager.PluginDeactivated += OnPluginChanged;
    }

    private void OnPluginChanged(string pluginId) => PluginStateChanged?.Invoke();

    public IReadOnlyList<string> GetActivePluginIds() =>
        _pluginManager.GetActivePluginIds();

    public void Dispose()
    {
        _pluginManager.PluginLoaded -= OnPluginChanged;
        _pluginManager.PluginLoadFailed -= OnPluginChanged;
        _pluginManager.PluginUnloaded -= OnPluginChanged;
        _pluginManager.PluginRemoved -= OnPluginChanged;
        _pluginManager.PluginReloaded -= OnPluginChanged;
        _pluginManager.PluginActivated -= OnPluginChanged;
        _pluginManager.PluginDeactivated -= OnPluginChanged;
    }
}
