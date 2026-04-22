using Ivy.Core;

namespace Ivy.Test.Widgets;

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
        var rw = props["width"]!.AsObject();

        Assert.Equal("Full", rw["mobile"]!.GetValue<string>());
        Assert.Equal("Fraction:0.5", rw["desktop"]!.GetValue<string>());
    }

    [Fact]
    public void WidgetBase_SimpleWidth_SerializedAsPlainValue()
    {
        var badge = new Badge("test")
            .Width(Size.Px(100));
        badge.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(badge);
        var props = result["props"]!.AsObject();

        Assert.Equal("Px:100", props["width"]!.GetValue<string>());
    }

    [Fact]
    public void WidgetBase_ResponsiveHeight_SerializedCorrectly()
    {
        var badge = new Badge("test")
            .Height(Size.Full().At(Breakpoint.Mobile));
        badge.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(badge);
        var props = result["props"]!.AsObject();
        var rh = props["height"]!.AsObject();

        Assert.Equal("Full", rh["mobile"]!.GetValue<string>());
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
        var ro = props["orientation"]!.AsObject();

        Assert.Equal("Vertical", ro["mobile"]!.GetValue<string>());
        Assert.Equal("Horizontal", ro["desktop"]!.GetValue<string>());
    }

    [Fact]
    public void StackLayout_Orientation_ImplicitConversion_SerializesAsSimpleValue()
    {
        var layout = Layout.Horizontal();
        var widget = (StackLayout)layout.Build()!;
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        Assert.Equal("Horizontal", props["orientation"]!.GetValue<string>());
    }

    [Fact]
    public void StackLayout_RowGap_ImplicitConversion_SerializesAsSimpleValue()
    {
        var layout = Layout.Horizontal().Gap(8);
        var widget = (StackLayout)layout.Build()!;
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        Assert.Equal(8, props["rowGap"]!.GetValue<int>());
        Assert.Equal(8, props["columnGap"]!.GetValue<int>());
    }

    [Fact]
    public void StackLayout_Padding_ImplicitConversion_SerializesAsSimpleValue()
    {
        var layout = Layout.Horizontal().Padding(4);
        var widget = (StackLayout)layout.Build()!;
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        Assert.NotNull(props["padding"]);
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
        var rc = props["columns"]!.AsObject();

        Assert.Equal(1, rc["mobile"]!.GetValue<int>());
        Assert.Equal(3, rc["desktop"]!.GetValue<int>());
    }

    [Fact]
    public void GridLayout_SimpleColumns_SerializedAsPlainValue()
    {
        var grid = Layout.Grid()
            .Columns(3);
        var widget = (GridLayout)grid.Build()!;
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        Assert.Equal(3, props["columns"]!.GetValue<int>());
    }

    [Fact]
    public void GridLayout_ResponsiveGap_SerializedCorrectly()
    {
        var grid = Layout.Grid();
        var def = new GridDefinition
        {
            RowGap = 2.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 8),
            ColumnGap = 4.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 12)
        };
        var widget = new GridLayout(def);
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        var rg = props["rowGap"]!.AsObject();
        Assert.Equal(2, rg["mobile"]!.GetValue<int>());
        Assert.Equal(8, rg["desktop"]!.GetValue<int>());

        var cg = props["columnGap"]!.AsObject();
        Assert.Equal(4, cg["mobile"]!.GetValue<int>());
        Assert.Equal(12, cg["desktop"]!.GetValue<int>());
    }

    [Fact]
    public void GridLayout_SimpleGap_SerializedAsPlainValue()
    {
        var grid = Layout.Grid()
            .Gap(8);
        var widget = (GridLayout)grid.Build()!;
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        Assert.Equal(8, props["rowGap"]!.GetValue<int>());
        Assert.Equal(8, props["columnGap"]!.GetValue<int>());
    }
}
