using Ivy.Samples.Shared.Helpers;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Expand, group: ["Widgets", "Primitives"], searchHints: ["spacer", "grow", "layout", "flex", "fill", "spacing"])]
public class SpacerApp : SampleBase
{
    protected override object? BuildSample()
    {
        var containerStyle = new Box().BorderThickness(1).BorderStyle(BorderStyle.Dashed).BorderRadius(BorderRadius.Rounded).Padding(2).ContentAlign(null);

        var flexGrowTab = Layout.Vertical().Gap(6)
               | Text.H2("Default Grow Behavior (Flex Grow)")
               | Text.P("Without any explicit sizing, a Spacer acts as a flexible element that expands to fill the remaining layout space. This is commonly used to align components to opposite sides.")
               | new DemoView(_ =>
                   Layout.Vertical().Gap(2)
                   | containerStyle.Content(
                       Layout.Horizontal()
                       | new Box("Left Component").Background(Colors.Primary).Padding(2).BorderRadius(BorderRadius.Rounded)
                       | new Spacer()
                       | new Box("Right Component").Background(Colors.Secondary).Padding(2).BorderRadius(BorderRadius.Rounded)
                   )
               )

               | Text.H2("Distributing Space with Multiple Spacers")
               | Text.P("Placing multiple Spacers distributes the remaining space equally among them.")
               | new DemoView(_ =>
                   containerStyle.Content(
                       Layout.Horizontal()
                       | new Box("Start").Background(Colors.Primary).Padding(2).BorderRadius(BorderRadius.Rounded)
                       | new Spacer()
                       | new Box("Center").Background(Colors.Secondary).Padding(2).BorderRadius(BorderRadius.Rounded)
                       | new Spacer()
                       | new Box("End").Background(Colors.Warning).Padding(2).BorderRadius(BorderRadius.Rounded)
                   )
               );

        var dimensionsTab = Layout.Vertical().Gap(6)
               | Text.H2("Explicit Dimensions (Width & Height)")
               | Text.P("Instead of growing to fill the space, you can set fixed or custom dimensions on a Spacer to control exact gaps in horizontal or vertical layouts.")

               | Text.H3("Fixed Width Spacer")
               | Text.P("Use .Width() to define a fixed horizontal gap between items.")
               | new DemoView(_ =>
                   Layout.Vertical().Gap(2)
                   | containerStyle.Content(
                       Layout.Horizontal()
                       | new Box("Box A").Background(Colors.Primary).Padding(2).BorderRadius(BorderRadius.Rounded)
                       | new Spacer().Width(Size.Units(12))
                       | new Box("Box B").Background(Colors.Secondary).Padding(2).BorderRadius(BorderRadius.Rounded)
                       | new Spacer().Width(Size.Units(4))
                       | new Box("Box C").Background(Colors.Warning).Padding(2).BorderRadius(BorderRadius.Rounded)
                   )
               )

               | Text.H3("Fixed Height Spacer")
               | Text.P("Use .Height() to define a vertical gap in a stack layout.")
               | new DemoView(_ =>
                   containerStyle.Content(
                       Layout.Vertical()
                       | new Box("Top Section").Background(Colors.Primary).Padding(2).BorderRadius(BorderRadius.Rounded)
                       | new Spacer().Height(Size.Units(6))
                       | new Box("Middle Section").Background(Colors.Secondary).Padding(2).BorderRadius(BorderRadius.Rounded)
                       | new Spacer().Height(Size.Units(3))
                       | new Box("Bottom Section").Background(Colors.Warning).Padding(2).BorderRadius(BorderRadius.Rounded)
                   )
               );

        var realWorldTab = Layout.Vertical().Gap(6)
               | Text.H2("Application Example: Navigation Bar")
               | Text.P("Spacers are highly useful in headers and navigation bars to separate branding, links, and user controls cleanly.")
               | new DemoView(_ =>
                   new Box(
                       Layout.Horizontal()
                       | Text.H3("🚀 AppName")
                       | new Spacer().Width(Size.Units(8))
                       | new Button("Dashboard", variant: ButtonVariant.Ghost)
                       | new Button("Settings", variant: ButtonVariant.Ghost)
                       | new Spacer()
                       | new Button("Log Out", variant: ButtonVariant.Outline)
                   )
                   .Background(Colors.Secondary)
                   .Padding(3)
                   .BorderRadius(BorderRadius.Rounded)
                   .Width(Size.Full())
                   .ContentAlign(null)
               );

        return Layout.Vertical().Gap(4)
               | Text.H1("Spacer Widget")
               | Text.P("The Spacer widget creates empty space between elements. By default, it grows to fill all available space in the parent layout's direction, pushing adjacent widgets to the outer edges.")
               | Layout.Tabs(
                   new Tab("Flex Grow", flexGrowTab).Icon(Icons.Expand),
                   new Tab("Dimensions", dimensionsTab).Icon(Icons.Ruler),
                   new Tab("Real-world Demos", realWorldTab).Icon(Icons.LayoutPanelTop)
               ).Variant(TabsVariant.Content);
    }
}
