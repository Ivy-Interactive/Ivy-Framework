# SidebarLayout

<Ingress>
SidebarLayout provides a flexible sidebar navigation layout with a main content area and collapsible sidebar. It supports header and footer sections, responsive behavior, and can be configured as the main application sidebar with toggle functionality.
</Ingress>

Sidebars are essential navigation components in modern applications, providing users with quick access to different sections while keeping the main content area uncluttered. They can be used for primary navigation, contextual tools, or supplementary information display.

## Basic Usage

The `SidebarLayout` creates a layout with a sidebar and main content area. The sidebar can optionally include header and footer sections:

```csharp demo-tabs
public class BasicSidebarExample : ViewBase
{
    public override object? Build()
    {
        return new SidebarLayout(
            mainContent: new Card(
                "This is the main content area. It takes up the remaining space after the sidebar."
            ).Title("Main Content"),
            sidebarContent: new Card(
                "This is the sidebar content where you can put navigation, menus, or other controls."
            ).Title("Sidebar")
        );
    }
}
```

### With Header and Footer

You can add optional header and footer sections to the sidebar:

```csharp demo-tabs
public class SidebarWithHeaderFooterExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return new SidebarLayout(
            mainContent: new Card(
                Layout.Vertical().Gap(2)
                    | Text.H2("Welcome to the App")
                    | Text.P("This is the main content area with a sidebar navigation.")
                    | new Button("Action Button")
                        .Variant(ButtonVariant.Primary)
                        .HandleClick(_ => client.Toast("Action performed!"))
            ).Title("Dashboard"),
            sidebarContent: Layout.Vertical().Gap(2)
                | new Button("Home").Variant(ButtonVariant.Ghost)
                | new Button("Profile").Variant(ButtonVariant.Ghost)
                | new Button("Settings").Variant(ButtonVariant.Ghost)
                | new Button("Help").Variant(ButtonVariant.Ghost),
            sidebarHeader: Layout.Vertical().Gap(1)
                | Text.H3("Navigation")
                | new Separator(),
            sidebarFooter: Layout.Vertical().Gap(1)
                | new Separator()
                | Text.Small("Version 1.0.0").Color(Colors.Gray)
        );
    }
}
```

### SidebarMenu Integration

The `SidebarMenu` widget is designed to work seamlessly with `SidebarLayout` for navigation:

```csharp demo-tabs
public class SidebarMenuExample : ViewBase
{
    public override object? Build()
    {
        var selectedPage = UseState("home");
        var client = UseService<IClientProvider>();
        
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
                    .Icon(Icons.UserCog).Tag("account"),
                MenuItem.Default("Preferences")
                    .Icon(Icons.Settings).Tag("preferences")
            ),
            MenuItem.Default("Help").Icon(Icons.CircleHelp).Tag("help")
        };

        var sidebarMenu = new SidebarMenu(
            onSelect: evt => {
                selectedPage.Value = evt.Value.ToString()!;
                client.Toast($"Navigated to {evt.Value}");
            },
            items: menuItems
        );

        object RenderContent()
        {
            return selectedPage.Value switch
            {
                "home" => new Card("Welcome to the Dashboard!")
                    .Title("Dashboard"),
                "analytics" => new Card("Analytics and reports go here.")
                    .Title("Analytics"),
                "profile" => new Card("User profile settings.")
                    .Title("Profile"),
                "account" => new Card("Account management.")
                    .Title("Account"),
                "preferences" => new Card("User preferences.")
                    .Title("Preferences"),
                "help" => new Card("Help and documentation.")
                    .Title("Help & Support"),
                _ => new Card("Page not found.")
                    .Title("404")
            };
        }

        return new SidebarLayout(
            mainContent: RenderContent(),
            sidebarContent: sidebarMenu,
            sidebarHeader: Layout.Vertical().Gap(2)
                | Text.H3("My App")
                | new TextInput(placeholder: "Search...", variant: TextInputs.Search),
            sidebarFooter: Layout.Horizontal().Gap(2)
                | new Avatar("JD").Size(20)
                | Layout.Vertical()
                    | Text.Small("John Doe").NoWrap()
                    | Text.Small("john@example.com").Color(Colors.Gray)
        );
    }
}
```

### Main App Sidebar

When used as the main application sidebar, the layout includes a toggle button and responsive behavior:

<Callout Type="tip">
"You can also create a toggle sidebar layout by defining the sidebar as .MainAppSidebar()">
</Callout>

```csharp demo-tabs
public class MainAppSidebarExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        
        return new SidebarLayout(
            mainContent: Layout.Vertical().Gap(4)
                | new Card(
                    Layout.Vertical().Gap(2)
                        | Text.H1("Main Application")
                        | Text.P("This sidebar is configured as the main app sidebar with toggle functionality.")
                        | new Button("Test Action").HandleClick(_ => client.Toast("Action performed!"))
                ).Title("Welcome")
                | new Card(
                    "Additional content can be placed here. The sidebar will automatically collapse on smaller screens."
                ).Title("Content Area"),
            sidebarContent: Layout.Vertical().Gap(1)
                | new Button("Dashboard").Variant(ButtonVariant.Ghost).HandleClick(_ => client.Toast("Dashboard"))
                | new Button("Projects").Variant(ButtonVariant.Ghost).HandleClick(_ => client.Toast("Projects"))
                | new Button("Team").Variant(ButtonVariant.Ghost).HandleClick(_ => client.Toast("Team"))
                | new Button("Calendar").Variant(ButtonVariant.Ghost).HandleClick(_ => client.Toast("Calendar")),
            sidebarHeader: Layout.Vertical().Gap(2)
                | Text.H3("Workspace")
                | new TextInput(placeholder: "Search...", variant: TextInputs.Search)
        ); 
    }
}
```

<Callout Type="tip">
"There is default padding of 2 in main content accessible via MainContentPadding by default."
</Callout>

### SidebarMenu Widget

The `SidebarMenu` widget is specifically designed for sidebar navigation and provides advanced features like search highlighting and keyboard navigation:

```csharp demo-tabs
public class SidebarMenuAdvancedExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var selectedItem = UseState("");
        
        MenuItem[] menuItems = new[]
        {
            MenuItem.Default("Getting Started")
                .Icon(Icons.Play).Children(
                MenuItem.Default("Installation")
                    .Icon(Icons.Download).Tag("install"),
                MenuItem.Default("Quick Start")
                    .Icon(Icons.Zap).Tag("quickstart"),
                MenuItem.Default("Examples")
                    .Icon(Icons.Code).Tag("examples")
            ),
            MenuItem.Default("Components")
                .Icon(Icons.Package).Children(
                MenuItem.Default("Buttons")
                    .Icon(Icons.MousePointer).Tag("buttons"),
                MenuItem.Default("Forms")
                    .Icon(Icons.FileText).Tag("forms"),
                MenuItem.Default("Charts")
                    .Icon(Icons.ChartBar).Tag("charts"),
                MenuItem.Default("Tables")
                    .Icon(Icons.Table).Tag("tables")
            ),
            MenuItem.Default("Advanced")
                .Icon(Icons.Cpu).Children(
                MenuItem.Default("Hooks")
                .Icon(Icons.Link).Tag("hooks"),
                MenuItem.Default("State Management")
                .Icon(Icons.Database).Tag("state"),
                MenuItem.Default("Performance")
                .Icon(Icons.Gauge).Tag("performance")
            )
        };

        var menu = new SidebarMenu(
            onSelect: evt => {
                selectedItem.Value = evt.Value?.ToString() ?? "";
                client.Toast($"Selected: {evt.Value}");
            },
            items: menuItems
        ) {
            OnCtrlRightClickSelect = evt => {
                client.Toast($"Right-clicked: {evt.Value} (Ctrl+Right-click for special actions)");
            }
        };

        return new SidebarLayout(
            mainContent: new Card(
                Layout.Vertical().Gap(2)
                    | Text.H2("Documentation")
                    | Text.P($"Currently viewing: {(string.IsNullOrEmpty(selectedItem.Value) ? "None" : selectedItem.Value)}")
                    | Text.Small("Use Ctrl+Right-click on menu items for additional actions.")
            ).Title("Content Area"),
            sidebarContent: menu,
            sidebarHeader: Text.H3("Documentation Menu")
        );
    }
}
```

 <WidgetDocs Type="Ivy.SidebarLayout" ExtensionTypes="Ivy.SidebarLayoutExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Layouts/SidebarLayout.cs"/>
