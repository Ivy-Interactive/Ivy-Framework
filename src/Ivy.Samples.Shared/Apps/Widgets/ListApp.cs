using Ivy.Samples.Shared.Helpers;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.List, path: ["Widgets"], searchHints: ["items", "collection", "scroll", "menu", "rows", "vertical"])]
public class ListApp : SampleBase
{
    protected override object? BuildSample()
    {
        var blades = UseBlades(() => new ListBlade(), "List");
        return blades;
    }
}

public class ListBlade : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var products = UseMemo(() => SampleData.GetUsers(100), []);
        var searchString = UseState("");
        var filteredProducts = UseState(products);

        var blades = UseContext<IBladeService>();

        UseEffect(() =>
        {
            var filtered = products.Where(p => p.Name.Contains(searchString.Value)).ToArray();
            filteredProducts.Set(filtered);
        }, [searchString]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var user = (User)e.Sender.Tag!;
            blades.Push(this, new DetailsBlade(user), user.Name);
        });

        ListItem CreateItem(User user) => new ListItem(title: user.Name, onClick: onItemClicked, tag: user, subtitle: user.Email, badge: user.Age.ToString())
            .Disabled(user.Age > 80); // Example: disable elderly users (just for demo)

        var items = filteredProducts.Value.Take(10).Select(CreateItem).ToList();

        // Add some manual examples of rich items
        items.Insert(0, new ListItem("Framework Updates", icon: Icons.Activity, subtitle: "Important system notifications")
            .Badge("New")
            .Content(new Badge("Critical", BadgeVariant.Destructive)));

        items.Insert(1, new ListItem("Settings", icon: Icons.Settings, subtitle: "Configure your profile")
            .Disabled()
            .Badge("Locked"));

        var onCreate = new Action<Event<Button>>(e =>
        {
            client.Toast("Create button clicked");
        });

        var header = Layout.Horizontal(
            searchString.ToSearchInput().Placeholder("Search..."),
            new Button(icon: Icons.Plus, onClick: onCreate, variant: ButtonVariant.Outline)
        ).Gap(1);

        return new Fragment()
               | new BladeHeader(header)
               | new List(items);
    }
}

public class DetailsBlade(User user) : ViewBase
{
    public override object? Build()
    {
        return user.ToDetails();
    }
}
