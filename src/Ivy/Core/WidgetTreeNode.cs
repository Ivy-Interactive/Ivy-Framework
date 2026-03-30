using System.Text.Json.Nodes;
using Ivy.Core.Hooks;

namespace Ivy.Core;

public class WidgetTreeNode(string id, int index, TreePath parentTreePath, WidgetTreeNode[] children, int? memoizedHashCode, IWidget? widget, IView? view, IViewContext? context, IViewContext? ancestorContext, string? parentId) : IAsyncDisposable
{
    public static WidgetTreeNode FromWidget(IWidget widget, int index, TreePath treePath, WidgetTreeNode[] children, IViewContext? ancestorContext, string? parentId)
    {
        return new WidgetTreeNode(widget.Id!, index, treePath, children, null, widget, null, null, ancestorContext, parentId);
    }

    public static WidgetTreeNode FromView(IView view, int index, TreePath treePath, WidgetTreeNode? child, IViewContext? context, int? memoizedHashCode, IViewContext? ancestorContext, string? parentId)
    {
        var children = child == null ? Array.Empty<WidgetTreeNode>() : [child];
        return new WidgetTreeNode(view.Id!, index, treePath, children, memoizedHashCode, null, view, context, ancestorContext, parentId);
    }

    // public bool ShouldRebuild { get; set; }

    public string Id { get; } = id;
    public int Index { get; } = index;
    public TreePath ParentTreePath { get; } = parentTreePath;
    public WidgetTreeNode[] Children { get; } = children;
    public int? MemoizedHashCode { get; } = memoizedHashCode;
    public IWidget? Widget { get; } = widget;
    public IView? View { get; } = view;
    public IViewContext? Context { get; } = context;
    public IViewContext? AncestorContext { get; } = ancestorContext;
    public string? ParentId { get; } = parentId;
    public Type NodeType => IsWidget ? Widget!.GetType() : View!.GetType();
    private JsonNode? _previousSerialization = null;

    public IWidget? GetWidgetTree()
    {
        if (IsWidget)
        {
            var children = Children
                .Select(child => child.GetWidgetTree()).Cast<object>().ToArray();

            if (Widget is AbstractWidget widget)
            {
                return widget with
                {
                    Children = children
                };
            }

            throw new NotSupportedException("Widgets must be of type WidgetBase.");
        }

        if (IsView)
        {
            //views always have none or one child
            return Children.SingleOrDefault()?.GetWidgetTree();
        }

        throw new NotSupportedException("Node must be either an IWidget or an IView.");
    }

    public bool IsWidget => Widget != null;
    public bool IsView => View != null;

    public async ValueTask DisposeAsync()
    {
        View?.Dispose();
        if (Context != null)
        {
            await Context.DisposeAsync();
        }
    }

    public JsonNode? GetSerializedWidgetTree()
    {
        //We cache the previous serialization to avoid recomputing it multiple times
        if (_previousSerialization != null)
        {
            return _previousSerialization;
        }
        return _previousSerialization = this.GetWidgetTree()?.Serialize();
    }

    public void InvalidateSerializationCache()
    {
        _previousSerialization = null;
    }

    public int[] GetWidgetTreeIndices()
    {
        //The top of the tree is always a view
        //A view can only have one child which is either a view or a widget
        //We want get the indices as if the views has been removed and only the widgets remain

        var indices = new List<int>() { };
        var node = this.ParentTreePath;

        var previousSegment = new PathSegment("", null, this.Index, this.IsWidget);

        while (node != null && node.Count > 0)
        {
            var segment = node.Segment;
            if (segment.IsWidget)
            {
                indices.Add(previousSegment.Index);
            }
            previousSegment = segment;
            node = node.Parent;
        }

        indices.Reverse();
        return indices.ToArray();
    }

    // public void CancelRebuild()
    // {
    //     this.ShouldRebuild = false;
    //     foreach (var child in Children)
    //     {
    //         child.CancelRebuild();
    //     }
    // }
}