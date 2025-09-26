using Ivy.Core;

namespace Ivy.Widgets.Internal;

/// <summary>
/// Represents a sidebar news widget that displays RSS or news feed content in a sidebar layout.
/// </summary>
public record SidebarNews : WidgetBase<SidebarNews>
{
    /// <summary>
    /// Initializes a new instance of the SidebarNews class with the specified feed URL.
    /// </summary>
    /// <param name="feedUrl">The URL of the RSS feed or news source to display in the sidebar.</param>
    public SidebarNews(string feedUrl) : base()
    {
        FeedUrl = feedUrl;
    }

    /// <summary>
    /// Gets or sets the URL of the RSS feed or news source to display in the sidebar.
    /// </summary>
    [Prop] public string? FeedUrl { get; set; }
}