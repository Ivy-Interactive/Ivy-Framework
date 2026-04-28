using Ivy;
using MyProject.Apps.Products;

namespace MyProject.Apps;

[App(icon: Icons.ShoppingBag, group: ["Apps"])]
public class ProductsApp : ViewBase
{
    public override object? Build()
    {
        var blades = UseBlades;
        return blades(() => new ProductListBlade(), "Search");
    }
}
