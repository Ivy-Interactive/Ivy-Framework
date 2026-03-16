namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.ChevronRight, path: ["Widgets"], searchHints: ["breadcrumbs", "navigation", "trail", "hierarchy", "path"])]
public class BreadcrumbsApp : SampleBase
{
    protected override object? BuildSample()
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
                new BreadcrumbItem("Home", () => currentPage.Set("Home"), Icons.House),
                new BreadcrumbItem("Products", () => currentPage.Set("Products")),
                new BreadcrumbItem("Product Details")
            },
            _ => Array.Empty<BreadcrumbItem>()
        };

        return Layout.Vertical()
            | Text.H1("Breadcrumbs")
            | Text.H2("Basic Usage")
            | new Breadcrumbs(items)
            | Text.H2("With Custom Separator")
            | new Breadcrumbs(items).Separator(">")
            | Text.H2("With Icons")
            | new Breadcrumbs(
                new BreadcrumbItem("Home", () => currentPage.Set("Home"), Icons.House),
                new BreadcrumbItem("Products", () => currentPage.Set("Products"), Icons.ShoppingCart),
                new BreadcrumbItem("Details")
            )
            | Text.H2("Disabled State")
            | new Breadcrumbs(items).Disabled()
            | Text.P($"Current page: {currentPage.Value}").Muted()
            | (Layout.Horizontal().Gap(2)
                | new Button("Navigate to Products", () => currentPage.Set("Products")).Secondary()
                | new Button("Navigate to Details", () => currentPage.Set("Details")).Secondary()
                | new Button("Back to Home", () => currentPage.Set("Home")).Outline()
            );
    }
}
