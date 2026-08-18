using System.Text.RegularExpressions;

namespace Ivy.Hooks.Pty;

/// <summary>
/// Strips ANSI escape sequences and other terminal control codes from raw PTY output, so text
/// captured via <see cref="PtyOptions.OnOutput"/> or <see cref="PtyHandle.Output"/> can be read,
/// logged, or matched against as plain text.
/// </summary>
/// <remarks>
/// A C# port of the frontend's stripper in
/// <c>src/widgets/Ivy.Widgets.Xterm/frontend/src/Terminal.tsx</c> (the <c>markStreamDataIfVisible</c>
/// regex chain). Applied in the same order: OSC first, since OSC bodies can themselves contain
/// <c>[</c>, which would otherwise confuse the CSI pattern.
/// </remarks>
public static class AnsiEscape
{
    // OSC (Operating System Command), e.g. window title: ESC ] ... (BEL | ESC \) terminated.
    // Falls back to end-of-string so an OSC sequence truncated mid-chunk doesn't swallow
    // everything that follows it in a later Decode call.
    private static readonly Regex Osc = new(@"\x1b\][\s\S]*?(\x07|\x1b\\|$)", RegexOptions.Compiled);

    // CSI (Control Sequence Introducer), e.g. colors, cursor movement: ESC [ params intermediates final.
    private static readonly Regex Csi = new(@"\x1b\[[0-9;?]*[ -/]*[@-~]", RegexOptions.Compiled);

    // Any other two-character ESC sequence (e.g. character-set selection).
    private static readonly Regex Esc = new(@"\x1b[\s\S]", RegexOptions.Compiled);

    // Remaining control characters, deliberately keeping \t (0x09), \n (0x0A) and \r (0x0D) so the
    // result stays readable multi-line text. The frontend version strips those too, because it only
    // needs to answer "is anything visible here" for its loading-overlay heuristic.
    private static readonly Regex Control = new(@"[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]", RegexOptions.Compiled);

    /// <summary>
    /// Removes ANSI/VT escape sequences and non-whitespace control characters from <paramref name="text"/>.
    /// Newlines, carriage returns and tabs are preserved. Returns <see cref="string.Empty"/> for
    /// <c>null</c> or empty input.
    /// </summary>
    public static string Strip(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var result = Osc.Replace(text, "");
        result = Csi.Replace(result, "");
        result = Esc.Replace(result, "");
        result = Control.Replace(result, "");
        return result;
    }
}
