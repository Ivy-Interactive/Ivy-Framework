# IvyML

IvyML is an XML-based markup language for describing Ivy widget trees. It maps directly to Ivy's widget system -- each XML element is a widget, each attribute is a prop.

## Syntax

```xml
<WidgetName PropName="value" AnotherProp="value">
  <ChildWidget Prop="value" />
</WidgetName>
```

- Element names are widget type names (case-insensitive).
- Attributes map to properties marked with `[Prop]` on the widget.
- Child elements become the widget's children.
- Property elements use dot notation: `<Card.Padding>10,10,10,10</Card.Padding>`

## Value Types

| Type      | Example values                                        |
|-----------|-------------------------------------------------------|
| string    | `"Hello"`                                             |
| bool      | `true`, `false`                                       |
| int       | `42`                                                  |
| float     | `3.14`                                                |
| enum      | `Primary`, `Success`, `H1` (case-insensitive)         |
| Size      | `Full`, `Fit`, `Auto`, `Half`, `200px`, `50%`, `4rem` |
| Thickness | `10` or `10,20` or `10,20,10,20`                      |

## CLI Usage

```
ivyml draw -i <IVYML> [-o <PATH>] [-w <WIDTH>] [-h <HEIGHT>]
```

| Option          | Description                         | Default |
|-----------------|-------------------------------------|---------|
| `-i`, `--input` | IvyML markup string (required)      |         |
| `-o`, `--output`| Output file path (png, jpg, webp)   | temp    |
| `-w`, `--width` | Viewport width in pixels            | 300     |
| `-h`, `--height`| Viewport height in pixels           | 200     |

If `-o` is omitted, the screenshot is saved to a temp file and the path is printed to stdout.

```
ivyml docs
```

Print this guide and a list of all available widgets.

```
ivyml docs <widget>
```

Print all props and events for a specific widget.

## Examples

Simple text:
```xml
<TextBlock Content="Hello World" Variant="H1" />
```

Button:
```xml
<Button Title="Submit" Variant="Primary" />
```

Card with content:
```xml
<Card>
  <TextBlock Content="Card body" />
</Card>
```

Badge:
```xml
<Badge Title="New" Variant="Success" />
```

Progress bar:
```xml
<Progress Value="75" />
```
