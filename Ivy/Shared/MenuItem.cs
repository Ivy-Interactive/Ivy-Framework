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

    public static MenuItem Separator() => new(Variant: MenuItemVariant.Separator);
    public static MenuItem Checkbox(string label, object? tag = null) => new(Variant: MenuItemVariant.Checkbox, Label: label, Tag: tag ?? label);
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

public static class MenuItemExtensions
{
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
}
