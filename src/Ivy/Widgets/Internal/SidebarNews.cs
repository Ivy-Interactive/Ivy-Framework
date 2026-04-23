namespace Ivy.Widgets.Internal;

/// <summary>
/// Displays a news feed, typically in a sidebar.
/// </summary>
public record SidebarNews : WidgetBase<SidebarNews>
{
    public SidebarNews(SidebarNewsArticle[] articles)
    {
        Articles = articles;
    }

    internal SidebarNews() { }

    [Prop] public SidebarNewsArticle[]? Articles { get; set; }
}

public record SidebarNewsArticle(string Id, string Href, string Title, string Summary, string Image);
