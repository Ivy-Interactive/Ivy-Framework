using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs.Formatters;
using System.Text.Json.Nodes;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Core;

public class TreeNode(string id, int index, Path parentPath, TreeNode[] children, int? memoizedHashCode, IWidget? widget, IView? view, IViewContext? context, IViewContext? ancestorContext) : IDisposable
{
    public static TreeNode FromWidget(IWidget widget, int index, Path path, TreeNode[] children, IViewContext? ancestorContext)
    {
        return new TreeNode(widget.Id!, index, path, children, null, widget, null, null, ancestorContext);
    }

    public static TreeNode FromView(IView view, int index, Path path, TreeNode? child, IViewContext? context, int? memoizedHashCode, IViewContext? ancestorContext)
    {
        var children = child == null ? Array.Empty<TreeNode>() : [child];
        return new TreeNode(view.Id!, index, path, children, memoizedHashCode, null, view, context, ancestorContext);
    }

    //public bool ShouldRebuild { get; set; }

    public string Id { get; } = id;
    public int Index { get; } = index;
    public Path ParentPath { get; } = parentPath;
    public TreeNode[] Children { get; } = children;
    public int? MemoizedHashCode { get; } = memoizedHashCode;
    public IWidget? Widget { get; } = widget;
    public IView? View { get; } = view;
    public IViewContext? Context { get; } = context;
    public IViewContext? AncestorContext { get; } = ancestorContext;

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

    public void Dispose()
    {
        View?.Dispose();
        Context?.Dispose();
    }

    public int[] GetWidgetTreeIndices()
    {
        //The top of the tree is always a view
        //A view can only have one child which is either a view or a widget
        //We want get the indices as if the views has been removed and only the widgets remain

        var indices = new List<int>() { };
        var path = this.ParentPath.Clone();

        var previousSegment = new PathSegment("", null, this.Index, this.IsWidget);

        while (path.Count > 0)
        {
            var segment = path.Pop();
            if (segment.IsWidget)
            {
                indices.Add(previousSegment.Index);
            }
            previousSegment = segment;
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

public class WidgetTreeChanged(string viewId, int[] indices, JsonNode? patch)
{
    public string ViewId { get; } = viewId;
    public int[] Indices { get; } = indices;
    public JsonNode? Patch { get; } = patch;
}

public class WidgetTree : IWidgetTree, IObservable<WidgetTreeChanged[]>
{
    private readonly Dictionary<string, TreeNode> _nodes = new();
    private readonly Dictionary<string, string> _parents = new();

    public IView RootView { get; }
    public TreeNode? NodeTree { get; private set; }

    private readonly Subject<WidgetTreeChanged[]> _treeChangedSubject = new();
    private readonly Subject<string> _buildRequestedSubject = new();
    private readonly SemaphoreSlim _buildRequestedSemaphore = new(1, 1);
    private readonly Disposables _disposables = new();
    private readonly IContentBuilder _builder;
    private readonly IServiceProvider _appServices;

    public WidgetTree(IView rootView, IContentBuilder builder, IServiceProvider appServices)
    {
        _builder = builder;
        _appServices = appServices;
        RootView = rootView;

        async void OnNext(string[] requestedViewIds) =>
            await RefreshRequested(requestedViewIds);

        var subscription = _buildRequestedSubject
            .Buffer(TimeSpan.FromMilliseconds(100)) //33 = 30fps
            .Select(batch => batch.Distinct().ToArray())
            .Where(batch => batch.Length > 0)
            .Subscribe(OnNext);

        _disposables.Add(subscription);
    }

    public async Task BuildAsync()
    {
        await _buildRequestedSemaphore.WaitAsync();
        try
        {
            Path path = new();
            var tree = BuildView(RootView, path, 0, null, null, false, false);
            NodeTree = tree ?? throw new NotSupportedException("Build must return an TreeNode.");
            PrintDebug();
        }
        catch (ObjectDisposedException)
        {
            //ignore
        }
        finally
        {
            try
            {
                _buildRequestedSemaphore.Release();
            }
            catch (ObjectDisposedException)
            {
                //ignore
            }
        }
    }

    public IWidget GetWidgets()
    {
        if (NodeTree == null) throw new NotSupportedException("Tree must be built before getting widgets.");
        var widgets = NodeTree.GetWidgetTree();
        if (widgets == null) throw new NotSupportedException("Tree must be non-null.");
        return widgets;
    }

    public void RefreshView(string viewId)
    {
        // if(!_nodes.TryGetValue(viewId, out var node))
        //     throw new NotSupportedException($"Node '{viewId}' not found.");
        //         
        // if(!node.IsView)
        //     throw new NotSupportedException($"Node '{viewId}' is not a view.");
        //
        // node.ShouldRebuild = true;
        _buildRequestedSubject.OnNext(viewId);
    }

    private async Task RefreshRequested(string[] requestedViewIds)
    {
        await _buildRequestedSemaphore.WaitAsync();
        try
        {
            //todo: if the node A is a child of node B, both are request but there's a memoized node 
            //in between, we need to rebuild them both.

            // List<string[]> paths = new();
            // foreach (var viewId in requestedViewIds)
            // {
            //     if (!_nodes.TryGetValue(viewId, out var node))
            //         throw new NotSupportedException($"Node '{viewId}' not found.");
            //
            //     if (!node.IsView)
            //         throw new NotSupportedException($"Node '{viewId}' is not a view.");
            //
            //     //if (node.ShouldRebuild) //this might have changed since we requested the build
            //     {
            //         var path = new List<string>();
            //         var current = node;
            //         path.Add(node.Id);
            //         while (_parents.TryGetValue(current.Id, out var parentId))
            //         {
            //             current = _nodes[parentId];
            //             path.Add(current.Id);
            //         }
            //         path.Reverse();
            //         paths.Add(path.ToArray());
            //     }
            // }

            //var nodesToRebuild = TreeRebuildSolver.FindMinimalRebuildNodes(paths.ToArray());

            var nodesToRebuild = requestedViewIds;

            List<WidgetTreeChanged> changes = new();
            //todo: we should be able to be done in parallel?
            foreach (var nodeId in nodesToRebuild)
            {
                if (_nodes.TryGetValue(nodeId, out var node))
                {
                    //node.CancelRebuild();
                    var changed = _RefreshView(node.Id, false);
                    if (changed != null)
                    {
                        changes.Add(changed);
                    }
                }
            }

            if (changes.Count > 0)
            {
                _treeChangedSubject.OnNext(changes.ToArray());
            }
        }
        catch (ObjectDisposedException)
        {
            //ignore
        }
        finally
        {
            try
            {
                _buildRequestedSemaphore.Release();
            }
            catch (ObjectDisposedException)
            {
                //ignore
            }
        }
    }

    private WidgetTreeChanged? _RefreshView(string viewId, bool isHotReload)
    {
        if (!_nodes.TryGetValue(viewId, out var node))
            throw new NotSupportedException($"Node '{viewId}' not found.");

        if (!node.IsView)
            throw new NotSupportedException($"Node '{viewId}' is not a view.");

        _parents.TryGetValue(viewId, out var parentId);

        var indices = node.GetWidgetTreeIndices();

        var previous = node.GetWidgetTree()?.Serialize();

        var partial = BuildView(node.View!, node.ParentPath.Clone(), node.Index, parentId, node.AncestorContext, isRefreshingView: true, isHotReload);

        if (partial == null) throw new NotSupportedException("View must return an IWidget.");

        if (parentId == null)
        {
            NodeTree = partial;
        }
        else
        {
            if (_nodes.TryGetValue(parentId, out var parent))
            {
                if (parent.Children.Length <= node.Index)
                {
                    //Console.WriteLine("Parent children length is less than node index that we are trying to update.");    
                }
                else
                {
                    parent.Children[node.Index] = partial;
                }
            }
        }
        try
        {
            var update = partial.GetWidgetTree()?.Serialize();
            var patch = previous.Diff(update, new JsonPatchDeltaFormatter());
            return new WidgetTreeChanged(viewId, indices, patch);
        }
        catch (ObjectDisposedException)
        {
            //ignore
        }
        return null!;
    }

    private TreeNode? BuildView(IView view,
        Path path,
        int index,
        string? parentId,
        IViewContext? ancestorContext,
        bool isRefreshingView,
        bool isHotReload
    )
    {
        path.Push(view, index);

        view.Id = GenerateId(path);

        int? memoizedHashCode = null;
        bool memoized = false;

        if (view is IMemoized memo)
        {
            memoizedHashCode = CalculateMemoizedHashCode(view.Id, memo.GetMemoValues());
            memoized = true;
        }

        var previousNode = _nodes.GetValueOrDefault(view.Id);

        TreeNode? node;
        IViewContext? context = previousNode?.Context;
        if (!memoized || isHotReload || isRefreshingView || previousNode == null || previousNode.MemoizedHashCode != memoizedHashCode)
        {
            // if (!isHotReload && !isRefreshingView && previousNode != null)
            // {
            //     previousNode.Dispose();
            //     _nodes.Remove(previousNode.Id);
            //     _parents.Remove(previousNode.Id);
            //     previousNode = null;
            // }

            if (view is IStateless)
            {
                //small optimization for stateless views to skip context creation - not sure this really matters
                node = BuildObject(view.Build(), path.Clone(), index, view.Id, ancestorContext, isHotReload);
            }
            else
            {
                context = previousNode?.Context;

                if (context == null)
                {
                    var id = new string(view.Id.ToCharArray()); //ensuring this is cloned.
                    context = new ViewContext(
                        () =>
                        {
                            RefreshView(id);
                        },
                        ancestorContext,
                        _appServices
                    );
                }

                view.BeforeBuild(context);

                object? buildResult;
                try
                {
                    buildResult = view.Build();
                }
                catch (Exception e)
                {
                    buildResult = e;
                }

                node = BuildObject(buildResult, path.Clone(), index, view.Id, context, isHotReload);
                view.AfterBuild();
                context.Reset();
            }
        }
        else
        {
            //No need to destroy anything. Just reuse the previous widget tree.
            node = previousNode.Children.SingleOrDefault();
        }

        path.Pop();

        _nodes[view.Id] = TreeNode.FromView(view, index, path.Clone(), node, context, memoizedHashCode, ancestorContext);

        if (parentId != null)
            _parents[view.Id] = parentId;

        if (previousNode != null)
        {
            if (node == null)
            {
                DestroyNode(view.Id);
            }
            else
            {
                DestroyRemovedNodes(previousNode, _nodes[view.Id], isRefreshingView ? view.Id : null);
            }
        }

        return _nodes[view.Id];
    }

    internal static int CalculateMemoizedHashCode(string viewId, object?[] props)
    {
        var hash = new HashCode();
        hash.Add(Utils.StableHash(viewId));
        foreach (var prop in props)
        {
            if (prop == null) continue;
            if (prop is string stringProp)
            {
                hash.Add(Utils.StableHash(stringProp));
            }
            else if (prop.GetType().IsValueType)
            {
                hash.Add(prop);
            }
            else
            {
                var json = JsonSerializer.Serialize(prop);
                hash.Add(Utils.StableHash(json));
            }

        }
        return hash.ToHashCode();
    }

    private TreeNode? BuildObject(object? anything, Path path, int index, string parentId, IViewContext? ancestorContext, bool isHotReload)
    {
        var formatted = _builder.Format(anything);
        if (formatted == null) return null;

        if (formatted is not IView && formatted is not IWidget)
            throw new NotSupportedException("IContentFormatter must return either an IView or an IWidget.");

        TreeNode? node = null;
        if (formatted is IView newView)
        {
            node = BuildView(newView, path.Clone(), index, parentId, ancestorContext, false, isHotReload);
        }

        if (formatted is IWidget newWidget)
        {
            node = BuildWidget(newWidget, path.Clone(), index, parentId, ancestorContext, isHotReload);
        }

        return node;
    }

    private TreeNode BuildWidget(IWidget widget, Path path, int index, string parentId, IViewContext? ancestorContext, bool isHotReload)
    {
        path.Push(widget, index);

        widget.Id = GenerateId(path);

        var children = new List<TreeNode>();
        if (widget.Children == null!) widget.Children = [];
        for (var i = 0; i < widget.Children.Length; i++)
        {
            var child = widget.Children[i];
            var newWidget = BuildObject(child, path.Clone(), i, widget.Id, ancestorContext, isHotReload);
            if (newWidget == null) continue;
            children.Add(newWidget);
        }

        path.Pop();

        var node = TreeNode.FromWidget(widget, index, path.Clone(), children.ToArray(), ancestorContext);

        _nodes[widget.Id] = node;
        _parents[widget.Id] = parentId;

        return node;
    }

    private static string GenerateId(Path path)
    {
        var input = path.ToString();
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        string hash = Convert.ToBase64String(hashBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .ToLower();
        string alphanumericHash = new string(hash.Where(char.IsLetterOrDigit).ToArray());
        return alphanumericHash[..10]; //With 1 million widgets, the collision probability is extremely low (0.0000596%)
    }

    public bool TriggerEvent(string widgetId, string eventName, JsonArray args)
    {
        if (!_nodes.TryGetValue(widgetId, out var node))
            throw new NotSupportedException($"Node '{widgetId}' not found.");

        if (!node.IsWidget)
            throw new NotSupportedException($"Node '{widgetId}' is not a widget.");

        var widget = node.Widget!;

        var result = widget.InvokeEvent(eventName, args);

        return result;
    }

    public void HotReload()
    {
        var change = _RefreshView(NodeTree!.Id, true);
        if (change != null)
        {
            _treeChangedSubject.OnNext([change]);
        }
    }

    private void PrintDebug()
    {
        //Console.WriteLine($"Nodes:{_nodes.Count}");
        //PrintTree(NodeTree, 0);
    }

    private void PrintTree(TreeNode? node, int i)
    {
        if (node == null) return;
        Console.Write(new string(' ', i));
        Console.Write($"{node.Id}:{(node.Widget == null ? "View" : "Widget")}:");
        Console.Write($"{(node.Widget?.GetType().Name ?? node.View?.GetType().Name)}");
        //Console.Write($":{node.Path}");
        Console.WriteLine();

        foreach (var child in node.Children)
        {
            PrintTree(child, i + 1);
        }
    }

    private void DestroyNode(string nodeId, string? skipViewId = null)
    {
        if (_nodes.TryGetValue(nodeId, out var node))
        {
            if (nodeId != skipViewId)
            {
                node.Dispose();
                _nodes.Remove(nodeId);
                _parents.Remove(nodeId);
            }
            foreach (var child in node.Children)
            {
                DestroyNode(child.Id);
            }
        }
    }

    private void DestroyRemovedNodes(TreeNode previousNode, TreeNode node, string? skipViewId)
    {
        if (previousNode.Id != node.Id)
        {
            throw new NotSupportedException("Node Ids must match.");
        }

        // Remove all children in previousNode that are not in node using Id as key
        var previousChildren = previousNode.Children.ToDictionary(x => x.Id);
        var newChildrenIds = node.Children
            .Select(x => x.Id)
            .ToHashSet();

        // Destroy children that don't exist in the new tree
        foreach (var prevChild in previousChildren.Values)
        {
            if (!newChildrenIds.Contains(prevChild.Id))
            {
                DestroyNode(prevChild.Id, skipViewId);
            }
        }

        // Recursively check surviving children
        foreach (var newChild in node.Children)
        {
            if (previousChildren.TryGetValue(newChild.Id, out var previousChild))
            {
                DestroyRemovedNodes(previousChild, newChild, skipViewId);
            }
        }
    }

    public IDisposable Subscribe(IObserver<WidgetTreeChanged[]> observer) => _treeChangedSubject.Subscribe(observer);

    public void Dispose()
    {
        _buildRequestedSemaphore.Wait();
        try
        {
            if (NodeTree != null)
            {
                DestroyNode(NodeTree!.Id);
            }
            _disposables.Dispose();
            _treeChangedSubject.Dispose();
            _buildRequestedSubject.Dispose();
        }
        finally
        {
            _buildRequestedSemaphore.Dispose();
        }
    }
}