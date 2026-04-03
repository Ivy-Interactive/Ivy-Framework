using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs.Formatters;
using System.Text.Json.Nodes;
using Ivy;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Core;

public class WidgetTreeChanged(string viewId, int[] indices, JsonNode patch, int iteration, string? treeHash)
{
    public string ViewId { get; } = viewId;
    public int[] Indices { get; } = indices;
    public JsonNode Patch { get; } = patch;
    public int Iteration { get; set; } = iteration;
    public string? TreeHash { get; } = treeHash;
}

public class WidgetTree : IWidgetTree, IObservable<WidgetTreeChanged[]>
{
    private readonly Dictionary<string, WidgetTreeNode> _nodes = new();
    private int _iteration;

    public IView RootView { get; }
    public WidgetTreeNode? NodeTree { get; private set; }

    public static readonly JsonDiffOptions JsonDiffOptions = new()
    {
        ArrayObjectItemKeyFinder = (node, _) =>
        {
            if (node is JsonObject obj && obj.TryGetPropertyValue("id", out var id))
            {
                return id?.GetValue<string>();
            }
            return null;
        }
    };

    private readonly Subject<WidgetTreeChanged[]> _treeChangedSubject = new();
    private readonly Subject<string> _buildRequestedSubject = new();
    private readonly SemaphoreSlim _buildRequestedSemaphore = new(1, 1);
    private readonly Disposables _disposables = new();
    private readonly IContentBuilder _contentBuilder;
    private readonly IServiceProvider _appServices;

    public WidgetTree(IView rootView, IContentBuilder contentBuilder, IServiceProvider appServices)
    {
        _contentBuilder = contentBuilder;
        _appServices = appServices;
        RootView = rootView;

        async void OnNext(string[] requestedViewIds) =>
            await RefreshRequested(requestedViewIds);

        var subscription = _buildRequestedSubject
            .Buffer(TimeSpan.FromMilliseconds(16))
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
            TreePath treePath = new();
            var tree = BuildView(RootView, treePath, 0, null, null, false, false);
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
        _buildRequestedSubject.OnNext(viewId);
    }

    private async Task RefreshRequested(string[] requestedViewIds) //are ensure to be unique
    {
        await _buildRequestedSemaphore.WaitAsync(); //ensure only one refresh operation at a time
        try
        {
            var nodesToRebuild = requestedViewIds;

            List<WidgetTreeChanged> changes = new();

            if (nodesToRebuild.Length > 1)
            {
                //Building more than one node
                //What happens if A is parent of B? What about the order they are built in? If A is built first, B might be invalidated
            }

            foreach (var nodeId in nodesToRebuild)
            {
                if (_nodes.TryGetValue(nodeId, out var node))
                {
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
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] WidgetTree.RefreshRequested failed: {ex}");
            // Don't rethrow — this runs from async void, rethrowing would crash the process
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
        try
        {
#if DEBUG
            var stopWatch = Stopwatch.StartNew();
#endif

            if (!_nodes.TryGetValue(viewId, out var node))
                throw new NotSupportedException($"Node '{viewId}' not found.");

            if (!node.IsView)
                throw new NotSupportedException($"Node '{viewId}' is not a view.");

            var parentId = node.ParentId;

            var indices = node.GetWidgetTreeIndices();

            var previous = node.GetSerializedWidgetTree();

            var partial = BuildView(node.View!, node.ParentTreePath.Clone(), node.Index, parentId, node.AncestorContext, isRefreshingView: true, isHotReload);

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
                        // Silent bound guard check
                    }
                    else
                    {
                        parent.Children[node.Index] = partial;

                        // Invalidate serialization cache for all ancestors since their child changed
                        InvalidateAncestorCaches(parentId);
                    }
                }
                else
                {
                    // Unresolved parent during active flush loop
                }
            }

            var update = partial.GetSerializedWidgetTree();

            var previousId = previous?["id"]?.GetValue<string>();
            var updateId = update?["id"]?.GetValue<string>();

            JsonNode? patch;

            if (previousId != null && updateId != null && previousId != updateId)
            {
                patch = new JsonArray(new JsonObject
                {
                    ["op"] = "replace",
                    ["path"] = "",
                    ["value"] = update?.DeepClone()
                });
            }
            else if (update != null && previous != null)
            {
                // [Native Patch Integration] Execute mathematically independent zero-allocation diffing via C/Rust!
                var oldBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(previous);
                var newBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(update);
                patch = NativeJsonDiff.ComputePatch(oldBytes, newBytes);
            }
            else
            {
                patch = null;
            }

            if (patch == null || patch.AsArray().Count == 0)
            {
                return null!;
            }

            _iteration += 1;
            string? hash = null;

#if DEBUG
            DebugHelpers.CheckIfIdUsedMultipleTimes(NodeTree);
            hash = DebugHelpers.CalculateTreeHash(NodeTree?.GetWidgetTree()?.Serialize());
            if (Environment.GetEnvironmentVariable("IVY_DUMP_WIDGET_TREES") == "1")
            {
                stopWatch.Start();
                DebugHelpers.LogUpdatedTree(previous, update, patch, stopWatch.ElapsedMilliseconds, _iteration, hash);
            }
#endif
            return new WidgetTreeChanged(viewId, indices, patch, _iteration, hash);
        }
        catch (ObjectDisposedException)
        {
            //ignore
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] WidgetTree._RefreshView failed for {viewId}: {ex}");
        }
        return null!;
    }

    private WidgetTreeNode? BuildView(IView view,
        TreePath treePath,
        int index,
        string? parentId,
        IViewContext? ancestorContext,
        bool isRefreshingView,
        bool isHotReload
    )
    {
        var childTreePath = treePath.Push(view, index);

        view.Id = childTreePath.GenerateId();

        int? memoizedHashCode = null;
        bool memoized = false;

        if (view is IMemoized memo)
        {
            memoizedHashCode = CalculateMemoizedHashCode(view.Id, memo.GetMemoValues());
            memoized = true;
        }

        var previousNode = _nodes.GetValueOrDefault(view.Id);

        WidgetTreeNode? node;
        IViewContext? context = previousNode?.Context;
        if (!memoized || isHotReload || isRefreshingView || previousNode == null || previousNode.MemoizedHashCode != memoizedHashCode)
        {
            if (view is IStateless)
            {
                //Small optimization for stateless views to skip context creation - not sure this really matters
#if DEBUG
                AbstractWidget.CurrentViewCallSite.Value = view.CallSite;
#endif
                TextInputBuildContext.SetCurrent(ancestorContext);
                try
                {
                    node = BuildObject(view.Build(), childTreePath, 0, view.Id, ancestorContext, isHotReload);
                }
                finally
                {
                    TextInputBuildContext.SetCurrent(null);
                }
#if DEBUG
                AbstractWidget.CurrentViewCallSite.Value = null;
#endif
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
#if DEBUG
                AbstractWidget.CurrentViewCallSite.Value = view.CallSite;
                try
                {
#endif
                    TextInputBuildContext.SetCurrent(context);
                    try
                    {
                        buildResult = view.Build();
                    }
                    catch (Exception e)
                    {
                        buildResult = e;
                    }
                    finally
                    {
                        TextInputBuildContext.SetCurrent(null);
                    }
#if DEBUG
                }
                finally
                {
                    AbstractWidget.CurrentViewCallSite.Value = null;
                }
#endif

                node = BuildObject(buildResult, childTreePath, 0, view.Id, context, isHotReload);
                view.AfterBuild();
                context.Reset();
            }
        }
        else
        {
            //No need to destroy anything. Just reuse the previous widget tree.
            node = previousNode.Children.SingleOrDefault();
        }

        _nodes[view.Id] = WidgetTreeNode.FromView(view, index, treePath, node, context, memoizedHashCode, ancestorContext, parentId);

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
        return Utils.StableHash([viewId, .. props]);
    }

    private WidgetTreeNode? BuildObject(object? anything, TreePath treePath, int index, string parentId, IViewContext? ancestorContext, bool isHotReload)
    {
        var formatted = _contentBuilder.Format(anything);
        if (formatted == null) return null;

        if (formatted is not IView && formatted is not IWidget)
            throw new NotSupportedException("IContentFormatter must return either an IView or an IWidget.");

        WidgetTreeNode? node = null;
        if (formatted is IView newView)
        {
            node = BuildView(newView, treePath.Clone(), index, parentId, ancestorContext, false, isHotReload);
        }

        if (formatted is IWidget newWidget)
        {
            node = BuildWidget(newWidget, treePath.Clone(), index, parentId, ancestorContext, isHotReload);
        }

        return node;
    }

    private WidgetTreeNode BuildWidget(IWidget widget, TreePath treePath, int index, string parentId, IViewContext? ancestorContext, bool isHotReload)
    {
        var childTreePath = treePath.Push(widget, index);

        widget.Id = childTreePath.GenerateId();
#if DEBUG
        widget.Path = childTreePath.ToString();
#endif

        var children = new List<WidgetTreeNode>();
        if (widget.Children == null!) widget.Children = [];
        for (var i = 0; i < widget.Children.Length; i++)
        {
            var child = widget.Children[i];
            var newWidget = BuildObject(child, childTreePath, i, widget.Id, ancestorContext, isHotReload);
            if (newWidget == null) continue;
            children.Add(newWidget);
        }

        var node = WidgetTreeNode.FromWidget(widget, index, treePath, children.ToArray(), ancestorContext, parentId);

        _nodes[widget.Id] = node;

        return node;
    }

    public async Task<bool> TriggerEventAsync(string widgetId, string eventName, JsonArray args)
    {
        if (!_nodes.TryGetValue(widgetId, out var node))
            throw new NotSupportedException($"Node '{widgetId}' not found.");

        if (!node.IsWidget)
            throw new NotSupportedException($"Node '{widgetId}' is not a widget.");

        var widget = node.Widget!;

        var result = await widget.InvokeEventAsync(eventName, args);

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
    }

    private void PrintTree(WidgetTreeNode? node, int i)
    {
        if (node == null) return;
        Console.Write(new string(' ', i));
        Console.Write($"{node.Id}:{(node.Widget == null ? "View" : "Widget")}:");
        Console.Write($"{(node.Widget?.GetType().Name ?? node.View?.GetType().Name)}");
        Console.WriteLine();

        foreach (var child in node.Children)
        {
            PrintTree(child, i + 1);
        }
    }

    // Sync version for use during tree building - disposes fire-and-forget
    private void DestroyNode(string nodeId, string? skipViewId = null)
    {
        if (_nodes.TryGetValue(nodeId, out var node))
        {
            if (nodeId != skipViewId)
            {
                _ = node.DisposeAsync(); // Fire and forget
                _nodes.Remove(nodeId);
            }
            foreach (var child in node.Children)
            {
                DestroyNode(child.Id);
            }
        }
    }

    // Async version for proper cleanup on shutdown
    private async ValueTask DestroyNodeAsync(string nodeId, string? skipViewId = null)
    {
        if (_nodes.TryGetValue(nodeId, out var node))
        {
            if (nodeId != skipViewId)
            {
                await node.DisposeAsync();
                _nodes.Remove(nodeId);
            }
            foreach (var child in node.Children)
            {
                await DestroyNodeAsync(child.Id);
            }
        }
    }

    private void DestroyRemovedNodes(WidgetTreeNode previousNode, WidgetTreeNode node, string? skipViewId)
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

    private void InvalidateAncestorCaches(string nodeId)
    {
        // Walk up the tree and invalidate serialization cache for all ancestors
        var currentId = nodeId;
        while (currentId != null)
        {
            if (_nodes.TryGetValue(currentId, out var node))
            {
                node.InvalidateSerializationCache();
                currentId = node.ParentId;
            }
            else
            {
                currentId = null;
            }
        }
    }

    public IDisposable Subscribe(IObserver<WidgetTreeChanged[]> observer) => _treeChangedSubject.Subscribe(observer);

    public async ValueTask DisposeAsync()
    {
        await _buildRequestedSemaphore.WaitAsync();
        try
        {
            if (NodeTree != null)
            {
                await DestroyNodeAsync(NodeTree!.Id);
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
