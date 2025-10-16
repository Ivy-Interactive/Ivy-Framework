---
prepare: |
  var client = this.UseService<IClientProvider>();
searchHints:
  - container
  - panel
  - box
  - section
  - wrapper
  - border
---

# Card

<Ingress>
Organize content in visually grouped containers with headers, footers, and actions to create structured, professional layouts.
</Ingress>

The `Card` widget is a versatile container used to group related content and actions in Ivy apps. It can hold text, buttons, charts, and other widgets, making it a fundamental building block for creating structured layouts.

## Basic Usage

Here's a simple example of a card containing text and a button that shows a toast message when clicked.

```csharp demo-below
new Card(
    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc",
    new Button("Sign Me Up", _ => client.Toast("You have signed up!"))
).Title("Card App").Description("This is a card app.").Width(Size.Units(100))
```

## Click Listener

HandleClick attaches an event listener and makes the card clickable.

```csharp demo-below
new Card(
    "This card is clickable."
).Title("Clickable Card")
 .Description("Demonstrating click and mouse hover.")
 .HandleClick(_ => client.Toast("Card clicked!"))
 .Width(Size.Units(100))
```

## Border Customization

Cards support the same border functionality as Box widgets, allowing you to customize the appearance while maintaining the default card styling when no border properties are specified.

```csharp demo-below
new Card(
    "This card has a custom border with dashed style and primary color.",
    new Button("Action", _ => client.Toast("Button clicked!"))
).Title("Custom Border Card")
 .Description("Demonstrating border customization")
 .BorderThickness(3)
 .BorderStyle(BorderStyle.Dashed)
 .BorderColor(Colors.Primary)
 .BorderRadius(BorderRadius.Rounded)
 .Width(Size.Units(100))
```

## Dashboard Metrics

For dashboard applications, Ivy provides the specialized `MetricView` component that extends Card functionality with KPI-specific features like trend indicators and goal tracking.

```csharp demo-below
new MetricView(
    "Revenue", 
    Icons.DollarSign,
    () => Task.FromResult(new MetricRecord(
        "$125,430", 
        0.12, // 12% increase
        0.85, // 85% of goal
        "Target: $150,000"
    ))
)
```

The `MetricView` automatically handles loading states, error handling, and displays trend arrows with color-coded indicators for performance tracking. See the [MetricView documentation](MetricView.md) for more details.

<WidgetDocs Type="Ivy.Card" ExtensionTypes="Ivy.CardExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Card.cs"/>
