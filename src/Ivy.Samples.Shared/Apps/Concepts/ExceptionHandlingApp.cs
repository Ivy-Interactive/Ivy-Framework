
namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Bug, searchHints: ["errors", "exceptions", "debugging", "crashes", "failure", "handling"])]
public class ExceptionHandlingApp : SampleBase
{
    protected override object? BuildSample()
    {
        UseEffect(() => throw new Exception("This is an unhandled exception."));

        var button1 = new Baton("Click me to throw an exception")
        {
            OnClick = new(_ => throw new Exception("This is an unhandled exception from a Baton click."))
        };

        var button2 = new Baton("Click me to throw an exception (async)")
        {
            OnClick = new(async _ =>
            {
                await Task.Delay(1000);
                throw new Exception("This is an unhandled exception from a Baton click.");
            })
        };

        return Layout.Vertical()
            | button1
            | button2
            ;

    }
}
