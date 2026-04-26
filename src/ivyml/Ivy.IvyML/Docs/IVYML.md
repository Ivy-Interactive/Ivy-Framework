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

### Size

Size values control width and height of widgets. All keyword values are case-insensitive.

| Format     | Example  | Description                                 |
|------------|----------|---------------------------------------------|
| keyword    | `Full`   | Fill available space                         |
| keyword    | `Fit`    | Shrink to fit content                        |
| keyword    | `Auto`   | Automatic sizing                             |
| keyword    | `Half`   | 50% of available space                       |
| keyword    | `Screen` | Full viewport size                           |
| pixels     | `200px`  | Fixed size in pixels                         |
| percentage | `50%`    | Fraction of available space                  |
| rem        | `4rem`   | Size in rem units                            |
| unitless   | `10`     | Plain number interpreted as unit-less value  |

### Thickness

Thickness values are used for `Padding`, `Margin`, and `BorderThickness`. Values are comma-separated integers.

| Format          | Example       | Description                                                           |
|-----------------|---------------|-----------------------------------------------------------------------|
| 1 value         | `10`          | Uniform -- all four sides                                             |
| 2 values        | `10,20`       | `horizontal,vertical` -- Left=Right=10, Top=Bottom=20                 |
| 4 values        | `10,20,10,20` | `left,top,right,bottom`                                               |

### Responsive Breakpoints

Props that support responsive values (like `Width`, `Height`, `Padding`) can be set per breakpoint using the `Prop.Breakpoint` attribute syntax. The base attribute sets the default value; breakpoint-suffixed attributes override it at specific screen widths.

| Breakpoint | Min width |
|------------|-----------|
| `Mobile`   | 640px     |
| `Tablet`   | 768px     |
| `Desktop`  | 1024px    |
| `Wide`     | 1280px    |

```xml
<StackLayout Width="Full" Width.Mobile="300px" Width.Desktop="80%" />
```

You can combine a default with any number of breakpoints:

```xml
<Badge Width="Full" Width.Mobile="200px" Width.Tablet="Half" Width.Desktop="80%" />
```

Or use breakpoints without a default:

```xml
<Badge Width.Desktop="Full" />
```

Breakpoint names are case-insensitive (`Width.mobile` and `Width.Mobile` are equivalent).

## CLI Usage

```
ivyml draw -i <IVYML> [-o <PATH>] [-w <WIDTH>] [-h <HEIGHT>] [--debug]
ivyml draw -f <FILE>  [-o <PATH>] [-w <WIDTH>] [-h <HEIGHT>] [--debug]
```

| Option          | Description                                          | Default |
|-----------------|------------------------------------------------------|---------|
| `-i`, `--input` | IvyML markup string                                  |         |
| `-f`, `--file`  | Path to an IvyML file                                |         |
| `-o`, `--output`| Output file path (png, jpg, webp)                    | temp    |
| `-w`, `--width` | Viewport width in pixels                             | 300     |
| `-h`, `--height`| Viewport height in pixels                            | 200     |
| `-d`, `--debug` | Draw debug overlays showing layout bounds and sizes  | false   |

Provide markup via `-i` (inline string) or `-f` (file path), but not both. If `-o` is omitted, the screenshot is saved to a temp file and the path is printed to stdout.

### Debug Mode

`--debug` annotates the output image with layout diagnostics:

- **Bounding boxes** around every widget, color-coded by nesting depth (red → blue → green → purple → ...).
- **Size labels** at each widget showing its type and resolved dimensions (e.g. `StackLayout 468x727`).
- **Padding regions** shaded as translucent fills between widget edge and content area.
- **Zero-size markers** -- any widget that resolved to 0x0 pixels gets a red ✖ with a label, making collapsed or invisible widgets immediately obvious.

```
ivyml docs
```

Print this guide and a list of all available widgets.

```
ivyml docs <widget>
```

Print all props and events for a specific widget.

## Spacing Scale

Integer values for `RowGap`, `ColumnGap`, `Padding`, and `Margin` follow the Tailwind CSS spacing scale: each unit is 0.25rem.

| Value | Size     |
|-------|----------|
| `1`   | 0.25rem  |
| `2`   | 0.5rem   |
| `4`   | 1rem     |
| `8`   | 2rem     |
| `16`  | 4rem     |

Layout widgets (`StackLayout`, `GridLayout`) default to a gap of 4 (1rem). Do not add `RowGap="4"` or `ColumnGap="4"` -- it is the default.

Padding is rarely needed. Layouts have appropriate padding by default. Only add `Padding` when you need extra inner spacing for a specific reason.

## Size vs Density

`Width` and `Height` control the dimensions of a widget. `Density` controls the visual density -- it adjusts text size, internal padding, and overall compactness. They are independent.

| Prop      | What it controls                  | Values                              |
|-----------|-----------------------------------|-------------------------------------|
| `Width`   | Horizontal dimension              | Size (`Full`, `200px`, `50%`, ...)  |
| `Height`  | Vertical dimension                | Size (`Full`, `200px`, `50%`, ...)  |
| `Density` | Visual compactness (text, padding)| `Small`, `Medium`, `Large`          |

## Colors

The `Colors` enum is flat -- there are no shade levels.

Use `Color="Red"`, not `Color="Red500"`. Available values: Black, White, Slate, Gray, Zinc, Neutral, Stone, Red, Orange, Amber, Yellow, Lime, Green, Emerald, Teal, Cyan, Sky, Blue, Indigo, Violet, Purple, Fuchsia, Pink, Rose, Primary, Secondary, Destructive, Success, Warning, Info, Muted, IvyGreen.

## Design Tips

- Use `<Separator />` between major sections to create visual hierarchy and prevent content from blending together.
- Prefer semantic variants (`Primary`, `Success`, `Muted`) over raw colors when available on a widget.

## Attached Properties

Some layout widgets define properties that are set on **child** elements rather than the layout itself. These are called attached properties. In IvyML, use them as attributes on any child widget inside that layout.

```xml
<CanvasLayout Width="Full" Height="300px">
  <TextBlock Content="Hello" CanvasLeft="50px" CanvasTop="20px" />
</CanvasLayout>
```

Attached props are resolved automatically -- the child doesn't need to know about them.

## Wireframe Widgets

Wireframe widgets have a hand-drawn, Balsamiq-style appearance for sketching and prototyping.

### WireframeNote

A sticky note with a folded corner, drop shadow, and hand-drawn font.

```xml
<WireframeNote Text="Remember this" />
<WireframeNote Text="Urgent item" Color="Pink" />
```

| Prop    | Type              | Default  | Values                                         |
|---------|-------------------|----------|-------------------------------------------------|
| `Text`  | string            |          | The note content. Use `&#10;` for line breaks.  |
| `Color` | Colors            | Yellow   | Any color from the Colors enum                  |

### WireframeCallout

A hand-drawn numbered circle for annotations and step markers.

```xml
<WireframeCallout Label="1" />
<WireframeCallout Label="!" Color="Pink" />
```

| Prop    | Type              | Default  | Values                                         |
|---------|-------------------|----------|-------------------------------------------------|
| `Label` | string            |          | Short text shown inside the circle.             |
| `Color` | Colors            | Yellow   | Any color from the Colors enum                  |

### CanvasLayout

A free-form layout that positions children at absolute coordinates using attached properties.

```xml
<CanvasLayout Width="Full" Height="400px">
  <WireframeNote Text="Top left" CanvasLeft="20px" CanvasTop="20px" />
  <WireframeNote Text="Center" CanvasLeft="200px" CanvasTop="150px" />
</CanvasLayout>
```

| Prop         | Type      | Description                |
|--------------|-----------|----------------------------|
| `Padding`    | Thickness | Inner padding              |
| `Background` | Colors    | Background color           |

**Attached props** (set on children):

| Prop         | Type | Description                         |
|--------------|------|-------------------------------------|
| `CanvasLeft` | Size | Horizontal offset from left edge    |
| `CanvasTop`  | Size | Vertical offset from top edge       |

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

Wireframe sketch:
```xml
<CanvasLayout Width="Full" Height="400px">
  <WireframeNote Text="Step 1: User signs up" Color="Yellow" CanvasLeft="30px" CanvasTop="20px" />
  <WireframeNote Text="Step 2: Verify email" Color="Blue" CanvasLeft="220px" CanvasTop="100px" />
  <WireframeNote Text="Step 3: Onboarding" Color="Green" CanvasLeft="410px" CanvasTop="40px" />
  <TextBlock Content="Signup Flow" Variant="H3" CanvasLeft="180px" CanvasTop="300px" />
</CanvasLayout>
```
