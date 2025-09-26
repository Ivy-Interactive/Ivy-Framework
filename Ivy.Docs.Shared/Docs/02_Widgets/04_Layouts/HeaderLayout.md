# HeaderLayout

<Ingress>
HeaderLayout provides a fixed header area above scrollable content, perfect for toolbars, navigation, and persistent controls that should remain visible while users scroll through content.
</Ingress>

The `HeaderLayout` widget creates a layout with a fixed header section at the top and a scrollable content area below. Perfect for applications that need persistent navigation, toolbars, or status information while allowing the main content to scroll independently.

## Basic Usage

The simplest HeaderLayout takes a header and content as parameters:

```csharp demo-tabs
public class BasicHeaderExample : ViewBase
{
    public override object? Build()
    {
        return new HeaderLayout(
            header: new Card("Fixed Header Content")
                .Title("Header Area"),
            content: Layout.Vertical().Gap(4)
                | Text.P("The header above remains fixed while content scrolls.")
                | Text.P("Add as much content as needed.")
        );
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
            Layout.Horizontal().Gap(4)
                | searchText.ToTextInput()
                    .Placeholder("Search items...")
                    .Variant(TextInputs.Search)
                | new Button("Add Item")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => client.Toast("Add item clicked"))
                | new Button("Filter")
                    .Icon(Icons.Search)
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => client.Toast("Filter clicked"))
                | new Button("Export")
                    .Icon(Icons.Download)
                    .Variant(ButtonVariant.Ghost)
                    .HandleClick(_ => client.Toast("Export clicked"))
        );

        var content = Layout.Vertical().Gap(4)
            | new Card("Item 1 - This is some sample content")
            | new Card("Item 2 - More content that will scroll");
        return new HeaderLayout(toolbar, content);
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
            Layout.Horizontal().Gap(4)
                | Text.P("Analytics Dashboard")
                | new Spacer()
                | Layout.Horizontal().Gap(3)
                    | new Badge("Live")
                    | new Button("Refresh")
                        .Icon(Icons.RefreshCw)
                        .Variant(ButtonVariant.Outline)
                        .HandleClick(_ => client.Toast("Refreshing data..."))
        );

        var dashboardContent = Layout.Grid().Columns(3).Rows(2).Gap(4)
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.Small("Users")
                    | Text.Label("12.3K").Color(Colors.Primary)
            ).GridColumn(1).GridRow(1)
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.Small("Revenue")
                    | Text.Label("$54K").Color(Colors.Primary)
            ).GridColumn(2).GridRow(1)
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.Small("Growth")
                    | Text.Label("+23%").Color(Colors.Primary)
            ).GridColumn(3).GridRow(1)
            | new Card("Chart Area - Interactive dashboard content")
                .GridColumn(1).GridRow(2).GridColumnSpan(2)
            | new Card("Performance Metrics - System health status")
                .GridColumn(3).GridRow(2);

        return new HeaderLayout(dashboardHeader, dashboardContent);
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
            Layout.Horizontal().Gap(3)
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
                | new Button("Advanced")
                    .Variant(currentSection.Value == "advanced" ? ButtonVariant.Primary : ButtonVariant.Ghost)
                    .HandleClick(_ => {
                        currentSection.Value = "advanced";
                        client.Toast("Navigated to Advanced");
                    })
                | new Spacer()
                | new Button("Export").Icon(Icons.Download).Variant(ButtonVariant.Outline)
        );

        object GetSectionContent()
        {
            return currentSection.Value switch
            {
                "introduction" => Layout.Vertical().Gap(4)
                    | Text.Label("Introduction")
                    | Text.P("Welcome to our comprehensive guide. This section covers the fundamental concepts you need to understand.")
                    | new Card("Key concepts highlighted here")
                    | new Card("Getting familiar with the framework")
                    | new Card("Understanding core principles")
                    | Text.P("Continue reading to learn more about the framework's capabilities."),
                
                "getting-started" => Layout.Vertical().Gap(4)
                    | Text.Label("Getting Started")
                    | Text.P("Follow these steps to get started quickly with your first project.")
                    | new Card("Step 1: Install the framework")
                    | new Card("Step 2: Create your first app")
                    | new Card("Step 3: Build and run")
                    | Text.Code("npm install ivy-framework")
                    | Text.P("Once installed, you can start building amazing applications."),
                
                "advanced" => Layout.Vertical().Gap(4)
                    | Text.Label("Advanced Topics")
                    | Text.P("Advanced usage patterns and techniques for experienced developers.")
                    | new Card("Custom components and widgets")
                    | new Card("Performance optimization techniques")
                    | new Card("Advanced state management")
                    | new Card("Integration with external services")
                    | Text.P("These topics require a solid understanding of the framework basics."),
                
                _ => Text.P("Section not found")
            };
        }

        return new HeaderLayout(navHeader, GetSectionContent());
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
            Layout.Horizontal().Gap(4)
                | Text.Label("Edit Profile")
                | new Spacer().Width(Size.Grow())
                | new Button("Cancel").Variant(ButtonVariant.Ghost)
                | new Button("Save").Variant(ButtonVariant.Primary)
                        .HandleClick(_ => client.Toast("Profile saved!"))
        );

        var formContent = Layout.Vertical().Gap(4)
            | new Card(
                Layout.Vertical().Gap(3)
                    | Text.Small("Personal Information")
                    | name.ToTextInput().Placeholder("Full Name")
                    | email.ToTextInput().Placeholder("Email")
                    | bio.ToTextInput().Placeholder("Bio").Variant(TextInputs.Textarea)
            )
            | new Card(
                Layout.Vertical().Gap(3)
                    | Text.Small("Account Settings")
                    | new BoolInput<bool>(UseState(true)).Label("Email notifications")
                    | new BoolInput<bool>(UseState(false)).Label("SMS notifications")
                    | new BoolInput<bool>(UseState(true)).Label("Marketing emails")
            )
            | new Card(
                Layout.Vertical().Gap(3)
                    | Text.Small("Privacy")
                    | new BoolInput<bool>(UseState(true)).Label("Profile visibility")
                    | new BoolInput<bool>(UseState(false)).Label("Show online status")
            );

        return new HeaderLayout(formHeader, formContent);
    }
}
```

<Callout Type="tip">
HeaderLayout automatically handles height calculations and scrolling behavior, making it perfect for full-height application layouts.
</Callout>

<WidgetDocs Type="Ivy.HeaderLayout" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Layouts/HeaderLayout.cs"/>
