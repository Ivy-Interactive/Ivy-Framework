using Ivy;
using Ivy.Core;

namespace Ivy.Tests.Views;

public class LayoutTests
{
    [Fact]
    public void Vertical_Expands_Enumerable_Children()
    {
        var items = new object[] { "a", "b", "c" }.Select(x => (object?)x);

        var layout = Layout.Vertical(items, "d");
        var widget = (StackLayout)layout.Build()!;

        Assert.Equal(4, widget.Children.Length);
        Assert.Equal("a", widget.Children[0]);
        Assert.Equal("b", widget.Children[1]);
        Assert.Equal("c", widget.Children[2]);
        Assert.Equal("d", widget.Children[3]);
    }

    [Fact]
    public void Horizontal_Expands_Enumerable_Children()
    {
        var items = new object[] { "a", "b" }.Select(x => (object?)x);

        var layout = Layout.Horizontal(items, "c");
        var widget = (StackLayout)layout.Build()!;

        Assert.Equal(3, widget.Children.Length);
    }

    [Fact]
    public void Vertical_Does_Not_Expand_Strings()
    {
        var layout = Layout.Vertical("hello", "world");
        var widget = (StackLayout)layout.Build()!;

        Assert.Equal(2, widget.Children.Length);
        Assert.Equal("hello", widget.Children[0]);
        Assert.Equal("world", widget.Children[1]);
    }
}
