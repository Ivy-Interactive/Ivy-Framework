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
    public static Article ShowToc(this Article article, bool showToc = true) => article with { ShowToc = showToc };

    public static Article ShowFooter(this Article article, bool showFooter = true) => article with { ShowFooter = showFooter };

    public static Article Previous(this Article article, InternalLink? navigateBack) => article with { Previous = navigateBack };

    public static Article Next(this Article article, InternalLink? navigateForward) => article with { Next = navigateForward };

    public static Article DocumentSource(this Article article, string? documentSource) => article with { DocumentSource = documentSource };

    [OverloadResolutionPriority(1)]
    public static Article HandleLinkClick(this Article article, Func<Event<Article, string>, ValueTask> onLinkClick) => article with { OnLinkClick = onLinkClick };

    // Overload for Action<Event<Article, string>>
    public static Article HandleLinkClick(this Article article, Action<Event<Article, string>> onLinkClick) => article with { OnLinkClick = onLinkClick.ToValueTask() };

    public static Article HandleLinkClick(this Article article, Action<string> onLinkClick) => article with { OnLinkClick = @event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; } };
}