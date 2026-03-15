
namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Download, searchHints: ["positioning", "layout", "center", "vertical", "horizontal", "justify"])]
public class AlignApp : ViewBase
{
    public override object? Build()
    {
        var squareBox = new Box().Width(Size.Units(5)).Height(Size.Units(5));
        var tallBox = new Box().Width(Size.Units(5)).Height(Size.Units(7));
        var tallestBox = new Box().Width(Size.Units(5)).Height(Size.Units(10));
        var wideBox = new Box().Width(Size.Units(7)).Height(Size.Units(5));
        var widestBox = new Box().Width(Size.Units(10)).Height(Size.Units(5));

        var container = new Box().Width(Size.Units(32)).Height(Size.Units(32)).Background(Colors.Pink).Padding(0).ContentAlign(null);

        object AlignHorizontalTest(Align align) =>
            container.Content(
                Layout.Horizontal().Align(align) | squareBox | tallBox | tallestBox
            );

        object AlignVerticalTest(Align align) =>
            container.Content(
                Layout.Vertical().Align(align).Height(Size.Full()) | squareBox | wideBox | widestBox
            );

        var alignValues = (Align[])Enum.GetValues(typeof(Align));

        var header = new object[] { null!, Text.Monospaced("Layout.Vertical()"), Text.Monospaced("Layout.Horizontal()") };

        var values = alignValues.Select(e => new[]
        {
            Text.Monospaced("Align." + e),
            AlignVerticalTest(e),
            AlignHorizontalTest(e)
        }).SelectMany(e => e).ToArray();

        return Layout.Grid().Columns(3)
               | (object[])[.. header, .. values];
    }
}
