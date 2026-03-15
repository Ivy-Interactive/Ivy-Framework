
namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.PanelRight, path: ["Widgets"], searchHints: ["sidebar", "drawer", "panel", "slide-out", "modal", "overlay", "side"])]
public class SheetApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Horizontal().Gap(2)
            | new Button("Right (Default)").WithSheet(
                () => new SheetView(),
                title: "Right Sheet",
                description: "This sheet slides in from the right side.",
                width: Size.Rem(24),
                side: SheetSide.Right
            )
            | new Button("Left").WithSheet(
                () => new SheetView(),
                title: "Left Sheet",
                description: "This sheet slides in from the left side. Great for navigation panels.",
                width: Size.Rem(24),
                side: SheetSide.Left
            )
            | new Button("Top").WithSheet(
                () => new SheetView(),
                title: "Top Sheet",
                description: "This sheet slides in from the top.",
                width: Size.Rem(16),
                side: SheetSide.Top
            )
            | new Button("Bottom").WithSheet(
                () => new SheetView(),
                title: "Bottom Sheet",
                description: "This sheet slides in from the bottom. Common for mobile action menus.",
                width: Size.Rem(16),
                side: SheetSide.Bottom
            );
    }
}

public class SheetView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var feedbackState = UseState(0);

        return new FooterLayout(
            new Button("Save", onClick: _ => client.Toast("Sheet Saved")),
            feedbackState.ToFeedbackInput()
        );
    }
}
