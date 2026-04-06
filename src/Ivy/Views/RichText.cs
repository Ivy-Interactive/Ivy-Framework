using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Fluent builder for <see cref="RichTextBlock"/>.
/// Use <see cref="Text.Rich()"/> to create an instance.
/// </summary>
public class RichTextBuilder : ViewBase, IStateless
{
    private readonly List<TextRun> _runs = new();
    private TextAlignment? _textAlignment;
    private bool _noWrap;
    private Overflow? _overflow;
    private Density? _density;
    private IWriteStream<TextRun>? _stream;
    private Func<Event<RichTextBlock, string>, ValueTask>? _onLinkClick;

    // --- Run builders (atomic) ---

    /// <summary>Add a text run with optional styling.</summary>
    public RichTextBuilder Run(string content, bool bold = false, bool italic = false,
        bool strikeThrough = false, Colors? color = null, Colors? highlightColor = null,
        bool word = false, string? link = null, LinkTarget linkTarget = LinkTarget.Self)
    {
        _runs.Add(new TextRun(content)
        {
            Bold = bold,
            Italic = italic,
            StrikeThrough = strikeThrough,
            Color = color,
            HighlightColor = highlightColor,
            Word = word,
            Link = link,
            LinkTarget = linkTarget
        });
        return this;
    }

    /// <summary>Add a bold text run.</summary>
    public RichTextBuilder Bold(string content, bool italic = false, bool strikeThrough = false,
        Colors? color = null, Colors? highlightColor = null, bool word = false, string? link = null,
        LinkTarget linkTarget = LinkTarget.Self)
        => Run(content, bold: true, italic: italic, strikeThrough: strikeThrough,
            color: color, highlightColor: highlightColor, word: word, link: link, linkTarget: linkTarget);

    /// <summary>Add an italic text run.</summary>
    public RichTextBuilder Italic(string content, bool bold = false, bool strikeThrough = false,
        Colors? color = null, Colors? highlightColor = null, bool word = false, string? link = null,
        LinkTarget linkTarget = LinkTarget.Self)
        => Run(content, bold: bold, italic: true, strikeThrough: strikeThrough,
            color: color, highlightColor: highlightColor, word: word, link: link, linkTarget: linkTarget);

    /// <summary>Add a strikethrough text run.</summary>
    public RichTextBuilder StrikeThrough(string content, bool bold = false, bool italic = false,
        Colors? color = null, Colors? highlightColor = null, bool word = false, string? link = null,
        LinkTarget linkTarget = LinkTarget.Self)
        => Run(content, bold: bold, italic: italic, strikeThrough: true,
            color: color, highlightColor: highlightColor, word: word, link: link, linkTarget: linkTarget);

    /// <summary>Add a muted (secondary color) text run.</summary>
    public RichTextBuilder Muted(string content, bool bold = false, bool italic = false,
        bool strikeThrough = false, Colors? highlightColor = null,
        bool word = false, string? link = null, LinkTarget linkTarget = LinkTarget.Self)
        => Run(content, bold: bold, italic: italic, strikeThrough: strikeThrough,
            color: Colors.Muted, highlightColor: highlightColor, word: word, link: link, linkTarget: linkTarget);

    /// <summary>Add a plain text run with Word=true for auto-spacing.</summary>
    public RichTextBuilder Word(string content, bool bold = false, bool italic = false,
        bool strikeThrough = false, Colors? color = null, Colors? highlightColor = null, string? link = null,
        LinkTarget linkTarget = LinkTarget.Self)
        => Run(content, bold: bold, italic: italic, strikeThrough: strikeThrough,
            color: color, highlightColor: highlightColor, word: true, link: link, linkTarget: linkTarget);

    /// <summary>Add a link run. Links are words by default (auto-spaced).</summary>
    public RichTextBuilder Link(string content, string url, bool bold = false, bool italic = false,
        bool strikeThrough = false, Colors? color = null, Colors? highlightColor = null,
        bool word = true, LinkTarget linkTarget = LinkTarget.Self)
        => Run(content, bold: bold, italic: italic, strikeThrough: strikeThrough,
            color: color, highlightColor: highlightColor, word: word, link: url, linkTarget: linkTarget);

    /// <summary>Add a line break run.</summary>
    public RichTextBuilder LineBreak()
    {
        _runs.Add(new TextRun { LineBreak = true });
        return this;
    }

    // --- Block-level styling ---

    /// <summary>Prevent text from wrapping.</summary>
    public RichTextBuilder NoWrap() { _noWrap = true; return this; }

    /// <summary>Set overflow behavior.</summary>
    public RichTextBuilder Overflow(Overflow overflow) { _overflow = overflow; return this; }

    /// <summary>Set the density of the rich text block.</summary>
    public RichTextBuilder Density(Density density) { _density = density; return this; }
    public RichTextBuilder Small() => Density(Ivy.Density.Small);
    public RichTextBuilder Medium() => Density(Ivy.Density.Medium);
    public RichTextBuilder Large() => Density(Ivy.Density.Large);

    /// <summary>Set text alignment.</summary>
    public RichTextBuilder Align(TextAlignment alignment) { _textAlignment = alignment; return this; }
    public RichTextBuilder Left() => Align(TextAlignment.Left);
    public RichTextBuilder Center() => Align(TextAlignment.Center);
    public RichTextBuilder Right() => Align(TextAlignment.Right);

    /// <summary>Attach a stream for dynamically appending runs after the initial <see cref="RichTextBlock.Runs"/>.</summary>
    public RichTextBuilder UseStream(IWriteStream<TextRun> stream) { _stream = stream; return this; }

    /// <summary>Set a callback invoked when a link run is clicked. Receives the link URL.</summary>
    public RichTextBuilder OnLinkClick(Func<Event<RichTextBlock, string>, ValueTask> handler) { _onLinkClick = handler; return this; }

    /// <summary>Set a callback invoked when a link run is clicked. Receives the link URL.</summary>
    public RichTextBuilder OnLinkClick(Action<Event<RichTextBlock, string>> handler) { _onLinkClick = handler.ToValueTask(); return this; }

    /// <summary>Set a callback invoked when a link run is clicked. Receives the link URL.</summary>
    public RichTextBuilder OnLinkClick(Action<string> handler)
    {
        _onLinkClick = @event => { handler(@event.Value); return ValueTask.CompletedTask; };
        return this;
    }

    public override object? Build()
    {
        return new RichTextBlock(_runs.ToArray())
        {
            TextAlignment = _textAlignment,
            NoWrap = _noWrap,
            Overflow = _overflow,
            Density = _density,
            Stream = _stream,
            OnLinkClick = _onLinkClick
        };
    }
}
