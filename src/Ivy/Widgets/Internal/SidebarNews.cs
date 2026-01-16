using Ivy.Core;

namespace Ivy.Widgets.Internal;

/// <summary>
/// Displays a news feed, typically in a sidebar.
/// </summary>
public record SidebarNews : WidgetBase<SidebarNews>
{
    public SidebarNews(string feedUrl)
    {
        FeedUrl = feedUrl;
    }

    internal SidebarNews() { }

    [Prop] public string? FeedUrl { get; set; }
}