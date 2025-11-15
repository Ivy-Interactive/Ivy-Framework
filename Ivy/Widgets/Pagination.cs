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

    [Prop] public int? Page { get; set; }

    [Prop] public int? NumPages { get; set; }

    [Prop] public int? Siblings { get; set; }

    [Prop] public int? Boundaries { get; set; }

    [Prop] public bool Disabled { get; set; } = false;

    [Event] public Func<Event<Pagination, int>, ValueTask>? OnChange { get; }
}

public static class PaginationExtensions
{
    /// <param name="widget">The pagination widget to configure.</param>
    /// <param name="siblings">Number of siblings to show.</param>
    /// <returns>Pagination instance for method chaining.</returns>
    public static Pagination Siblings(this Pagination widget, int siblings)
    {
        widget.Siblings = siblings;
        return widget;
    }

    /// <param name="widget">The pagination widget to configure.</param>
    /// <param name="boundaries">Number of boundaries to show.</param>
    /// <returns>Pagination instance for method chaining.</returns>
    public static Pagination Boundaries(this Pagination widget, int boundaries)
    {
        widget.Boundaries = boundaries;
        return widget;
    }

    /// <param name="widget">The pagination widget to configure.</param>
    /// <param name="disabled">Whether widget should be disabled.</param>
    /// <returns>Pagination instance for method chaining.</returns>
    public static Pagination Disabled(this Pagination widget, bool disabled)
    {
        widget.Disabled = disabled;
        return widget;
    }
}