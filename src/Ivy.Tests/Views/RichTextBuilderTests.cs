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
}
