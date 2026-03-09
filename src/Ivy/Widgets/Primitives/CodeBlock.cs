using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum Languages
{
    Csharp,
    Javascript,
    Typescript,
    Python,
    Sql,
    Html,
    Css,
    Json,
    Dbml,
    Markdown,
    Text,
    Xml,
    Yaml,
}

/// <summary>
/// Displays inline or block code snippets.
/// </summary>
public record CodeBlock : WidgetBase<CodeBlock>
{
    public CodeBlock(string content, Languages language = Languages.Csharp)
    {
        Content = content;
        Language = language;
    }

    internal CodeBlock()
    {
        Width = Size.Full();
        Height = Size.MaxContent().Max(Size.Px(800));
    }

    [Prop] public string Content { get; set; } = string.Empty;

    [Prop] public Languages Language { get; set; } = Languages.Csharp;

    [Prop] public bool ShowLineNumbers { get; set; }

    [Prop] public int StartingLineNumber { get; set; } = 1;

    [Prop] public bool ShowCopyButton { get; set; } = true;

    [Prop] public bool ShowBorder { get; set; } = true;

    [Prop] public bool WrapLines { get; set; }

}

public static class CodeBlockExtensions
{
    public static CodeBlock Content(this CodeBlock code, string content)
    {
        return code with { Content = content };
    }

    public static CodeBlock Language(this CodeBlock code, Languages language)
    {
        return code with { Language = language };
    }

    public static CodeBlock ShowLineNumbers(this CodeBlock code, bool showLineNumbers = true)
    {
        return code with { ShowLineNumbers = showLineNumbers };
    }

    public static CodeBlock StartingLineNumber(this CodeBlock code, int startingLineNumber)
    {
        return code with { StartingLineNumber = startingLineNumber };
    }

    public static CodeBlock ShowCopyButton(this CodeBlock code, bool showCopyButton = true)
    {
        return code with { ShowCopyButton = showCopyButton };
    }

    public static CodeBlock ShowBorder(this CodeBlock code, bool showBorder = true)
    {
        return code with { ShowBorder = showBorder };
    }

    public static CodeBlock WrapLines(this CodeBlock code, bool wrapLines = true)
    {
        return code with { WrapLines = wrapLines };
    }

}
