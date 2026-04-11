namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.ChevronRight, group: ["Widgets", "Primitives"], searchHints: ["breadcrumbs", "navigation", "trail", "hierarchy", "path"])]
public class BreadcrumbsApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Basic", new BreadcrumbsBasicExample()),
            new Tab("Separator", new BreadcrumbsSeparatorExample()),
            new Tab("Icons", new BreadcrumbsIconsExample()),
            new Tab("Disabled", new BreadcrumbsDisabledExample())
        ).Variant(TabsVariant.Content);
    }
}

public class BreadcrumbsBasicExample : ViewBase
{
    public override object? Build()
    {
        var currentPage = UseState("Home");

        var items = currentPage.Value switch
        {
            "Home" => new[]
            {
                new BreadcrumbItem("Home")
            },
            "Products" => new[]
            {
                new BreadcrumbItem("Home", () => currentPage.Set("Home")),
                new BreadcrumbItem("Products")
            },
            "Details" => new[]
            {
                new BreadcrumbItem("Home", () => currentPage.Set("Home")),
                new BreadcrumbItem("Products", () => currentPage.Set("Products")),
                new BreadcrumbItem("Product Details")
            },
            _ => Array.Empty<BreadcrumbItem>()
        };

        return Layout.Vertical()
            | new Breadcrumbs(items)
            | (Layout.Horizontal().Gap(2)
                | new Button("Navigate to Products", () => currentPage.Set("Products")).Secondary()
                | new Button("Navigate to Details", () => currentPage.Set("Details")).Secondary()
                | new Button("Back to Home", () => currentPage.Set("Home")).Outline()
            )
            | Text.P($"Current page: {currentPage.Value}").Muted();
    }
}

public class BreadcrumbsSeparatorExample : ViewBase
{
    public override object? Build()
    {
        var items = new[]
        {
            new BreadcrumbItem("Home"),
            new BreadcrumbItem("Products"),
            new BreadcrumbItem("Details")
        };

        return new Breadcrumbs(items).Separator(">");
    }
}

public class BreadcrumbsIconsExample : ViewBase
{
    public override object? Build()
    {
        return new Breadcrumbs(
            new BreadcrumbItem("Home", null, Icons.House),
            new BreadcrumbItem("Products", null, Icons.ShoppingCart),
            new BreadcrumbItem("Details")
        );
    }
}

public class BreadcrumbsDisabledExample : ViewBase
{
    public override object? Build()
    {
        var items = new[]
        {
            new BreadcrumbItem("Home"),
            new BreadcrumbItem("Products"),
            new BreadcrumbItem("Details")
        };

        return new Breadcrumbs(items).Disabled();
    }
}
