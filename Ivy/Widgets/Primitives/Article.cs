using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A structured document widget designed for displaying long-form content with navigation and organizational features.
/// Provides comprehensive document presentation capabilities including table of contents, navigation links,
/// footer information, and interactive link handling for documentation, articles, and content-rich applications.
/// </summary>
public record Article : WidgetBase<Article>
{
    /// <summary>
    /// Initializes a new article with the specified content elements.
    /// Creates a structured document container that can hold various types of content including text,
    /// headings, images, and other widgets while providing document-specific navigation and organization features.
    /// </summary>
    /// <param name="content">The content elements to include in the article. Can be text, headings, paragraphs, images, or other widgets.</param>
    /// <remarks>
    /// The Article widget is designed for long-form content presentation and includes features commonly found in documentation systems:
    /// <list type="bullet">
    /// <item><description><strong>Table of Contents:</strong> Automatically generated from heading elements</description></item>
    /// <item><description><strong>Navigation:</strong> Previous/Next links for sequential content browsing</description></item>
    /// <item><description><strong>Footer:</strong> Document metadata and additional information display</description></item>
    /// <item><description><strong>Link Handling:</strong> Custom handling for internal and external links</description></item>
    /// </list>
    /// </remarks>
    public Article(params IEnumerable<object> content) : base(content)
    {
    }

    /// <summary>Gets or sets whether to display the table of contents for the article.</summary>
    /// <value>true to show the table of contents (default); false to hide it.</value>
    /// <remarks>
    /// The table of contents is automatically generated from heading elements within the article content.
    /// It provides quick navigation to different sections of the document and improves user experience for long articles.
    /// </remarks>
    [Prop] public bool ShowToc { get; set; } = true;

    /// <summary>Gets or sets whether to display the footer section of the article.</summary>
    /// <value>true to show the footer (default); false to hide it.</value>
    /// <remarks>
    /// The footer typically contains document metadata, publication information, navigation links,
    /// and other supplementary information relevant to the article content.
    /// </remarks>
    [Prop] public bool ShowFooter { get; set; } = true;

    /// <summary>Gets or sets the link to the previous article or document in a sequence.</summary>
    /// <value>An InternalLink to the previous document, or null if there is no previous document.</value>
    /// <remarks>
    /// Used for sequential navigation in documentation systems, tutorials, or article series.
    /// Enables users to navigate backward through related content in a logical order.
    /// </remarks>
    [Prop] public InternalLink? Previous { get; set; }

    /// <summary>Gets or sets the link to the next article or document in a sequence.</summary>
    /// <value>An InternalLink to the next document, or null if there is no next document.</value>
    /// <remarks>
    /// Used for sequential navigation in documentation systems, tutorials, or article series.
    /// Enables users to navigate forward through related content in a logical order.
    /// </remarks>
    [Prop] public InternalLink? Next { get; set; }

    /// <summary>Gets or sets the source path or identifier for the document content.</summary>
    /// <value>The source path, URL, or identifier for the document, or null if not specified.</value>
    /// <remarks>
    /// Can be used to track the original source of the article content, enable editing links,
    /// or provide attribution information. Commonly used in documentation systems to link back to source files.
    /// </remarks>
    [Prop] public string? DocumentSource { get; set; }

    /// <summary>Gets or sets the event handler called when a link within the article is clicked.</summary>
    /// <value>The link click event handler that receives the article and the clicked link URL, or null if no handler is set.</value>
    /// <remarks>
    /// Enables custom handling of link clicks within the article content, such as internal navigation,
    /// external link processing, or analytics tracking. The event provides both the article context and the clicked link.
    /// </remarks>
    [Event] public Func<Event<Article, string>, ValueTask>? OnLinkClick { get; set; }
}

/// <summary>
/// Provides extension methods for configuring article widgets with fluent syntax.
/// Enables convenient configuration of article display options, navigation links, and event handling
/// through method chaining for improved readability and ease of use.
/// </summary>
public static class ArticleExtensions
{
    /// <summary>
    /// Configures whether to display the table of contents for the article.
    /// </summary>
    /// <param name="article">The article to configure.</param>
    /// <param name="showToc">Whether to show the table of contents (default is true).</param>
    /// <returns>The article with the specified table of contents visibility setting.</returns>
    public static Article ShowToc(this Article article, bool showToc = true) => article with { ShowToc = showToc };

    /// <summary>
    /// Configures whether to display the footer section of the article.
    /// </summary>
    /// <param name="article">The article to configure.</param>
    /// <param name="showFooter">Whether to show the footer (default is true).</param>
    /// <returns>The article with the specified footer visibility setting.</returns>
    public static Article ShowFooter(this Article article, bool showFooter = true) => article with { ShowFooter = showFooter };

    /// <summary>
    /// Sets the previous navigation link for sequential article browsing.
    /// </summary>
    /// <param name="article">The article to configure.</param>
    /// <param name="navigateBack">The internal link to the previous article, or null if there is no previous article.</param>
    /// <returns>The article with the specified previous navigation link.</returns>
    public static Article Previous(this Article article, InternalLink? navigateBack) => article with { Previous = navigateBack };

    /// <summary>
    /// Sets the next navigation link for sequential article browsing.
    /// </summary>
    /// <param name="article">The article to configure.</param>
    /// <param name="navigateForward">The internal link to the next article, or null if there is no next article.</param>
    /// <returns>The article with the specified next navigation link.</returns>
    public static Article Next(this Article article, InternalLink? navigateForward) => article with { Next = navigateForward };

    /// <summary>
    /// Sets the document source path or identifier for the article.
    /// </summary>
    /// <param name="article">The article to configure.</param>
    /// <param name="documentSource">The source path, URL, or identifier for the document content.</param>
    /// <returns>The article with the specified document source.</returns>
    public static Article DocumentSource(this Article article, string? documentSource) => article with { DocumentSource = documentSource };

    /// <summary>
    /// Sets the event handler for link clicks within the article content.
    /// Provides full event context including both the article and the clicked link URL.
    /// </summary>
    /// <param name="article">The article to configure.</param>
    /// <param name="onLinkClick">The event handler that receives the full event context with article and link URL.</param>
    /// <returns>The article with the specified link click event handler.</returns>
    [OverloadResolutionPriority(1)]
    public static Article HandleLinkClick(this Article article, Func<Event<Article, string>, ValueTask> onLinkClick) => article with { OnLinkClick = onLinkClick };

    // Overload for Action<Event<Article, string>>
    public static Article HandleLinkClick(this Article article, Action<Event<Article, string>> onLinkClick) => article with { OnLinkClick = onLinkClick.ToValueTask() };

    /// <summary>
    /// Sets a simplified event handler for link clicks within the article content.
    /// Convenience method that provides only the clicked link URL without the full event context.
    /// </summary>
    /// <param name="article">The article to configure.</param>
    /// <param name="onLinkClick">The simplified event handler that receives only the clicked link URL.</param>
    /// <returns>The article with the specified simplified link click event handler.</returns>
    public static Article HandleLinkClick(this Article article, Action<string> onLinkClick) => article with { OnLinkClick = @event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; } };
}