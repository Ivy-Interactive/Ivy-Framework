using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Pagination control allowing users to navigate through pages of content with page numbers and next/previous buttons.</summary>
public record Pagination : WidgetBase<Pagination>
{
    /// <param name="page">Initial page, starting from 1.</param>
    public Pagination(int? page, int? numPages, Func<Event<Pagination, int>, ValueTask> onChange, bool disabled = false)
    {
        Page = page;
        NumPages = numPages;
        OnChange = onChange;
        Disabled = disabled;
    }

    /// <param name="page">Initial page, starting from 1.</param>
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
    public static Pagination Siblings(this Pagination widget, int siblings)
    {
        widget.Siblings = siblings;
        return widget;
    }

    public static Pagination Boundaries(this Pagination widget, int boundaries)
    {
        widget.Boundaries = boundaries;
        return widget;
    }

    public static Pagination Disabled(this Pagination widget, bool disabled)
    {
        widget.Disabled = disabled;
        return widget;
    }
}