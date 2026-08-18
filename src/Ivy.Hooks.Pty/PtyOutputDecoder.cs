using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Ivy.Hooks.Pty.Test")]

namespace Ivy.Hooks.Pty;

// Owns the stateful UTF-8 decoder used to turn raw PTY read chunks into text, plus the optional
// capped transcript backing PtyHandle.Output. A single instance lives for the lifetime of one
// UsePty hook invocation (held in a UseRef so it survives PtyHandle being rebuilt every render).
internal sealed class PtyOutputDecoder(bool captureOutput, int maxCaptureLength)
{
    // Stateful decoder: a multi-byte UTF-8 sequence that straddles two 4KB reads is only decoded
    // correctly if the trailing partial bytes of one Decode call are remembered for the next one.
    // Encoding.UTF8.GetString (stateless) would instead emit a U+FFFD replacement character here.
    private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();
    private readonly StringBuilder? _transcript = captureOutput ? new StringBuilder() : null;
    private readonly int _maxCaptureLength = Math.Max(1, maxCaptureLength);
    private readonly object _gate = new();
    private char[] _chars = [];

    // Called only from the single PTY reader task, so the decoder itself needs no lock.
    public string Decode(byte[] buffer, int count)
    {
        var max = Encoding.UTF8.GetMaxCharCount(count);
        if (_chars.Length < max) _chars = new char[max];
        var written = _decoder.GetChars(buffer, 0, count, _chars, 0);
        var text = new string(_chars, 0, written);
        if (text.Length > 0) Append(text);
        return text;
    }

    // Read from the render path (or a UseInterval poll) while Decode runs on the reader task.
    public string Text
    {
        get
        {
            lock (_gate) return _transcript?.ToString() ?? string.Empty;
        }
    }

    private void Append(string text)
    {
        if (_transcript == null) return;

        lock (_gate)
        {
            _transcript.Append(text);
            if (_transcript.Length > _maxCaptureLength)
            {
                // Drop an extra 1/8 of the cap so trimming is amortized instead of running a
                // full memmove on every 4 KB chunk once the cap is reached. Keeps the newest
                // output (a tail is what you want from a long-running interactive CLI).
                var excess = _transcript.Length - _maxCaptureLength + _maxCaptureLength / 8;
                _transcript.Remove(0, Math.Min(excess, _transcript.Length));
            }
        }
    }
}
