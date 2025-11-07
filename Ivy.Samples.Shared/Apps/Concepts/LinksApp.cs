using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Samples.Shared.Apps.Concepts;

public record LinksAppArgs(string Foo, int Bar);

[App(icon: Icons.PanelLeft, searchHints: ["navigation", "routing", "arguments", "parameters", "urls", "deeplink"])]
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

        return new Button("Go to Hidden App").HandleClick(() =>
        {
            navigator.Navigate("app://hidden/hidden-args", new Hidden.HiddenArgsAppArgs("Niels", 123));
        });

    }
}

