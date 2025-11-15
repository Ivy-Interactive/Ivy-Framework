using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual style and semantic meaning of text content.
/// </summary>
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

/// <summary>Low-level text widget rendering text content with customizable styling and variants. Rarely used directly - use Text helper instead.</summary>
public record TextBlock : WidgetBase<TextBlock>
{
    /// <summary>Initializes TextBlock with specified content and styling options.</summary>
    internal TextBlock(string content = "", TextVariant variant = TextVariant.Literal, Size? width = null,
        bool strikeThrough = false, Colors? color = null, bool noWrap = false, Overflow? overflow = null,
        bool bold = false, bool italic = false, bool muted = false)
    {
        Content = content;
        Variant = variant;
        StrikeThrough = strikeThrough;
        Width = width;
        Color = color;
        NoWrap = noWrap;
        Overflow = overflow;
        Bold = bold;
        Italic = italic;
        Muted = muted;
    }

    /// <summary>How text overflow is handled.</summary>
    [Prop] public Overflow? Overflow { get; set; }

    /// <summary>Whether text wrapping is disabled.</summary>
    [Prop] public bool NoWrap { get; set; }

    /// <summary>Text content to display.</summary>
    [Prop] public string Content { get; set; }

    /// <summary>Text variant determining styling and semantic meaning.</summary>
    [Prop] public TextVariant Variant { get; set; }

    /// <summary>Whether strikethrough styling is applied.</summary>
    [Prop] public bool StrikeThrough { get; set; }

    /// <summary>Color override for text, or null to use default color for variant.</summary>
    [Prop] public Colors? Color { get; set; }

    /// <summary>Whether bold styling is applied.</summary>
    [Prop] public bool Bold { get; set; }

    /// <summary>Whether italic styling is applied.</summary>
    [Prop] public bool Italic { get; set; }

    /// <summary>Whether muted styling is applied.</summary>
    [Prop] public bool Muted { get; set; }
}