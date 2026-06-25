namespace Ivy.Plugins;

public enum PluginShutdownReason
{
    Unload = 0,
    Reload = 1,
    Reconfigure = 2,
    HostExit = 3,
}
