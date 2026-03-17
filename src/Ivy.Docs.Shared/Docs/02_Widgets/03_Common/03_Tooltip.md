---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - hint
  - hover
  - popover
  - help
  - info
  - tooltip
---

# Tooltip

<Ingress>
Enhance user experience with contextual tooltips that provide helpful information on hover or focus without cluttering the interface.
</Ingress>

`Tooltip`s provide contextual information when hovering or focusing on a [widget](../../01_Onboarding/02_Concepts/03_Widgets.md). They are essential for improving user experience by offering additional context without cluttering the interface.

## Basic Usage

Here's a simple example of a tooltip on a button:

```csharp demo-below
new Button("Hover Me").WithTooltip("Hello World!")
```

### Form Validation

Tooltips are perfect for displaying validation errors or helpful hints on form inputs:

```csharp demo-tabs
Layout.Vertical()
    | UseState("").ToTextInput().Placeholder("Enter email").WithTooltip("Enter a valid email address")
    | UseState("").ToPasswordInput().Placeholder("Enter password").WithTooltip("Password must be at least 8 characters long")
    | UseState(0).ToNumberInput().Placeholder("Enter age").WithTooltip("Must be between 18 and 100")
```

### With Icons

Use tooltips to explain the meaning of icons, especially in toolbars or [navigation](../../01_Onboarding/02_Concepts/09_Navigation.md) (using icon-only buttons):

```csharp demo-tabs
Layout.Horizontal()
    | new Button().Icon(Icons.Save).WithTooltip("Save changes")
    | new Button().Icon(Icons.Download).WithTooltip("Download file")
    | new Button().Icon(Icons.Settings).WithTooltip("Open settings")
    | new Button().Icon(Icons.Info).WithTooltip("Get help")
```

### Status Indicators

Provide additional context for status badges and indicators:

```csharp demo-tabs
Layout.Horizontal()
    | new Badge("Online", icon: Icons.Circle).WithTooltip("User is currently active")
    | new Badge("Away", icon: Icons.Circle).WithTooltip("User is away from keyboard")
    | new Badge("Busy", icon: Icons.Circle).WithTooltip("User is in a meeting")
    | new Badge("Offline", icon: Icons.Circle).WithTooltip("User is not available")
```

### Rich Content

Tooltips can contain detailed multi-line text to provide comprehensive information.

```csharp demo-below
new Button("Advanced Tooltip")
    .WithTooltip("This tooltip contains multiple lines of text and can be quite detailed to provide comprehensive information to the user.")
```

## Examples

<Details>
<Summary>
Various Widgets
</Summary>
<Body>
Tooltips work with any interactive [element](../../01_Onboarding/02_Concepts/03_Widgets.md).

```csharp demo-tabs
Layout.Grid().Columns(2)
    | new Button("Primary").WithTooltip("Primary action button")
    | new Button("Secondary").Secondary().WithTooltip("Secondary action button")
    | new Badge("Success").WithTooltip("Operation completed successfully")
    | new Badge("Error", variant: BadgeVariant.Destructive).WithTooltip("An error occurred")
    | new Card("Card Title").WithTooltip("This card contains detailed information")
    | Text.Literal("Important Text").WithTooltip("This text requires attention")
```

</Body>
</Details>

<Details>
<Summary>
Navigation
</Summary>
<Body>
Help users understand navigation elements:

```csharp demo-below
Layout.Horizontal()
    | new Button("Dashboard").Icon(Icons.House).WithTooltip("View your main dashboard")
    | new Button("Projects").Icon(Icons.Folder).WithTooltip("Manage your projects")
    | new Button("Settings").Icon(Icons.Settings).WithTooltip("Configure your account settings")
    | new Button("Profile").Icon(Icons.User).WithTooltip("View and edit your profile")
    | new Button("Help").Icon(Icons.Info).WithTooltip("Get help and support")
```

</Body>
</Details>

<Details>
<Summary>
Form Help
</Summary>
<Body>
Provide contextual help for form fields:

```csharp demo-below
Layout.Vertical()
    | UseState("").ToTextInput().Placeholder("Enter username").WithTooltip("Choose a unique username that will be visible to other users")
    | UseState("").ToTextInput().Placeholder("Enter email").WithTooltip("We'll use this email for account verification and notifications")
    | UseState(0).ToNumberInput().Placeholder("Enter age").WithTooltip("You must be at least 13 years old to create an account")
    | UseState(false).ToBoolInput().Label("Newsletter").WithTooltip("Receive updates about new features and improvements")
```

</Body>
</Details>

## Best Practices

### Keep Tooltips Concise

Tooltips should provide quick, helpful information without being overwhelming:

```csharp demo-tabs
Layout.Horizontal()
    | new Button("Good").WithTooltip("Clear and concise")
    | new Button("Too Long").WithTooltip("This tooltip is unnecessarily long and verbose, providing more information than the user needs at this moment, which can be distracting and counterproductive to the user experience")
```

### Use Consistent Language

Maintain consistent terminology and tone across your tooltips:

```csharp demo-tabs
Layout.Horizontal()
    | new Button("Save").WithTooltip("Save changes")
    | new Button("Cancel").WithTooltip("Cancel changes")
    | new Button("Reset").WithTooltip("Reset to default")
    | new Button("Apply").WithTooltip("Apply changes")
```

### Provide Actionable Information

When possible, tell users what will happen when they interact with an element:

```csharp demo-tabs
Layout.Horizontal()
    | new Button("Delete").WithTooltip("Permanently delete this item (cannot be undone)")
    | new Button("Archive").WithTooltip("Move to archive (can be restored later)")
    | new Button("Share").WithTooltip("Share this item with other users")
    | new Button("Export").WithTooltip("Download as a file")
```

## Accessibility Considerations

Tooltips enhance accessibility by providing additional context for screen readers and keyboard navigation:

```csharp demo-below
Layout.Vertical()
    | Text.Literal("Accessible content").WithTooltip("This tooltip provides additional context for assistive technologies")
    | new Button("Accessible button").WithTooltip("This button performs a specific action that is described in the tooltip")
    | new Badge("Status").WithTooltip("Current status is clearly indicated for all users")
```

<WidgetDocs Type="Ivy.Tooltip" ExtensionTypes="Ivy.TooltipExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Tooltip.cs"/>
