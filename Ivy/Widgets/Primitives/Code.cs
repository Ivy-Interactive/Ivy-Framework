using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the programming languages and formats supported for syntax highlighting in code display widgets.
/// Each language provides specific syntax highlighting rules, keywords, and formatting appropriate
/// for that language's syntax and conventions.
/// </summary>
public enum Languages
{
    /// <summary>C# programming language with .NET-specific syntax highlighting.</summary>
    Csharp,
    /// <summary>JavaScript programming language with ECMAScript syntax highlighting.</summary>
    Javascript,
    /// <summary>TypeScript programming language with type annotations and modern JavaScript features.</summary>
    Typescript,
    /// <summary>Python programming language with Python-specific syntax and indentation highlighting.</summary>
    Python,
    /// <summary>SQL database query language with database-specific keywords and syntax.</summary>
    Sql,
    /// <summary>HTML markup language with tag, attribute, and structure highlighting.</summary>
    Html,
    /// <summary>CSS stylesheet language with selector, property, and value highlighting.</summary>
    Css,
    /// <summary>JSON data format with structure, key, and value highlighting.</summary>
    Json,
    /// <summary>DBML (Database Markup Language) for database schema definitions.</summary>
    Dbml,
    /// <summary>Markdown lightweight markup language with formatting and structure highlighting.</summary>
    Markdown,
    /// <summary>Plain text without syntax highlighting, displayed in monospace font.</summary>
    Text,
}

/// <summary>
/// A code display widget that provides syntax-highlighted presentation of source code and text content.
/// Offers comprehensive code display features including syntax highlighting for multiple programming languages,
/// line numbers, copy functionality, and customizable appearance for documentation, tutorials, and code examples.
/// </summary>
public record Code : WidgetBase<Code>
{
    /// <summary>
    /// Initializes a new code display widget with the specified content and programming language.
    /// Creates a syntax-highlighted code block with default settings optimized for readability
    /// and user interaction, including copy functionality and bordered appearance.
    /// </summary>
    /// <param name="content">The source code or text content to display with syntax highlighting.</param>
    /// <param name="language">The programming language for syntax highlighting. Default is C#.</param>
    /// <remarks>
    /// The Code widget is designed for displaying source code and formatted text:
    /// <list type="bullet">
    /// <item><description><strong>Documentation:</strong> Display code examples in tutorials and API documentation</description></item>
    /// <item><description><strong>Code sharing:</strong> Present code snippets with proper formatting and highlighting</description></item>
    /// <item><description><strong>Configuration display:</strong> Show configuration files, JSON, and structured data</description></item>
    /// <item><description><strong>Educational content:</strong> Teach programming concepts with highlighted syntax</description></item>
    /// </list>
    /// <para>Default sizing provides full width with content-based height, capped at 800px for readability.</para>
    /// </remarks>
    public Code(string content, Languages language = Languages.Csharp)
    {
        Content = content;
        Language = language;
        Width = Size.Full();
        Height = Size.MaxContent().Max(Size.Px(800));
    }

    /// <summary>Gets or sets the source code or text content to display.</summary>
    /// <value>The code content as a string, preserving whitespace and formatting.</value>
    /// <remarks>
    /// The content is displayed with syntax highlighting appropriate for the specified language.
    /// Whitespace, indentation, and line breaks are preserved to maintain code structure and readability.
    /// </remarks>
    [Prop] public string Content { get; set; }

    /// <summary>Gets or sets the programming language for syntax highlighting.</summary>
    /// <value>The Languages enumeration value determining the syntax highlighting rules to apply.</value>
    /// <remarks>
    /// The language setting controls keyword highlighting, comment formatting, string literals,
    /// and other language-specific syntax elements to improve code readability and comprehension.
    /// </remarks>
    [Prop] public Languages Language { get; set; }

    /// <summary>Gets or sets whether to display line numbers alongside the code content.</summary>
    /// <value>true to show line numbers; false to hide them (default).</value>
    /// <remarks>
    /// Line numbers help users reference specific lines of code and are particularly useful
    /// in educational contexts, code reviews, and when discussing specific parts of the code.
    /// </remarks>
    [Prop] public bool ShowLineNumbers { get; set; }

    /// <summary>Gets or sets whether to display a copy button for copying the code content to clipboard.</summary>
    /// <value>true to show the copy button (default); false to hide it.</value>
    /// <remarks>
    /// The copy button enables users to easily copy code content for use in their own projects,
    /// improving the user experience when working with code examples and snippets.
    /// </remarks>
    [Prop] public bool ShowCopyButton { get; set; } = true;

    /// <summary>Gets or sets whether to display a border around the code block.</summary>
    /// <value>true to show the border (default); false for a borderless appearance.</value>
    /// <remarks>
    /// The border provides visual separation between the code block and surrounding content,
    /// helping to clearly define the code area and improve overall layout organization.
    /// </remarks>
    [Prop] public bool ShowBorder { get; set; } = true;
}

/// <summary>
/// Provides extension methods for configuring code display widgets with fluent syntax.
/// Enables convenient configuration of code properties including content, language, display options,
/// and visual features through method chaining for improved readability and ease of use.
/// </summary>
public static class CodeExtensions
{
    /// <summary>
    /// Sets the source code or text content to display in the code widget.
    /// </summary>
    /// <param name="code">The code widget to configure.</param>
    /// <param name="content">The source code or text content to display with syntax highlighting.</param>
    /// <returns>The code widget with the specified content.</returns>
    /// <remarks>
    /// The content will be displayed with syntax highlighting appropriate for the widget's language setting.
    /// Whitespace and formatting are preserved to maintain code structure and readability.
    /// </remarks>
    public static Code Content(this Code code, string content)
    {
        return code with { Content = content };
    }

    /// <summary>
    /// Sets the programming language for syntax highlighting.
    /// </summary>
    /// <param name="code">The code widget to configure.</param>
    /// <param name="language">The programming language from the Languages enumeration.</param>
    /// <returns>The code widget with the specified language for syntax highlighting.</returns>
    /// <remarks>
    /// The language setting determines the syntax highlighting rules, keywords, and formatting
    /// applied to the code content for improved readability and comprehension.
    /// </remarks>
    public static Code Language(this Code code, Languages language)
    {
        return code with { Language = language };
    }

    /// <summary>
    /// Configures whether to display line numbers alongside the code content.
    /// </summary>
    /// <param name="code">The code widget to configure.</param>
    /// <param name="showLineNumbers">Whether to show line numbers (default is true).</param>
    /// <returns>The code widget with the specified line number display setting.</returns>
    /// <remarks>
    /// Line numbers are helpful for referencing specific lines of code and are particularly
    /// useful in educational contexts, code reviews, and technical discussions.
    /// </remarks>
    public static Code ShowLineNumbers(this Code code, bool showLineNumbers = true)
    {
        return code with { ShowLineNumbers = showLineNumbers };
    }

    /// <summary>
    /// Configures whether to display a copy button for copying code content to clipboard.
    /// </summary>
    /// <param name="code">The code widget to configure.</param>
    /// <param name="showCopyButton">Whether to show the copy button (default is true).</param>
    /// <returns>The code widget with the specified copy button display setting.</returns>
    /// <remarks>
    /// The copy button improves user experience by enabling easy copying of code examples
    /// and snippets for use in development projects and learning activities.
    /// </remarks>
    public static Code ShowCopyButton(this Code code, bool showCopyButton = true)
    {
        return code with { ShowCopyButton = showCopyButton };
    }

    /// <summary>
    /// Configures whether to display a border around the code block.
    /// </summary>
    /// <param name="code">The code widget to configure.</param>
    /// <param name="showBorder">Whether to show the border (default is true).</param>
    /// <returns>The code widget with the specified border display setting.</returns>
    /// <remarks>
    /// The border provides visual separation and helps clearly define the code area
    /// within the surrounding content layout for better organization and readability.
    /// </remarks>
    public static Code ShowBorder(this Code code, bool showBorder = true)
    {
        return code with { ShowBorder = showBorder };
    }
}