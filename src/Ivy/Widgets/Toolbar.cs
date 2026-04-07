// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A container for organizing action buttons and controls with grouping and separators.
/// </summary>
[ChildType(typeof(MenuItem))]
public record Toolbar : WidgetBase<Toolbar>
{
    public Toolbar(params MenuItem[] items)
    {
        Items = items;
    }

    internal Toolbar() { }

    [Prop] public MenuItem[] Items { get; set; } = [];
    [Prop] public bool Disabled { get; set; }

    public static Toolbar operator |(Toolbar widget, MenuItem item)
    {
        return widget with { Items = widget.Items.Append(item).ToArray() };
    }
}

public static class ToolbarExtensions
{
    public static Toolbar Items(this Toolbar toolbar, params MenuItem[] items)
    {
        return toolbar with { Items = items };
    }

    public static Toolbar Disabled(this Toolbar toolbar, bool disabled = true)
    {
        return toolbar with { Disabled = disabled };
    }
}
