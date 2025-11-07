using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Hidden;

public record HiddenArgsAppArgs(string Name, int Value);

[App(icon: Icons.EyeOff, isVisible: false, searchHints: ["navigation", "arguments", "routing", "hidden", "parameters", "deeplink"])]
public class HiddenArgsApp : SampleBase
{
    protected override object? BuildSample()
    {
        HiddenArgsAppArgs? args = UseArgs<HiddenArgsAppArgs>();
        var navigator = this.UseNavigation();

        if (args != null)
        {
            return Layout.Vertical(
                Text.H2("Hidden App with Arguments"),
                Text.P($"Name: {args.Name}"),
                Text.P($"Value: {args.Value}"),
                new Button("Back to Links").HandleClick(() =>
                {
                    navigator.Navigate("app://concepts/links");
                })
            );
        }

        return Text.P("This hidden app should only be accessed with arguments.");
    }
}
