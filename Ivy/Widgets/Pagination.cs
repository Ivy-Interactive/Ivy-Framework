using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Pagination control allowing users to navigate through pages of content with page numbers and next/previous buttons.</summary>
public record Pagination : WidgetBase<Pagination>
{
    /// <summary>Initializes Pagination with explicit value.</summary>
    /// <param name="page">Initial page, starting from 1.</param>
    /// <param name="numPages">Total number of pages.</param>
    /// <param name="onChange">Event handler called when page changes.</param>
    /// <param name="disabled">Whether input should be disabled initially.</param>
    public Pagination(int? page, int? numPages, Func<Event<Pagination, int>, ValueTask> onChange, bool disabled = false)
    {
        Page = page;
        NumPages = numPages;
        OnChange = onChange;
        Disabled = disabled;
    }

    /// <summary>Initializes Pagination with explicit value.</summary>
    /// <param name="page">Initial page, starting from 1.</param>
    /// <param name="numPages">Total number of pages.</param>
    /// <param name="onChange">Event handler called when page changes.</param>
    /// <param name="disabled">Whether input should be disabled initially.</param>
    public Pagination(int? page, int? numPages, Action<Event<Pagination, int>> onChange, bool disabled = false)
    {
        Page = page;
        NumPages = numPages;
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
        Disabled = disabled;
    }

    /// <summary>Page value starting from 1. When null, no page highlighted.</summary>
    [Prop] public int? Page { get; set; }

    /// <summary>Total number of pages to display. When null, only previous and next buttons displayed.</summary>
    [Prop] public int? NumPages { get; set; }

    /// <summary>Number of siblings controlling page numbers displayed adjacent to current page. Defaults to 1.</summary>
    [Prop] public int? Siblings { get; set; }

    /// <summary>Number of boundaries controlling page numbers displayed at beginning and end of pagination. Defaults to 1.</summary>
    [Prop] public int? Boundaries { get; set; }

    /// <summary>Whether pagination widget is disabled and cannot be interacted with.</summary>
    [Prop] public bool Disabled { get; set; } = false;

    /// <summary>Gets the event handler called when the page value changes.</summary>
    [Event] public Func<Event<Pagination, int>, ValueTask>? OnChange { get; }
}

/// <summary>Extension methods for Pagination widget.</summary>
public static class PaginationExtensions
{
    /// <summary>Sets Siblings property of pagination widget.</summary>
    /// <param name="widget">The pagination widget to configure.</param>
    /// <param name="siblings">Number of siblings to show.</param>
    /// <returns>Pagination instance for method chaining.</returns>
    public static Pagination Siblings(this Pagination widget, int siblings)
    {
        widget.Siblings = siblings;
        return widget;
    }

    /// <summary>Sets Boundaries property of pagination widget.</summary>
    /// <param name="widget">The pagination widget to configure.</param>
    /// <param name="boundaries">Number of boundaries to show.</param>
    /// <returns>Pagination instance for method chaining.</returns>
    public static Pagination Boundaries(this Pagination widget, int boundaries)
    {
        widget.Boundaries = boundaries;
        return widget;
    }

    /// <summary>Sets disabled state of pagination widget. When disabled, page cannot be changed and buttons greyed out.</summary>
    /// <param name="widget">The pagination widget to configure.</param>
    /// <param name="disabled">Whether widget should be disabled.</param>
    /// <returns>Pagination instance for method chaining.</returns>
    public static Pagination Disabled(this Pagination widget, bool disabled)
    {
        widget.Disabled = disabled;
        return widget;
    }
}