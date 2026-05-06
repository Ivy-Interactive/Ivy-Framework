# Spacer

An empty area to add space between components. Preserves white space in layouts when surrounding components are collapsed, hidden, or resized.

## Retool

```toolscript
spacer1.setHidden(false)
spacer1.margin = "4px 8px"
```

## Ivy

```csharp
// Basic spacer
new Spacer();

// Spacer with explicit height
new Spacer().Height(Size.Units(10));

// Flexible spacer that grows to fill available space
new Spacer().Height(Size.Grow());

// Usage in a vertical layout
Layout.Vertical()
    | new Card("First Element")
    | new Spacer()
    | new Card("Second Element");
```

## Parameters

| Parameter              | Documentation                                         | Ivy                                               |
|------------------------|-------------------------------------------------------|----------------------------------------------------|
| `hidden`               | Whether the component is hidden from view             | `Visible` property                                 |
| `margin`               | Amount of margin rendered outside (4px 8px / 0)       | Not supported (implicit spacing)                   |
| `isHiddenOnMobile`     | Hide on mobile layout                                 | Not supported                                      |
| `isHiddenOnDesktop`    | Hide on desktop layout                                | Not supported                                      |
| `maintainSpaceWhenHidden` | Reserve space when hidden                          | Not supported                                      |
| `showInEditor`         | Visible in editor when hidden                         | Not supported                                      |
| `setHidden(hidden)`    | Toggles component visibility                          | `Visible` property                                 |
| N/A                    | N/A                                                   | `.Height(Size)` for vertical spacing               |
| N/A                    | N/A                                                   | `.Width(Size)` for horizontal spacing              |
| N/A                    | N/A                                                   | `Size.Grow()` for flexible fill spacing            |
