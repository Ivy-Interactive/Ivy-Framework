using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Samples.Shared.Apps.Concepts;

public record LinksAppArgs(string Foo, int Bar);

public record TestBeacon(string id, string Name);

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

        var navigationBeaconButton = new object();
        if (navigator.HasNavigationBeaconFor<TestBeacon>())
        {
            var testBeacon = new TestBeacon("2", "");
            navigationBeaconButton = new Button("Go to Navigation Beacon App").HandleClick(() =>
            {
                navigator.NavigateToBeacon(testBeacon);
            });
        }

        return Layout.Vertical()
            | new Button("Go to Hidden App").HandleClick(() =>
                    {
                        navigator.Navigate("app://hidden/hidden-args-app", new Hidden.HiddenArgsAppArgs("Niels", 123));
                    })
            | navigationBeaconButton;

    }
}

