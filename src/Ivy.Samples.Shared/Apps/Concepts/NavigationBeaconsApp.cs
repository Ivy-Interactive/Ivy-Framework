namespace Ivy.Samples.Shared.Apps.Concepts;

// Define entity types for this demo
public class Customer { public int Id { get; set; } public string Name { get; set; } = ""; }
public class Order { public int Id { get; set; } public int CustomerId { get; set; } }

public record CustomerDetailsArgs(int CustomerId);

// App that registers a beacon for Customer entities
[App(icon: Icons.Users, isVisible: false)]
[NavigationBeacon(typeof(Customer), nameof(GetCustomerBeacon))]
public class BeaconCustomerDetailsApp : ViewBase
{
    public static NavigationBeacon<Customer> GetCustomerBeacon() => new(
        AppId: "concepts/beacon-customer-details",
        ArgsBuilder: customer => new CustomerDetailsArgs(customer.Id)
    );

    public override object? Build()
    {
        var args = UseArgs<CustomerDetailsArgs>();
        var customerId = args?.CustomerId ?? 0;

        return Layout.Vertical(
            Text.H1($"Customer Details: #{customerId}"),
            Text.P("This app registered a NavigationBeacon<Customer>"),
            Text.Muted("Other apps can navigate here without knowing the app ID")
        );
    }
}

public record OrderDetailsArgs(int OrderId, int CustomerId);

// App that registers a beacon for Order entities
[App(icon: Icons.ShoppingCart, isVisible: false)]
[NavigationBeacon(typeof(Order), nameof(GetOrderBeacon))]
public class BeaconOrderDetailsApp : ViewBase
{
    public static NavigationBeacon<Order> GetOrderBeacon() => new(
        AppId: "concepts/beacon-order-details",
        ArgsBuilder: order => new OrderDetailsArgs(order.Id, order.CustomerId)
    );

    public override object? Build()
    {
        var args = UseArgs<OrderDetailsArgs>();
        var navigator = UseNavigation();
        var customerBeacon = UseNavigationBeacon<Customer>();

        var orderId = args?.OrderId ?? 0;
        var customerId = args?.CustomerId ?? 0;

        return Layout.Vertical(
            Text.H1($"Order Details: #{orderId}"),
            Text.P($"Customer ID: {customerId}"),

            // Conditionally show "View Customer" button if beacon exists
            customerBeacon != null
                ? new Button("View Customer")
                    .OnClick(() => navigator.Navigate(customerBeacon, new Customer { Id = customerId }))
                : Text.Muted("Customer details not available")
        );
    }
}

// Demo app that shows navigation beacon usage
[App("concepts/navigation-beacons", "Navigation Beacon Demo", icon: Icons.Navigation)]
public class NavigationBeaconDemoApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Tabs(
            new Tab("Interactive Demo", new BeaconInteractiveDemo()),
            new Tab("Overview", new BeaconOverview())
        ).Variant(TabsVariant.Content);
    }
}

public class BeaconInteractiveDemo : ViewBase
{
    public override object? Build()
    {
        var navigator = UseNavigation();
        var customerBeacon = UseNavigationBeacon<Customer>();
        var orderBeacon = UseNavigationBeacon<Order>();

        var customers = new[]
        {
            new Customer { Id = 1, Name = "Alice" },
            new Customer { Id = 2, Name = "Bob" }
        };

        var orders = new[]
        {
            new Order { Id = 101, CustomerId = 1 },
            new Order { Id = 102, CustomerId = 2 }
        };

        return Layout.Vertical(
            Text.H2("Navigation Beacons Demo"),
            Text.P("Apps can advertise their ability to handle entity types using Navigation Beacons."),

            new Card(Layout.Vertical(
                Text.H2("Customer Beacon"),
                Text.P(customerBeacon != null
                    ? "✓ Available (BeaconCustomerDetailsApp registered)"
                    : "✗ Not available"),

                Layout.Horizontal(
                    customers.Select(customer =>
                        new Button($"View {customer.Name}")
                            .Disabled(customerBeacon == null)
                            .OnClick(() =>
                            {
                                if (customerBeacon != null)
                                    navigator.Navigate(customerBeacon, customer);
                            })
                    )
                )
            )),

            new Card(Layout.Vertical(
                Text.H2("Order Beacon"),
                Text.P(orderBeacon != null
                    ? "✓ Available (BeaconOrderDetailsApp registered)"
                    : "✗ Not available"),

                Layout.Horizontal(
                    orders.Select(order =>
                        new Button($"View Order #{order.Id}")
                            .Disabled(orderBeacon == null)
                            .OnClick(() =>
                            {
                                if (orderBeacon != null)
                                    navigator.Navigate(orderBeacon, order);
                            })
                    )
                )
            ))
        );
    }
}

public class BeaconOverview : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            new Card(Layout.Vertical(
                Text.H2("What are Beacons?"),
                Text.P("Navigation Beacons provide a type-safe way to navigate between apps without hardcoding App IDs."),
                Text.P("By registering a beacon for an entity type, an app essentially says: 'I know how to show details for this type of object'.")
            )),

            new Card(Layout.Vertical(
                Text.H2("Benefits"),
                Layout.Vertical(
                    Text.P("• Apps don't need to know each other's IDs"),
                    Text.P("• Type-safe navigation with compile-time checking"),
                    Text.P("• Dynamic discovery of navigation capabilities"),
                    Text.P("• Graceful handling when target app is unavailable")
                )
            ))
        );
    }
}
