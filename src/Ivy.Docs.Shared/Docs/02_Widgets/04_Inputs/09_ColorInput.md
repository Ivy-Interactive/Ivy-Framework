---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - picker
  - palette
  - hex
  - rgb
  - swatch
  - color
---

# ColorInput

<Ingress>
Select colors visually with an intuitive color picker [interface](../../01_Onboarding/02_Concepts/02_Views.md) that returns values suitable for [styling](../../01_Onboarding/02_Concepts/02_Views.md) and [theming](../../01_Onboarding/02_Concepts/12_Theming.md) applications.
</Ingress>

The `ColorInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a color picker interface for selecting color values. It allows users to visually choose colors and returns the selected color in a format suitable for use in styles and [themes](../../01_Onboarding/02_Concepts/12_Theming.md).

## Basic Usage

Here's a simple example of a `ColorInput` that updates a [state](../../03_Hooks/02_Core/03_UseState.md) with the selected color:

```csharp demo-below
public class ColorDemo : ViewBase
{
    public override object? Build()
    {
        var colorState = UseState("#ff0000");
        return colorState.ToColorInput();
    }
}
```

### Using the Non-Generic Constructor

For convenience, you can create a `ColorInput` without specifying the generic type, which defaults to `string`:

```csharp
// Using the non-generic constructor (defaults to string)
var colorInput = new ColorInput();

// With placeholder
var colorInputWithPlaceholder = new ColorInput("Choose a color");

// With all options
var colorInputFull = new ColorInput(
    placeholder: "Select your favorite color",
    disabled: false,
    variant: ColorInputVariant.TextAndPicker
);
```

## Variants

`ColorInput` has four variants:

| Variant                           | Description                                  |
| --------------------------------- | -------------------------------------------- |
| `ColorInputVariant.Text`          | Text input for entering hex codes manually   |
| `ColorInputVariant.Picker`        | Color picker only                            |
| `ColorInputVariant.TextAndPicker` | Text input with color picker (default)       |
| `ColorInputVariant.Swatch`        | Grid of predefined colors from `Colors` enum |

The following code shows all variants in action:

```csharp demo-below
public class ColorVariantsDemo : ViewBase
{
    public override object? Build()
    {
        var colorState = UseState("red");
        return Layout.Grid().Columns(2).ColumnWidths(Size.Units(30), null)
            | Text.P("Just Text").Small() | colorState.ToColorInput().Variant(ColorInputVariant.Text)
            | Text.P("Just Picker").Small() | colorState.ToColorInput().Variant(ColorInputVariant.Picker)
            | Text.P("Text and Picker").Small() | colorState.ToColorInput().Variant(ColorInputVariant.TextAndPicker)
            | Text.P("Swatch").Small() | colorState.ToColorInput().Variant(ColorInputVariant.Swatch);
    }
}
```

### Swatch Variant

The `Swatch` variant displays a grid of predefined colors from the `Colors` enum. This is useful when you want users to select from a specific set of theme-aware colors rather than arbitrary hex values.

```csharp demo-below
public class ColorSwatchDemo : ViewBase
{
    public override object? Build()
    {
        var colorState = UseState(Colors.Blue);
        return Layout.Vertical()
            | colorState.ToColorInput().Variant(ColorInputVariant.Swatch)
            | Text.P($"Selected: {colorState.Value}");
    }
}
```

## Event Handling

ColorInput can handle change events using the `onChange` parameter.
The following demo shows how the `Picker` variant can be used with a code
block so that

```csharp demo-below
public class ColorChangedDemo : ViewBase
{

    public override object? Build()
    {
        var colorState = UseState("#ff0000");
        var colorName = UseState(colorState.Value);
        var onChangeHandler = (Event<IInput<string>, string> e) =>
        {
            colorName.Set(e.Value);
            colorState.Set(e.Value);
        };
        return Layout.Vertical()
                | H3("Hex Color Picker")
                | (Layout.Horizontal()
                | new ColorInput<string>
                       (colorState.Value, onChangeHandler)
                      .Variant(ColorInputVariant.Picker)
                | new CodeBlock(colorName.Value)
                    .ShowCopyButton()
                    .ShowBorder());
    }
}
```

## Styling

`ColorInput` can be customized with various styling options, such as setting a placeholder or disabling the input.

```csharp demo-below
public class ColorStylingDemo : ViewBase
{
    public override object? Build()
    {
        var colorState = UseState("#ff0000");
        return Layout.Grid().Columns(2).ColumnWidths(Size.Units(30), null)
            | Text.P("Disabled").Small() | colorState.ToColorInput().Disabled()
            | Text.P("Invalid").Small() | colorState.ToColorInput().Invalid("Invalid color value");
    }
}
```

## Alpha Channel

When building apps that require semi-transparent colors (e.g., overlays, backgrounds with opacity), you can enable the `AllowAlpha` option. This adds an opacity slider next to the color picker, and the value is stored in `#RRGGBBAA` format.

```csharp demo-below
public class ColorAlphaDemo : ViewBase
{
    public override object? Build()
    {
        var colorState = UseState("#ff000080");
        return Layout.Vertical()
            | colorState.ToColorInput().AllowAlpha()
            | Text.P($"Selected: {colorState.Value}");
    }
}
```

<WidgetDocs Type="Ivy.ColorInput" ExtensionTypes="Ivy.ColorInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/ColorInput.cs"/>

## Examples

<Details>
<Summary>
ColorPicker control can be used in a developer tool setting that generates CSS blocks.
</Summary>
<Body>

```csharp demo-tabs
public class CSSColorDemo : ViewBase
{
    public override object? Build()
    {
        var color = UseState("#333333");
        var bgColor = UseState("#F5F5F5");
        var border = UseState("#CCCCCC");
        var template = """
                     .my-element {
                            color: [COLOR];
                            background-color: [BG_COLOR];
                            border: 1px solid [BORDER];
                      }
        """;
        var genCode = UseState("");
        genCode.Set(template.Replace("[COLOR]",color.Value)
                            .Replace("[BG_COLOR]",bgColor.Value)
                            .Replace("[BORDER]",border.Value));
        return Layout.Vertical()
                | H3("CSS Block Generator")
                | (Layout.Horizontal()
                   | Text.Monospaced("color")
                         .Width(Size.Units(35))
                   | color.ToColorInput()
                          .Variant(ColorInputVariant.Picker))
                | (Layout.Horizontal()
                   | Text.Monospaced("background-color")
                         .Width(Size.Units(35))
                   | bgColor.ToColorInput()
                          .Variant(ColorInputVariant.Picker))
                | (Layout.Horizontal()
                   | Text.Monospaced("border")
                         .Width(Size.Units(35))
                   | border.ToColorInput()
                          .Variant(ColorInputVariant.Picker))
                   | new CodeBlock(genCode.Value)
                         .Language(Languages.Css)
                         .ShowCopyButton();
    }
}
```

</Body>
</Details>
