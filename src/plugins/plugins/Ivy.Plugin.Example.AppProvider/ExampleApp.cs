// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A simple example app component that displays a message.
/// This demonstrates the minimal structure needed for an app registered via plugin.
/// </summary>
public class ExampleApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H1("Example App")
               | Text.P("This app was added by the Ivy.Plugin.Example.AppProvider plugin.")
               | Text.Muted("Plugins can add apps to the host application by casting IPluginContext to IIvyPluginContext.");
    }
}
