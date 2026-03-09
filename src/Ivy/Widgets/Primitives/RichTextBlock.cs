using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A styled text segment within a RichTextBlock.
/// Each run carries its own formatting: bold, italic, strikethrough, text color, and highlight color.
/// When <see cref="Word"/> is true, a space is automatically prepended before the content
/// (except for the first run in the block).
/// When <see cref="Link"/> is set, the run is rendered as a clickable link.
/// <see cref="LinkTarget"/> controls whether the link opens in a new tab or the same tab.
/// </summary>
public record TextRun
{
    public TextRun(string content = "")
    {
        Content = content;
    }

    internal TextRun() { }

    [Prop] public string Content { get; set; } = string.Empty;
    [Prop] public bool Bold { get; set; }
    [Prop] public bool Italic { get; set; }
    [Prop] public bool StrikeThrough { get; set; }
    [Prop] public Colors? Color { get; set; }
    [Prop] public Colors? HighlightColor { get; set; }
    [Prop] public bool Word { get; set; }
    [Prop] public string? Link { get; set; }
    [Prop] public LinkTarget LinkTarget { get; set; } = LinkTarget.Self;
}

/// <summary>
/// Displays rich text composed of individually styled <see cref="TextRun"/> segments.
/// Supports streaming new runs via <see cref="Stream"/>.
/// When streaming is used, <see cref="Runs"/> provides the initial runs;
/// all streamed runs are appended after these on the frontend.
/// </summary>
public record RichTextBlock : WidgetBase<RichTextBlock>
{
    public RichTextBlock(params TextRun[] runs)
    {
        Runs = runs;
    }

    internal RichTextBlock() { }

    /// <summary>
    /// The text runs to display. When <see cref="Stream"/> is set, these serve as
    /// the initial runs — streamed runs are appended after them on the frontend.
    /// </summary>
    [Prop] public TextRun[] Runs { get; set; } = [];
    [Prop] public IWriteStream<TextRun>? Stream { get; set; }
    [Prop] public TextAlignment? TextAlignment { get; set; }
    [Prop] public bool NoWrap { get; set; }
    [Prop] public Overflow? Overflow { get; set; }

    [Event] public Func<Event<RichTextBlock, string>, ValueTask>? OnLinkClick { get; set; }
}
