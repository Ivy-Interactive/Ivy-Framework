using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

public record LinksAppArgs(string Foo, int Bar);

[App(icon: Icons.PanelLeft)]
public class LinksApp : SampleBase
{
    protected override object? BuildSample()
    {
        LinksAppArgs? args = UseArgs<LinksAppArgs>();
        var navigator = this.UseNavigation();

        if (args != null)
        {
            return args.ToDetails();
        }

        return new Button("Go").HandleClick(() =>
        {
            navigator.Navigate(typeof(LinksApp), new LinksAppArgs("Niels", 123));
        });

    }
}

