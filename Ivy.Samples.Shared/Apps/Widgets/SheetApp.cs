using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.PanelRight, path: ["Widgets"])]
public class SheetApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Button("Open Sheet").WithSheet(
            () => new SheetView(),
            title: "This is a sheet",
            description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            width: Size.Fraction(1 / 2f)
        );
    }
}

public class SheetView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        return new FooterLayout(
            new Button("Save", onClick: _ => client.Toast("Sheet Saved")),
            "This is the content"
        );
    }
}