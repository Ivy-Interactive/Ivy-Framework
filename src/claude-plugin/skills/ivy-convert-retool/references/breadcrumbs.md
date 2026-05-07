# Breadcrumbs

A secondary navigation menu that displays a trail of links representing the user's location within a hierarchy. Each item can trigger actions on click, display icons, tooltips, and link to other apps or screens.

## Retool

```toolscript
// Breadcrumbs with mapped options
breadcrumbs1.itemMode = "dynamic"
breadcrumbs1.labels = ["Home", "Products", "Details"]
breadcrumbs1.events = [{ event: "click", type: "script" }]

// Methods
breadcrumbs1.setValue("Details")
breadcrumbs1.clearValue()
breadcrumbs1.resetValue()
breadcrumbs1.setDisabled(true)
breadcrumbs1.setHidden(false)
breadcrumbs1.scrollIntoView({ behavior: "smooth", block: "nearest" })
```

## Ivy

Ivy does not have a Breadcrumbs widget. Navigation between apps is handled via `UseNavigation()`:

```csharp
var nav = UseNavigation();

new Button("Home", () => nav.Navigate<HomeApp>());
new Button("Products", () => nav.Navigate<ProductsApp>());
new Text("Details"); // current page, no link
```

## Parameters

| Parameter              | Documentation                                                          | Ivy           |
|------------------------|------------------------------------------------------------------------|---------------|
| labels                 | A list of labels for each breadcrumb item                              | Not supported |
| value                  | The currently selected value                                           | Not supported |
| data                   | Custom data attached to the component                                  | Not supported |
| disabled               | Whether interaction is disabled                                        | Not supported |
| disabledByIndex        | Per-item disabled state by index                                       | Not supported |
| itemMode               | Configuration mode: `dynamic` (mapped) or `static` (manual)           | Not supported |
| iconByIndex            | A list of icons for each item by index                                 | Not supported |
| iconPositionByIndex    | Icon position (left/right) for each item by index                      | Not supported |
| tooltipByIndex         | A list of tooltips for each item by index                              | Not supported |
| tooltipText            | Tooltip text displayed on hover near the label                         | Not supported |
| appTargetByIndex       | A list of app IDs for each item to navigate to                         | Not supported |
| screenTargetIdByIndex  | Screen targets for each item by index                                  | Not supported |
| clickable              | Whether there is an enabled Click event handler                        | Not supported |
| events                 | Configured event handlers that trigger actions                         | Not supported |
| isHiddenOnDesktop      | Whether hidden in the desktop layout                                   | Not supported |
| isHiddenOnMobile       | Whether hidden in the mobile layout                                    | Not supported |
| maintainSpaceWhenHidden| Whether to reserve space when hidden                                   | Not supported |
| showInEditor           | Whether visible in the editor when hidden                              | Not supported |
| margin                 | Margin around the component (`4px 8px` or `0`)                         | Not supported |
| style                  | Custom style options                                                   | Not supported |
| id                     | Unique identifier (name) for the component                             | Not supported |
