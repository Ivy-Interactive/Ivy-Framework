using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

namespace Ivy.Shared;

public enum MenuItemVariant
{
    Default,
    Separator,
    Checkbox,
    Radio,
    Group
}

/// <summary>Menu item with hierarchical structure, icons, shortcuts, and selection handling.</summary>
/// <param name="Label">Display text for menu item.</param>
/// <param name="Children">Child menu items for creating hierarchical menus.</param>
/// <param name="Icon">Optional icon to display alongside label.</param>
/// <param name="Tag">Associated data object for identification and event handling.</param>
/// <param name="Variant">Visual and behavioral variant of menu item.</param>
/// <param name="Checked">Whether item is checked (for Checkbox and Radio variants).</param>
/// <param name="Disabled">Whether item is disabled and non-interactive.</param>
/// <param name="Shortcut">Keyboard shortcut text to display.</param>
/// <param name="Expanded">Whether child items are expanded in hierarchical menus.</param>
/// <param name="OnSelect">Event handler called when item is selected.</param>
/// <param name="SearchHints">Tags used for the search functionality.</param>
public record MenuItem(
    string? Label = null,
    MenuItem[]? Children = null,
    Icons? Icon = null,
    object? Tag = null,
    MenuItemVariant Variant = MenuItemVariant.Default,
    bool Checked = false,
    bool Disabled = false,
    string? Shortcut = null,
    bool Expanded = false,
    string? Tooltip = null,
    Action<MenuItem>? OnSelect = null,
    string[]? SearchHints = null)
{

    public static MenuItem Separator() => new(Variant: MenuItemVariant.Separator);

    public static MenuItem Checkbox(string label, object? tag = null) => new(Variant: MenuItemVariant.Checkbox, Label: label, Tag: tag ?? label);

    public static MenuItem Default(string label, object? tag = null)
        => new(Variant: MenuItemVariant.Default, Label: label, Tag: tag ?? label);

    public static MenuItem Default(Icons icon, object? tag = null)
        => new(Variant: MenuItemVariant.Default, Icon: icon, Tag: tag ?? icon.ToString());

    private readonly Action<MenuItem>? _onSelect = OnSelect;
    [System.Text.Json.Serialization.JsonIgnore]
    public Action<MenuItem>? OnSelect
    {
        get => _onSelect;
        init
        {
            _onSelect = value;
        }
    }

    public static MenuItem operator |(MenuItem parent, MenuItem child)
    {
        return parent with
        {
            Children = [.. parent.Children ?? [], child]
        };
    }


}

/// <summary>Extension methods for MenuItem manipulation and fluent configuration.</summary>
public static class MenuItemExtensions
{
    /// <returns>All menu items including nested children in depth-first order.</returns>
    public static IEnumerable<MenuItem> Flatten(this IEnumerable<MenuItem> menuItem)
    {
        foreach (var item in menuItem)
        {
            yield return item;
            if (item.Children is { Length: > 0 })
            {
                foreach (var child in item.Children.Flatten())
                {
                    yield return child;
                }
            }
        }
    }

    /// <returns>Selection handler action, or null if no matching item found.</returns>
    public static Action? GetSelectHandler(this MenuItem[] menuItem, object value)
    {
        foreach (var item in menuItem)
        {
            //depth first search
            var handler = item.Children?.GetSelectHandler(value);
            if (handler != null)
            {
                return handler;
            }

            if (Equals(item.Tag, value) || item.Label == (string?)value)
            {
                if (item.OnSelect == null)
                {
                    return null;
                }
                return () => item.OnSelect(item);
            }
        }
        return null;
    }

    public static MenuItem Disabled(this MenuItem menuItem, bool disabled = true)
    {
        return menuItem with { Disabled = disabled };
    }

    public static MenuItem Checked(this MenuItem menuItem, bool isChecked = true)
    {
        return menuItem with { Checked = isChecked };
    }

    public static MenuItem Shortcut(this MenuItem menuItem, string shortcut)
    {
        return menuItem with { Shortcut = shortcut };
    }

    public static MenuItem Icon(this MenuItem menuItem, Icons icon)
    {
        return menuItem with { Icon = icon };
    }

    public static MenuItem Tag(this MenuItem menuItem, object tag)
    {
        return menuItem with { Tag = tag };
    }

    public static MenuItem Label(this MenuItem menuItem, string label)
    {
        return menuItem with { Label = label };
    }

    public static MenuItem Tooltip(this MenuItem menuItem, string tooltip)
    {
        return menuItem with { Tooltip = tooltip };
    }

    public static MenuItem Expanded(this MenuItem menuItem, bool expanded = true)
    {
        return menuItem with { Expanded = expanded };
    }

    public static MenuItem Children(this MenuItem menuItem, params MenuItem[] children)
    {
        return menuItem with { Children = children };
    }

    public static MenuItem HandleSelect(this MenuItem menuItem, Action<MenuItem> onSelect)
    {
        return menuItem with { OnSelect = onSelect };
    }

    public static MenuItem HandleSelect(this MenuItem menuItem, Action onSelect)
    {
        return menuItem with { OnSelect = _ => onSelect() };
    }

    public static MenuItem SearchHints(this MenuItem menuItem, string[] searchHints)
    {
        return menuItem with { SearchHints = searchHints };
    }
}
