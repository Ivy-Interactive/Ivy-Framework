using Ivy.Core;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A container for article content.
/// </summary>
public record Article : WidgetBase<Article>
{
    public Article(params IEnumerable<object> content) : base(content)
    {
    }

    internal Article()
    {
    }

    [Prop] public bool ShowToc { get; set; } = true;

    [Prop] public bool ShowFooter { get; set; } = true;

    [Prop] public InternalLink? Previous { get; set; }

    [Prop] public InternalLink? Next { get; set; }

    [Prop] public string? DocumentSource { get; set; }

    [Prop] public List<ArticleHeading> Headings { get; set; } = [];

    [Prop] public int Gap { get; set; } = 4;

    [Event] public EventHandler<Event<Article, string>>? OnLinkClick { get; set; }
}

public record ArticleHeading(string Id, string Text, int Level);

public static class ArticleExtensions
{
    public static Article ShowToc(this Article article, bool showToc = true) => article with { ShowToc = showToc };

    public static Article ShowFooter(this Article article, bool showFooter = true) => article with { ShowFooter = showFooter };

    public static Article Previous(this Article article, InternalLink? navigateBack) => article with { Previous = navigateBack };

    public static Article Next(this Article article, InternalLink? navigateForward) => article with { Next = navigateForward };

    public static Article DocumentSource(this Article article, string? documentSource) => article with { DocumentSource = documentSource };

    public static Article Gap(this Article article, int gap) => article with { Gap = gap };

    public static Article Headings(this Article article, List<ArticleHeading> headings) => article with { Headings = headings };

    [OverloadResolutionPriority(1)]
    public static Article OnLinkClick(this Article article, Func<Event<Article, string>, ValueTask> onLinkClick) => article with { OnLinkClick = new(onLinkClick) };

    // Overload for Action<Event<Article, string>>
    public static Article OnLinkClick(this Article article, Action<Event<Article, string>> onLinkClick) => article with { OnLinkClick = new(onLinkClick.ToValueTask()) };

    public static Article OnLinkClick(this Article article, Action<string> onLinkClick) => article with { OnLinkClick = new(@event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; }) };
}