---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
- responsive
- breakpoint
- mobile
- tablet
- desktop
- wide
- adaptive
---

# Responsive Design

<Ingress>
Build adaptive layouts that respond to viewport size using the built-in breakpoint system. Vary width, height, visibility, columns, orientation, gap, and density at different screen sizes.
</Ingress>

Ivy's responsive design system lets you vary widget properties by viewport size using `Responsive<T>` values with mobile-first cascading.

## Breakpoints

| Breakpoint | Max Width | Usage |
|---|---|---|
| `Mobile` | 640px | Phones |
| `Tablet` | 768px | Tablets |
| `Desktop` | 1024px | Laptops |
| `Wide` | 1280px | Large screens |

Values cascade upward: a `Mobile` value applies to all sizes unless overridden by a larger breakpoint.

## Responsive Width

Use `.At()` and `.And()` to create responsive values:

```csharp
new Box(Text.P("Responsive box"))
    .Width(Size.Full().At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Size.Half()))
```

```csharp demo
Layout.Vertical()
    | new Box(Text.P("Full on mobile, half on desktop"))
        .Width(Size.Full().At(Breakpoint.Mobile)
            .And(Breakpoint.Desktop, Size.Half()))
        .Background(Colors.Primary)
```

## Conditional Visibility

Hide or show widgets at specific breakpoints:

```csharp
// Hidden on mobile and tablet
new Badge("Desktop only").HideOn(Breakpoint.Mobile, Breakpoint.Tablet)

// Only visible on mobile
new Badge("Mobile only").ShowOn(Breakpoint.Mobile)
```

```csharp demo
Layout.Vertical()
    | new Badge("Desktop only").HideOn(Breakpoint.Mobile, Breakpoint.Tablet)
    | new Badge("Mobile only").ShowOn(Breakpoint.Mobile)
    | new Badge("Always visible")
```

## Responsive Grid Columns

Adjust grid column count by viewport:

```csharp
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile)
        .And(Breakpoint.Tablet, 2)
        .And(Breakpoint.Desktop, 3))
    .Gap(4)
```

```csharp demo
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile)
        .And(Breakpoint.Tablet, 2)
        .And(Breakpoint.Desktop, 3))
    .Gap(4)
    | new Card("Card 1")
    | new Card("Card 2")
    | new Card("Card 3")
    | new Card("Card 4")
    | new Card("Card 5")
    | new Card("Card 6")
```

## Responsive Orientation

Switch between horizontal and vertical layouts:

```csharp
Layout.Horizontal()
    .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Orientation.Horizontal))
```

```csharp demo
Layout.Horizontal()
    .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Orientation.Horizontal))
    | new Button("Action 1")
    | new Button("Action 2")
    | new Button("Action 3")
```

## Responsive Gap

Vary spacing between items:

```csharp
Layout.Vertical()
    .Gap(2.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
```

```csharp demo
Layout.Vertical()
    .Gap(2.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
    | Text.P("Item 1")
    | Text.P("Item 2")
    | Text.P("Item 3")
```

## Responsive Density

Adjust touch target size by device:

```csharp
new Button("Adaptive Button")
    .Density(Density.Large.At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Density.Medium))
```

```csharp demo
new Button("Adaptive Button")
    .Density(Density.Large.At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Density.Medium))
```

## Common Patterns

### Collapsing Sidebar

```csharp
Layout.Horizontal()
    | new Box(Text.P("Sidebar"))
        .Width(Size.Units(60))
        .HideOn(Breakpoint.Mobile)
    | new Box(Text.P("Main Content"))
        .Width(Size.Grow())
```

### Mobile-First Card Grid

```csharp
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile)
        .And(Breakpoint.Tablet, 2)
        .And(Breakpoint.Desktop, 3)
        .And(Breakpoint.Wide, 4))
    .Gap(4)
```

### Mobile-First Form

```csharp
Layout.Vertical()
    .Width(Size.Full().At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Size.Fraction(0.5f)))
    .Gap(4.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
```
