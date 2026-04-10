using Ivy.Core;

namespace Ivy.Tests.Widgets;

public class ResponsiveWidgetTests
{
    [Fact]
    public void WidgetBase_ResponsiveWidth_SerializedCorrectly()
    {
        var badge = new Badge("test")
            .Width(Size.Full().At(Breakpoint.Mobile).And(Breakpoint.Desktop, Size.Half()));
        badge.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(badge);
        var props = result["props"]!.AsObject();
        var rw = props["responsiveWidth"]!.AsObject();

        Assert.Equal("Full", rw["mobile"]!.GetValue<string>());
        Assert.Equal("Fraction:0.5", rw["desktop"]!.GetValue<string>());
    }

    [Fact]
    public void StackLayout_ResponsiveOrientation_SerializedCorrectly()
    {
        var layout = Layout.Horizontal()
            .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
                .And(Breakpoint.Desktop, Orientation.Horizontal));
        var widget = layout.Build()!;
        ((StackLayout)widget).Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize((StackLayout)widget);
        var props = result["props"]!.AsObject();
        var ro = props["responsiveOrientation"]!.AsObject();

        Assert.Equal("Vertical", ro["mobile"]!.GetValue<string>());
        Assert.Equal("Horizontal", ro["desktop"]!.GetValue<string>());
    }

    [Fact]
    public void GridLayout_ResponsiveColumns_SerializedCorrectly()
    {
        var grid = Layout.Grid()
            .Columns(1.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 3))
            .Gap(4);
        var widget = (GridLayout)grid.Build()!;
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();
        var rc = props["responsiveColumns"]!.AsObject();

        Assert.Equal(1, rc["mobile"]!.GetValue<int>());
        Assert.Equal(3, rc["desktop"]!.GetValue<int>());
    }
}
