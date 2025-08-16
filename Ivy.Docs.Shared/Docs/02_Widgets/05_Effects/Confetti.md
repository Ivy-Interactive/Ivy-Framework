# Confetti

<Ingress>
Add celebratory confetti effects to any widget with customizable triggers for automatic, click, or hover activation.
</Ingress>

The `Confetti` animation can be triggered automatically, on click, or when the mouse hovers over the widget. Perfect for celebrating user achievements, form submissions, or adding delightful interactions to your interface.

## Basic Usage

Wrap any widget with confetti using the `WithConfetti()` extension method:

```csharp demo-tabs ivy-bg
new Button("Confetti on click!")
    .WithConfetti(AnimationTrigger.Click)
```

### Auto Trigger

Confetti fires automatically when the widget is first rendered, perfect for welcoming users or celebrating initial page loads.

```csharp demo-tabs ivy-bg
Text.Block("Welcome!")
    .WithConfetti(AnimationTrigger.Auto)
```

### Hover Trigger

Confetti activates when the mouse hovers over the widget, providing immediate visual feedback for interactive elements.

```csharp demo-tabs ivy-bg
new Card("Hover over me")
    .WithConfetti(AnimationTrigger.Hover)
```

### List Usage

Demonstrates how to add confetti to list items, making each selection feel special and celebratory.

```csharp demo-tabs ivy-bg
Layout.Vertical().Gap(10)
    | new List(new[] { "First option on click", "Second option on click" }
        .Select(level => new ListItem(level, onClick: _ => {}, icon: Icons.Circle)
            .WithConfetti(AnimationTrigger.Click)))
```

### Interactive Elements

Shows various interactive widgets with different confetti triggers, demonstrating the flexibility of the confetti system.

```csharp demo-tabs ivy-bg
Layout.Vertical().Gap(10)
    | Text.Block("Interactive elements with confetti:")
    | new Button("Click me").WithConfetti(AnimationTrigger.Click)
    | new Card("Hover me").WithConfetti(AnimationTrigger.Hover)
    | new Badge("Auto confetti").WithConfetti(AnimationTrigger.Auto)
```

<WidgetDocs Type="Ivy.Confetti" ExtensionTypes="Ivy.ConfettiExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Effects/Confetti.cs"/>

## Advanced Examples

### Integration with Other Widgets

Confetti works seamlessly with all Ivy widgets, allowing you to add celebratory effects to any interface element.

```csharp demo-tabs ivy-bg
Layout.Vertical().Gap(10)
    | new Button("Action").WithConfetti(AnimationTrigger.Click)
    | new Card("Content").WithConfetti(AnimationTrigger.Hover)
    | new ListItem("Item").WithConfetti(AnimationTrigger.Click)
    | Text.Block("Message").WithConfetti(AnimationTrigger.Hover)
    | new Badge("Success").WithConfetti(AnimationTrigger.Hover)
```
