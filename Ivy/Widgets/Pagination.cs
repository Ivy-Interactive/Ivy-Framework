using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A pagination control that allows users to navigate through pages of content.
/// It displays page numbers, next/previous buttons, and handles user interactions
/// to change the current page. The control can be customized with properties such
/// as the current page, total number of pages, and the number of sibling and boundary
/// pages to display. It also supports disabling the control to prevent user interaction.
/// </summary>
public record Pagination : WidgetBase<Pagination>
{
    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="page">The initial page, starting from 1.</param>
    /// <param name="numPages">The total number of pages</param>
    /// <param name="onChange">Event handler called when the page changes.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public Pagination(int? page, int? numPages, Func<Event<Pagination, int>, ValueTask> onChange, bool disabled = false)
    {
        Page = page;
        NumPages = numPages;
        OnChange = onChange;
        Disabled = disabled;
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="page">The initial page, starting from 1.</param>
    /// <param name="numPages">The total number of pages</param>
    /// <param name="onChange">Event handler called when the page changes.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public Pagination(int? page, int? numPages, Action<Event<Pagination, int>> onChange, bool disabled = false)
    {
        Page = page;
        NumPages = numPages;
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
        Disabled = disabled;
    }

    /// <summary>
    /// Gets or sets the page value, starting from 1.
    /// 
    /// When null, no page will be highlighted.
    /// </summary>
    [Prop] public int? Page { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages to display.
    /// 
    /// When null, only the previous and next buttons will be displayed.
    /// </summary>
    [Prop] public int? NumPages { get; set; }

    /// <summary>
    /// Gets or sets the number of siblings to use. The siblings control how many
    /// page numbers are displayed adjacent to the current page. Defaults to 1.
    /// </summary>
    [Prop] public int? Siblings { get; set; }

    /// <summary>
    /// Gets or sets the number of boundaries to use. The boundaries control how
    /// many page numbers are displayed at the beginning and end of the pagination
    /// control. Defaults to 1.
    /// </summary>
    [Prop] public int? Boundaries { get; set; }

    /// <summary>
    /// Gets or sets whether the pagination widget is disabled and cannot
    /// be interacted with. This property controls the interactive state
    /// of the widget, preventing users from changing the current page
    /// when true.
    /// </summary>
    [Prop] public bool Disabled { get; set; } = false;

    /// <summary>Gets the event handler called when the page value changes.</summary>
    [Event] public Func<Event<Pagination, int>, ValueTask>? OnChange { get; }
}

/// <summary>
/// Provides extension methods for the Pagination widget that enable a fluent API
/// for configuring pagination behavior and appearance. These methods allow you
/// to easily set properties and configure the widget for optimal presentation
/// and functionality.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Sets the number of siblings of the pagination widget.
    /// 
    /// The siblings control how many page numbers are displayed
    /// adjacent to the current page.
    /// </summary>
    /// <param name="widget">The Pagination widget to configure.</param>
    /// <param name="siblings">The number of siblings. Defaults to 1.</param>
    /// <returns>The Pagination instance for method chaining.</returns>
    public static Pagination Siblings(this Pagination widget, int siblings)
    {
        widget.Siblings = siblings;
        return widget;
    }

    /// <summary>
    /// Sets the number of boundaries of the pagination widget.
    /// 
    /// The boundaries control how many page numbers are displayed
    /// at the beginning and end of the pagination control.
    /// </summary>
    /// <param name="widget">The Pagination widget to configure.</param>
    /// <param name="boundaries">The number of boundaries. Defaults to 1.</param>
    /// <returns>The Pagination instance for method chaining.</returns>
    public static Pagination Boundaries(this Pagination widget, int boundaries)
    {
        widget.Boundaries = boundaries;
        return widget;
    }

    /// <summary>
    /// Sets the disabled state of the pagination widget.
    /// This method allows you to control whether the widget can be
    /// interacted with, enabling you to lock the pagination in its
    /// current state based on application logic or user permissions.
    /// 
    /// When disabled, the page cannot be changed and buttons will be
    /// greyed out, maintaining its current state until re-enabled.
    /// </summary>
    /// <param name="widget">The Pagination widget to configure.</param>
    /// <param name="disabled">Whether the widget should be disabled (true) or enabled (false).</param>
    /// <returns>The Pagination instance for method chaining.</returns>
    public static Pagination Disabled(this Pagination widget, bool disabled)
    {
        widget.Disabled = disabled;
        return widget;
    }
}