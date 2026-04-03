using Ivy;

namespace Ivy.Tests.Widgets;

public class StackedProgressTests
{
    [Fact]
    public void Constructor_WithSegments_SetsSegments()
    {
        var segments = new[]
        {
            new ProgressSegment(30, Colors.Red, "Failed"),
            new ProgressSegment(70, Colors.Green, "Passed")
        };

        var widget = new StackedProgress(segments);

        Assert.Equal(2, widget.Segments.Length);
        Assert.Equal(30, widget.Segments[0].Value);
        Assert.Equal(Colors.Red, widget.Segments[0].Color);
        Assert.Equal("Failed", widget.Segments[0].Label);
        Assert.Equal(70, widget.Segments[1].Value);
    }

    [Fact]
    public void Constructor_Default_HasEmptySegments()
    {
        var widget = new StackedProgress();

        Assert.Empty(widget.Segments);
        Assert.Equal(8, widget.BarHeight);
        Assert.False(widget.ShowLabels);
        Assert.True(widget.Rounded);
    }

    [Fact]
    public void BarHeight_Extension_SetsHeight()
    {
        var widget = new StackedProgress().BarHeight(16);

        Assert.Equal(16, widget.BarHeight);
    }

    [Fact]
    public void ShowLabels_Extension_SetsShowLabels()
    {
        var widget = new StackedProgress().ShowLabels();

        Assert.True(widget.ShowLabels);
    }

    [Fact]
    public void ShowLabels_Extension_WithFalse_ClearsShowLabels()
    {
        var widget = new StackedProgress().ShowLabels().ShowLabels(false);

        Assert.False(widget.ShowLabels);
    }

    [Fact]
    public void Rounded_Extension_SetsRounded()
    {
        var widget = new StackedProgress().Rounded(false);

        Assert.False(widget.Rounded);
    }

    [Fact]
    public void ProgressSegment_Equality()
    {
        var a = new ProgressSegment(50, Colors.Blue, "Test");
        var b = new ProgressSegment(50, Colors.Blue, "Test");

        Assert.Equal(a, b);
    }

    [Fact]
    public void ProgressSegment_WithExpression()
    {
        var original = new ProgressSegment(50, Colors.Blue, "Test");
        var modified = original with { Value = 75 };

        Assert.Equal(75, modified.Value);
        Assert.Equal(Colors.Blue, modified.Color);
        Assert.Equal("Test", modified.Label);
    }

    [Fact]
    public void ProgressSegment_DefaultOptionalParameters()
    {
        var segment = new ProgressSegment(42);

        Assert.Equal(42, segment.Value);
        Assert.Null(segment.Color);
        Assert.Null(segment.Label);
    }

    [Fact]
    public void WidthDefault_IsFull()
    {
        var widget = new StackedProgress();

        Assert.Equal(Size.Full(), widget.Width);
    }

    [Fact]
    public void PipeOperator_ThrowsNotSupported()
    {
        var widget = new StackedProgress();

        Assert.Throws<NotSupportedException>(() => widget | "child");
    }
}
