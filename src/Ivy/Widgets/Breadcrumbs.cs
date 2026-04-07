using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a single breadcrumb item with label and optional click action.
/// </summary>
public record BreadcrumbItem(string Label, [property: JsonIgnore] Action? OnClick = null, Icons? Icon = null, string? Tooltip = null, bool Disabled = false)
{
    public bool HasOnClick => OnClick != null;
}

/// <summary>
/// A secondary navigation component showing hierarchical location with clickable trail.
/// </summary>
public record Breadcrumbs : WidgetBase<Breadcrumbs>
{
    [OverloadResolutionPriority(1)]
    public Breadcrumbs(params IEnumerable<BreadcrumbItem> items)
    {
        Items = items.ToArray();
        OnItemClick = DefaultItemClickHandler();
    }

    public Breadcrumbs(params BreadcrumbItem[] items)
    {
        Items = items;
        OnItemClick = DefaultItemClickHandler();
    }

    internal Breadcrumbs()
    {
    }

    [Prop] public BreadcrumbItem[] Items { get; set; } = [];

    [Prop] public string Separator { get; set; } = "/";

    [Prop] public bool Disabled { get; set; }

    [Event] public EventHandler<Event<Breadcrumbs, int>>? OnItemClick { get; set; }

    private static EventHandler<Event<Breadcrumbs, int>> DefaultItemClickHandler()
    {
        return new(@evt =>
        {
            var index = @evt.Value;
            if (index >= 0 && index < @evt.Sender.Items.Length)
            {
                @evt.Sender.Items[index].OnClick?.Invoke();
            }
            return ValueTask.CompletedTask;
        });
    }
}

public static class BreadcrumbsExtensions
{
    public static Breadcrumbs Separator(this Breadcrumbs breadcrumbs, string separator) => breadcrumbs with { Separator = separator };

    public static Breadcrumbs Disabled(this Breadcrumbs breadcrumbs, bool disabled = true) => breadcrumbs with { Disabled = disabled };
}
