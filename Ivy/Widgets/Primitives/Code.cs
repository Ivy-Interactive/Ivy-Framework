using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Supported programming languages for syntax highlighting.</summary>
public enum Languages
{
    /// <summary>C# (.NET)</summary>
    Csharp,
    /// <summary>JavaScript</summary>
    Javascript,
    /// <summary>TypeScript</summary>
    Typescript,
    /// <summary>Python</summary>
    Python,
    /// <summary>SQL</summary>
    Sql,
    /// <summary>HTML</summary>
    Html,
    /// <summary>CSS</summary>
    Css,
    /// <summary>JSON</summary>
    Json,
    /// <summary>DBML (Database Markup Language)</summary>
    Dbml,
    /// <summary>Markdown</summary>
    Markdown,
    /// <summary>Plain text (no highlighting)</summary>
    Text,
}

/// <summary>Code display widget with syntax highlighting, line numbers, and copy functionality.</summary>
public record Code : WidgetBase<Code>
{
    /// <summary>Initializes code widget with content and language. Default size: full width, max 800px height.</summary>
    /// <param name="content">Source code to display.</param>
    /// <param name="language">Programming language. Default: C#.</param>
    public Code(string content, Languages language = Languages.Csharp)
    {
        Content = content;
        Language = language;
        Width = Size.Full();
        Height = Size.MaxContent().Max(Size.Px(800));
    }

    /// <summary>Source code content to display (whitespace preserved).</summary>
    [Prop] public string Content { get; set; }

    /// <summary>Programming language for syntax highlighting.</summary>
    [Prop] public Languages Language { get; set; }

    /// <summary>Whether to show line numbers. Default: false.</summary>
    [Prop] public bool ShowLineNumbers { get; set; }

    /// <summary>Whether to show copy button. Default: true.</summary>
    [Prop] public bool ShowCopyButton { get; set; } = true;

    /// <summary>Whether to show border. Default: true.</summary>
    [Prop] public bool ShowBorder { get; set; } = true;
}

/// <summary>Extension methods for Code widget configuration.</summary>
public static class CodeExtensions
{
    /// <summary>Sets the source code content.</summary>
    public static Code Content(this Code code, string content)
    {
        return code with { Content = content };
    }

    /// <summary>Sets the programming language for syntax highlighting.</summary>
    public static Code Language(this Code code, Languages language)
    {
        return code with { Language = language };
    }

    /// <summary>Sets whether to show line numbers. Default: true.</summary>
    public static Code ShowLineNumbers(this Code code, bool showLineNumbers = true)
    {
        return code with { ShowLineNumbers = showLineNumbers };
    }

    /// <summary>Sets whether to show copy button. Default: true.</summary>
    public static Code ShowCopyButton(this Code code, bool showCopyButton = true)
    {
        return code with { ShowCopyButton = showCopyButton };
    }

    /// <summary>Sets whether to show border. Default: true.</summary>
    public static Code ShowBorder(this Code code, bool showBorder = true)
    {
        return code with { ShowBorder = showBorder };
    }
}