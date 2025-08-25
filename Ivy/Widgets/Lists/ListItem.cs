using Ivy.Core;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A structured list item widget that provides a consistent layout for displaying information in lists.
/// Supports title, subtitle, icon, badge, and click interactions with optional child content,
/// making it ideal for navigation menus, data lists, and interactive item collections.
/// </summary>
public record ListItem : WidgetBase<ListItem>
{
    /// <summary>
    /// Initializes a new list item with comprehensive configuration options.
    /// Provides a flexible structure for displaying hierarchical information with visual elements
    /// and interactive capabilities suitable for various list-based user interface scenarios.
    /// </summary>
    /// <param name="title">The primary text displayed as the main content of the list item.</param>
    /// <param name="subtitle">Optional secondary text displayed below the title for additional context.</param>
    /// <param name="onClick">Optional click event handler for making the list item interactive.</param>
    /// <param name="icon">Optional icon displayed alongside the title for visual identification.</param>
    /// <param name="badge">Optional badge content (converted to string) displayed as a visual indicator or count.</param>
    /// <param name="tag">Optional tag object for storing additional data associated with the list item (not serialized).</param>
    /// <param name="items">Optional child items or widgets to display within the list item for nested content.</param>
    /// <remarks>
    /// The ListItem provides a standardized structure commonly used in:
    /// <list type="bullet">
    /// <item><description>Navigation menus with icons and descriptions</description></item>
    /// <item><description>Data lists with primary and secondary information</description></item>
    /// <item><description>Interactive item collections with click handlers</description></item>
    /// <item><description>Hierarchical content with nested child items</description></item>
    /// </list>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public ListItem(string? title = null, string? subtitle = null, Func<Event<ListItem>, ValueTask>? onClick = null, Icons? icon = Icons.None, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick;
    }

    // Overload for Action<Event<ListItem>>
    public ListItem(string? title = null, string? subtitle = null, Action<Event<ListItem>>? onClick = null, Icons? icon = Icons.None, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick?.ToValueTask();
    }

    // Overload for simple Action (no parameters)
    public ListItem(string? title = null, string? subtitle = null, Action? onClick = null, Icons? icon = Icons.None, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick == null ? null : (_ => { onClick(); return ValueTask.CompletedTask; });
    }

    /// <summary>Gets the primary text displayed as the main content of the list item.</summary>
    /// <value>The title text, or null if no title is specified.</value>
    [Prop] public string? Title { get; }

    /// <summary>Gets the secondary text displayed below the title for additional context.</summary>
    /// <value>The subtitle text, or null if no subtitle is specified.</value>
    [Prop] public string? Subtitle { get; }

    /// <summary>Gets the icon displayed alongside the title for visual identification.</summary>
    /// <value>The icon from the Icons enumeration, or null if no icon is specified.</value>
    [Prop] public Icons? Icon { get; }

    /// <summary>Gets or sets the badge content displayed as a visual indicator or count.</summary>
    /// <value>The badge text (converted from the original object), or null if no badge is specified.</value>
    /// <remarks>Badge content is automatically converted to string representation for display purposes.</remarks>
    [Prop] public string? Badge { get; set; }

    /// <summary>Gets the tag object for storing additional data associated with the list item.</summary>
    /// <value>The tag object containing custom data, or null if no tag is specified.</value>
    /// <remarks>
    /// The Tag property is not serialized as a widget property and is intended for storing
    /// application-specific data such as IDs, model objects, or other contextual information
    /// that can be accessed in event handlers or other application logic.
    /// </remarks>
    public object? Tag { get; } //not a prop!

    /// <summary>Gets or sets the event handler called when the list item is clicked.</summary>
    /// <value>The click event handler that receives the list item, or null if the item is not interactive.</value>
    /// <remarks>When set, the list item becomes interactive and responds to user click events.</remarks>
    [Event] public Func<Event<ListItem>, ValueTask>? OnClick { get; set; }
}