using Ivy.Core;
using Ivy.Views;

namespace Ivy.Apps;

[App]
public class NotFoundApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Center()
               | (Layout.Vertical()
                   .Gap(4)
                   .Center()
                   | Text.H1("Ouch! :|").Bold()
                   | Text.Muted("Apologies, the app you were looking for was not found.")
               );
    }
}
