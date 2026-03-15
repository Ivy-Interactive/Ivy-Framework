using System.Threading.Tasks;
using Ivy.Core;

namespace Ivy;

public class TreeRowActionClickEventArgs
{
    /// <summary> Tag of the tree item where the action was clicked. </summary>
    public object? ItemValue { get; set; }

    /// <summary> Tag of the menu item (action) that was clicked. </summary>
    public object? ActionTag { get; set; }
}

public record Tree : WidgetBase<Tree>
{
    public Tree(params MenuItem[] items)
    {
        Items = items;
    }

    public Tree(IEnumerable<MenuItem> items)
    {
        Items = items.ToArray();
    }

    internal Tree()
    {
    }

    [Prop] public MenuItem[] Items { get; set; } = [];

    [Prop] public MenuItem[]? RowActions { get; set; }

    [Event] public EventHandler<Event<Tree, object>>? OnSelect { get; set; }

    [Event] public EventHandler<Event<Tree, TreeRowActionClickEventArgs>>? OnRowAction { get; set; }

    public static Func<Event<Tree, object>, ValueTask> DefaultSelectHandler()
    {
        return (@evt) =>
        {
            @evt.Sender.Items.GetSelectHandler(@evt.Value)?.Invoke();
            return ValueTask.CompletedTask;
        };
    }

    public static Tree operator |(Tree tree, MenuItem item)
    {
        return tree with { Items = [.. tree.Items, item] };
    }
}

public static class TreeWidgetExtensions
{
    public static Tree OnRowAction(this Tree tree, Func<Event<Tree, TreeRowActionClickEventArgs>, ValueTask> handler)
        => tree with { OnRowAction = new(handler) };

    public static Tree OnRowAction(this Tree tree, Action<Event<Tree, TreeRowActionClickEventArgs>> handler)
        => tree with { OnRowAction = new(handler.ToValueTask()) };

    public static Tree RowActions(this Tree tree, params MenuItem[] actions)
        => tree with { RowActions = actions };
}

public static class TreeExtensions
{
    public static Tree Items(this Tree tree, params MenuItem[] items)
    {
        return tree with { Items = items };
    }

    public static Tree Items(this Tree tree, IEnumerable<MenuItem> items)
    {
        return tree with { Items = items.ToArray() };
    }

    public static Tree OnSelect(this Tree tree, Func<Event<Tree, object>, ValueTask> onSelect)
    {
        return tree with { OnSelect = new(onSelect) };
    }

    public static Tree OnSelect(this Tree tree, Action<Event<Tree, object>> onSelect)
    {
        return tree with { OnSelect = new(onSelect.ToValueTask()) };
    }

    public static Tree OnSelect(this Tree tree, Action<object> onSelect)
    {
        return tree with { OnSelect = new(@event => { onSelect(@event.Value); return ValueTask.CompletedTask; }) };
    }
}
