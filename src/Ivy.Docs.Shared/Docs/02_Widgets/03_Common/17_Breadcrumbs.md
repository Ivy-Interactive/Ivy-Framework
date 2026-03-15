---
searchHints:
  - breadcrumbs
  - navigation
  - trail
  - hierarchy
  - path
  - crumbs
---

# Breadcrumbs

<Ingress>
A secondary navigation component that shows the user's location within a hierarchy. Perfect for multi-level navigation and hierarchical applications.
</Ingress>

The `Breadcrumbs` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) renders a navigation trail showing the user's location within a hierarchy. Each item is clickable (except the current/last item), enabling quick navigation to ancestor pages.

## Basic Usage

Create a simple breadcrumb trail:

```csharp demo-below
new Breadcrumbs(
    new BreadcrumbItem("Home", () => { }),
    new BreadcrumbItem("Products", () => { }),
    new BreadcrumbItem("Details")
)
```

The last item in the list is rendered as non-clickable, representing the current page.

## With Navigation

Integrate breadcrumbs with state management for navigation:

```csharp demo-tabs
public class BreadcrumbsNavigationDemo : ViewBase
{
    public override object? Build()
    {
        var currentPage = UseState("Details");

        return new Breadcrumbs(
            new BreadcrumbItem("Home", () => currentPage.Set("Home")),
            new BreadcrumbItem("Products", () => currentPage.Set("Products")),
            new BreadcrumbItem("Product Details")
        );
    }
}
```

## Configuration Options

### Custom Separator

Change the separator character between items:

```csharp demo-below
new Breadcrumbs(
    new BreadcrumbItem("Home", () => { }),
    new BreadcrumbItem("Products", () => { }),
    new BreadcrumbItem("Details")
).Separator(">")
```

### With Icons

Add icons to breadcrumb items:

```csharp demo-below
new Breadcrumbs(
    new BreadcrumbItem("Home", () => { }, Icons.House),
    new BreadcrumbItem("Products", () => { }, Icons.ShoppingCart),
    new BreadcrumbItem("Details")
)
```

### Tooltips

Add helpful tooltips to items:

```csharp demo-tabs
public class BreadcrumbsTooltipDemo : ViewBase
{
    public override object? Build()
    {
        return new Breadcrumbs(
            new BreadcrumbItem("Home", () => { }, Tooltip: "Go to homepage"),
            new BreadcrumbItem("Products", () => { }, Tooltip: "View all products"),
            new BreadcrumbItem("Details")
        );
    }
}
```

### Disabled State

Disable the entire breadcrumb trail or individual items:

```csharp demo-below
new Breadcrumbs(
    new BreadcrumbItem("Home", () => { }),
    new BreadcrumbItem("Products", Disabled: true),
    new BreadcrumbItem("Details")
).Disabled()
```

<WidgetDocs Type="Ivy.Breadcrumbs" ExtensionTypes="Ivy.BreadcrumbsExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Breadcrumbs.cs"/>
