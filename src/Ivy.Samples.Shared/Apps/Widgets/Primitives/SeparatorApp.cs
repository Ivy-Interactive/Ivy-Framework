namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Minus, group: ["Widgets", "Primitives"], searchHints: ["divider", "line", "horizontal", "vertical", "separator", "hr"])]
public class SeparatorApp : SampleBase
{
    protected override object? BuildSample()
    {
        return
            Layout.Vertical(
                Layout.Horizontal(
                    new Baton(icon: Icons.Plus, variant: BatonVariant.Outline),
                    new Baton(icon: Icons.Minus, variant: BatonVariant.Outline),
                    new Separator(orientation: Orientation.Vertical),
                    new Baton(icon: Icons.Save, variant: BatonVariant.Outline),
                    new Baton(icon: Icons.Trash, variant: BatonVariant.Outline)
                ),
                new Separator(),
                new Separator("Left Aligned").TextAlign(TextAlignment.Left),
                new Separator("Center Aligned").TextAlign(TextAlignment.Center),
                new Separator("Right Aligned").TextAlign(TextAlignment.Right)
            );
    }
}
