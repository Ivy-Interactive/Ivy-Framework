---
searchHints:
  - spacing
  - gap
  - margin
  - padding
  - layout
  - whitespace
---

# Spacer

<Ingress>
Add precise spacing between layout elements for fine-tuned control over alignment and visual balance in your [interfaces](../../01_Onboarding/02_Concepts/02_Views.md).
</Ingress>

The `Spacer` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) creates empty space between elements in your layout. By default, it grows to fill available space in the parent layout's direction, making it easy to push elements apart. It's useful for fine-tuning spacing and alignment.

## Basic Usage

Create simple spacing between elements:

```csharp demo-tabs
public class BasicSpacerView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | new Card("Spacer after First element")
            | new Spacer()
            | new Card("Second Element with no Spacer")
            | new Card("Third Element");
    }
}
```

### Flexible Spacing

A bare `Spacer` grows to fill available space by default, automatically pushing elements apart:

```csharp demo-tabs
public class FlexibleSpacerView : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal().Gap(4)
            | new Button("Left Button").Variant(ButtonVariant.Outline)
            | new Spacer()
            | new Button("Right Button").Variant(ButtonVariant.Primary);
    }
}
```

<Callout Type="tip">
The `Spacer` defaults to grow behavior (`flex-grow: 1`), making it take up all available space in the parent layout's direction. This effectively pushes sibling elements to opposite sides. See [Size](../../04_ApiReference/IvyShared/Size.md) for other sizing options like explicit widths or heights.
</Callout>

### Header Layout with Spacing

Create navigation headers with proper spacing:

```csharp demo-tabs
public class HeaderSpacerView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        
        var header = new Card(
            Layout.Horizontal().Gap(3)
                | new Button("Home").Variant(ButtonVariant.Ghost)
                | new Button("About").Variant(ButtonVariant.Ghost)
                | new Button("Contact").Variant(ButtonVariant.Ghost)
                | new Spacer().Width(60)
                | new Button("Login").Variant(ButtonVariant.Outline)
                | new Button("Sign Up").Variant(ButtonVariant.Primary)
        );
        
        var content = Layout.Vertical().Gap(4)
            | new Card("Welcome to our application")
            | new Card("This demonstrates how Spacer creates balanced layouts")
            | new Card("Elements are properly distributed across the available space");
            
        return Layout.Vertical().Gap(4)
            | header
            | content;
    }
}
```

### Height-Based Spacing

Add vertical spacing with specific heights:

```csharp demo-tabs
public class HeightSpacerView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(2)
            | new Card("Top Section")
            | new Spacer().Height(2)
            | new Card("Middle Section")
            | new Spacer().Height(10)
            | new Card("Bottom Section");
    }
}
```

<Callout Type="info">
When using `Spacer().Height()` or `Spacer().Width()`, the values represent units in the Ivy Framework's spacing system, not pixels. The framework automatically converts these units to appropriate spacing based on the current [theme](../../01_Onboarding/02_Concepts/12_Theming.md) and design system.
</Callout>

### Form Layout with Spacing

Organize form elements with consistent spacing:

```csharp demo-tabs
public class FormSpacerView : ViewBase
{
    public override object? Build()
    {
        var name = UseState("");
        var email = UseState("");
        var message = UseState("");
        
        return Layout.Vertical().Gap(3)
            | new Card(
                Layout.Vertical().Gap(3)
                    | Text.Label("Contact Form")
                    | new Separator()
                    | Text.Label("Name:")
                    | name.ToTextInput().Placeholder("Enter your name")
                    | new Spacer().Height(4)
                    | Text.Label("Email:")
                    | email.ToTextInput().Placeholder("Enter your email")
                    | new Spacer().Height(4)
                    | Text.Label("Message:")
                    | message.ToTextareaInput().Placeholder("Enter your message")
                    | new Spacer().Height(10)
                    | (Layout.Horizontal().Gap(3)
                        | new Button("Cancel").Variant(ButtonVariant.Outline)
                        | new Button("Submit").Variant(ButtonVariant.Primary))
            );
    }
}
```

<WidgetDocs Type="Ivy.Spacer" ExtensionTypes="Ivy.SpacerExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/Spacer.cs"/>

## Examples

<Details>
<Summary>
Dashboard Grid with Spacing
</Summary>
<Body>
Create responsive dashboard layouts with proper spacing:

```csharp demo-tabs
public class DashboardSpacerView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        
        var statsRow = Layout.Horizontal().Gap(4)
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("Total Users").Small()
                    | Text.Label("12.3K").Color(Colors.Primary)
            )
            | new Spacer()
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("Revenue").Small()
                    | Text.Label("$54K").Color(Colors.Green)
            )
            | new Spacer()
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("Growth").Small()
                    | Text.Label("+23%").Color(Colors.Amber)
            );
            
        var actionBar = Layout.Horizontal().Gap(3)
            | new Button("Export Data").Icon(Icons.Download).Variant(ButtonVariant.Outline)
            | new Spacer()
            | new Button("Refresh").Icon(Icons.RefreshCw).Variant(ButtonVariant.Ghost)
            | new Button("Settings").Icon(Icons.Settings).Variant(ButtonVariant.Ghost);
            
        return Layout.Vertical().Gap(4)
            | statsRow
            | new Spacer().Height(2)
            | actionBar
            | new Card("Main Content Area").Height(Size.Units(50));
    }
}
```

</Body>
</Details>
