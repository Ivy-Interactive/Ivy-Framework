using Ivy.IvyML.Studio.Apps.Views;

namespace Ivy.IvyML.Studio.Apps;

[App(id: "studio", title: "IvyML Studio")]
public class StudioApp : ViewBase
{
    public override object? Build()
    {
        // Horizontal split: A (30%) | right (70%).
        // The right panel is split vertically into B (50%) / C (50%).
        //
        //  A | B
        //    | C
        return new ResizablePanelGroup(
            new ResizablePanel(Size.Fraction(0.40f).Min(0.15f).Max(0.6f),
                new ChatView()),
            new ResizablePanel(Size.Fraction(0.60f),
                new ResizablePanelGroup(
                    new ResizablePanel(Size.Fraction(0.50f),
                        new PreviewView()),
                    new ResizablePanel(Size.Fraction(0.50f),
                        new CodeView())
                ).Vertical())
        ).Horizontal().Height(Size.Screen());
    }
}
