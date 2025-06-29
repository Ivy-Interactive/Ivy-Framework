using Ivy.Core;

namespace Ivy.Widgets.Internal;

public record SidebarNews : WidgetBase<SidebarNews>
{
    public SidebarNews(string feedUrl) : base()
    {
        FeedUrl = feedUrl;
    }

    [Prop] public string? FeedUrl { get; set; }
}