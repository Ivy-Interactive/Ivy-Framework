using Rustino.NET;

namespace Ivy.Desktop;

public class DesktopMenu
{
    private readonly List<Action<RustinoMenu>> _actions = new();

    public DesktopMenu AddItem(string id, string label, string? accelerator = null, bool enabled = true)
    {
        _actions.Add(m => m.AddItem(id, label, accelerator, enabled));
        return this;
    }

    public DesktopMenu AddCheckItem(string id, string label, bool isChecked = false, bool enabled = true)
    {
        _actions.Add(m => m.AddCheckItem(id, label, isChecked, enabled));
        return this;
    }

    public DesktopMenu AddSeparator()
    {
        _actions.Add(m => m.AddSeparator());
        return this;
    }

    public DesktopMenu AddSubmenu(string label, Action<DesktopMenu> build, bool enabled = true)
    {
        var sub = new DesktopMenu();
        build(sub);
        _actions.Add(m => m.AddSubmenu(label, inner =>
        {
            foreach (var action in sub._actions)
                action(inner);
        }, enabled));
        return this;
    }

    internal RustinoMenu ToRustinoMenu()
    {
        var menu = new RustinoMenu();
        foreach (var action in _actions)
            action(menu);
        return menu;
    }
}
