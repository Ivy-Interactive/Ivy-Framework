
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Minus, path: ["Widgets", "Primitives"], searchHints: ["divider", "line", "horizontal", "vertical", "separator", "hr"])]
public class SeparatorApp : SampleBase
{
    protected override object? BuildSample()
    {
        return
            Layout.Vertical(
                Layout.Horizontal(
                    new Button(icon: Icons.Plus, variant: ButtonVariant.Outline),
                    new Button(icon: Icons.Minus, variant: ButtonVariant.Outline),
                    new Separator(orientation: Orientation.Vertical),
                    new Button(icon: Icons.Save, variant: ButtonVariant.Outline),
                    new Button(icon: Icons.Trash, variant: ButtonVariant.Outline)
                ),
                new Separator(),
                new Separator("Left Aligned").TextAlign(TextAlignment.Left),
                new Separator("Center Aligned").TextAlign(TextAlignment.Center),
                new Separator("Right Aligned").TextAlign(TextAlignment.Right)
            );
    }
}
