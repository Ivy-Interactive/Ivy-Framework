using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

namespace Ivy.Shared;

/// <summary>
/// Specifies the visual and behavioral variant of a menu item.
/// </summary>
public enum MenuItemVariant
{
    /// <summary>Standard clickable menu item.</summary>
    Default,
    /// <summary>Visual separator line between menu sections.</summary>
    Separator,
    /// <summary>Checkable menu item with toggle state.</summary>
    Checkbox,
    /// <summary>Radio button menu item for exclusive selection.</summary>
    Radio,
    /// <summary>Group header for organizing related menu items.</summary>
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
    Action<MenuItem>? OnSelect = null)
{

    /// <summary>Creates a separator menu item for visual grouping.</summary>
    public static MenuItem Separator() => new(Variant: MenuItemVariant.Separator);

    /// <summary>Creates checkbox menu item with toggle functionality.</summary>
    /// <param name="label">Display text for checkbox item.</param>
    /// <param name="tag">Optional tag for identification, defaults to label if null.</param>
    public static MenuItem Checkbox(string label, object? tag = null) => new(Variant: MenuItemVariant.Checkbox, Label: label, Tag: tag ?? label);

    /// <summary>Creates standard clickable menu item.</summary>
    /// <param name="label">Display text for menu item.</param>
    /// <param name="tag">Optional tag for identification, defaults to label if null.</param>
    public static MenuItem Default(string label, object? tag = null)
        => new(Variant: MenuItemVariant.Default, Label: label, Tag: tag ?? label);

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
}

/// <summary>Extension methods for MenuItem manipulation and fluent configuration.</summary>
public static class MenuItemExtensions
{
    /// <summary>Flattens hierarchical menu structure into flat enumerable sequence.</summary>
    /// <param name="menuItem">Menu items to flatten.</param>
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

    /// <summary>Finds and returns selection handler for menu item matching specified value.</summary>
    /// <param name="menuItem">Menu items to search through.</param>
    /// <param name="value">Tag or label value to match against.</param>
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

            if (item.Tag == value || item.Label == (string?)value)
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

    /// <summary>Sets the disabled state of the menu item.</summary>
    public static MenuItem Disabled(this MenuItem menuItem, bool disabled = true)
    {
        return menuItem with { Disabled = disabled };
    }

    /// <summary>Sets the checked state for checkbox and radio menu items.</summary>
    public static MenuItem Checked(this MenuItem menuItem, bool isChecked = true)
    {
        return menuItem with { Checked = isChecked };
    }

    /// <summary>Sets the keyboard shortcut text to display.</summary>
    public static MenuItem Shortcut(this MenuItem menuItem, string shortcut)
    {
        return menuItem with { Shortcut = shortcut };
    }

    /// <summary>Sets the icon to display alongside the menu item.</summary>
    public static MenuItem Icon(this MenuItem menuItem, Icons icon)
    {
        return menuItem with { Icon = icon };
    }

    /// <summary>Sets the tag object for identification and event handling.</summary>
    public static MenuItem Tag(this MenuItem menuItem, object tag)
    {
        return menuItem with { Tag = tag };
    }

    /// <summary>Sets the display label text.</summary>
    public static MenuItem Label(this MenuItem menuItem, string label)
    {
        return menuItem with { Label = label };
    }

    /// <summary>Sets whether child menu items are expanded in hierarchical menus.</summary>
    public static MenuItem Expanded(this MenuItem menuItem, bool expanded = true)
    {
        return menuItem with { Expanded = expanded };
    }

    /// <summary>Sets the child menu items for creating hierarchical menus.</summary>
    public static MenuItem Children(this MenuItem menuItem, params MenuItem[] children)
    {
        return menuItem with { Children = children };
    }

    /// <summary>Sets the selection handler that receives the menu item when selected.</summary>
    public static MenuItem HandleSelect(this MenuItem menuItem, Action<MenuItem> onSelect)
    {
        return menuItem with { OnSelect = onSelect };
    }

    /// <summary>Sets the selection handler with a simple action callback.</summary>
    public static MenuItem HandleSelect(this MenuItem menuItem, Action onSelect)
    {
        return menuItem with { OnSelect = _ => onSelect() };
    }
}
