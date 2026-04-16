// ReSharper disable once CheckNamespace
namespace Ivy;

public enum TextVariant
{
    Literal,
    H1,
    H2,
    H3,
    H4,
    H5,
    H6,
    Block,
    P,
    Inline,
    Blockquote,
    Monospaced,
    Lead,
    Muted,
    Danger,
    Warning,
    Success,
    //Invalid values. Only used in Text helper.
    Code,
    Markdown,
    Json,
    Xml,
    Html,
    Latex,
    Label,
    Strong,
    Display
}

/// <summary>
/// Displays text.
/// </summary>
public record TextBlock : WidgetBase<TextBlock>
{
    internal TextBlock(string content = "", TextVariant variant = TextVariant.Literal, Size? width = null,
        bool strikeThrough = false, Colors? color = null, bool noWrap = false, Overflow? overflow = null,
        bool bold = false, bool italic = false, bool muted = false, TextAlignment? textAlignment = null)
    {
        Content = content;
        Variant = variant;
        StrikeThrough = strikeThrough;
        Width = width.ToResponsive();
        Color = color;
        NoWrap = noWrap;
        Overflow = overflow;
        Bold = bold;
        Italic = italic;
        Muted = muted;
        TextAlignment = textAlignment;
    }

    internal TextBlock()
    {
    }

    [Prop] public Overflow? Overflow { get; set; }

    [Prop] public bool NoWrap { get; set; }

    [Prop] public string Content { get; set; } = String.Empty;

    [Prop] public TextVariant Variant { get; set; } = TextVariant.Literal;

    [Prop] public bool StrikeThrough { get; set; }

    [Prop] public Colors? Color { get; set; }

    [Prop] public bool Bold { get; set; }

    [Prop] public bool Italic { get; set; }

    [Prop] public bool Muted { get; set; }

    [Prop] public TextAlignment? TextAlignment { get; set; }

    [Prop] public string? Anchor { get; set; }

}