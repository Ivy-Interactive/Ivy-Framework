using System.Text.Json;

namespace Ivy.Tests.Shared;

public class ResponsiveTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Responsive_ImplicitConversion_FromSingleValue()
    {
        Responsive<Size> responsive = Size.Full();
        Assert.Equal(Size.Full(), responsive.Default);
        Assert.Null(responsive.Mobile);
        Assert.Null(responsive.Desktop);
    }

    [Fact]
    public void Responsive_At_CreatesBreakpointValue()
    {
        var responsive = Size.Full().At(Breakpoint.Mobile);
        Assert.Equal(Size.Full(), responsive.Mobile);
        Assert.Null(responsive.Default);
        Assert.Null(responsive.Desktop);
    }

    [Fact]
    public void Responsive_And_ChainsBreakpoints()
    {
        var responsive = Size.Full().At(Breakpoint.Mobile)
            .And(Breakpoint.Desktop, Size.Half());
        Assert.Equal(Size.Full(), responsive.Mobile);
        Assert.Equal(Size.Half(), responsive.Desktop);
        Assert.Null(responsive.Default);
    }

    [Fact]
    public void Responsive_JsonSerialization_SingleValue_WritesPlainString()
    {
        Responsive<Size> responsive = Size.Full();
        var json = JsonSerializer.Serialize(responsive, SerializerOptions);
        Assert.Equal("\"Full\"", json);
    }

    [Fact]
    public void Responsive_JsonSerialization_MultiBreakpoint_WritesObject()
    {
        var responsive = Size.Full().At(Breakpoint.Mobile)
            .And(Breakpoint.Desktop, Size.Half());
        var json = JsonSerializer.Serialize(responsive, SerializerOptions);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal("Full", root.GetProperty("mobile").GetString());
        Assert.Equal("Fraction:0.5", root.GetProperty("desktop").GetString());
        Assert.False(root.TryGetProperty("default", out _));
    }

    [Fact]
    public void HideOn_SetsVisibleFalseForBreakpoints()
    {
        var badge = new Badge("test").HideOn(Breakpoint.Mobile, Breakpoint.Tablet);
        var visible = badge.ResponsiveVisible!;

        Assert.True(visible.Default);
        Assert.False(visible.Mobile);
        Assert.False(visible.Tablet);
        Assert.Null(visible.Desktop);
    }

    [Fact]
    public void ShowOn_SetsVisibleTrueForBreakpoints()
    {
        var badge = new Badge("test").ShowOn(Breakpoint.Mobile);
        var visible = badge.ResponsiveVisible!;

        Assert.False(visible.Default);
        Assert.True(visible.Mobile);
        Assert.Null(visible.Desktop);
    }

    [Theory]
    [InlineData(Breakpoint.Mobile)]
    [InlineData(Breakpoint.Tablet)]
    [InlineData(Breakpoint.Desktop)]
    [InlineData(Breakpoint.Wide)]
    public void Int_At_SetsCorrectBreakpoint(Breakpoint bp)
    {
        var responsive = 42.At(bp);
        AssertBreakpoint(responsive, bp, 42);
    }

    [Theory]
    [InlineData(Breakpoint.Mobile)]
    [InlineData(Breakpoint.Tablet)]
    [InlineData(Breakpoint.Desktop)]
    [InlineData(Breakpoint.Wide)]
    public void Int_And_SetsCorrectBreakpoint(Breakpoint bp)
    {
        var responsive = 1.At(Breakpoint.Mobile).And(bp, 99);
        Assert.Equal(99, GetBreakpointValue(responsive, bp));
    }

    [Theory]
    [InlineData(Breakpoint.Mobile)]
    [InlineData(Breakpoint.Tablet)]
    [InlineData(Breakpoint.Desktop)]
    [InlineData(Breakpoint.Wide)]
    public void Orientation_At_SetsCorrectBreakpoint(Breakpoint bp)
    {
        var responsive = Orientation.Horizontal.At(bp);
        AssertBreakpoint(responsive, bp, Orientation.Horizontal);
    }

    [Theory]
    [InlineData(Breakpoint.Mobile)]
    [InlineData(Breakpoint.Tablet)]
    [InlineData(Breakpoint.Desktop)]
    [InlineData(Breakpoint.Wide)]
    public void Density_At_SetsCorrectBreakpoint(Breakpoint bp)
    {
        var responsive = Density.Small.At(bp);
        AssertBreakpoint(responsive, bp, Density.Small);
    }

    [Theory]
    [InlineData(Breakpoint.Mobile)]
    [InlineData(Breakpoint.Tablet)]
    [InlineData(Breakpoint.Desktop)]
    [InlineData(Breakpoint.Wide)]
    public void Bool_At_SetsCorrectBreakpoint(Breakpoint bp)
    {
        var responsive = true.At(bp);
        AssertBreakpoint(responsive, bp, true);
    }

    [Theory]
    [InlineData(Breakpoint.Mobile)]
    [InlineData(Breakpoint.Tablet)]
    [InlineData(Breakpoint.Desktop)]
    [InlineData(Breakpoint.Wide)]
    public void Thickness_At_SetsCorrectBreakpoint(Breakpoint bp)
    {
        var responsive = new Thickness(4).At(bp);
        AssertBreakpoint(responsive, bp, new Thickness(4));
    }

    [Fact]
    public void AllValueTypes_At_And_Chain()
    {
        var intR = 1.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 2);
        Assert.Equal(1, intR.Mobile);
        Assert.Equal(2, intR.Desktop);

        var orientR = Orientation.Horizontal.At(Breakpoint.Tablet).And(Breakpoint.Wide, Orientation.Vertical);
        Assert.Equal(Orientation.Horizontal, orientR.Tablet);
        Assert.Equal(Orientation.Vertical, orientR.Wide);

        var densityR = Density.Small.At(Breakpoint.Mobile).And(Breakpoint.Desktop, Density.Large);
        Assert.Equal(Density.Small, densityR.Mobile);
        Assert.Equal(Density.Large, densityR.Desktop);

        var boolR = true.At(Breakpoint.Mobile).And(Breakpoint.Desktop, false);
        Assert.True(boolR.Mobile);
        Assert.False(boolR.Desktop);

        var thicknessR = new Thickness(4).At(Breakpoint.Mobile).And(Breakpoint.Desktop, new Thickness(8));
        Assert.Equal(new Thickness(4), thicknessR.Mobile);
        Assert.Equal(new Thickness(8), thicknessR.Desktop);
    }

    private static void AssertBreakpoint<T>(Responsive<T> responsive, Breakpoint bp, T expected)
    {
        Assert.Equal(expected, GetBreakpointValue(responsive, bp));

        // All other breakpoints should be default
        foreach (var other in new[] { Breakpoint.Mobile, Breakpoint.Tablet, Breakpoint.Desktop, Breakpoint.Wide })
        {
            if (other != bp)
                Assert.Equal(default, GetBreakpointValue(responsive, other));
        }
    }

    private static T? GetBreakpointValue<T>(Responsive<T> responsive, Breakpoint bp) => bp switch
    {
        Breakpoint.Mobile => responsive.Mobile,
        Breakpoint.Tablet => responsive.Tablet,
        Breakpoint.Desktop => responsive.Desktop,
        Breakpoint.Wide => responsive.Wide,
        _ => throw new ArgumentOutOfRangeException(nameof(bp))
    };
}
