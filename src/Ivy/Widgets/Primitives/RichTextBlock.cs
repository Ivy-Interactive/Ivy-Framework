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

    /// <summary>When true, renders as a line break.</summary>
    [Prop] public bool LineBreak { get; set; }

    /// <summary>When true, renders content in monospace/code style.</summary>
    [Prop] public bool Code { get; set; }

    /// <summary>Heading level (1-6). When set, renders as a heading element.</summary>
    [Prop] public int Heading { get; set; }

    /// <summary>When true, this run starts a new paragraph.</summary>
    [Prop] public bool Paragraph { get; set; }

    /// <summary>Bullet list item nesting level (1+). When greater than 0, renders as a bullet point.</summary>
    [Prop] public int BulletItem { get; set; }

    /// <summary>Ordered list item number. When greater than 0, renders as a numbered list item.</summary>
    [Prop] public int OrderedItem { get; set; }

    /// <summary>When true, renders as a horizontal rule.</summary>
    [Prop] public bool HorizontalRule { get; set; }

    /// <summary>When set, renders content as a fenced code block with the given language.</summary>
    [Prop] public string? CodeBlock { get; set; }

    /// <summary>When true, renders as a blockquote.</summary>
    [Prop] public bool Blockquote { get; set; }

    /// <summary>When set, contains raw markdown table text to render via the markdown renderer.</summary>
    [Prop] public string? Table { get; set; }

    /// <summary>When set, contains LaTeX math content to render. "inline" for inline math, "display" for display math.</summary>
    [Prop] public string? Math { get; set; }
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

    /// <summary>
    /// Optional stream for dynamically appending runs after <see cref="Runs"/>.
    /// Create with <c>Context.UseStream&lt;TextRun&gt;()</c> and attach via
    /// <see cref="RichTextBuilder.UseStream"/>. Call <see cref="IWriteStream{T}.Write"/>
    /// to push new runs to the frontend in real time.
    /// </summary>
    [Prop] public IWriteStream<TextRun>? Stream { get; set; }
    [Prop] public TextAlignment? TextAlignment { get; set; }
    [Prop] public bool NoWrap { get; set; }
    [Prop] public Overflow? Overflow { get; set; }

    [Event] public Func<Event<RichTextBlock, string>, ValueTask>? OnLinkClick { get; set; }
}
