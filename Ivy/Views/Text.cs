using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Size = Ivy.Shared.Size;

namespace Ivy.Views;

public static class Text
{
    public static TextBuilder Literal(string content)
    {
        return new TextBuilder(content, TextVariant.Literal);
    }

    public static TextBuilder Literal(IAnyState state) => Literal(state.ToString() ?? "");

    public static TextBuilder H1(string content)
    {
        return new TextBuilder(content, TextVariant.H1);
    }

    public static TextBuilder H1(IAnyState state) => H1(state.ToString() ?? "");

    public static TextBuilder H2(string content)
    {
        return new TextBuilder(content, TextVariant.H2);
    }

    public static TextBuilder H2(IAnyState state) => H2(state.ToString() ?? "");

    public static TextBuilder H3(string content)
    {
        return new TextBuilder(content, TextVariant.H3);
    }

    public static TextBuilder H3(IAnyState state) => H3(state.ToString() ?? "");

    public static TextBuilder H4(string content)
    {
        return new TextBuilder(content, TextVariant.H4);
    }

    public static TextBuilder H4(IAnyState state) => H4(state.ToString() ?? "");

    public static TextBuilder P(string content)
    {
        return new TextBuilder(content, TextVariant.P);
    }

    public static TextBuilder P(IAnyState state) => P(state.ToString() ?? "");

    public static TextBuilder Inline(string content)
    {
        return new TextBuilder(content, TextVariant.Inline);
    }

    public static TextBuilder Inline(IAnyState state) => Inline(state.ToString() ?? "");


    public static TextBuilder Block(string content)
    {
        return new TextBuilder(content, TextVariant.Block);
    }

    public static TextBuilder Block(IAnyState state) => Block(state.ToString() ?? "");

    public static TextBuilder Blockquote(string content)
    {
        return new TextBuilder(content, TextVariant.Blockquote);
    }

    public static TextBuilder Blockquote(IAnyState state) => Blockquote(state.ToString() ?? "");

    public static TextBuilder InlineCode(string content)
    {
        return new TextBuilder(content, TextVariant.InlineCode);
    }

    public static TextBuilder InlineCode(IAnyState state) => InlineCode(state.ToString() ?? "");

    public static TextBuilder Lead(string content)
    {
        return new TextBuilder(content, TextVariant.Lead);
    }

    public static TextBuilder Lead(IAnyState state) => Lead(state.ToString() ?? "");

    public static TextBuilder Large(string content)
    {
        return new TextBuilder(content, TextVariant.Large);
    }

    public static TextBuilder Large(IAnyState state) => Large(state.ToString() ?? "");

    public static TextBuilder Small(string content)
    {
        return new TextBuilder(content, TextVariant.Small);
    }

    public static TextBuilder Small(IAnyState state) => Small(state.ToString() ?? "");

    public static TextBuilder Label(string content)
    {
        return new TextBuilder(content, TextVariant.Label);
    }

    public static TextBuilder Label(IAnyState state) => Label(state.ToString() ?? "");

    public static TextBuilder Muted(string content)
    {
        return new TextBuilder(content, TextVariant.Muted);
    }

    public static TextBuilder Muted(IAnyState state) => Muted(state.ToString() ?? "");

    public static TextBuilder Strong(string content)
    {
        return new TextBuilder(content, TextVariant.Strong);
    }

    public static TextBuilder Strong(IAnyState state) => Strong(state.ToString() ?? "");

    public static TextBuilder Danger(string content)
    {
        return new TextBuilder(content, TextVariant.Danger);
    }

    public static TextBuilder Danger(IAnyState state) => Danger(state.ToString() ?? "");

    public static TextBuilder Warning(string content)
    {
        return new TextBuilder(content, TextVariant.Warning);
    }

    public static TextBuilder Warning(IAnyState state) => Warning(state.ToString() ?? "");

    public static TextBuilder Success(string content)
    {
        return new TextBuilder(content, TextVariant.Success);
    }

    public static TextBuilder Success(IAnyState state) => Success(state.ToString() ?? "");

    public static TextBuilder Code(string content, Languages language = Languages.Csharp)
    {
        return new TextBuilder(content, TextVariant.Code, codeLanguage: language);
    }

    public static TextBuilder Code(IAnyState state, Languages language = Languages.Csharp) => Code(state.ToString() ?? "", language);

    public static TextBuilder Markdown(string content)
    {
        return new TextBuilder(content, TextVariant.Markdown);
    }

    public static TextBuilder Markdown(IAnyState state) => Markdown(state.ToString() ?? "");

    public static TextBuilder Json(string content)
    {
        return new TextBuilder(content, TextVariant.Json);
    }

    public static TextBuilder Json(IAnyState state) => Json(state.ToString() ?? "");

    public static TextBuilder Xml(string content)
    {
        return new TextBuilder(content, TextVariant.Xml);
    }

    public static TextBuilder Xml(IAnyState state) => Xml(state.ToString() ?? "");

    public static TextBuilder Html(string content)
    {
        return new TextBuilder(content, TextVariant.Html);
    }

    public static TextBuilder Html(IAnyState state) => Html(state.ToString() ?? "");

    public static TextBuilder Latex(string content)
    {
        return new TextBuilder(content, TextVariant.Latex);
    }

    public static TextBuilder Latex(IAnyState state) => Latex(state.ToString() ?? "");
}

public class TextBuilder(string content, TextVariant variant, Languages codeLanguage = Languages.Csharp) : ViewBase, IStateless
{
    private bool _strikeThrough;
    private Size? _width;
    private Colors? _color;
    private bool _noWrap;
    private Overflow? _overflow;

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

    public TextBuilder StrikeThrough(bool value = true)
    {
        _strikeThrough = value;
        return this;
    }

    public TextBuilder Width(Size width)
    {
        _width = width;
        return this;
    }

    public TextBuilder Width(int units)
    {
        _width = Size.Units(units);
        return this;
    }

    public TextBuilder Width(float fraction)
    {
        _width = Size.Fraction(fraction);
        return this;
    }

    public TextBuilder Width(double fraction)
    {
        _width = Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public TextBuilder Color(Colors color)
    {
        _color = color;
        return this;
    }

    public TextBuilder NoWrap()
    {
        _noWrap = true;
        return this;
    }

    public TextBuilder Overflow(Overflow overflow)
    {
        _overflow = overflow;
        return this;
    }
}
