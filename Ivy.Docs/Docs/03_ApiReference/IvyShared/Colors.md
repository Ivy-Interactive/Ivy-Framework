# Colors

Ivy provides a comprehensive set of predefined colors designed for modern applications, with full support for light and dark themes.

## Color System Overview

Ivy's color system is based on a carefully curated palette that provides:

- **Neutral colors**: Black, White, and grayscale variations (Slate, Gray, Zinc, Neutral, Stone)
- **Chromatic colors**: A full spectrum from Red to Rose, including popular hues like Blue, Green, Purple
- **Semantic colors**: Primary, Secondary, and Destructive for consistent UI patterns
- **Theme-aware variants**: Each color automatically adapts to light and dark modes

## Color Selection Philosophy

The colors were selected following these principles:

1. **Accessibility**: All colors meet WCAG contrast requirements when paired with appropriate foreground colors
2. **Consistency**: Colors follow a systematic approach with consistent saturation and luminosity levels
3. **Flexibility**: Each color works well in both light and dark themes
4. **Modern aesthetic**: The palette reflects contemporary design trends while remaining timeless

## Theme Integration

Colors automatically adapt to the current theme:
- **Light theme**: Uses the base color values with dark foregrounds
- **Dark theme**: Colors are adjusted for better visibility on dark backgrounds
- **System theme**: Follows the user's system preference

## Available Colors

### All Colors

```csharp demo-tabs
public class AllColorsView : ViewBase
{
    public override object? Build()
    {
        Colors[] colors = Enum.GetValues<Colors>();
        
        return Layout.Vertical(
            colors.Select(color =>
                new Box(color.ToString())
                    .Width(Size.Auto())
                    .Height(10)
                    .Color(color).BorderRadius(BorderRadius.Rounded)
                    .Padding(3)
            )
        );
    }
}
```

### Neutral Colors

```csharp demo-tabs
public class NeutralColorsView : ViewBase
{
    public override object? Build()
    {
        var neutralColors = new Colors[] { Colors.Black, Colors.White, Colors.Slate, Colors.Gray, Colors.Zinc, Colors.Neutral, Colors.Stone };
        
        return Layout.Vertical(
            neutralColors.Select(color =>
                new Box(color.ToString())
                    .Width(Size.Auto())
                    .Height(10)
                    .Color(color).BorderRadius(BorderRadius.Rounded)
                    .Padding(3)
            )
        );
    }
}
```

### Chromatic Colors

```csharp demo-tabs
public class ChromaticColorsView : ViewBase
{
    public override object? Build()
    {
        var chromaticColors = new Colors[] { 
            Colors.Red, Colors.Orange, Colors.Amber, Colors.Yellow, Colors.Lime, Colors.Green, 
            Colors.Emerald, Colors.Teal, Colors.Cyan, Colors.Sky, Colors.Blue, Colors.Indigo, 
            Colors.Violet, Colors.Purple, Colors.Fuchsia, Colors.Pink, Colors.Rose 
        };
        
        return Layout.Vertical(
            chromaticColors.Select(color =>
                new Box(color.ToString())
                    .Width(Size.Auto())
                    .Height(10)
                    .Color(color).BorderRadius(BorderRadius.Rounded)
                    .Padding(3)
            )
        );
    }
}
```

### Semantic Colors

```csharp demo-tabs
public class SemanticColorsView : ViewBase
{
    public override object? Build()
    {
        var semanticColors = new Colors[] { Colors.Primary, Colors.Secondary, Colors.Destructive };
        
        return Layout.Vertical(
            semanticColors.Select(color =>
                new Box(color.ToString())
                    .Width(Size.Auto())
                    .Height(10)
                    .Color(color).BorderRadius(BorderRadius.Rounded)
                    .Padding(3)
            )
        );
    }
}
```

## Practical Examples

### Colors on Different Backgrounds
This example demonstrates how colors appear on both light and dark backgrounds:

```csharp demo-tabs
public class ColorsOnBackgroundsView : ViewBase
{
    public override object? Build()
    {
        Colors[] colors = Enum.GetValues<Colors>();

        object GenerateColors()
        {
            return Layout.Vertical(
                colors.Select(color =>
                    new Box(color.ToString())
                        .Width(Size.Auto())
                        .Height(10)
                        .Color(color).BorderRadius(BorderRadius.Rounded)
                        .Padding(3)
                )
            );
        }

        var lightBackground = Layout.Vertical(
            Text.Block("Light Background").Color(Colors.Black),
            GenerateColors()
        ).Padding(10);

        var darkBackground = Layout.Vertical(
            Text.Block("Dark Background").Color(Colors.White),
            GenerateColors()
        ).Padding(10).Background(Colors.Black);

        return Layout.Grid().Columns(2)
            | lightBackground
            | darkBackground;
    }
}
```

### Common Usage Patterns

#### Status Indicators

```csharp demo-tabs
public class StatusIndicatorsView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            new Box("Success").Color(Colors.Green).Padding(5).BorderRadius(BorderRadius.Rounded),
            new Box("Warning").Color(Colors.Amber).Padding(5).BorderRadius(BorderRadius.Rounded),
            new Box("Error").Color(Colors.Destructive).Padding(5).BorderRadius(BorderRadius.Rounded),
            new Box("Info").Color(Colors.Blue).Padding(5).BorderRadius(BorderRadius.Rounded)
        ).Gap(5);
    }
}
```

#### Button Colors

```csharp demo-tabs
public class ButtonColorsView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            new Button("Primary Action").Variant(ButtonVariant.Default),
            new Button("Secondary Action").Variant(ButtonVariant.Secondary),
            new Button("Destructive Action").Variant(ButtonVariant.Destructive)
        ).Gap(10);
    }
}
```

## Best Practices

1. **Use semantic colors** for consistent UI patterns (Primary, Secondary, Destructive)
2. **Test on both backgrounds** to ensure proper contrast and readability
3. **Consider color meaning** - use red/destructive for errors, green for success
4. **Maintain consistency** - stick to your chosen color scheme throughout the application
5. **Accessibility first** - ensure proper contrast ratios for text and backgrounds

## Technical Implementation

Colors are defined as an enum in `Ivy.Shared.Colors` and map to CSS custom properties that automatically adapt to the current theme. Each color includes variants for different states and theme modes.

```csharp
// Get all available colors dynamically
Colors[] colors = Enum.GetValues<Colors>();

// Using colors with widgets
new Box("Content")
    .Color(Colors.Primary)
    .Background(Colors.Secondary);
```