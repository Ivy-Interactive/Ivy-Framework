# HeaderLayout

<Ingress>
HeaderLayout provides a fixed header area above scrollable content, perfect for toolbars, navigation, and persistent controls that should remain visible while users scroll through content.
</Ingress>

The `HeaderLayout` widget creates a layout with a fixed header section at the top and a scrollable content area below. This pattern is commonly used for applications that need persistent navigation, toolbars, or status information while allowing the main content to scroll independently.

## Basic Usage

The simplest HeaderLayout takes a header and content as parameters:

```csharp demo-tabs
public class BasicHeaderExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
new HeaderLayout(
                header: new Card("Fixed Header Content")
                    .Title("Header Area")
                    .Height(Size.Units(30)),
                content: new Card(
                    Layout.Vertical().Gap(2)
                        | Text.P("This is scrollable content area.")
                        | Text.P("The header above will remain fixed while this content scrolls.")
                        | Text.P("You can add as much content as needed here.")
                ).Height(Size.Units(50))
            )
        ).Height(Size.Units(100));
    }
}
```

## Common Use Cases

### Toolbar with Actions

HeaderLayout is perfect for creating toolbars with action buttons that remain accessible:

```csharp demo-tabs
public class ToolbarExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var searchText = UseState("");
        
        var toolbar = new Card(
            Layout.Horizontal().Gap(2).Padding(2)
                | searchText.ToTextInput()
                    .Placeholder("Search items...")
                    .Variant(TextInputs.Search)
                | new Button("Add Item")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => client.Toast("Add item clicked"))
                | new Button("Filter")
                    .Icon(Icons.Filter)
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => client.Toast("Filter clicked"))
                | new Button("Export")
                    .Icon(Icons.Download)
                    .Variant(ButtonVariant.Ghost)
                    .HandleClick(_ => client.Toast("Export clicked"))
        ).Height(Size.Units(20));

        var content = new Card(
            Layout.Vertical().Gap(2)
                | new Card("Item 1 - This is some sample content").Height(Size.Units(20))
                | new Card("Item 2 - More content that will scroll").Height(Size.Units(20))
        ).Height(Size.Units(50));

        return Layout.Vertical(
            new HeaderLayout(toolbar, content)
        ).Height(Size.Units(90));
    }
}
```

### Dashboard Header

Use HeaderLayout for dashboard-style interfaces with status indicators and quick actions:

```csharp demo-tabs
public class DashboardHeaderExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        
        var dashboardHeader = new Card(
            Layout.Horizontal().Gap(2).Padding(1)
                | Text.H4("Analytics Dashboard")
                | new Spacer()
                | Layout.Horizontal().Gap(1)
                    | new Badge("Live")
                    | new Button("Refresh")
                        .Icon(Icons.RefreshCw)
                        .Variant(ButtonVariant.Outline)
                        .HandleClick(_ => client.Toast("Refreshing data..."))
        ).Height(Size.Units(20));

        var dashboardContent = new Card(
            Layout.Vertical().Gap(1)
                | Layout.Horizontal().Gap(1)
                    | new Card(
                        Layout.Vertical().Gap(0)
                            | Text.Small("Users")
                            | Text.H3("12.3K").Color(Colors.Primary)
                    ).Height(Size.Units(25))
                    | new Card(
                        Layout.Vertical().Gap(0)
                            | Text.Small("Revenue")
                            | Text.H3("$54K").Color(Colors.Primary)
                    ).Height(Size.Units(25))
                | new Card("Chart Area").Height(Size.Units(20))
        ).Height(Size.Units(85));

        return Layout.Vertical(
            new HeaderLayout(dashboardHeader, dashboardContent)
        ).Height(Size.Units(120));
    }
}
```

### Navigation Header

Create navigation headers for content-heavy pages:

```csharp demo-tabs
public class NavigationHeaderExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var currentSection = UseState("introduction");
        
        var navHeader = new Card(
            Layout.Horizontal().Gap(1).Padding(1)
                | new Button("Intro")
                    .Variant(currentSection.Value == "introduction" ? ButtonVariant.Primary : ButtonVariant.Ghost)
                    .HandleClick(_ => {
                        currentSection.Value = "introduction";
                        client.Toast("Navigated to Introduction");
                    })
                | new Button("Guide")
                    .Variant(currentSection.Value == "getting-started" ? ButtonVariant.Primary : ButtonVariant.Ghost)
                    .HandleClick(_ => {
                        currentSection.Value = "getting-started";
                        client.Toast("Navigated to Getting Started");
                    })
                | new Spacer()
                | new Button("Export").Icon(Icons.Download).Variant(ButtonVariant.Outline)
        ).Height(Size.Units(18));

        object GetSectionContent()
        {
            object content = currentSection.Value switch
            {
                "introduction" => Layout.Vertical().Gap(1)
                    | Text.H3("Introduction")
                    | Text.P("Welcome to our comprehensive guide.")
                    | new Card("Key concepts highlighted here").Height(Size.Units(15)),
                
                "getting-started" => Layout.Vertical().Gap(1)
                    | Text.H3("Getting Started")
                    | Text.P("Follow these steps to get started.")
                    | new Card("npm install ivy-framework").Height(Size.Units(15)),
                
                "advanced" => Layout.Vertical().Gap(1)
                    | Text.H3("Advanced Topics")
                    | Text.P("Advanced usage patterns and techniques.")
                    | new Card("Advanced features").Height(Size.Units(15)),
                
                _ => (object)Text.P("Section not found")
            };
            
            return new Card(content).Height(Size.Units(60));
        }

        return Layout.Vertical(
            new HeaderLayout(navHeader, GetSectionContent())
        ).Height(Size.Units(95));
    }
}
```

### Form with Header Actions

HeaderLayout works well for forms with header-level actions:

```csharp demo-tabs
public class FormHeaderExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var name = UseState("John Doe");
        var email = UseState("john@example.com");
        var bio = UseState("Software developer with 5 years of experience...");

        var formHeader = new Card(
            Layout.Horizontal().Gap(1).Padding(1)
                | Text.H4("Edit Profile")
                | new Spacer()
                | new Button("Cancel").Variant(ButtonVariant.Ghost)
                | new Button("Save").Variant(ButtonVariant.Primary)
                    .HandleClick(_ => client.Toast("Profile saved!"))
        ).Height(Size.Units(18));

        var formContent = new Card(
            Layout.Vertical().Gap(1)
                | new Card(
                    Layout.Vertical().Gap(1)
                        | Text.Small("Personal Information")
                        | name.ToTextInput().Placeholder("Full Name")
                        | email.ToTextInput().Placeholder("Email")
                ).Height(Size.Units(35))
                | new Card(
                    Layout.Vertical().Gap(1)
                        | Text.Small("Preferences")
                        | new BoolInput<bool>(UseState(true)).Label("Email notifications")
                ).Height(Size.Units(25))
        ).Height(Size.Units(75));

        return Layout.Vertical(
            new HeaderLayout(formHeader, formContent)
        ).Height(Size.Units(110));
    }
}
```

<Callout Type="tip">
HeaderLayout automatically handles height calculations and scrolling behavior, making it perfect for full-height application layouts.
</Callout>

<WidgetDocs Type="Ivy.HeaderLayout" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Layouts/HeaderLayout.cs"/>
