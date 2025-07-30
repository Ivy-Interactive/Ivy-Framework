using Ivy.Shared;

namespace Ivy.Samples.Apps.Concepts;

[App(icon: Icons.Bug)]
public class ExceptionHandlingApp : ViewBase
{
    public override object? Build()
    {
        UseEffect(() => throw new Exception("This is an unhandled exception."));

        var button = new Button("Click me to throw an exception")
        {
            OnClick = _ => throw new Exception("This is an unhandled exception from a Button click.")
        };

        return button;
    }
}