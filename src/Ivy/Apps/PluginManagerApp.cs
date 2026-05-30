using Ivy.Plugins;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Apps;

/// <summary>
/// Central plugin management UI for loading, unloading, and reloading plugins.
/// Automatically available when UsePlugins() is called.
/// </summary>
[App(icon: Icons.Plug, title: "Plugin Manager", isVisible: false)]
public class PluginManagerApp : ViewBase
{
    public override object? Build()
    {
        var pluginManager = this.UseService<IPluginManager>();
        var activePlugins = pluginManager.GetActivePluginIds();
        var unconfiguredPlugins = pluginManager.GetUnconfiguredPlugins();
        var unloadedPlugins = pluginManager.GetUnloadedPlugins();
        var pluginStatus = UseState("");
        UsePluginState();

        return Vertical().Gap(6).Padding(4)
            | H1("Plugin Manager")
            | new Badge($"{activePlugins.Count} active, {unconfiguredPlugins.Count} unconfigured, {unloadedPlugins.Count} unloaded", BadgeVariant.Info)
            | new Separator()
            | H2("Active Plugins")
            | (activePlugins.Count == 0
                ? Muted("No plugins currently active")
                : activePlugins.Select(id => (object)(Horizontal()
                    | new Badge(id, BadgeVariant.Secondary)
                    | new Button("Reload", onClick: _ =>
                    {
                        pluginStatus.Set(pluginManager.ReloadPlugin(id)
                            ? $"Reloaded '{id}'"
                            : $"Failed to reload '{id}'");
                        return ValueTask.CompletedTask;
                    }, variant: ButtonVariant.Outline, icon: Icons.RefreshCw)
                    | new Button("Unload", onClick: _ =>
                    {
                        pluginStatus.Set(pluginManager.UnloadPlugin(id)
                            ? $"Unloaded '{id}'"
                            : $"Failed to unload '{id}'");
                        return ValueTask.CompletedTask;
                    }, variant: ButtonVariant.Outline, icon: Icons.Power)
                )).ToArray())
            | new Separator()
            | H2("Unconfigured Plugins")
            | (unconfiguredPlugins.Count == 0
                ? Muted("No unconfigured plugins")
                : unconfiguredPlugins.Select(p => (object)(Vertical().Gap(2)
                    | (Horizontal()
                        | new Badge(p.Id, BadgeVariant.Warning)
                        | Muted(string.Join(", ", p.ValidationErrors)))
                    | new Button("Reconfigure", onClick: _ =>
                    {
                        pluginStatus.Set(pluginManager.ReconfigurePlugin(p.Id)
                            ? $"Activated '{p.Id}'"
                            : $"Configuration still invalid for '{p.Id}'");
                        return ValueTask.CompletedTask;
                    }, variant: ButtonVariant.Outline, icon: Icons.Settings)
                )).ToArray())
            | new Separator()
            | H2("Unloaded Plugins")
            | (unloadedPlugins.Count == 0
                ? Muted("No unloaded plugins found")
                : unloadedPlugins.Select(p => (object)(Horizontal()
                    | new Badge(p.Id, p.FailureReason is not null ? BadgeVariant.Destructive : BadgeVariant.Outline)
                    | (p.FailureReason is not null ? Muted(p.FailureReason) : Muted("unloaded"))
                    | new Button(p.FailureReason is not null ? "Retry" : "Load", onClick: _ =>
                    {
                        pluginStatus.Set(pluginManager.LoadPlugin(p.Directory)
                            ? $"Loaded '{p.Id}'"
                            : $"Failed to load '{p.Id}'");
                        return ValueTask.CompletedTask;
                    }, variant: ButtonVariant.Outline, icon: p.FailureReason is not null ? Icons.RefreshCw : Icons.Plus)
                )).ToArray())
            | (string.IsNullOrEmpty(pluginStatus.Value)
                ? null
                : pluginStatus.Value.StartsWith("Failed") || pluginStatus.Value.Contains("Error") || pluginStatus.Value.Contains("invalid")
                    ? new Badge(pluginStatus.Value, BadgeVariant.Destructive)
                    : new Badge(pluginStatus.Value, BadgeVariant.Success));
    }
}
