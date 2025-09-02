# Expandable

<Ingress>
Create collapsible content sections that users can expand and collapse to maintain clean, organized layouts.
</Ingress>

The `Expandable` widget allows you to hide and show content interactively, providing a clean and organized way to present information. It's perfect for organizing content into collapsible sections, FAQs, or any scenario where you want to reduce visual clutter.

## Basic Usage

Here's a simple example of an expandable widget.

```csharp demo-below
new Expandable("Click to expand", 
    "This is the hidden content that appears when you expand the widget.")
```

### Rich Content

You can include more complex content like formatted text, icons, and other widgets.

```csharp demo-tabs
new Expandable(
    "Rich Content Example", 
    Layout.Vertical().Gap(2)
        | Text.H3("Welcome to Ivy Framework")
        | Text.Muted("This expandable contains multiple elements")
        | new Badge("New").Secondary()
        | Text.Small("Click to collapse this content")
)
```

### Nested Expandables

Create hierarchical structures by nesting expandable widgets.

```csharp demo-tabs
new Expandable("Main Section", 
    Layout.Vertical().Gap(2)
        | Text.H4("Overview")
        | Text.Muted("This is the main content")
        | new Expandable("Subsection 1", "Details about subsection 1")
        | new Expandable("Subsection 2", "Details about subsection 2")
        | new Expandable("Subsection 3", "Details about subsection 3")
)
```

### Disabled

<Callout Type="info">
The Disabled property allows you to prevent users from expanding content when it's not available or relevant, improving the overall user experience.
</Callout>

Set to `true` to disable the expandable functionality

```csharp demo-tabs
Layout.Vertical().Gap(2)
    | new Expandable("Normal", "This expandable works normally")
    | new Expandable("Disabled", "This expandable is disabled").Disabled(true)
```

## Layout Integration

### In Vertical Layouts

Expandables work seamlessly with vertical layouts for organized content presentation.

```csharp demo-tabs
Layout.Vertical().Gap(2)
    | new Expandable("Getting Started", "Follow these steps to get started with Ivy Framework...")
    | new Expandable("Configuration", "Configure your application with these settings...")
    | new Expandable("Deployment", "Learn how to deploy your Ivy application...")
    | new Expandable("Troubleshooting", "Common issues and their solutions...")
```

### In Grid Layouts

Use expandables in grid layouts for organized information display.

```csharp demo-tabs
Layout.Grid().Columns(2).Gap(2)
    | new Expandable("Features", "Discover all the amazing features of Ivy Framework")
    | new Expandable("Performance", "Learn about performance optimizations and best practices")
    | new Expandable("Security", "Security considerations and best practices")
    | new Expandable("Scalability", "How to scale your Ivy applications")
```

### In Cards

Combine expandables with cards for enhanced visual presentation.

```csharp demo-tabs
Layout.Grid().Columns(2).Gap(2)
    | new Card(new Expandable("Documentation", "Access comprehensive documentation and guides"))
    | new Card(new Expandable("Examples", "Browse through practical examples and use cases"))
    | new Card(new Expandable("API Reference", "Detailed API documentation and examples"))
    | new Card(new Expandable("Community", "Join our community and get support"))
```

<WidgetDocs Type="Ivy.Expandable" ExtensionTypes="Ivy.ExpandableExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Expandable.cs"/>

## Examples

<Details>
<Summary>
Form Sections
</Summary>
<Body>
Organize forms into logical, collapsible sections.

```csharp demo-tabs
public class SimpleFormExample : ViewBase
{
    public record PersonalInfo(string FirstName, string LastName, string Email, string Phone);
    public record AddressInfo(string Street, string City, string State, string ZipCode);
    public record UserPreferences(bool EmailNotifications, bool SmsNotifications, string Language, string Theme);

    public override object? Build()
    {
        var personalInfo = UseState(() => new PersonalInfo("", "", "", ""));
        var addressInfo = UseState(() => new AddressInfo("", "", "", ""));
        var preferences = UseState(() => new UserPreferences(false, false, "en", "light"));

        return Layout.Vertical().Gap(2)
            | new Expandable("Personal Information", personalInfo.ToForm())
            | new Expandable("Address", addressInfo.ToForm())
            | new Expandable("Preferences", preferences.ToForm());
    }
}
```

</Body>
</Details>
