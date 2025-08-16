---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# ColorInput

<Ingress>
Select colors visually with an intuitive color picker interface that returns values suitable for styling and theming applications.
</Ingress>

The `ColorInput` widget provides a color picker interface for selecting color values. It allows users to visually choose colors and returns the selected color in a format suitable for use in styles and themes.

## Basic Usage

Here's a simple example of a `ColorInput` that updates a state with the selected color:

```csharp demo-below 
public class ColorDemo : ViewBase
{
    public override object? Build()
    {    
        var colorState = this.UseState("#ff0000");
        return Layout.Horizontal()
                | colorState.ToColorInput();
    }   
}
```

### Using the Non-Generic Constructor

For convenience, you can create a `ColorInput` without specifying the generic type, which defaults to `string`:

```csharp
// Using the non-generic constructor (defaults to string)
var`ColorInput`= new ColorInput();

// With placeholder
var colorInputWithPlaceholder = new ColorInput("Choose a color");

// With all options
var colorInputFull = new ColorInput(
    placeholder: "Select your favorite color",
    disabled: false,
    variant: ColorInputs.TextAndPicker
);
```

## Variants

`ColorInput` has three variants. `ColorInputs.Text` variant should be used to let users enter color hex codes
manually. `ColorInputs.TextAndPicker` variant should be used in most cases as it allows users to
select the color and copy the selected color in the textbox. This is also the default variant.
Sometimes, it may be necessary to just use the color picker. In such situations `ColorInputs.Picker`
should be used.

The following code shows all these three variants in action.

```csharp demo-below 
public class ColorDemo : ViewBase
{
    public override object? Build()
    {    
        var textColorState = UseState("#ff0000");
        var pickerColorState = UseState("#ff0000");
        var textAndPickerColorState = UseState("#ff0000");
        return Layout.Vertical()
                | (Layout.Horizontal()
                    | Text.Small("Just Text")
                          .Width(25)
                    | textColorState
                          .ToColorInput()
                          .Variant(ColorInputs.Text))
                | (Layout.Horizontal()
                    | Text.Small("Just Picker")
                          .Width(25)
                    | pickerColorState
                          .ToColorInput()
                          .Variant(ColorInputs.Picker))
                | (Layout.Horizontal()
                    | Text.Small("Text and Picker")
                          .Width(25)
                    | textAndPickerColorState
                          .ToColorInput()
                          .Variant(ColorInputs.TextAndPicker));
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
        var colorState = this.UseState("#ff0000");
        var colorName = UseState("");
        var onChangeHandler = (Event<IInput<string>, string> e) =>
        {
            colorName.Set(e.Value);
            colorState.Set(e.Value);
        };
        return Layout.Vertical() 
                | H3("Hex Color Picker")
                | new ColorInput<string>
                       (colorState.Value, onChangeHandler)
                      .Variant(ColorInputs.Picker) 
                | new Code(colorName.Value)
                     .ShowCopyButton()
                     .ShowBorder();
    }    
}    
```

## Styling

`ColorInput` can be customized with various styling options, such as setting a placeholder or disabling the input.

### Disabled

To render a disabled `ColorInput` the function `Disabled` should be used.  

```csharp demo-below 
public class DisabledColorInput : ViewBase
{
    public override object? Build()
    {    
        return Layout.Vertical()
                | new ColorInput<string>("#ff0000")
                        .Disabled();
    }
}    
```

### Invalid

To represent that there is something wrong with a `ColorInput` the `Invalid` function
should be used.

```csharp demo-below 
public class InvalidStyleDemo : ViewBase
{ 
    public override object? Build()
    {    
        return Layout.Vertical()
                | new ColorInput<string>("#ff0000")
                        .Invalid("This is not used now");
    }
}

```

<WidgetDocs Type="Ivy.ColorInput" ExtensionTypes="Ivy.ColorInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/ColorInput.cs"/>

## Examples

The following example shows how `ColorPicker` control can be used in a developer tool setting that
generates CSS blocks.

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
                   | Text.InlineCode("color")
                         .Width(35)
                   | color.ToColorInput()
                          .Variant(ColorInputs.Picker))
                | (Layout.Horizontal()
                   | Text.InlineCode("background-color")
                         .Width(35)
                   | bgColor.ToColorInput()
                          .Variant(ColorInputs.Picker))
                | (Layout.Horizontal()
                   | Text.InlineCode("border")
                         .Width(35)
                   | border.ToColorInput()
                          .Variant(ColorInputs.Picker))
                   | new Code(genCode.Value)
                         .Language(Languages.Css)
                         .ShowCopyButton();
    }
}
```
