using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views;

public class NotFoundView : ViewBase
{
    public override object? Build()
    {
        return Layout.Center()
               | (Layout.Vertical()
                   .Gap(4)
                   .Center()
                   | Text.H1("Ouch! :|").Bold()
                   | Text.H3("Sorry, this app does not exist.")
                   | Text.Muted("Apologies, the app you were looking for was not found")
               );
    }
}
