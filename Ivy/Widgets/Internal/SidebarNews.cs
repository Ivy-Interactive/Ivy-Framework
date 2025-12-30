using Ivy.Core;

namespace Ivy.Widgets.Internal;

public record SidebarNews : WidgetBase<SidebarNews>
{
    public SidebarNews(string feedUrl)
    {
        FeedUrl = feedUrl;
    }

    internal SidebarNews() { }

    [Prop] public string? FeedUrl { get; set; }
}