// ReSharper disable once CheckNamespace
namespace Ivy;

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
    string? Badge = null,
    bool Expanded = false,
    string? Tooltip = null,
    EventHandler<MenuItem>? OnSelect = null,
    string[]? SearchHints = null,
    string? Path = null,
    Colors? Color = null)
{

    public static MenuItem Separator() => new(Variant: MenuItemVariant.Separator);

    public static MenuItem Checkbox(string label, object? tag = null) => new(Variant: MenuItemVariant.Checkbox, Label: label, Tag: tag ?? label);

    public static MenuItem Default(string label, object? tag = null)
        => new(Variant: MenuItemVariant.Default, Label: label, Tag: tag ?? label);

    public static MenuItem Default(Icons icon, object? tag = null)
        => new(Variant: MenuItemVariant.Default, Icon: icon, Tag: tag ?? icon.ToString());

    private readonly EventHandler<MenuItem>? _onSelect = OnSelect;
    [System.Text.Json.Serialization.JsonIgnore]
    public EventHandler<MenuItem>? OnSelect
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

    /// <summary>
    /// Flattens the menu tree and yields leaf items along with their folder path (parent labels).
    /// Path is the concatenated labels of ancestors, e.g. "Widgets / Primitives" for an item under Primitives within Widgets.
    /// </summary>
    public static IEnumerable<(MenuItem Item, string Path)> FlattenWithPath(this IEnumerable<MenuItem> menuItems, string parentPath = "")
    {
        foreach (var item in menuItems)
        {
            var currentPath = string.IsNullOrEmpty(parentPath)
                ? (item.Label ?? "")
                : $"{parentPath} / {item.Label}";

            if (item.Children is { Length: > 0 })
            {
                foreach (var (child, path) in item.Children.FlattenWithPath(currentPath))
                {
                    yield return (child, path);
                }
            }
            else
            {
                yield return (item, parentPath);
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

            if (Equals(item.Tag, value) || item.Label == (string?)value)
            {
                if (item.OnSelect == null)
                {
                    return null;
                }
                return () => item.OnSelect.Invoke(item);
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

    public static MenuItem Badge(this MenuItem menuItem, string? badge)
    {
        return menuItem with { Badge = badge };
    }

    public static MenuItem Expanded(this MenuItem menuItem, bool expanded = true)
    {
        return menuItem with { Expanded = expanded };
    }

    public static MenuItem Children(this MenuItem menuItem, params MenuItem[] children)
    {
        return menuItem with { Children = children };
    }

    public static MenuItem OnSelect(this MenuItem menuItem, Action<MenuItem> onSelect)
    {
        return menuItem with { OnSelect = new Func<MenuItem, ValueTask>(m => { onSelect(m); return ValueTask.CompletedTask; }) };
    }

    public static MenuItem OnSelect(this MenuItem menuItem, Action onSelect)
    {
        return menuItem with { OnSelect = new Func<MenuItem, ValueTask>(_ => { onSelect(); return ValueTask.CompletedTask; }) };
    }

    public static MenuItem SearchHints(this MenuItem menuItem, string[] searchHints)
    {
        return menuItem with { SearchHints = searchHints };
    }

    public static MenuItem Path(this MenuItem menuItem, string? path)
    {
        return menuItem with { Path = path };
    }

    public static MenuItem Destructive(this MenuItem menuItem)
    {
        return menuItem with { Color = Colors.Destructive };
    }

    public static MenuItem Primary(this MenuItem menuItem)
    {
        return menuItem with { Color = Colors.Primary };
    }

    public static MenuItem Secondary(this MenuItem menuItem)
    {
        return menuItem with { Color = Colors.Secondary };
    }

    public static MenuItem Success(this MenuItem menuItem)
    {
        return menuItem with { Color = Colors.Success };
    }

    public static MenuItem Warning(this MenuItem menuItem)
    {
        return menuItem with { Color = Colors.Warning };
    }

    public static MenuItem Info(this MenuItem menuItem)
    {
        return menuItem with { Color = Colors.Info };
    }

    public static MenuItem Color(this MenuItem menuItem, Colors color)
    {
        return menuItem with { Color = color };
    }
}
