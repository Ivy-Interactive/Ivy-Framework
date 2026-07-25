using System.Reactive.Subjects;

namespace Ivy.Core.Apps;

public interface IAppRepositoryNode
{
    public string Title { get; set; }

    public Icons? Icon { get; set; }

    public int Order { get; set; }

    public MenuItem GetMenuItem();

    public InternalLink? Next { get; set; }

    public InternalLink? Previous { get; set; }
}

public interface IAppRepositoryGroup : IAppRepositoryNode
{
    public List<IAppRepositoryNode> Children { get; set; }

    public bool Expanded { get; set; }
}

public class AppRepositoryGroup(string title) : IAppRepositoryGroup
{
    public List<IAppRepositoryNode> Children { get; set; } = new();

    public string Title { get; set; } = title;

    public Icons? Icon { get; set; } = Icons.Folder;

    public int Order { get; set; }

    public bool Expanded { get; set; }

    public MenuItem GetMenuItem()
    {
        return new MenuItem(
            Title,
            Children.OrderBy(e => e.Order).ThenBy(e => e.Title).Select(e => e.GetMenuItem()).ToArray(),
            Icon,
            Expanded: Expanded
        );
    }

    public InternalLink? Next { get; set; } = null;

    public InternalLink? Previous { get; set; } = null;
}

public class AppRepository : IAppRepository
{
    private readonly Subject<Unit> _reloaded = new();
    private readonly Subject<IReadOnlySet<string>> _appsRefreshRequested = new();
    private readonly List<Func<AppDescriptor[]>> _factories = [];
    private readonly object _lock = new();

    // Serializes Reload. Held across the whole rebuild, unlike _lock, which only guards the two
    // fields that cannot live in a snapshot.
    private readonly object _reloadLock = new();

    public IObservable<Unit> Reloaded => _reloaded;
    public IObservable<IReadOnlySet<string>> AppsRefreshRequested => _appsRefreshRequested;

    public void RequestAppRefresh(IReadOnlySet<string> appIds)
    {
        if (appIds.Count > 0)
            _appsRefreshRequested.OnNext(appIds);
    }

    /// <summary>
    /// A view of the app tree. <see cref="Reload"/> builds a replacement and swaps it in, so readers
    /// always observe a fully built repository instead of one that is midway through a rebuild.
    /// </summary>
    /// <remarks>
    /// Only the container is immutable: the group tree and the app dictionary are never touched again
    /// once published. The <see cref="AppDescriptor"/> instances inside them are not. Factories
    /// registered through <c>Server.AddApp(AppDescriptor)</c> or <c>IIvyPluginContext.AddApp</c> close
    /// over a single descriptor and return that same instance from every reload, and the leaf-link pass
    /// in <see cref="Reload"/> writes <see cref="AppDescriptor.Next"/> and
    /// <see cref="AppDescriptor.Previous"/> on whatever instances the factories hand back. A later
    /// reload can therefore change those two properties on descriptors that this snapshot — and any
    /// live session holding an injected descriptor — still references.
    /// </remarks>
    private sealed class Snapshot
    {
        public required AppRepositoryGroup Root { get; init; }

        public required IReadOnlyDictionary<string, AppDescriptor> Apps { get; init; }
    }

    private Snapshot _snapshot = new()
    {
        Root = new AppRepositoryGroup("Root"),
        Apps = new Dictionary<string, AppDescriptor>(),
    };

    // Deliberately spans reloads and is reset explicitly via ClearInvalidAppIds, so it cannot live in
    // the snapshot. Guarded by _lock instead.
    public IReadOnlySet<string> InvalidAppIds
    {
        get
        {
            lock (_lock)
            {
                return _invalidAppIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    private HashSet<string> _invalidAppIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    public void Reload(IReadOnlySet<string> reservedPaths)
    {
        // Serialized end to end so that a reload which starts later also publishes later. Publishing in
        // completion order instead would let a slow reload overwrite the result of one that started
        // after it, republishing a tree that predates the factories the later reload picked up. At
        // startup that silently drops the apps of every plugin the deferred loader brought in.
        lock (_reloadLock)
        {
            ReloadCore(reservedPaths);
        }

        // Fired outside _reloadLock. The notification carries no payload and subscribers respond by
        // re-reading the current snapshot, so a redundant or late notification is harmless — whereas
        // holding the lock across subscriber callbacks would stall every other reload behind arbitrary
        // view code.
        _reloaded.OnNext(default);
    }

    private void ReloadCore(IReadOnlySet<string> reservedPaths)
    {
        Func<AppDescriptor[]>[] factoriesSnapshot;
        lock (_lock)
        {
            factoriesSnapshot = _factories.ToArray();
        }

        //collect apps first, so each id resolves to exactly one descriptor before the tree is built.
        //the last registration of an id wins, which is what lets a caller override a built-in app by
        //registering their own (UseErrorNotFound over the default error app, for example).
        var apps = new Dictionary<string, AppDescriptor>();
        var appIdsInRegistrationOrder = new List<string>();
        foreach (var appDescriptor in factoriesSnapshot.SelectMany(factory => factory()))
        {
            if (!ValidateAppId(appDescriptor.Id, reservedPaths))
            {
                lock (_lock)
                {
                    _invalidAppIds.Add(appDescriptor.Id);
                }
                // Do not add invalid apps to repository
                continue;
            }

            if (!apps.ContainsKey(appDescriptor.Id))
            {
                appIdsInRegistrationOrder.Add(appDescriptor.Id);
            }

            apps[appDescriptor.Id] = appDescriptor;
        }

        //add apps to tree — one node per id, so two factories yielding the same id cannot produce two
        //menu items for it
        var root = new AppRepositoryGroup("Root");
        var indexFixups = new List<(IAppRepositoryGroup, AppDescriptor)>();
        foreach (var appId in appIdsInRegistrationOrder)
        {
            var appDescriptor = apps[appId];

            if (appDescriptor.IsVisible || appDescriptor.IsIndex)
            {
                IAppRepositoryNode current = root;
                foreach (var part in appDescriptor.Group)
                {
                    if (current is not IAppRepositoryGroup group)
                    {
                        throw new InvalidOperationException("Group part is not a group.");
                    }

                    var next = group.Children.OfType<AppRepositoryGroup>().FirstOrDefault(e => e.Title == part);
                    if (next == null)
                    {
                        next = new AppRepositoryGroup(part);
                        group.Children.Add(next);
                    }
                    current = next;
                }

                if (current is not IAppRepositoryGroup group2)
                {
                    throw new InvalidOperationException("Group part is not a group.");
                }

                if (appDescriptor.IsIndex)
                {
                    //we need to fixup this group's properties later.
                    //doing it here could change the title and break lookup in later iterations of this loop.
                    indexFixups.Add((group2, appDescriptor));
                }
                else
                {
                    group2.Children.Add(appDescriptor);
                }
            }
        }

        //fixup properties of index groups
        foreach (var (group, appDescriptor) in indexFixups)
        {
            group.Order = appDescriptor.Order;
            group.Icon = appDescriptor.Icon ?? group.Icon;
            group.Title = appDescriptor.Title;
            group.Expanded = appDescriptor.GroupExpanded;
        }

        //traverse the tree and on each leaf (nodes that are not groups) set link next and previous
        // Get all leaf nodes in a flat list, maintaining their order
        var leafNodes = GetAllLeafNodes(root);

        // Set next and previous links for each leaf node
        for (int i = 0; i < leafNodes.Count; i++)
        {
            // Set previous link (except for first node)
            if (i > 0)
            {
                var previousNode = leafNodes[i - 1];
                var previousLink = new InternalLink(previousNode.Title, previousNode is AppDescriptor app ? app.Id : throw new InvalidOperationException("Previous node is not an app."));
                leafNodes[i].Previous = previousLink;
            }
            else
            {
                leafNodes[i].Previous = null;
            }

            // Set next link (except for last node)
            if (i < leafNodes.Count - 1)
            {
                var nextNode = leafNodes[i + 1];
                var nextLink = new InternalLink(nextNode.Title, nextNode is AppDescriptor app ? app.Id : throw new InvalidOperationException("Next node is not an app."));
                leafNodes[i].Next = nextLink;
            }
            else
            {
                leafNodes[i].Next = null;
            }
        }

        // Publish the new state in a single reference assignment. Readers hold either the old snapshot
        // or the new one, never a partially populated tree.
        Volatile.Write(ref _snapshot, new Snapshot { Root = root, Apps = apps });
    }

    private List<IAppRepositoryNode> GetAllLeafNodes(IAppRepositoryGroup group)
    {
        var result = new List<IAppRepositoryNode>();

        foreach (var child in group.Children.OrderBy(e => e.Order).ThenBy(e => e.Title))
        {
            if (child is IAppRepositoryGroup childGroup)
            {
                // If this is a group, recursively get its leaf nodes
                result.AddRange(GetAllLeafNodes(childGroup));
            }
            else
            {
                // If this is a leaf node, add it to the result
                result.Add(child);
            }
        }

        return result;
    }

    public void AddFactory(Func<AppDescriptor[]> factory)
    {
        lock (_lock)
        {
            _factories.Add(factory);
        }
    }

    public bool RemoveFactory(Func<AppDescriptor[]> factory)
    {
        lock (_lock)
        {
            return _factories.Remove(factory);
        }
    }

    public AppDescriptor GetAppOrDefault(string? id)
    {
        var apps = Volatile.Read(ref _snapshot).Apps;

        var app = id != null
            ? apps.GetValueOrDefault(id)
            : null;

        return app
            ?? apps.Values.FirstOrDefault(x => !AppIds.ShouldNotBeAutoDefaultApps.Contains(x.Id))
            ?? apps.GetValueOrDefault(AppIds.ErrorNotFound)
            ?? throw new InvalidOperationException("No serviceable apps are registered on this server.");
    }

    public AppDescriptor? GetApp(string id)
    {
        return Volatile.Read(ref _snapshot).Apps.Values.FirstOrDefault(e => e.Id == id);
    }

    public AppDescriptor? GetApp(Type type)
    {
        return Volatile.Read(ref _snapshot).Apps.Values.FirstOrDefault(e => e.Type == type);
    }

    public MenuItem[] GetMenuItems()
    {
        var root = Volatile.Read(ref _snapshot).Root;
        return root.Children.OrderBy(e => e.Order).ThenBy(e => e.Title).Select(e => e.GetMenuItem()).ToArray();
    }

    public IEnumerable<AppDescriptor> All()
    {
        return Volatile.Read(ref _snapshot).Apps.Values.ToArray();
    }

    private bool ValidateAppId(string appId, IReadOnlySet<string> reservedPaths)
    {
        lock (_lock)
        {
            // Already reported on an earlier reload; don't log the same error again.
            if (_invalidAppIds.Contains(appId))
            {
                return false;
            }
        }

        switch (AppRoutingHelpers.ValidateAppId(appId, reservedPaths))
        {
            case AppIdValidationResult.Valid:
                return true;
            case AppIdValidationResult.Empty:
                Console.WriteLine($"[ERROR] App ID is empty. Please provide a valid App ID.");
                break;
            case AppIdValidationResult.StartsWithSlash:
                Console.WriteLine($"[ERROR] App ID '{appId}' is invalid. App IDs should not start with '/'.");
                break;
            case AppIdValidationResult.UnsafeCharacters:
                Console.WriteLine($"[ERROR] App ID '{appId}' is invalid. App IDs must be URL-friendly (alphanumeric, dashes, underscores, etc.).");
                break;
            case AppIdValidationResult.ReservedPathConflict:
                Console.WriteLine($"[ERROR] App ID '{appId}' collides with a reserved path '/{appId}'. Please choose a different App ID.");
                break;
            case AppIdValidationResult.StaticFileExtensionConflict:
                Console.WriteLine($"[ERROR] App ID '{appId}' collides with a static file extension. Please choose a different App ID.");
                break;
            default:
                Console.WriteLine($"[ERROR] App ID '{appId}' is invalid. Please choose a different App ID.");
                break;
        }

        return false;
    }

    public void ClearInvalidAppIds()
    {
        lock (_lock)
        {
            _invalidAppIds.Clear();
        }
    }
}
