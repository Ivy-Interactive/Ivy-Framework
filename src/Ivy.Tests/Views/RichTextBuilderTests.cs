using Ivy;

namespace Ivy.Tests.Views;

public class RichTextBuilderTests
{
    [Fact]
    public void Should_Build_LineBreak_Run()
    {
        var block = (RichTextBlock)Text.Rich().Run("before").LineBreak().Run("after").Build()!;
        Assert.Equal(3, block.Runs.Length);
        Assert.Equal("before", block.Runs[0].Content);
        Assert.True(block.Runs[1].LineBreak);
        Assert.Equal(string.Empty, block.Runs[1].Content);
        Assert.Equal("after", block.Runs[2].Content);
    }

    [Fact]
    public void Should_Build_Muted_Run()
    {
        var block = (RichTextBlock)Text.Rich().Muted("secondary").Build()!;
        Assert.Single(block.Runs);
        Assert.Equal("secondary", block.Runs[0].Content);
        Assert.Equal(Colors.Muted, block.Runs[0].Color);
        Assert.False(block.Runs[0].Bold);
    }

    [Fact]
    public void Should_Build_Muted_Bold_Run()
    {
        var block = (RichTextBlock)Text.Rich().Muted("secondary", bold: true).Build()!;
        Assert.Single(block.Runs);
        Assert.Equal(Colors.Muted, block.Runs[0].Color);
        Assert.True(block.Runs[0].Bold);
    }
}
