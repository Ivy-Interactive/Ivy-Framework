---
prepare: |
  var client = UseService<IClientProvider>();
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
Organize content in visually grouped containers with headers, footers, and actions to create structured, professional [layouts](../02_Layouts/_Index.md).
</Ingress>

The `Card` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a versatile container used to group related content and actions in Ivy [apps](../../01_Onboarding/02_Concepts/10_Apps.md). It can hold text, buttons, charts, and other [widgets](../../01_Onboarding/02_Concepts/03_Widgets.md), making it a fundamental [building block](../../01_Onboarding/02_Concepts/02_Views.md) for creating structured layouts.

## Basic Usage

Here's a simple example of a card containing text and a button that shows a [toast message](../../01_Onboarding/02_Concepts/13_Clients.md) when clicked. Use [Size](../../04_ApiReference/IvyShared/Size.md) for `.Width()` to control card width.

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

Cards support the same border functionality as [Box widgets](../01_Primitives/04_Box.md), allowing you to customize the appearance with [Colors](../../04_ApiReference/IvyShared/Colors.md) and border options while maintaining the default card styling when no border properties are specified.

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

For dashboard applications, Ivy provides the specialized `MetricView` component that extends Card functionality with KPI-specific features like trend indicators and goal tracking. It uses [UseQuery](../../03_Hooks/02_Core/09_UseQuery.md) hooks for data fetching.

```csharp demo-below
new MetricView(
    "Revenue",
    Icons.DollarSign,
    ctx => ctx.UseQuery(
        key: "revenue",
        fetcher: () => Task.FromResult(new MetricRecord(
            "$125,430",
            0.12, // 12% increase
            0.85, // 85% of goal
            "Target: $150,000"
        ))
    )
)
```

The `MetricView` uses UseQuery hooks for data loading, which automatically handles loading states, error handling, and caching. It also displays trend arrows with color-coded indicators for performance tracking. See the [MetricView documentation](13_MetricView.md) for more details.

<WidgetDocs Type="Ivy.Card" ExtensionTypes="Ivy.CardExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Card.cs"/>
