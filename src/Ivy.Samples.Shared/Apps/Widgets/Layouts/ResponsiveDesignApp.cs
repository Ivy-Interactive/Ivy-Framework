namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.Monitor, searchHints: ["responsive", "breakpoint", "mobile", "tablet", "desktop"])]
public class ResponsiveDesignApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
            | Text.H1("Responsive Design")

            // 1. Responsive width
            | Text.H2("Responsive Width")
            | new Box(Text.P("Full on mobile, half on desktop"))
                .Width(Size.Full().At(Breakpoint.Mobile)
                    .And(Breakpoint.Desktop, Size.Half()))
                .Background(Colors.Primary)

            // 2. Visibility: hide/show by breakpoint
            | Text.H2("Conditional Visibility")
            | new Badge("Desktop only").HideOn(Breakpoint.Mobile, Breakpoint.Tablet)
            | new Badge("Mobile only").ShowOn(Breakpoint.Mobile)

            // 3. Responsive grid columns
            | Text.H2("Responsive Grid")
            | (Layout.Grid()
                .Columns(1.At(Breakpoint.Mobile)
                    .And(Breakpoint.Tablet, 2)
                    .And(Breakpoint.Desktop, 3))
                .Gap(4)
                | new Card("Card 1")
                | new Card("Card 2")
                | new Card("Card 3")
                | new Card("Card 4")
                | new Card("Card 5")
                | new Card("Card 6"))

            // 4. Responsive orientation
            | Text.H2("Responsive Orientation")
            | (Layout.Horizontal()
                .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
                    .And(Breakpoint.Desktop, Orientation.Horizontal))
                | new Button("Action 1")
                | new Button("Action 2")
                | new Button("Action 3"))

            // 5. Responsive gap
            | Text.H2("Responsive Gap")
            | (Layout.Vertical()
                .Gap(2.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
                | Text.P("Item 1")
                | Text.P("Item 2")
                | Text.P("Item 3"))

            // 6. Responsive density
            | Text.H2("Responsive Density")
            | new Button("Adaptive Button")
                .Density(Density.Large.At(Breakpoint.Mobile)
                    .And(Breakpoint.Desktop, Density.Medium));
    }
}
