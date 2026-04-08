using Ivy.Tendril.Apps;

namespace Ivy.Tendril.Test;

public class JobOutputFormatterTests
{
    [Fact]
    public void AssistantTextMessages_RenderedAsPlainText()
    {
        var lines = new[]
        {
            """{"type":"assistant","message":{"content":[{"type":"text","text":"Let me read the file."}]}}"""
        };

        var result = JobsApp.FormatJobOutput(lines);

        Assert.Equal("Let me read the file.", result);
    }

    [Fact]
    public void ToolUse_RenderedAsBoldArrow()
    {
        var lines = new[]
        {
            """{"type":"assistant","message":{"content":[{"type":"tool_use","name":"Read","input":{"file_path":"/tmp/test"}}]}}"""
        };

        var result = JobsApp.FormatJobOutput(lines);

        Assert.Contains("**→ Read**", result);
    }

    [Fact]
    public void StderrLines_RenderedAsWarningBlockquote()
    {
        var lines = new[] { "[stderr] some warning message" };

        var result = JobsApp.FormatJobOutput(lines);

        Assert.Equal("> ⚠ some warning message", result);
    }

    [Fact]
    public void ResultSuccess_RenderedWithSeparator()
    {
        var lines = new[]
        {
            """{"type":"result","subtype":"success","result":"Task completed successfully.","cost_usd":0.04}"""
        };

        var result = JobsApp.FormatJobOutput(lines);

        Assert.Contains("---", result);
        Assert.Contains("Task completed successfully.", result);
    }

    [Fact]
    public void ResultError_RenderedAsErrorBlockquote()
    {
        var lines = new[]
        {
            """{"type":"result","subtype":"error","error":"Something went wrong"}"""
        };

        var result = JobsApp.FormatJobOutput(lines);

        Assert.Equal("> **Error:** Something went wrong", result);
    }

    [Fact]
    public void InitLine_ShowsSessionIdOnce()
    {
        var lines = new[]
        {
            """{"type":"system","subtype":"init","session_id":"abc-123","cwd":"/home/test"}""",
            """{"type":"system","subtype":"init","session_id":"abc-123","cwd":"/home/test"}"""
        };

        var result = JobsApp.FormatJobOutput(lines);

        // Should only appear once
        var count = result.Split("> Session:").Length - 1;
        Assert.Equal(1, count);
        Assert.Contains("`abc-123`", result);
    }

    [Fact]
    public void MalformedJsonLines_SkippedSilently()
    {
        var lines = new[]
        {
            "this is not json",
            "{broken json",
            """{"type":"assistant","message":{"content":[{"type":"text","text":"Hello"}]}}"""
        };

        var result = JobsApp.FormatJobOutput(lines);

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void EmptyLines_ProducesNoReadableOutput()
    {
        var lines = new[] { "", "  ", "\t" };

        var result = JobsApp.FormatJobOutput(lines);

        Assert.Equal("No readable output.", result);
    }

    [Fact]
    public void MultipleContentBlocks_AllRendered()
    {
        var lines = new[]
        {
            """{"type":"assistant","message":{"content":[{"type":"text","text":"Reading file..."},{"type":"tool_use","name":"Read","input":{}}]}}"""
        };

        var result = JobsApp.FormatJobOutput(lines);

        Assert.Contains("Reading file...", result);
        Assert.Contains("**→ Read**", result);
    }
}
