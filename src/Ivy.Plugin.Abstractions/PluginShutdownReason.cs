namespace Ivy.Plugins;

public enum PluginShutdownReason
{
    Unload,
    Reload,
    Reconfigure,
    HostExit
}
