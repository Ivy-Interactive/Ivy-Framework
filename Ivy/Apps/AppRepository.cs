using Ivy.Shared;
using Microsoft.OpenApi.Models;

namespace Ivy.Apps;

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
    private readonly List<Func<AppDescriptor[]>> _factories = [];

    private IAppRepositoryNode? Root { get; set; }
    private Dictionary<string, AppDescriptor> Apps { get; } = new();

    public void Reload()
    {
        Root = new AppRepositoryGroup("Root");
        Apps.Clear();

        //add apps to tree
        var indexFixups = new List<(IAppRepositoryGroup, AppDescriptor)>();
        foreach (var appDescriptor in _factories.SelectMany(factory => factory()))
        {
            Apps[appDescriptor.Id] = appDescriptor;

            if (appDescriptor.IsVisible || appDescriptor.IsIndex)
            {
                var current = Root;
                foreach (var part in appDescriptor.Path)
                {
                    if (current is not IAppRepositoryGroup group)
                    {
                        throw new InvalidOperationException("Path is not a group.");
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
                    throw new InvalidOperationException("Path is not a group.");
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
        if (Root is AppRepositoryGroup rootGroup)
        {
            // Get all leaf nodes in a flat list, maintaining their order
            var leafNodes = GetAllLeafNodes(rootGroup);

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
        }
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
        _factories.Add(factory);
    }

    public AppDescriptor GetAppOrDefault(string? id)
    {
        var app = id != null
            ? Apps.GetValueOrDefault(id)
            : null;

        return app ?? Apps.Values.First(app => app.Id != AppIds.Auth && app.Id != AppIds.Chrome);
    }

    public AppDescriptor? GetApp(string id)
    {
        return Apps.Values.FirstOrDefault(e => e.Id == id);
    }

    public AppDescriptor? GetApp(Type type)
    {
        return Apps.Values.FirstOrDefault(e => e.Type == type);
    }

    public MenuItem[] GetMenuItems()
    {
        if (Root is AppRepositoryGroup group)
        {
            return group.Children.OrderBy(e => e.Order).ThenBy(e => e.Title).Select(e => e.GetMenuItem()).ToArray();
        }
        return [];
    }

    public IEnumerable<AppDescriptor> All()
    {
        return Apps.Values;
    }
}