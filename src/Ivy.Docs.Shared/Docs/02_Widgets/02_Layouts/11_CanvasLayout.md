---
searchHints:
- canvas
- absolute
- position
- freeform
- wireframe
- coordinates
- placement
---

# CanvasLayout

<Ingress>
Position child widgets at absolute coordinates for free-form layouts, wireframe sketches, and precise visual placement.
</Ingress>

The `CanvasLayout` widget positions its children using absolute coordinates via the `CanvasLeft` and `CanvasTop` attached properties. Unlike flow-based layouts, children are placed at exact positions relative to the canvas origin (top-left corner).

## Basic Usage

Place widgets at specific pixel coordinates:

```csharp demo
Layout.Canvas().Width(Size.Full()).Height(Size.Px(200))
    | new Badge("Top Left").CanvasLeft(Size.Px(10)).CanvasTop(Size.Px(10))
    | new Badge("Center").Info().CanvasLeft(Size.Px(150)).CanvasTop(Size.Px(80))
    | new Badge("Bottom Right").Success().CanvasLeft(Size.Px(300)).CanvasTop(Size.Px(150))
```

## Attached Properties

`CanvasLeft` and `CanvasTop` are attached properties -- they are set on **child** widgets, not on the layout itself. Any widget placed inside a `CanvasLayout` can use them.

| Prop         | Type | Description                         |
|--------------|------|-------------------------------------|
| `CanvasLeft` | Size | Horizontal offset from left edge    |
| `CanvasTop`  | Size | Vertical offset from top edge       |

Both accept any `Size` value: pixels (`Size.Px(100)`), units (`Size.Units(20)`), percentages (`Size.Fraction(0.5)`), etc.

```csharp demo
Layout.Canvas().Width(Size.Full()).Height(Size.Px(200))
    | new Badge("Pixels").Primary().CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(20))
    | new Badge("Units").Info().CanvasLeft(Size.Units(60)).CanvasTop(Size.Px(80))
    | new Badge("50%").Success().CanvasLeft(Size.Fraction(0.5f)).CanvasTop(Size.Px(40))
```

## With Wireframe Notes

`CanvasLayout` pairs naturally with `WireframeNote` for sketching and brainstorming:

```csharp demo
Layout.Canvas().Width(Size.Full()).Height(Size.Px(300))
    | new WireframeNote("Step 1: User signs up", Colors.Yellow)
        .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(20))
    | new WireframeNote("Step 2: Verify email", Colors.Blue)
        .CanvasLeft(Size.Px(200)).CanvasTop(Size.Px(80))
    | new WireframeNote("Step 3: Dashboard", Colors.Green)
        .CanvasLeft(Size.Px(380)).CanvasTop(Size.Px(30))
```

## Background and Padding

Use `Background` and `Padding` to style the canvas container:

```csharp demo
Layout.Canvas()
    .Width(Size.Full())
    .Height(Size.Px(200))
    .Background(Colors.Neutral)
    .Padding(new Thickness(8))
    | new Badge("Whiteboard").Outline().CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(20))
    | new WireframeNote("Idea 1", Colors.Yellow)
        .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(60))
    | new WireframeNote("Idea 2", Colors.Pink)
        .CanvasLeft(Size.Px(200)).CanvasTop(Size.Px(60))
```

<WidgetDocs Type="Ivy.CanvasLayout" ExtensionTypes="Ivy.CanvasExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Layouts/CanvasLayout.cs"/>
