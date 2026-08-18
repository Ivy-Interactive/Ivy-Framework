using System.Text;

namespace Ivy.Hooks.Pty.Test;

public class PtyOutputDecoderTests
{
    private const string MixedText = "héllo ✅ 🎉 ┌─┐";

    public static IEnumerable<object[]> SplitIndexes()
    {
        var byteCount = Encoding.UTF8.GetByteCount(MixedText);
        for (var i = 0; i <= byteCount; i++)
        {
            yield return [i];
        }
    }

    [Theory]
    [MemberData(nameof(SplitIndexes))]
    public void Decode_MultiByteSplitAcrossChunks_RoundTrips(int splitIndex)
    {
        var bytes = Encoding.UTF8.GetBytes(MixedText);
        var decoder = new PtyOutputDecoder(captureOutput: false, maxCaptureLength: 1_000_000);

        var first = decoder.Decode(bytes[..splitIndex], splitIndex);
        var second = decoder.Decode(bytes[splitIndex..], bytes.Length - splitIndex);

        var result = first + second;
        Assert.Equal(MixedText, result);
        Assert.DoesNotContain('�', result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Decode_FourByteEmoji_SplitAtEveryBoundary(int splitIndex)
    {
        const string emoji = "🎉"; // 4 UTF-8 bytes
        var bytes = Encoding.UTF8.GetBytes(emoji);
        Assert.Equal(4, bytes.Length);

        var decoder = new PtyOutputDecoder(captureOutput: false, maxCaptureLength: 1_000_000);
        var first = decoder.Decode(bytes[..splitIndex], splitIndex);
        var second = decoder.Decode(bytes[splitIndex..], bytes.Length - splitIndex);

        Assert.Equal(emoji, first + second);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Decode_BoxDrawingGlyphs_SplitAcrossChunks(int splitIndex)
    {
        const string boxDrawing = "┌─┐"; // each glyph is a 3-byte UTF-8 sequence
        var bytes = Encoding.UTF8.GetBytes(boxDrawing);

        var decoder = new PtyOutputDecoder(captureOutput: false, maxCaptureLength: 1_000_000);
        var first = decoder.Decode(bytes[..splitIndex], splitIndex);
        var second = decoder.Decode(bytes[splitIndex..], bytes.Length - splitIndex);

        Assert.Equal(boxDrawing, first + second);
    }

    [Fact]
    public void Utf8GetString_OnSplitInput_ProducesReplacementChars()
    {
        // Regression guard documenting the old stateless behavior this plan replaces:
        // Encoding.UTF8.GetString, called independently per chunk, corrupts a multi-byte
        // sequence split across the chunk boundary.
        const string emoji = "🎉";
        var bytes = Encoding.UTF8.GetBytes(emoji);

        var firstChunk = Encoding.UTF8.GetString(bytes, 0, 2);
        var secondChunk = Encoding.UTF8.GetString(bytes, 2, 2);

        Assert.Contains('�', firstChunk + secondChunk);
        Assert.NotEqual(emoji, firstChunk + secondChunk);
    }

    [Fact]
    public void Decode_WithoutCapture_TextIsEmpty()
    {
        var decoder = new PtyOutputDecoder(captureOutput: false, maxCaptureLength: 1_000_000);
        var bytes = Encoding.UTF8.GetBytes("hello");

        var chunk = decoder.Decode(bytes, bytes.Length);

        Assert.Equal("hello", chunk);
        Assert.Equal("", decoder.Text);
    }

    [Fact]
    public void Capture_AccumulatesAcrossChunks()
    {
        var decoder = new PtyOutputDecoder(captureOutput: true, maxCaptureLength: 1_000_000);

        foreach (var chunk in new[] { "hello ", "world", "!" })
        {
            var bytes = Encoding.UTF8.GetBytes(chunk);
            decoder.Decode(bytes, bytes.Length);
        }

        Assert.Equal("hello world!", decoder.Text);
    }

    [Fact]
    public void Capture_NeverExceedsMaxLength()
    {
        const int max = 100;
        var decoder = new PtyOutputDecoder(captureOutput: true, maxCaptureLength: max);

        for (var i = 0; i < 20; i++)
        {
            var chunk = Encoding.UTF8.GetBytes(new string('x', 50));
            decoder.Decode(chunk, chunk.Length);
            Assert.True(decoder.Text.Length <= max);
        }
    }

    [Fact]
    public void Capture_KeepsNewestOutput_DropsOldest()
    {
        const int max = 50;
        var decoder = new PtyOutputDecoder(captureOutput: true, maxCaptureLength: max);

        var firstMarker = Encoding.UTF8.GetBytes("FIRST-MARKER-");
        decoder.Decode(firstMarker, firstMarker.Length);

        for (var i = 0; i < 10; i++)
        {
            var filler = Encoding.UTF8.GetBytes(new string('x', 20));
            decoder.Decode(filler, filler.Length);
        }

        var lastMarker = Encoding.UTF8.GetBytes("LAST-MARKER");
        decoder.Decode(lastMarker, lastMarker.Length);

        Assert.EndsWith("LAST-MARKER", decoder.Text);
        Assert.DoesNotContain("FIRST-MARKER", decoder.Text);
    }

    [Fact]
    public void Capture_SingleChunkLargerThanCap_IsTruncatedToCap()
    {
        const int max = 100;
        var decoder = new PtyOutputDecoder(captureOutput: true, maxCaptureLength: max);

        var huge = Encoding.UTF8.GetBytes(new string('y', 1000));
        decoder.Decode(huge, huge.Length);

        Assert.True(decoder.Text.Length <= max);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Capture_NonPositiveMaxLength_IsClampedAndDoesNotThrow(int maxCaptureLength)
    {
        var decoder = new PtyOutputDecoder(captureOutput: true, maxCaptureLength: maxCaptureLength);

        var exception = Record.Exception(() =>
        {
            var bytes = Encoding.UTF8.GetBytes("hello world");
            decoder.Decode(bytes, bytes.Length);
        });

        Assert.Null(exception);
        Assert.Equal(1, decoder.Text.Length);
    }
}
