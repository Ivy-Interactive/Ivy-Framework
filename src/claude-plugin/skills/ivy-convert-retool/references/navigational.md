# Navigational

Navigational menus and UI elements for web apps. Retool provides six navigational components: Breadcrumbs, Navigation, Page Input, Pagination, Steps, and Tabs. These components handle primary/secondary nav menus, paginated data navigation, stepped workflows, and tabbed interfaces.

---

## Breadcrumbs

A secondary navigation menu to trigger actions. Displays a trail of clickable items with icons and tooltips.

### Retool

```toolscript
// Configure breadcrumbs with mapped options
breadcrumbs1.itemMode = "dynamic"
// Labels and icons are mapped from data
// Event: Click - fires when an item is clicked

breadcrumbs1.setValue("step2");
breadcrumbs1.clearValue();
breadcrumbs1.setDisabled(true);
```

### Ivy

Ivy does not have a dedicated Breadcrumbs widget. Navigation trails can be built manually using buttons/text with the `UseNavigation()` hook.

```csharp
// Manual breadcrumb-style navigation
var navigator = UseNavigation();

return Layout.Row(
    new Button("Home").OnClick(() => navigator.Navigate(typeof(HomeApp))),
    Text.Default(" / "),
    new Button("Users").OnClick(() => navigator.Navigate(typeof(UsersApp))),
    Text.Default(" / "),
    Text.Default("Profile")
);
```

### Parameters

| Parameter              | Documentation                                              | Ivy           |
|------------------------|------------------------------------------------------------|---------------|
| `labels`               | List of labels for each breadcrumb item                    | Not supported |
| `value`                | The currently selected value                               | Not supported |
| `iconByIndex`          | List of icons for each item                                | Not supported |
| `iconPositionByIndex`  | Icon position (left/right) relative to labels              | Not supported |
| `tooltipByIndex`       | List of tooltips for each item                             | Not supported |
| `disabledByIndex`      | Per-item disabled state                                    | Not supported |
| `itemMode`             | Options config mode: `dynamic` (mapped) or `static`        | Not supported |
| `disabled`             | Whether the entire component is disabled                   | Not supported |
| `hidden`               | Whether the component is hidden                            | Not supported |
| `events` (Click)       | Fires when an item is clicked                              | Not supported |

---

## Navigation

A primary navigation menu with nested items, supporting horizontal/vertical orientation and a logo image. Items can link directly to other apps or pages.

### Retool

```toolscript
// Navigation with nested items and a logo
navigation1.itemMode = "dynamic"
// Supports logo via src/srcType/retoolStorageFileId
// Items can be linked to apps, auto-highlighting current app
// Events: Click, Logo Click

navigation1.setDisabled(false);
navigation1.setHidden(false);
```

### Ivy

Ivy handles primary navigation via Chrome configuration (tabs/sidebar) and the `UseNavigation()` hook. There is also a `DropDownMenu` for nested menu structures.

```csharp
// Chrome-level navigation (configured at app startup)
app.UseAppShell(AppShellSettings.Default()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<DashboardApp>()
);

// Programmatic navigation
var navigator = UseNavigation();
navigator.Navigate(typeof(DashboardApp));
navigator.Navigate(typeof(UsersApp));

// Nested menu via DropDownMenu
new DropDownMenu(
    evt => navigator.Navigate(evt.Value.ToString()),
    new Button("Menu"),
    MenuItem.Default("Dashboard"),
    MenuItem.Default("Users").Children(
        MenuItem.Default("All Users"),
        MenuItem.Default("Active Users")
    ),
    MenuItem.Default("Settings")
)
```

### Parameters

| Parameter              | Documentation                                              | Ivy                                      |
|------------------------|------------------------------------------------------------|------------------------------------------|
| `itemMode`             | Options config: `dynamic` or `static`                      | DropDownMenu `Items` array               |
| `appTargetByIndex`     | Linked app IDs for each nav item                           | `navigator.Navigate(typeof(App))`        |
| `screenTargetByIndex`  | Linked screen targets per item                             | `navigator.Navigate("app://name")`       |
| `iconByIndex`          | Icons for each item                                        | DropDownMenu items (no icon support)      |
| `captionByIndex`       | Captions for each item                                     | Not supported                            |
| `horizontalAlignment`  | Content alignment (left/center/right/justify)              | DropDownMenu `.Align()`                  |
| `src` / `srcType`      | Logo image source                                          | Not supported                            |
| `altText`              | Accessible description for logo                            | Not supported                            |
| `disabled`             | Whether the component is disabled                          | Not supported                            |
| `hidden`               | Whether the component is hidden                            | `Visible` property                       |
| `events` (Click)       | Fires when a nav item is clicked                           | DropDownMenu `onSelect`                  |
| `events` (Logo Click)  | Fires when the logo is clicked                             | Not supported                            |

---

## Page Input

An input field to jump to a specific page of data. Displays the total number of pages and the currently selected page.

### Retool

```toolscript
// Page input with max pages and prefix/suffix text
pageInput1.max = 50
pageInput1.textBefore = "Page"
pageInput1.textAfter = "of 50"

pageInput1.setValue(5);
pageInput1.clearValue();
pageInput1.resetValue();
```

### Ivy

Ivy does not have a dedicated Page Input widget. The `Pagination` widget provides page navigation with numbered links. A number input could be used for direct page entry.

```csharp
// Using Pagination for page navigation
var page = UseState(1);
return new Pagination(page.Value, 50, newPage => page.Set(newPage.Value));

// Or a manual text input approach for direct page jump
// (no built-in Page Input equivalent)
```

### Parameters

| Parameter      | Documentation                                     | Ivy                         |
|----------------|---------------------------------------------------|-----------------------------|
| `max`          | Maximum page number                               | Pagination `numPages`       |
| `value`        | Current page number (read-only)                   | Pagination `page`           |
| `textBefore`   | Prefix text (e.g., "Page")                        | Not supported               |
| `textAfter`    | Suffix text (e.g., "of 50")                       | Not supported               |
| `disabled`     | Whether the input is disabled                     | Pagination `Disabled`       |
| `hidden`       | Whether the component is hidden                   | Pagination `Visible`        |
| `horizontalAlign` | Content alignment                              | Not supported               |
| `events` (Change) | Fires when the value changes                   | Pagination `onChange`       |

---

## Pagination

A navigation menu to jump to a specific page of data using numbered links. Adjusts visible page numbers to fit width.

### Retool

```toolscript
// Pagination with max pages
pagination1.max = 20

pagination1.setValue(5);
pagination1.clearValue();
pagination1.resetValue();
// Event: Change - fires when page changes
```

### Ivy

```csharp
var page = UseState(5);

return new Pagination(page.Value, 20, newPage => page.Set(newPage.Value))
    .Siblings(2)
    .Boundaries(1);
```

### Parameters

| Parameter      | Documentation                                     | Ivy                         |
|----------------|---------------------------------------------------|-----------------------------|
| `max`          | Maximum number of pages                           | `numPages`                  |
| `valueNumber`  | Current page number (read-only)                   | `page`                      |
| `disabled`     | Whether the component is disabled                 | `Disabled`                  |
| `hidden`       | Whether the component is hidden                   | `Visible`                   |
| `margin`       | Margin spacing                                    | Not supported               |
| `style`        | Custom style options                              | `Scale`                     |
| N/A            | N/A                                               | `Siblings` (adjacent pages) |
| N/A            | N/A                                               | `Boundaries` (edge pages)   |
| `events` (Change) | Fires when the page value changes              | `onChange`                  |

---

## Steps

A group of sequential step indicators that can be linked to a Container to create a Stepped Container. Supports horizontal/vertical orientation, step numbers, and completed-step indicators.

### Retool

```toolscript
// Steps linked to a container
steps1.itemMode = "dynamic"
steps1.orientation = "horizontal"
steps1.showStepNumbers = true
steps1.indicateCompletedSteps = true
steps1.targetContainerId = "container1"

steps1.setValue("step2");
steps1.resetValue();
```

### Ivy

Ivy does not have a dedicated Steps/Stepper widget. A TabsLayout can approximate stepped workflows, or steps can be built manually with state management.

```csharp
// Approximate using TabsLayout
var step = UseState(0);

return Layout.Tabs(
    new Tab("1. Details", BuildDetailsStep()),
    new Tab("2. Review", BuildReviewStep()),
    new Tab("3. Confirm", BuildConfirmStep())
).OnSelect(e => step.Set(e.Value));
```

### Parameters

| Parameter               | Documentation                                         | Ivy                             |
|-------------------------|-------------------------------------------------------|---------------------------------|
| `labels`                | List of step labels                                   | Tab labels in TabsLayout        |
| `captionByIndex`        | Captions for each step                                | Not supported                   |
| `value`                 | Currently selected step value                         | TabsLayout `SelectedIndex`      |
| `selectedIndex`         | Selected step index                                   | TabsLayout `selectedIndex`      |
| `selectedLabel`         | Label of the selected step                            | Not supported                   |
| `orientation`           | Horizontal or vertical layout                         | Not supported (horizontal only) |
| `showStepNumbers`       | Display step numbers                                  | Not supported                   |
| `indicateCompletedSteps`| Show check marks on completed steps                   | Not supported                   |
| `navigateContainer`     | Whether linked to a container                         | Not supported                   |
| `targetContainerId`     | ID of the linked container                            | Not supported                   |
| `tooltipByIndex`        | Tooltips per step                                     | Not supported                   |
| `itemMode`              | Options config: `dynamic` or `static`                 | Tab array                       |
| `disabled`              | Whether interaction is disabled                       | Not supported                   |
| `hidden`                | Whether the component is hidden                       | TabsLayout `Visible`            |

---

## Tabs

A group of clickable tab elements. Can be linked to a Container to create a Tabbed Container. Supports icons, style variants (solid, line, pill), and alignment.

### Retool

```toolscript
// Tabs with style variant and container link
tabs1.itemMode = "dynamic"
tabs1.styleVariant = "pill"  // solid | lineBottom | pill
tabs1.alignment = "center"
tabs1.linePosition = "bottom"
tabs1.navigateContainer = true
tabs1.targetContainerId = "container1"

tabs1.setValue("tab2");
tabs1.clearValue();
tabs1.resetValue();
tabs1.setDisabled(false);
// Event: Change - fires when selection changes
```

### Ivy

```csharp
Layout.Tabs(
    new Tab("Customers", BuildCustomerList()).Icon(Icons.User).Badge("10"),
    new Tab("Orders", BuildOrderList()).Icon(Icons.DollarSign).Badge("5"),
    new Tab("Settings", BuildSettings()).Icon(Icons.Settings)
)
.Variant(TabsVariant.Tabs)  // Tabs | Content
.OnSelect(e => Console.WriteLine($"Selected tab: {e.Value}"))
.OnClose(e => Console.WriteLine($"Closed tab: {e.Value}"))
```

### Parameters

| Parameter              | Documentation                                      | Ivy                                 |
|------------------------|---------------------------------------------------|--------------------------------------|
| `labels`               | List of tab labels                                | Tab constructor `label` parameter    |
| `values`               | List of possible tab values                       | Tab array                            |
| `value`                | Currently selected tab value                      | `SelectedIndex`                      |
| `selectedIndex`        | Selected tab index                                | `selectedIndex` constructor param    |
| `selectedLabel`        | Label of the selected tab                         | Not supported (use SelectedIndex)    |
| `selectedItem`         | The selected item data                            | Not supported                        |
| `alignment`            | Horizontal alignment (left/center/right)          | Not supported                        |
| `styleVariant`         | Visual style: `solid`, `lineBottom`, `pill`       | `Variant` (Tabs, Content)            |
| `linePosition`         | Position of indicator line (top/bottom/left/right)| Not supported                        |
| `iconByIndex`          | Icons for each tab                                | `.Icon(Icons.Name)`                  |
| `iconPositionByIndex`  | Icon position (left/right)                        | Not supported (left only)            |
| `tooltipByIndex`       | Tooltips per tab                                  | Not supported                        |
| `disabledByIndex`      | Per-tab disabled state                            | Not supported                        |
| `hiddenByIndex`        | Per-tab hidden state                              | Not supported                        |
| `navigateContainer`    | Whether linked to a container                     | N/A (content is inline)              |
| `targetContainerId`    | ID of the linked container                        | N/A (content is inline)              |
| `itemMode`             | Options config: `dynamic` or `static`             | Tab array                            |
| `disabled`             | Whether the component is disabled                 | Not supported                        |
| `hidden`               | Whether the component is hidden                   | `Visible`                            |
| N/A                    | N/A                                               | `.Badge("count")` per tab            |
| N/A                    | N/A                                               | `.OnClose()` closable tabs           |
| N/A                    | N/A                                               | `.OnRefresh()` refresh handler       |
| N/A                    | N/A                                               | `.OnReorder()` drag-and-drop reorder |
| `events` (Change)      | Fires when the selected tab changes               | `onSelect`                           |
