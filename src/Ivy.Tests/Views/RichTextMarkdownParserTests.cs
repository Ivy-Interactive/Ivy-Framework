using Ivy;

namespace Ivy.Tests.Views;

public class RichTextMarkdownParserTests
{
    private static List<TextRun> ParseAll(string input)
    {
        var parser = new RichTextMarkdownParser();
        var runs = new List<TextRun>();
        runs.AddRange(parser.Append(input));
        runs.AddRange(parser.Flush());
        return runs;
    }

    [Fact]
    public void Should_Parse_PlainText()
    {
        var runs = ParseAll("Hello world");
        Assert.Single(runs);
        Assert.Equal("Hello world", runs[0].Content);
    }

    [Fact]
    public void Should_Parse_Bold()
    {
        var runs = ParseAll("**bold**\n");
        var bold = runs.First(r => r.Bold);
        Assert.Equal("bold", bold.Content);
    }

    [Fact]
    public void Should_Parse_Italic()
    {
        var runs = ParseAll("*italic*\n");
        var italic = runs.First(r => r.Italic);
        Assert.Equal("italic", italic.Content);
    }

    [Fact]
    public void Should_Parse_InlineCode()
    {
        var runs = ParseAll("`code`\n");
        var code = runs.First(r => r.Code);
        Assert.Equal("code", code.Content);
    }

    [Fact]
    public void Should_Parse_Heading()
    {
        var runs = ParseAll("## Title\n");
        var heading = runs.First(r => r.Heading > 0);
        Assert.Equal(2, heading.Heading);
        Assert.Equal("Title", heading.Content);
    }

    [Fact]
    public void Should_Parse_BulletList()
    {
        var runs = ParseAll("- item1\n- item2\n");
        var bullets = runs.Where(r => r.BulletItem > 0).ToList();
        Assert.Equal(2, bullets.Count);
        Assert.Equal("item1", bullets[0].Content);
        Assert.Equal("item2", bullets[1].Content);
        Assert.Equal(1, bullets[0].BulletItem);
    }

    [Fact]
    public void Should_Parse_OrderedList()
    {
        var runs = ParseAll("1. first\n2. second\n");
        var ordered = runs.Where(r => r.OrderedItem > 0).ToList();
        Assert.Equal(2, ordered.Count);
        Assert.Equal(1, ordered[0].OrderedItem);
        Assert.Equal(2, ordered[1].OrderedItem);
        Assert.Equal("first", ordered[0].Content);
        Assert.Equal("second", ordered[1].Content);
    }

    [Fact]
    public void Should_Parse_Link()
    {
        var runs = ParseAll("[text](url)\n");
        var link = runs.First(r => r.Link != null);
        Assert.Equal("text", link.Content);
        Assert.Equal("url", link.Link);
    }

    [Fact]
    public void Should_Parse_CodeBlock()
    {
        var runs = ParseAll("```csharp\nvar x = 1;\n```\n");
        var codeBlock = runs.First(r => r.CodeBlock != null);
        Assert.Equal("csharp", codeBlock.CodeBlock);
        Assert.Equal("var x = 1;\n", codeBlock.Content);
    }

    [Fact]
    public void Should_Parse_Table()
    {
        var runs = ParseAll("|a|b|\n|---|---|\n|1|2|\n\nnext");
        var table = runs.First(r => r.Table != null);
        Assert.Contains("|a|b|", table.Table);
        Assert.Contains("|1|2|", table.Table);
    }

    [Fact]
    public void Should_Parse_LineBreak()
    {
        var runs = ParseAll("line1\n\nline2");
        var lineBreaks = runs.Where(r => r.LineBreak).ToList();
        Assert.NotEmpty(lineBreaks);
    }

    [Fact]
    public void Should_Parse_Blockquote()
    {
        var runs = ParseAll("> quoted\n");
        var bq = runs.First(r => r.Blockquote);
        Assert.Equal("quoted", bq.Content);
    }

    [Fact]
    public void Should_Parse_HorizontalRule()
    {
        var runs = ParseAll("---\n");
        var hr = runs.First(r => r.HorizontalRule);
        Assert.True(hr.HorizontalRule);
    }

    [Fact]
    public void Should_Handle_SplitTokenBold()
    {
        var parser = new RichTextMarkdownParser();
        var runs = new List<TextRun>();
        runs.AddRange(parser.Append("**bo"));
        runs.AddRange(parser.Append("ld**\n"));
        runs.AddRange(parser.Flush());
        var bold = runs.First(r => r.Bold);
        Assert.Equal("bold", bold.Content);
    }

    [Fact]
    public void Should_Handle_SplitTokenCodeFence()
    {
        var parser = new RichTextMarkdownParser();
        var runs = new List<TextRun>();
        runs.AddRange(parser.Append("``"));
        runs.AddRange(parser.Append("`\ncode\n```\n"));
        runs.AddRange(parser.Flush());
        var codeBlock = runs.First(r => r.CodeBlock != null);
        Assert.Equal("code\n", codeBlock.Content);
    }

    [Fact]
    public void Should_Parse_Strikethrough()
    {
        var runs = ParseAll("~~struck~~\n");
        var struck = runs.First(r => r.StrikeThrough);
        Assert.Equal("struck", struck.Content);
    }

    [Fact]
    public void Should_Handle_Mixed_Inline()
    {
        var runs = ParseAll("plain **bold** and *italic*\n");
        var plain = runs.First(r => !r.Bold && !r.Italic && r.Content.Contains("plain"));
        var bold = runs.First(r => r.Bold);
        var italic = runs.First(r => r.Italic);
        Assert.Equal("bold", bold.Content);
        Assert.Equal("italic", italic.Content);
        Assert.Contains("plain", plain.Content);
    }

    [Fact]
    public void Should_Flush_Remaining_On_End()
    {
        var parser = new RichTextMarkdownParser();
        var runs = new List<TextRun>();
        runs.AddRange(parser.Append("partial"));
        runs.AddRange(parser.Flush());
        Assert.Contains(runs, r => r.Content == "partial");
    }
}
