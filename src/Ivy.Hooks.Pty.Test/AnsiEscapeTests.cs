namespace Ivy.Hooks.Pty.Test;

public class AnsiEscapeTests
{
    [Fact]
    public void Strip_CsiColorCodes()
    {
        var result = AnsiEscape.Strip("\x1b[31mred\x1b[0m");

        Assert.Equal("red", result);
    }

    [Fact]
    public void Strip_OscSequences_BelTerminated()
    {
        var result = AnsiEscape.Strip("\x1b]0;title\x07text");

        Assert.Equal("text", result);
    }

    [Fact]
    public void Strip_OscSequences_StTerminated()
    {
        var result = AnsiEscape.Strip("\x1b]0;title\x1b\\text");

        Assert.Equal("text", result);
    }

    [Fact]
    public void Strip_OscSequences_UnterminatedAtEndOfString()
    {
        var result = AnsiEscape.Strip("before\x1b]0;untermina");

        Assert.Equal("before", result);
    }

    [Fact]
    public void Strip_OtherEscapeSequences_TwoCharSequenceRemoved()
    {
        // ESI (ESC M) is a plain two-byte escape sequence with no CSI/OSC introducer.
        var result = AnsiEscape.Strip("a\x1bMb");

        Assert.Equal("ab", result);
    }

    [Fact]
    public void Strip_PreservesNewlinesTabsAndCarriageReturns()
    {
        const string input = "a\r\nb\tc";

        var result = AnsiEscape.Strip(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void Strip_RemovesRemainingControlChars()
    {
        // \x is a variable-width hex escape in C# and greedily consumes any hex-digit letters
        // (a-f/A-F) that immediately follow it, so e.g. "\x07b" would parse as the single
        // codepoint 0x07b rather than BEL followed by 'b'. \u (always exactly 4 hex digits) is
        // used here instead of \x to keep each control char and the following letter distinct.
        var result = AnsiEscape.Strip("a\u0007b\u0008c\u007fd");

        Assert.Equal("abcd", result);
    }

    [Fact]
    public void Strip_PlainText_Unchanged()
    {
        const string input = "just plain text, nothing special here.";

        Assert.Equal(input, AnsiEscape.Strip(input));
    }

    [Fact]
    public void Strip_EmptyAndNullInput_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, AnsiEscape.Strip(""));
        Assert.Equal(string.Empty, AnsiEscape.Strip(null));
    }

    [Fact]
    public void Strip_RealisticRedrawSequence_LeavesVisibleTextOnly()
    {
        // Cursor positioning + SGR (color) heavy sequence, similar to what a TUI redraw emits.
        var input = "\x1b[2J\x1b[H\x1b[1;32mStatus: \x1b[0mOK\x1b[10;1H\x1b[?25l";

        var result = AnsiEscape.Strip(input);

        Assert.Equal("Status: OK", result);
    }
}
