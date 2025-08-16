using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum TextVariant
{
    Literal,
    H1,
    H2,
    H3,
    H4,
    Block,
    P,
    Inline,
    Blockquote,
    InlineCode,
    Lead,
    Large,
    Small,
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
    Strong
}

public record TextBlock : WidgetBase<TextBlock>
{
    internal TextBlock(string content = "", TextVariant variant = TextVariant.Literal, Size? width = null,
        bool strikeThrough = false, Colors? color = null, bool noWrap = false, Overflow? overflow = null)
    {
        Content = content;
        Variant = variant;
        StrikeThrough = strikeThrough;
        Width = width;
        Color = color;
        NoWrap = noWrap;
        Overflow = overflow;
    }

    [Prop] public Overflow? Overflow { get; set; }
    [Prop] public bool NoWrap { get; set; }
    [Prop] public string Content { get; set; }
    [Prop] public TextVariant Variant { get; set; }
    [Prop] public bool StrikeThrough { get; set; }
    [Prop] public Colors? Color { get; set; }
}