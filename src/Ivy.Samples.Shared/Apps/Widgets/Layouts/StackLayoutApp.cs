
namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.Grid3x3, searchHints: ["layout", "vertical", "horizontal", "stack", "arrangement", "flexbox"])]
public class StackLayoutApp : SampleBase
{
    protected override object? BuildSample()
    {
        var squareBox = new Box().Width(Size.Units(5)).Height(Size.Units(5));
        var tallBox = new Box().Width(Size.Units(5)).Height(Size.Units(7));
        var tallestBox = new Box().Width(Size.Units(5)).Height(Size.Units(10));
        var wideBox = new Box().Width(Size.Units(7)).Height(Size.Units(5));
        var widestBox = new Box().Width(Size.Units(10)).Height(Size.Units(5));

        var container = new Box().Width(Size.Units(32)).Height(Size.Units(32)).Background(Colors.Pink).Padding(0).ContentAlign(null);

        object AlignHorizontalTest(Align align) =>
            Layout.Vertical(
                Text.Muted(align.ToString()),
                container.Content(
                    Layout.Horizontal().Align(align) | squareBox | tallBox | tallestBox
                )
            ).Gap(0);

        object AlignVerticalTest(Align align) =>
            Layout.Vertical(
                Text.Muted(align.ToString()),
                container.Content(
                    Layout.Vertical().Align(align).Height(Size.Full()) | squareBox | wideBox | widestBox
                )
            ).Gap(0);

        var horizontalAlign =
            Layout.Grid().Columns(3).Width(Size.Fit()).Gap(4)
            | AlignHorizontalTest(Align.TopLeft)
            | AlignHorizontalTest(Align.TopCenter)
            | AlignHorizontalTest(Align.TopRight)
            | AlignHorizontalTest(Align.Left)
            | AlignHorizontalTest(Align.Center)
            | AlignHorizontalTest(Align.Right)
            | AlignHorizontalTest(Align.BottomLeft)
            | AlignHorizontalTest(Align.BottomCenter)
            | AlignHorizontalTest(Align.BottomRight);

        var verticalAlign =
            Layout.Grid().Columns(3).Width(Size.Fit()).Gap(4)
            | AlignVerticalTest(Align.TopLeft)
            | AlignVerticalTest(Align.TopCenter)
            | AlignVerticalTest(Align.TopRight)
            | AlignVerticalTest(Align.Left)
            | AlignVerticalTest(Align.Center)
            | AlignVerticalTest(Align.Right)
            | AlignVerticalTest(Align.BottomLeft)
            | AlignVerticalTest(Align.BottomCenter)
            | AlignVerticalTest(Align.BottomRight);

        return Layout.Vertical()
               | Text.H1("Stack Layout")

               | Callout.Info("The helper functions `Layout.Vertical()` and `Layout.Horizontal()` can be combined with the `|` operator to compose readable layouts.")

               | Text.H2("Vertical")
               | new DemoView(_ => Layout.Vertical() | squareBox | squareBox | squareBox)

               | Text.H2("Horizontal")
               | new DemoView(_ => Layout.Horizontal() | squareBox | squareBox | squareBox)

               | Text.H2("Gap")
               | new DemoView(_ => Layout.Horizontal().Gap(1) | squareBox | squareBox | squareBox)

               | Text.H2("Align")
               | Text.H3("Horizontal")
               | horizontalAlign
               | Text.H3("Vertical")
               | verticalAlign

            ;
    }
}
