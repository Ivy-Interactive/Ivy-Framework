using Ivy;

namespace Ivy.Tests.Widgets.Primitives;

public class RichTextBlockTests
{
    [Fact]
    public void Rich_Run_AddsTextRun()
    {
        var builder = Text.Rich().Run("hello");
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Single(widget.Runs);
        Assert.Equal("hello", widget.Runs[0].Content);
    }

    [Fact]
    public void Rich_Bold_AddsBoldRun()
    {
        var builder = Text.Rich().Bold("bold text");
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Single(widget.Runs);
        Assert.True(widget.Runs[0].Bold);
        Assert.Equal("bold text", widget.Runs[0].Content);
    }

    [Fact]
    public void Rich_Italic_AddsItalicRun()
    {
        var builder = Text.Rich().Italic("italic text");
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Single(widget.Runs);
        Assert.True(widget.Runs[0].Italic);
        Assert.Equal("italic text", widget.Runs[0].Content);
    }

    [Fact]
    public void Rich_Word_SetsWordFlag()
    {
        var builder = Text.Rich().Word("hello").Word("world");
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Equal(2, widget.Runs.Length);
        Assert.True(widget.Runs[0].Word);
        Assert.True(widget.Runs[1].Word);
    }

    [Fact]
    public void Rich_Bold_WithWordFlag()
    {
        var builder = Text.Rich().Bold("spaced", word: true);
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Single(widget.Runs);
        Assert.True(widget.Runs[0].Bold);
        Assert.True(widget.Runs[0].Word);
    }

    [Fact]
    public void Rich_StylingDoesNotLeak()
    {
        var builder = Text.Rich().Bold("bold").Run("normal");
        var widget = (RichTextBlock)builder.Build()!;

        Assert.True(widget.Runs[0].Bold);
        Assert.False(widget.Runs[1].Bold);
    }

    [Fact]
    public void Rich_CombinedStyling()
    {
        var builder = Text.Rich().Bold("error", color: Colors.Red);
        var widget = (RichTextBlock)builder.Build()!;

        Assert.True(widget.Runs[0].Bold);
        Assert.Equal(Colors.Red, widget.Runs[0].Color);
    }

    [Fact]
    public void Rich_HighlightColor()
    {
        var builder = Text.Rich().Run("highlighted", highlightColor: Colors.Yellow);
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Equal(Colors.Yellow, widget.Runs[0].HighlightColor);
    }

    [Fact]
    public void Rich_Link()
    {
        var builder = Text.Rich().Link("click me", "https://example.com");
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Equal("https://example.com", widget.Runs[0].Link);
        Assert.True(widget.Runs[0].Word);
    }

    [Fact]
    public void Rich_Link_WithTarget()
    {
        var builder = Text.Rich().Link("click me", "https://example.com", linkTarget: LinkTarget.Blank);
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Equal("https://example.com", widget.Runs[0].Link);
        Assert.Equal(LinkTarget.Blank, widget.Runs[0].LinkTarget);
    }

    [Fact]
    public void Rich_BlockLevelStyling()
    {
        var builder = Text.Rich().Center().NoWrap().Run("text");
        var widget = (RichTextBlock)builder.Build()!;

        Assert.Equal(TextAlignment.Center, widget.TextAlignment);
        Assert.True(widget.NoWrap);
    }
}
