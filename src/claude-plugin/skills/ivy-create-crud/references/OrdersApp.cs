using Ivy;
using MyProject.Apps.Orders;

namespace MyProject.Apps;

[App(icon: Icons.ShoppingCart, group: ["Apps"])]
public class OrdersApp : ViewBase
{
    public override object? Build()
    {
        var blades = UseBlades;
        return blades(() => new OrderListBlade(), "Search");
    }
}
