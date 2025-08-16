---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# Thickness

`Thickness` is a value type that represents spacing values for the four sides of a rectangular area. It's commonly used for padding, margins, borders, and offsets in Ivy Framework widgets and layouts.

## Base usage

Suggested approach for configuring thickness:

```csharp demo-tabs 
new Box("Content")
    .Padding(new Thickness(10))
```

You can create elements with thickness. In the basic example, there is only one parameter to assign uniform thickness.

### Horizontal/Vertical Thickness

You can define thickness for horizontal (left and right) and vertical (top and bottom) sides separately using two parameters:

```csharp demo-tabs 
new Box("Content")
    .Width(Size.Units(10))
    .Height(Size.Units(30))
    .Padding(new Thickness(50, 15))
```

### Individual Side Thickness

To specify different thickness values for each side (left, top, right, bottom), use four parameters.

```csharp demo-tabs 
new Box("Content")
    .Width(Size.Units(30))
    .Height(Size.Units(30))
    .Padding(new Thickness(2, 10, 6, 4))
```

### Zero Thickness

Use Thickness.Zero to completely remove padding or borders by setting all sides to zero.

```csharp demo-tabs 
new Box("Content")
    .Padding(Thickness.Zero)
```

### Widget Padding

This example demonstrates different padding approaches on three cards.

The first card has uniform padding, the second has horizontal/vertical padding, and the third has individual side padding.

```csharp demo-tabs 
Layout.Vertical()
    | new Box("Card 1")
        .Width(Size.Units(170))
        .Height(Size.Units(20))
        .Padding(new Thickness(10))
    | new Box("Card 2")
        .Width(Size.Units(170))
        .Height(Size.Units(20))
        .Padding(new Thickness(20, 15))
    | new Box("Card 3")
        .Width(Size.Units(170))
        .Height(Size.Units(20))
        .Padding(new Thickness(5, 5, 100, 20))
```

## Layout Margins

Margins create space around elements.

They can be omitted or defined separately for horizontal and vertical spacing.

```csharp demo-tabs 
Layout.Vertical()
    | new Box("box without margins")
        .Width(Size.Units(170))
        .Height(Size.Units(20))
| Layout.Vertical()
    .Margin(15, 5)  // Larger margins for comparison
    | new Box("Large margins (15,5)")
        .Width(Size.Units(170))
        .Height(Size.Units(20))
    | new Box("Another box without margins")
        .Width(Size.Units(170))
        .Height(Size.Units(20))
```

### Horizontal Layout Margins

In horizontal layouts, margin values can be adjusted to control spacing between elements.

```csharp demo-tabs 
Layout.Horizontal()
    .Margin(50, 5)  // Creates Thickness(50, 5) internally
    | new Box("With margins (50, 5)").Width(Size.Units(30)).Height(Size.Units(20))
| Layout.Horizontal()
    .Margin(10, 5)
    | new Box("With margins (10, 5)").Width(Size.Units(30)).Height(Size.Units(20))
```

### Border Thickness

Border thickness defines the width of the border around an element.

It can be thin or thick, depending on the design needs:

```csharp demo-tabs 
Layout.Horizontal()
    | new Box("Thin Border")
        .Width(Size.Units(170))
        .Height(Size.Units(40))
        .BorderThickness(new Thickness(1))
    | new Box("Thick Border")
        .Width(Size.Units(170))
        .Height(Size.Units(40))
        .BorderThickness(new Thickness(10))
```

### Layout Views

You can apply both padding and margin to control the internal space of a component and the space around it.

```csharp demo-tabs 
Layout.Vertical()
    .Padding(8)
    .Margin(4)  
    | new Box("Content")
```

### Box Widget

A single element can use padding, margin, and border thickness at the same time to precisely control layout and appearance.

```csharp demo-tabs 
new Box("Content")
    .Padding(new Thickness(8))
    .Margin(new Thickness(4))
    .BorderThickness(new Thickness(8))
```

<WidgetDocs Type="Ivy.Shared.Thickness" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Shared/Thickness.cs"/>
