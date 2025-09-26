using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual style and semantic meaning of text content.
/// </summary>
public enum TextVariant
{
    /// <summary>Plain text with no special formatting.</summary>
    Literal,
    /// <summary>Main heading (h1 element).</summary>
    H1,
    /// <summary>Secondary heading (h2 element).</summary>
    H2,
    /// <summary>Tertiary heading (h3 element).</summary>
    H3,
    /// <summary>Quaternary heading (h4 element).</summary>
    H4,
    /// <summary>Block-level text container.</summary>
    Block,
    /// <summary>Paragraph text (p element).</summary>
    P,
    /// <summary>Inline text span.</summary>
    Inline,
    /// <summary>Blockquote text for quotations.</summary>
    Blockquote,
    /// <summary>Inline code text with monospace font.</summary>
    InlineCode,
    /// <summary>Lead text for prominent introductory content.</summary>
    Lead,
    /// <summary>Large text for emphasis.</summary>
    Large,
    /// <summary>Small text for secondary information.</summary>
    Small,
    /// <summary>Muted text with reduced opacity.</summary>
    Muted,
    /// <summary>Danger/error text with red styling.</summary>
    Danger,
    /// <summary>Warning text with amber styling.</summary>
    Warning,
    /// <summary>Success text with green styling.</summary>
    Success,
    //Invalid values. Only used in Text helper.
    /// <summary>Code text variant (used internally by Text helper).</summary>
    Code,
    /// <summary>Markdown text variant (used internally by Text helper).</summary>
    Markdown,
    /// <summary>JSON text variant (used internally by Text helper).</summary>
    Json,
    /// <summary>XML text variant (used internally by Text helper).</summary>
    Xml,
    /// <summary>HTML text variant (used internally by Text helper).</summary>
    Html,
    /// <summary>LaTeX text variant (used internally by Text helper).</summary>
    Latex,
    /// <summary>Label text for form fields and UI labels.</summary>
    Label,
    /// <summary>Strong/bold text for emphasis.</summary>
    Strong
}

/// <summary>Low-level text widget rendering text content with customizable styling and variants. Rarely used directly - use Text helper instead.</summary>
public record TextBlock : WidgetBase<TextBlock>
{
    /// <summary>Initializes TextBlock with specified content and styling options.</summary>
    /// <param name="content">Text content to display.</param>
    /// <param name="variant">Text variant determining styling and semantic meaning.</param>
    /// <param name="width">Optional width constraint for text.</param>
    /// <param name="strikeThrough">Whether to apply strikethrough styling.</param>
    /// <param name="color">Optional color override for text.</param>
    /// <param name="noWrap">Whether to prevent text wrapping.</param>
    /// <param name="overflow">How to handle text overflow.</param>
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
}