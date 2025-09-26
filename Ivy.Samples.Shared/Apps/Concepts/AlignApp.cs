using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Download)]
public class AlignApp : ViewBase
{
    public override object? Build()
    {
        var squareBox = new Box().Width(5).Height(5);
        var tallBox = new Box().Width(5).Height(7);
        var tallestBox = new Box().Width(5).Height(10);
        var wideBox = new Box().Width(7).Height(5);
        var widestBox = new Box().Width(10).Height(5);

        var container = new Box().Width(32).Height(32).Color(Colors.Pink).Padding(0).ContentAlign(null);

        object AlignHorizontalTest(Align align) =>
            container.Content(
                Layout.Horizontal().Align(align) | squareBox | tallBox | tallestBox
            );

        object AlignVerticalTest(Align align) =>
            container.Content(
                Layout.Vertical().Align(align).Height(Size.Full()) | squareBox | wideBox | widestBox
            );

        var alignValues = (Align[])Enum.GetValues(typeof(Align));

        var header = new object[] { null!, Text.InlineCode("Layout.Vertical()"), Text.InlineCode("Layout.Horizontal()") };

        var values = alignValues.Select(e => new[]
        {
            Text.InlineCode("Align." + e),
            AlignVerticalTest(e),
            AlignHorizontalTest(e)
        }).SelectMany(e => e).ToArray();

        return Layout.Grid().Columns(3)
               | (object[])[.. header, .. values];
    }
}