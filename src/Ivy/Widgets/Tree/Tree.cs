using Ivy.Core;

namespace Ivy;

public record Tree : WidgetBase<Tree>
{
    public Tree(params object[] items) : base(items)
    {
    }

    public Tree(IEnumerable<object> items) : base(items.ToArray())
    {
    }

    internal Tree()
    {
    }

    [Prop] public bool ShowLines { get; set; } = true;
}

public static class TreeExtensions
{
    public static Tree ShowLines(this Tree tree, bool showLines = true)
    {
        tree.ShowLines = showLines;
        return tree;
    }

    public static Tree HideLines(this Tree tree)
    {
        tree.ShowLines = false;
        return tree;
    }
}
