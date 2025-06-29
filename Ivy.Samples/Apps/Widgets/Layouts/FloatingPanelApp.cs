using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Layouts;

[App(icon: Icons.Toilet)]
public class FloatingPanelApp : ViewBase
{
    public override object? Build()
    {
        return new Fragment()
               | Text.H1("Floating Panel")
               | (new FloatingPanel().Align(Align.TopRight)
                  | (Layout.Horizontal()
                     | new Button("Back")
                        .Icon(Icons.ArrowLeft)
                        .BorderRadius(BorderRadius.Full)
                        .Secondary()
                        .Large()
                     | new Button("Create Something Amazing")
                         .BorderRadius(BorderRadius.Full)
                         .Icon(Icons.ArrowRight, Align.Right)
                         .Large()
                  ));
    }
}