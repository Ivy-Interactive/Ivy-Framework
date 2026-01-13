using Ivy.Samples.Shared.Helpers;
using Ivy.Shared;
using Ivy.Views.Blades;
using Ivy.Views.Builders;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.List, path: ["Widgets"], searchHints: ["items", "collection", "scroll", "menu", "rows", "vertical"])]
public class ListApp : SampleBase
{
    protected override object? BuildSample()
    {
        return UseBlades(() => new ListBlade(), "List");
    }
}

public class ListBlade : ViewBase
{
    public override object? Build()
    {
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

        ListItem CreateItem(User user) => new(title: user.Name, onClick: onItemClicked, tag: user, subtitle: user.Email, badge: user.Age.ToString());

        var items = filteredProducts.Value.Select(CreateItem);

        var onCreate = new Action<Event<Button>>(e =>
        {

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