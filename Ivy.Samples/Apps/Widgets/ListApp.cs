using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets;

[App(icon: Icons.List, path: ["Widgets"])]
public class ListApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Context.UseBlades(() => new ListBlade(), "List");
    }
}

public class ListBlade : ViewBase
{
    public override object? Build()
    {
        var products = this.UseMemo(() => SampleData.GetUsers(100), []);
        var searchString = this.UseState("");
        var filteredProducts = this.UseState(products);

        var blades = this.UseContext<IBladeController>();

        this.UseEffect(() =>
        {
            var filtered = products.Where(p => p.Name.Contains(searchString.Value)).ToArray();
            filteredProducts.Set(filtered);
        }, [searchString]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var user = (User)e.Sender.Tag!;
            blades.Push(this, new DetailsBlade(user), user.Name);
        });

        ListItem CreateItem(User user) => new(title: user.Name, onClick: onItemClicked, tag: user, subtitle: user.Email, badge: user.Age.ToString());

        var items = filteredProducts.Value.Select(CreateItem);

        var onCreate = new Action<Event<Button>>(e =>
        {

        });

        return BladeHelper.WithHeader(
            Layout.Horizontal(
                searchString.ToSearchInput().Placeholder("Search..."),
                new Button(icon: Icons.Plus, onClick: onCreate, variant: ButtonVariant.Outline)
            ).Gap(1),
            new List(items)
        );
    }
}

public class DetailsBlade(User user) : ViewBase
{
    public override object? Build()
    {
        return user.ToDetails();
    }
}