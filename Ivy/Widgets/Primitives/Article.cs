using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Structured document widget for long-form content with navigation, table of contents, and interactive link handling.</summary>
public record Article : WidgetBase<Article>
{
    /// <summary>Initializes Article with specified content elements.</summary>
    /// <param name="content">Content elements to include in article. Can be text, headings, paragraphs, images, or other widgets.</param>
    public Article(params IEnumerable<object> content) : base(content)
    {
    }

    /// <summary>Whether to display table of contents automatically generated from heading elements. Default is true.</summary>
    [Prop] public bool ShowToc { get; set; } = true;

    /// <summary>Whether to display footer section containing metadata and navigation links. Default is true.</summary>
    [Prop] public bool ShowFooter { get; set; } = true;

    /// <summary>Link to previous article or document in sequence for sequential navigation.</summary>
    [Prop] public InternalLink? Previous { get; set; }

    /// <summary>Link to next article or document in sequence for sequential navigation.</summary>
    [Prop] public InternalLink? Next { get; set; }

    /// <summary>Source path or identifier for document content.</summary>
    [Prop] public string? DocumentSource { get; set; }

    /// <summary>Event handler called when link within article is clicked.</summary>
    [Event] public Func<Event<Article, string>, ValueTask>? OnLinkClick { get; set; }
}

/// <summary>Extension methods for configuring article widgets with fluent syntax.</summary>
public static class ArticleExtensions
{
    /// <summary>Configures whether to display table of contents for article.</summary>
    /// <param name="article">Article to configure.</param>
    /// <param name="showToc">Whether to show table of contents (default is true).</param>
    /// <returns>Article with specified table of contents visibility setting.</returns>
    public static Article ShowToc(this Article article, bool showToc = true) => article with { ShowToc = showToc };

    /// <summary>Configures whether to display footer section of article.</summary>
    /// <param name="article">Article to configure.</param>
    /// <param name="showFooter">Whether to show footer (default is true).</param>
    /// <returns>Article with specified footer visibility setting.</returns>
    public static Article ShowFooter(this Article article, bool showFooter = true) => article with { ShowFooter = showFooter };

    /// <summary>Sets previous navigation link for sequential article browsing.</summary>
    /// <param name="article">Article to configure.</param>
    /// <param name="navigateBack">Internal link to previous article, or null if none.</param>
    /// <returns>Article with specified previous navigation link.</returns>
    public static Article Previous(this Article article, InternalLink? navigateBack) => article with { Previous = navigateBack };

    /// <summary>Sets next navigation link for sequential article browsing.</summary>
    /// <param name="article">Article to configure.</param>
    /// <param name="navigateForward">Internal link to next article, or null if none.</param>
    /// <returns>Article with specified next navigation link.</returns>
    public static Article Next(this Article article, InternalLink? navigateForward) => article with { Next = navigateForward };

    /// <summary>Sets document source path or identifier for article.</summary>
    /// <param name="article">Article to configure.</param>
    /// <param name="documentSource">Source path, URL, or identifier for document content.</param>
    /// <returns>Article with specified document source.</returns>
    public static Article DocumentSource(this Article article, string? documentSource) => article with { DocumentSource = documentSource };

    /// <summary>Sets event handler for link clicks within article content.</summary>
    /// <param name="article">Article to configure.</param>
    /// <param name="onLinkClick">Event handler receiving full event context with article and link URL.</param>
    /// <returns>Article with specified link click event handler.</returns>
    [OverloadResolutionPriority(1)]
    public static Article HandleLinkClick(this Article article, Func<Event<Article, string>, ValueTask> onLinkClick) => article with { OnLinkClick = onLinkClick };

    // Overload for Action<Event<Article, string>>
    public static Article HandleLinkClick(this Article article, Action<Event<Article, string>> onLinkClick) => article with { OnLinkClick = onLinkClick.ToValueTask() };

    /// <summary>Sets simplified event handler for link clicks providing only clicked link URL.</summary>
    /// <param name="article">Article to configure.</param>
    /// <param name="onLinkClick">Simplified event handler receiving only clicked link URL.</param>
    /// <returns>Article with specified simplified link click event handler.</returns>
    public static Article HandleLinkClick(this Article article, Action<string> onLinkClick) => article with { OnLinkClick = @event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; } };
}