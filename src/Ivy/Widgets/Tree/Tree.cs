using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

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

    [Event] public Func<Event<Tree, object>, ValueTask>? OnSelect { get; set; }

    [Event] public Func<Event<Tree, TreeRowActionClickEventArgs>, ValueTask>? OnRowAction { get; set; }

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
    public static Tree HandleRowAction(this Tree tree, Func<Event<Tree, TreeRowActionClickEventArgs>, ValueTask> handler)
        => tree with { OnRowAction = handler };

    public static Tree HandleRowAction(this Tree tree, Action<Event<Tree, TreeRowActionClickEventArgs>> handler)
        => tree with { OnRowAction = handler.ToValueTask() };

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

    public static Tree HandleSelect(this Tree tree, Func<Event<Tree, object>, ValueTask> onSelect)
    {
        return tree with { OnSelect = onSelect };
    }

    public static Tree HandleSelect(this Tree tree, Action<Event<Tree, object>> onSelect)
    {
        return tree with { OnSelect = onSelect.ToValueTask() };
    }

    public static Tree HandleSelect(this Tree tree, Action<object> onSelect)
    {
        return tree with { OnSelect = @event => { onSelect(@event.Value); return ValueTask.CompletedTask; } };
    }
}
