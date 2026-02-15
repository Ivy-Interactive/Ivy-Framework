using Ivy.Core;
using Ivy.Views;

namespace Ivy.Apps;

[App]
public class NoAppsRegisteredApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Center()
               | (Layout.Vertical()
                   .Gap(4)
                   .Center()
                   | Text.H1("Ouch! :|").Bold()
                   | Text.Muted("No serviceable apps are registered on this server. Add at least one app to get started.")
               );
    }
}
