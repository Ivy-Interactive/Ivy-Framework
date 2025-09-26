using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Minus, path: ["Widgets", "Primitives"])]
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
                new Separator()
            );
    }
}