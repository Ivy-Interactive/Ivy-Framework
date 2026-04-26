---
searchHints:
- wireframe
- callout
- annotation
- number
- circle
- marker
- balsamiq
---

# WireframeCallout

<Ingress>
A hand-drawn numbered circle widget for annotations, step markers, and low-fidelity callouts with a Balsamiq-style aesthetic.
</Ingress>

The `WireframeCallout` widget renders as a wobbly hand-drawn circle with a bold centered label. It is designed for numbering steps, annotating diagrams, and adding visual markers to wireframe sketches.

## Basic Usage

```csharp demo
new WireframeCallout("1")
```

## Colors

Use colors to categorize callouts by meaning:

```csharp demo
Layout.Horizontal().Gap(4)
    | new WireframeCallout("1")
    | new WireframeCallout("2", Colors.Blue)
    | new WireframeCallout("3", Colors.Green)
    | new WireframeCallout("!", Colors.Pink)
    | new WireframeCallout("?", Colors.Orange)
    | new WireframeCallout("A", Colors.Purple)
```

| Color    | Typical Use                    |
|----------|--------------------------------|
| Yellow   | General annotations            |
| Blue     | In-progress steps, questions   |
| Green    | Completed steps, approvals     |
| Pink     | Warnings, errors, blockers     |
| Orange   | Deferred items, follow-ups     |
| Purple   | Design decisions, references   |

## On a Canvas

Combine with `CanvasLayout` for annotated wireframe diagrams:

```csharp demo
Layout.Canvas().Width(Size.Full()).Height(Size.Px(200))
    | new WireframeCallout("1", Colors.Blue)
        .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(20))
    | new WireframeCallout("2", Colors.Blue)
        .CanvasLeft(Size.Px(80)).CanvasTop(Size.Px(80))
    | new WireframeCallout("3", Colors.Green)
        .CanvasLeft(Size.Px(140)).CanvasTop(Size.Px(30))
    | new WireframeCallout("!", Colors.Pink)
        .CanvasLeft(Size.Px(200)).CanvasTop(Size.Px(100))
```

## With Wireframe Notes

Pair callouts with sticky notes for annotated flow diagrams:

```csharp demo
Layout.Canvas().Width(Size.Full()).Height(Size.Px(300))
    | new WireframeCallout("1", Colors.Blue)
        .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(20))
    | new WireframeNote("User signs up", Colors.Yellow)
        .CanvasLeft(Size.Px(60)).CanvasTop(Size.Px(10))
    | new WireframeCallout("2", Colors.Blue)
        .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(100))
    | new WireframeNote("Verify email", Colors.Blue)
        .CanvasLeft(Size.Px(60)).CanvasTop(Size.Px(90))
    | new WireframeCallout("3", Colors.Green)
        .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(180))
    | new WireframeNote("Dashboard", Colors.Green)
        .CanvasLeft(Size.Px(60)).CanvasTop(Size.Px(170))
```

<WidgetDocs Type="Ivy.WireframeCallout" ExtensionTypes="Ivy.WireframeCalloutExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Wireframe/WireframeCallout.cs"/>
