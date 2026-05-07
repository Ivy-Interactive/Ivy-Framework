# Navigation

A primary navigation menu with nested items to trigger actions. Supports horizontal and vertical orientations, logo display, icons, captions, and automatic highlighting of active items. Hides items from users without access.

## Retool

```toolscript
// Navigation component with mapped items
navigation1.captionByIndex = ["Caption 1", "Caption 2"]
navigation1.iconByIndex = ["/icon/solid/home", "/icon/solid/chart-bar"]

// Event: click handler triggers on item selection
navigation1.events = [{ type: "click", action: "navigateToApp" }]

// Methods
navigation1.setDisabled(true)
navigation1.setHidden(false)
navigation1.scrollIntoView({ behavior: "smooth", block: "center" })
```

## Ivy

In Ivy, primary navigation is achieved with `SidebarLayout` + `SidebarMenu` for sidebar-style nav, or `UseNavigation()` for programmatic routing between apps.

```csharp
// Sidebar navigation with nested menu items
MenuItem[] menuItems = new[]
{
    MenuItem.Default("Dashboard")
        .Icon(Icons.LayoutDashboard).Tag("home"),
    MenuItem.Default("Analytics")
        .Icon(Icons.TrendingUp).Tag("analytics"),
    MenuItem.Default("Settings")
        .Icon(Icons.Settings).Children(
            MenuItem.Default("Profile")
                .Icon(Icons.User).Tag("profile"),
            MenuItem.Default("Account")
                .Icon(Icons.UserCog).Tag("account")
        )
};

new SidebarLayout(
    mainContent: mainView,
    sidebarContent: new SidebarMenu(onSelect: HandleNav, menuItems),
    sidebarHeader: logo,
    sidebarFooter: userInfo
) { MainAppSidebar = true };

// Programmatic navigation
var navigator = UseNavigation();
navigator.Navigate(typeof(DashboardApp));
navigator.Navigate(typeof(UserProfileApp), new UserProfileArgs(123));
```

## Parameters

| Parameter | Retool | Ivy |
|---|---|---|
| Items / Menu entries | `captionByIndex` (array of strings) | `MenuItem.Default("label")` composed in array |
| Icons per item | `iconByIndex` (array of strings) | `MenuItem.Icon(Icons.X)` fluent method |
| Nested items | Supported via manual/mapped item config | `MenuItem.Children(...)` method |
| Disabled state | `disabled` (boolean) / `disabledByIndex` (array) | Not supported (per-item) |
| Hidden state | `setHidden(hidden)` / `isHiddenOnDesktop` / `isHiddenOnMobile` | `Visible` property on SidebarLayout |
| Highlighted item | `highlightByIndex` (array of booleans, auto) | Automatic via `onSelect` + active tag |
| Logo | Built-in logo area with `src`, `srcType`, `altText` | `sidebarHeader` slot (custom content) |
| Logo click event | `Logo Click` event | Custom — attach click handler to header content |
| Item click event | `Click` event | `onSelect` callback with event tag |
| Tooltip | `tooltipText` (string) | Not supported |
| Caption per item | `captionByIndex` (array of strings) | Not supported (label only) |
| Orientation | Horizontal or vertical | Vertical only (sidebar) |
| App linking per item | `appTargetByIndex` (array of app IDs) | `UseNavigation().Navigate(typeof(App))` programmatic |
| Dynamic/mapped items | `itemMode`: "dynamic" or "manual" | Manual — build `MenuItem[]` from data in code |
| Height mode | `heightType`: "fixed" or "expand to fit" | `Height` property on SidebarLayout |
| Margin | `margin` (string, e.g. `"4px 8px"`) | Not supported (use layout nesting) |
| Horizontal alignment | `horizontalAlignment`: left/center/right/justify | Not supported (vertical layout only) |
| Scroll into view | `scrollIntoView(options)` method | Not supported |
| Responsive hiding | `isHiddenOnDesktop` / `isHiddenOnMobile` | `MainAppSidebar = true` enables responsive collapse |
| Keyboard navigation | Not documented | Supported (SidebarMenu built-in) |
| Search / filtering | Not documented | Supported (SidebarMenu search highlighting) |
