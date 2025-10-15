using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Size = Ivy.Shared.Size;

namespace Ivy.Views;

/// <summary>
/// Provides static factory methods for creating text views with different
/// variants including headings, paragraphs, code blocks, and semantic text styles.
/// </summary>
public static class Text
{
    /// <summary>
    /// Creates a literal text builder for plain text content.
    /// </summary>
    /// <param name="content">The text content to display.</param>
    /// <returns>A TextBuilder configured for literal text display.</returns>
    public static TextBuilder Literal(string content)
    {
        return new TextBuilder(content, TextVariant.Literal);
    }

    /// <summary>
    /// Creates a literal text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to text.</param>
    /// <returns>A TextBuilder configured for literal text display.</returns>
    public static TextBuilder Literal(IAnyState state) => Literal(state.ToString() ?? "");

    /// <summary>
    /// Creates an H1 heading text builder for main page titles.
    /// </summary>
    /// <param name="content">The heading text content.</param>
    /// <returns>A TextBuilder configured for H1 heading display.</returns>
    public static TextBuilder H1(string content)
    {
        return new TextBuilder(content, TextVariant.H1);
    }

    /// <summary>
    /// Creates an H1 heading text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to heading text.</param>
    /// <returns>A TextBuilder configured for H1 heading display.</returns>
    public static TextBuilder H1(IAnyState state) => H1(state.ToString() ?? "");

    /// <summary>
    /// Creates an H2 heading text builder for section titles.
    /// </summary>
    /// <param name="content">The heading text content.</param>
    /// <returns>A TextBuilder configured for H2 heading display.</returns>
    public static TextBuilder H2(string content)
    {
        return new TextBuilder(content, TextVariant.H2);
    }

    /// <summary>
    /// Creates an H2 heading text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to heading text.</param>
    /// <returns>A TextBuilder configured for H2 heading display.</returns>
    public static TextBuilder H2(IAnyState state) => H2(state.ToString() ?? "");

    /// <summary>
    /// Creates an H3 heading text builder for subsection titles.
    /// </summary>
    /// <param name="content">The heading text content.</param>
    /// <returns>A TextBuilder configured for H3 heading display.</returns>
    public static TextBuilder H3(string content)
    {
        return new TextBuilder(content, TextVariant.H3);
    }

    /// <summary>
    /// Creates an H3 heading text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to heading text.</param>
    /// <returns>A TextBuilder configured for H3 heading display.</returns>
    public static TextBuilder H3(IAnyState state) => H3(state.ToString() ?? "");

    /// <summary>
    /// Creates an H4 heading text builder for minor section titles.
    /// </summary>
    /// <param name="content">The heading text content.</param>
    /// <returns>A TextBuilder configured for H4 heading display.</returns>
    public static TextBuilder H4(string content)
    {
        return new TextBuilder(content, TextVariant.H4);
    }

    /// <summary>
    /// Creates an H4 heading text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to heading text.</param>
    /// <returns>A TextBuilder configured for H4 heading display.</returns>
    public static TextBuilder H4(IAnyState state) => H4(state.ToString() ?? "");

    /// <summary>
    /// Creates a paragraph text builder for body text content.
    /// </summary>
    /// <param name="content">The paragraph text content.</param>
    /// <returns>A TextBuilder configured for paragraph display.</returns>
    public static TextBuilder P(string content)
    {
        return new TextBuilder(content, TextVariant.P);
    }

    /// <summary>
    /// Creates a paragraph text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to paragraph text.</param>
    /// <returns>A TextBuilder configured for paragraph display.</returns>
    public static TextBuilder P(IAnyState state) => P(state.ToString() ?? "");

    /// <summary>
    /// Creates an inline text builder for text that flows with other content.
    /// </summary>
    /// <param name="content">The inline text content.</param>
    /// <returns>A TextBuilder configured for inline text display.</returns>
    public static TextBuilder Inline(string content)
    {
        return new TextBuilder(content, TextVariant.Inline);
    }

    /// <summary>
    /// Creates an inline text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to inline text.</param>
    /// <returns>A TextBuilder configured for inline text display.</returns>
    public static TextBuilder Inline(IAnyState state) => Inline(state.ToString() ?? "");

    /// <summary>
    /// Creates a block text builder for standalone text blocks.
    /// </summary>
    /// <param name="content">The block text content.</param>
    /// <returns>A TextBuilder configured for block text display.</returns>
    public static TextBuilder Block(string content)
    {
        return new TextBuilder(content, TextVariant.Block);
    }

    /// <summary>
    /// Creates a block text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to block text.</param>
    /// <returns>A TextBuilder configured for block text display.</returns>
    public static TextBuilder Block(IAnyState state) => Block(state.ToString() ?? "");

    /// <summary>
    /// Creates a blockquote text builder for quoted content.
    /// </summary>
    /// <param name="content">The blockquote text content.</param>
    /// <returns>A TextBuilder configured for blockquote display.</returns>
    public static TextBuilder Blockquote(string content)
    {
        return new TextBuilder(content, TextVariant.Blockquote);
    }

    /// <summary>
    /// Creates a blockquote text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to blockquote text.</param>
    /// <returns>A TextBuilder configured for blockquote display.</returns>
    public static TextBuilder Blockquote(IAnyState state) => Blockquote(state.ToString() ?? "");

    /// <summary>
    /// Creates an inline code text builder for short code snippets.
    /// </summary>
    /// <param name="content">The inline code content.</param>
    /// <returns>A TextBuilder configured for inline code display.</returns>
    public static TextBuilder InlineCode(string content)
    {
        return new TextBuilder(content, TextVariant.InlineCode);
    }

    /// <summary>
    /// Creates an inline code text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to inline code text.</param>
    /// <returns>A TextBuilder configured for inline code display.</returns>
    public static TextBuilder InlineCode(IAnyState state) => InlineCode(state.ToString() ?? "");

    /// <summary>
    /// Creates a lead text builder for introductory or emphasized text.
    /// </summary>
    /// <param name="content">The lead text content.</param>
    /// <returns>A TextBuilder configured for lead text display.</returns>
    public static TextBuilder Lead(string content)
    {
        return new TextBuilder(content, TextVariant.Lead);
    }

    /// <summary>
    /// Creates a lead text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to lead text.</param>
    /// <returns>A TextBuilder configured for lead text display.</returns>
    public static TextBuilder Lead(IAnyState state) => Lead(state.ToString() ?? "");

    /// <summary>
    /// Creates a large text builder for emphasized text content.
    /// </summary>
    /// <param name="content">The large text content.</param>
    /// <returns>A TextBuilder configured for large text display.</returns>
    public static TextBuilder Large(string content)
    {
        return new TextBuilder(content, TextVariant.Large);
    }

    /// <summary>
    /// Creates a large text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to large text.</param>
    /// <returns>A TextBuilder configured for large text display.</returns>
    public static TextBuilder Large(IAnyState state) => Large(state.ToString() ?? "");

    /// <summary>
    /// Creates a small text builder for fine print or secondary text.
    /// </summary>
    /// <param name="content">The small text content.</param>
    /// <returns>A TextBuilder configured for small text display.</returns>
    public static TextBuilder Small(string content)
    {
        return new TextBuilder(content, TextVariant.Small);
    }

    /// <summary>
    /// Creates a small text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to small text.</param>
    /// <returns>A TextBuilder configured for small text display.</returns>
    public static TextBuilder Small(IAnyState state) => Small(state.ToString() ?? "");

    /// <summary>
    /// Creates a label text builder for form labels or captions.
    /// </summary>
    /// <param name="content">The label text content.</param>
    /// <returns>A TextBuilder configured for label text display.</returns>
    public static TextBuilder Label(string content)
    {
        return new TextBuilder(content, TextVariant.Label);
    }

    /// <summary>
    /// Creates a label text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to label text.</param>
    /// <returns>A TextBuilder configured for label text display.</returns>
    public static TextBuilder Label(IAnyState state) => Label(state.ToString() ?? "");

    /// <summary>
    /// Creates a muted text builder for secondary or less prominent text.
    /// </summary>
    /// <param name="content">The muted text content.</param>
    /// <returns>A TextBuilder configured for muted text display.</returns>
    public static TextBuilder Muted(string content)
    {
        return new TextBuilder(content, TextVariant.Muted);
    }

    /// <summary>
    /// Creates a muted text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to muted text.</param>
    /// <returns>A TextBuilder configured for muted text display.</returns>
    public static TextBuilder Muted(IAnyState state) => Muted(state.ToString() ?? "");

    /// <summary>
    /// Creates a strong text builder for bold or emphasized text.
    /// </summary>
    /// <param name="content">The strong text content.</param>
    /// <returns>A TextBuilder configured for strong text display.</returns>
    public static TextBuilder Strong(string content)
    {
        return new TextBuilder(content, TextVariant.Strong);
    }

    /// <summary>
    /// Creates a strong text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to strong text.</param>
    /// <returns>A TextBuilder configured for strong text display.</returns>
    public static TextBuilder Strong(IAnyState state) => Strong(state.ToString() ?? "");

    /// <summary>
    /// Creates a danger text builder for error or warning messages.
    /// </summary>
    /// <param name="content">The danger text content.</param>
    /// <returns>A TextBuilder configured for danger text display.</returns>
    public static TextBuilder Danger(string content)
    {
        return new TextBuilder(content, TextVariant.Danger);
    }

    /// <summary>
    /// Creates a danger text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to danger text.</param>
    /// <returns>A TextBuilder configured for danger text display.</returns>
    public static TextBuilder Danger(IAnyState state) => Danger(state.ToString() ?? "");

    /// <summary>
    /// Creates a warning text builder for caution or notice messages.
    /// </summary>
    /// <param name="content">The warning text content.</param>
    /// <returns>A TextBuilder configured for warning text display.</returns>
    public static TextBuilder Warning(string content)
    {
        return new TextBuilder(content, TextVariant.Warning);
    }

    /// <summary>
    /// Creates a warning text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to warning text.</param>
    /// <returns>A TextBuilder configured for warning text display.</returns>
    public static TextBuilder Warning(IAnyState state) => Warning(state.ToString() ?? "");

    /// <summary>
    /// Creates a success text builder for positive or confirmation messages.
    /// </summary>
    /// <param name="content">The success text content.</param>
    /// <returns>A TextBuilder configured for success text display.</returns>
    public static TextBuilder Success(string content)
    {
        return new TextBuilder(content, TextVariant.Success);
    }

    /// <summary>
    /// Creates a success text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to success text.</param>
    /// <returns>A TextBuilder configured for success text display.</returns>
    public static TextBuilder Success(IAnyState state) => Success(state.ToString() ?? "");

    /// <summary>
    /// Creates a code text builder for code blocks with syntax highlighting.
    /// </summary>
    /// <param name="content">The code content to display.</param>
    /// <param name="language">The programming language for syntax highlighting (defaults to C#).</param>
    /// <returns>A TextBuilder configured for code block display.</returns>
    public static TextBuilder Code(string content, Languages language = Languages.Csharp)
    {
        return new TextBuilder(content, TextVariant.Code, codeLanguage: language);
    }

    /// <summary>
    /// Creates a code text builder from a state object with syntax highlighting.
    /// </summary>
    /// <param name="state">The state object to convert to code text.</param>
    /// <param name="language">The programming language for syntax highlighting (defaults to C#).</param>
    /// <returns>A TextBuilder configured for code block display.</returns>
    public static TextBuilder Code(IAnyState state, Languages language = Languages.Csharp) => Code(state.ToString() ?? "", language);

    /// <summary>
    /// Creates a markdown text builder for markdown-formatted content.
    /// </summary>
    /// <param name="content">The markdown content to render.</param>
    /// <returns>A TextBuilder configured for markdown display.</returns>
    public static TextBuilder Markdown(string content)
    {
        return new TextBuilder(content, TextVariant.Markdown);
    }

    /// <summary>
    /// Creates a markdown text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to markdown text.</param>
    /// <returns>A TextBuilder configured for markdown display.</returns>
    public static TextBuilder Markdown(IAnyState state) => Markdown(state.ToString() ?? "");

    /// <summary>
    /// Creates a JSON text builder for JSON-formatted content.
    /// </summary>
    /// <param name="content">The JSON content to display.</param>
    /// <returns>A TextBuilder configured for JSON display.</returns>
    public static TextBuilder Json(string content)
    {
        return new TextBuilder(content, TextVariant.Json);
    }

    /// <summary>
    /// Creates a JSON text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to JSON text.</param>
    /// <returns>A TextBuilder configured for JSON display.</returns>
    public static TextBuilder Json(IAnyState state) => Json(state.ToString() ?? "");

    /// <summary>
    /// Creates an XML text builder for XML-formatted content.
    /// </summary>
    /// <param name="content">The XML content to display.</param>
    /// <returns>A TextBuilder configured for XML display.</returns>
    public static TextBuilder Xml(string content)
    {
        return new TextBuilder(content, TextVariant.Xml);
    }

    /// <summary>
    /// Creates an XML text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to XML text.</param>
    /// <returns>A TextBuilder configured for XML display.</returns>
    public static TextBuilder Xml(IAnyState state) => Xml(state.ToString() ?? "");

    /// <summary>
    /// Creates an HTML text builder for HTML-formatted content.
    /// </summary>
    /// <param name="content">The HTML content to display.</param>
    /// <returns>A TextBuilder configured for HTML display.</returns>
    public static TextBuilder Html(string content)
    {
        return new TextBuilder(content, TextVariant.Html);
    }

    /// <summary>
    /// Creates an HTML text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to HTML text.</param>
    /// <returns>A TextBuilder configured for HTML display.</returns>
    public static TextBuilder Html(IAnyState state) => Html(state.ToString() ?? "");

    /// <summary>
    /// Creates a LaTeX text builder for mathematical content.
    /// </summary>
    /// <param name="content">The LaTeX content to display.</param>
    /// <returns>A TextBuilder configured for LaTeX display.</returns>
    public static TextBuilder Latex(string content)
    {
        return new TextBuilder(content, TextVariant.Latex);
    }

    /// <summary>
    /// Creates a LaTeX text builder from a state object.
    /// </summary>
    /// <param name="state">The state object to convert to LaTeX text.</param>
    /// <returns>A TextBuilder configured for LaTeX display.</returns>
    public static TextBuilder Latex(IAnyState state) => Latex(state.ToString() ?? "");
}

/// <summary>
/// A builder class for creating and configuring text views with various
/// styling options including width, color, text wrapping, and overflow handling.
/// This class provides a fluent API for text customization.
/// </summary>
public class TextBuilder(string content, TextVariant variant, Languages codeLanguage = Languages.Csharp) : ViewBase, IStateless
{
    private bool _strikeThrough;
    private Size? _width;
    private Colors? _color;
    private bool _noWrap;
    private Overflow? _overflow;

    /// <summary>
    /// Builds the final text widget based on the variant and configuration.
    /// Special variants like Code, Markdown, JSON, XML, HTML, and LaTeX
    /// create specialized widgets, while other variants create TextBlock widgets.
    /// </summary>
    /// <returns>A text widget configured with the current settings and variant.</returns>
    public override object? Build()
    {
        switch (variant)
        {
            case TextVariant.Code:
                return new Code(content, codeLanguage);
            case TextVariant.Markdown:
                return new Markdown(content);
            case TextVariant.Json:
                return new Json(content);
            case TextVariant.Xml:
                return new Xml(content);
            case TextVariant.Html:
                return new Html(content);
            case TextVariant.Latex:
                return new Markdown("$$" + Environment.NewLine + content + Environment.NewLine + "$$");
            default:
                {
                    var text = new TextBlock(
                        content, variant, _width, _strikeThrough, _color, _noWrap, _overflow);
                    return text;
                }
        }
    }

    /// <summary>
    /// Sets whether the text should be displayed with a strikethrough effect.
    /// </summary>
    /// <param name="value">True to apply strikethrough, false to remove it (defaults to true).</param>
    /// <returns>The current TextBuilder instance for method chaining.</returns>
    public TextBuilder StrikeThrough(bool value = true)
    {
        _strikeThrough = value;
        return this;
    }

    /// <summary>
    /// Sets the width of the text using a Size value.
    /// </summary>
    /// <param name="width">The Size value that determines the text's width.</param>
    /// <returns>The current TextBuilder instance for method chaining.</returns>
    public TextBuilder Width(Size width)
    {
        _width = width;
        return this;
    }

    /// <summary>
    /// Sets the width of the text in units.
    /// </summary>
    /// <param name="units">The width in units.</param>
    /// <returns>The current TextBuilder instance for method chaining.</returns>
    public TextBuilder Width(int units)
    {
        _width = Size.Units(units);
        return this;
    }

    /// <summary>
    /// Sets the width of the text as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The width as a fraction (0.0 to 1.0).</param>
    /// <returns>The current TextBuilder instance for method chaining.</returns>
    public TextBuilder Width(float fraction)
    {
        _width = Size.Fraction(fraction);
        return this;
    }

    /// <summary>
    /// Sets the width of the text as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The width as a fraction (0.0 to 1.0).</param>
    /// <returns>The current TextBuilder instance for method chaining.</returns>
    public TextBuilder Width(double fraction)
    {
        _width = Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    /// <summary>
    /// Sets the color of the text.
    /// </summary>
    /// <param name="color">The Colors value for the text color.</param>
    /// <returns>The current TextBuilder instance for method chaining.</returns>
    public TextBuilder Color(Colors color)
    {
        _color = color;
        return this;
    }

    /// <summary>
    /// Prevents the text from wrapping to new lines.
    /// </summary>
    /// <returns>The current TextBuilder instance for method chaining.</returns>
    public TextBuilder NoWrap()
    {
        _noWrap = true;
        return this;
    }

    /// <summary>
    /// Sets the overflow behavior for text that exceeds its container.
    /// </summary>
    /// <param name="overflow">The Overflow value that determines how text overflow is handled.</param>
    /// <returns>The current TextBuilder instance for method chaining.</returns>
    public TextBuilder Overflow(Overflow overflow)
    {
        _overflow = overflow;
        return this;
    }
}
