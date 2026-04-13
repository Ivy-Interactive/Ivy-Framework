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
}
