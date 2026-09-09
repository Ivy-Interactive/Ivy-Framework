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

### Content shorthand

If a widget has a string `Content` prop, you can write the value as inner text instead of an attribute. These two are equivalent:

```xml
<TextBlock>Hello</TextBlock>
<TextBlock Content="Hello" />
```

Surrounding whitespace is trimmed. An explicit `Content` attribute takes precedence over inner text, and the shorthand only applies when the element has no child elements.

### Slots

Some widgets expose named slots (for example, `Card` has `Header`, `Content`, and `Footer`). Fill a slot either with the explicit `<Slot>` element or with dotted notation on the parent. These two are equivalent:

```xml
<Card>
  <Slot Name="Header">
    <TextBlock>Hello</TextBlock>
  </Slot>
</Card>

<Card>
  <Card.Header>
    <TextBlock>Hello</TextBlock>
  </Card.Header>
</Card>
```

The dotted form `<Parent.SlotName>` is sugar for `<Slot Name="SlotName">`. Property elements take precedence: if the name after the dot is a `[Prop]` it is treated as a property element, otherwise a matching declared slot is used.

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

To check that markup parses into a valid widget tree without rendering an image, use `parse`. It takes the same input arguments as `draw` (`-i` for an inline string or `-f` for a file), prints `OK` and exits 0 on success, or prints the error and exits 1 on failure:

```
ivyml parse -i <IVYML>
ivyml parse -f <FILE>
```

| Option          | Description                                          | Default |
|-----------------|------------------------------------------------------|---------|
| `-i`, `--input` | IvyML markup string                                  |         |
| `-f`, `--file`  | Path to an IvyML file                                |         |
| `-o`, `--output`| Output file path (png, jpg, webp)                    | temp    |
| `-w`, `--width` | Viewport width in pixels                             | 300     |
| `-h`, `--height`| Viewport height in pixels                            | 200     |
| `-d`, `--debug` | Draw debug overlays showing layout bounds and sizes  | false   |
| `--theme`       | Theme YAML file or built-in theme name (see Theming) | theme.yaml |
| `--no-theme`    | Turn theming off entirely                            | false      |

Provide markup via `-i` (inline string) or `-f` (file path), but not both. If `-o` is omitted, the screenshot is saved to a temp file and the path is printed to stdout.

Rendering needs headless Chromium. If Playwright has not been provisioned on the machine, `draw` downloads it automatically on first use (roughly 145 MB, once) and then continues -- no manual `playwright install` step.

### Running a file

To serve a file as a live Ivy application instead of capturing a single image, use `run`. It hosts the widget tree until you stop it with Ctrl+C:

```
ivyml run <FILE> [-p <PORT>] [--browse]
```

| Option          | Description                                            | Default |
|-----------------|--------------------------------------------------------|---------|
| `-p`, `--port`  | Port to listen on; the next free port is used if taken | 5010    |
| `-b`, `--browse`| Open the app in the default browser once listening     | false   |
| `--theme`       | Theme YAML file or built-in theme name (see Theming)   | theme.yaml |
| `--no-theme`    | Turn theming off entirely                              | false      |

The file is parsed before the server starts, so invalid markup fails immediately with the same error `parse` would print.

### Theming

`draw` and `run` accept `--theme`, which takes either a path to a YAML file or the name of a theme built into the CLI:

```
ivyml draw -f wireframe.ivyml --theme wireframes
ivyml draw -f wireframe.ivyml --no-theme
ivyml draw -f wireframe.ivyml --theme ./my-theme.yaml
```

Built-in themes: `wireframes` -- sets the whole page in the same handwriting stack the `Wireframe*` widgets use for their own labels, so the output reads as a sketch rather than a finished screen.

If `--theme` is omitted, a `theme.yaml` (or `theme.yml`) sitting next to the IvyML file is used automatically, so a folder of wireframes can carry its own look without repeating the option.

To turn theming off, pass `--no-theme`. It suppresses both `--theme` and the `theme.yaml` convention, so a themed command can be re-run unthemed without editing it -- useful for seeing what a wireframe looks like with and without the theme. (`--theme none` does the same and still works.)

A theme file only states what it changes -- everything it leaves out keeps the Ivy default. Keys are matched case-insensitively and ignoring `-`/`_`, so `fontFamily`, `font-family` and `font_family` are the same key.

```yaml
extends: wireframes         # optional: start from a built-in theme, then override

name: Sketch
fontFamily: "'Architects Daughter', 'Segoe Print', cursive"
fontFamilyMono: "'Geist Mono', monospace"
fontSize: 16px

fontFaces:                  # emitted as @font-face, so fontFamily can name a web font
  - family: Architects Daughter
    src: https://fonts.gstatic.com/s/architectsdaughter/v20/KtkxAKiDZI_td1Lkx62xHZHDtgO_Y-bvTYlg4w.woff2
    weight: 400
    style: normal

borderRadius: 12px          # or a mapping: { boxes:, fields:, selectors: }
shadows: false              # or a mapping: { boxes:, fields:, selectors: }

colors:
  light:
    background: "#fdfcf8"
    foreground: "#1f1d1a"
    primary: "#1f1d1a"
    primaryForeground: "#fdfcf8"
    border: "#3a372f"
  dark:
    background: "#1b1a17"
```

Under `colors`, `light` and `dark` accept any of the Ivy theme colors: `primary`, `primaryForeground`, `secondary`, `secondaryForeground`, `background`, `foreground`, `destructive`, `destructiveForeground`, `success`, `successForeground`, `warning`, `warningForeground`, `info`, `infoForeground`, `border`, `input`, `ring`, `muted`, `mutedForeground`, `accent`, `accentForeground`, `card`, `cardForeground`, `popover`, `popoverForeground`.

A `fontFaces` entry's `src` may be a URL, a `data:` URI, a full CSS `src` value, or a path to a local `.woff2`/`.woff`/`.ttf`/`.otf` file relative to the theme file -- a local file is inlined as a data URI so the theme renders without network access.

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

Wireframe widgets have a hand-drawn, sketch-style appearance for sketching and prototyping.

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

### WireframePlaceholder

A hand-drawn box crossed by two diagonals, standing in for an image, map, chart, or
video that has not been designed yet. Size it with `Width` and `Height`.

```xml
<WireframePlaceholder Text="Image Placeholder" />
<WireframePlaceholder Text="Hero image" Width="Full" Height="200px" Color="Sky" />
<WireframePlaceholder Width="120px" Height="120px" />
```

| Prop    | Type              | Default  | Values                                          |
|---------|-------------------|----------|-------------------------------------------------|
| `Text`  | string            |          | Optional centered caption. Omit for a bare box. |
| `Color` | Colors            | Violet   | Any color from the Colors enum                  |

Defaults to 240x140 when no `Width`/`Height` is given.

### WireframeScratchOut

A dense marker scribble, for striking through an area that is being removed or
reworked. Size it with `Width` and `Height`.

```xml
<WireframeScratchOut />
<WireframeScratchOut Color="Red" Width="180px" Height="60px" />
```

| Prop    | Type              | Default  | Values                                          |
|---------|-------------------|----------|-------------------------------------------------|
| `Color` | Colors            | Black    | Any color from the Colors enum                  |

Defaults to 200x100 when no `Width`/`Height` is given. `Density` sets the pen weight.

### WireframeRedX

Two tapered brush strokes crossing corner to corner, for marking something as wrong,
rejected, or removed. Size it with `Width` and `Height`.

```xml
<WireframeRedX />
<WireframeRedX Color="Slate" Width="120px" Height="120px" />
```

| Prop    | Type              | Default  | Values                                          |
|---------|-------------------|----------|-------------------------------------------------|
| `Color` | Colors            | Red      | Any color from the Colors enum                  |

Defaults to 200x100 when no `Width`/`Height` is given. `Density` sets the brush weight.

### WireframeArrow

A hand-drawn arrow spanning its box. Point it with `Direction`, put heads on either or
both ends, bend it to one side, and dash it.

```xml
<WireframeArrow />
<WireframeArrow Direction="DownRight" Heads="Both" Width="200px" Height="120px" />
<WireframeArrow Bend="Left" Dashed="true" Color="Red" />
```

| Prop        | Type           | Default | Values                                                              |
|-------------|----------------|---------|---------------------------------------------------------------------|
| `Direction` | ArrowDirection | Right   | Right, Left, Up, Down, UpLeft, UpRight, DownLeft, DownRight         |
| `Heads`     | ArrowHeads     | End     | None, Start, End, Both                                              |
| `Bend`      | ArrowBend      | None    | None, Left, Right                                                   |
| `Dashed`    | bool           | false   | Draws the shaft as hand-measured dashes                             |
| `Color`     | Colors         | Black   | Any color from the Colors enum                                      |

Defaults to 160x60 when no `Width`/`Height` is given. `Density` sets the line weight and
head size. The arrow runs across its box, so `Width` and `Height` set both its length and
its slope -- a tall box with `Direction="Down"` gives a long vertical arrow.

### WireframeCurlyBrace

A hand-drawn curly brace for grouping or labelling a run of content.

```xml
<WireframeCurlyBrace Height="180px" />
<WireframeCurlyBrace Variant="Vertical" Width="240px" />
```

| Prop      | Type              | Default    | Values                                     |
|-----------|-------------------|------------|--------------------------------------------|
| `Variant` | CurlyBraceVariant | Horizontal | Horizontal, Vertical                       |
| `Color`   | Colors            | Black      | Any color from the Colors enum             |

`Variant` names the direction the brace's centre nub **points**, not the direction it
spans -- the convention the widget is modelled on:

- `Horizontal` is the familiar `{`. It spans downwards, nub pointing left. Put it to the
  left of the rows it groups. Defaults to 26x140.
- `Vertical` is that shape turned a quarter. It spans across, nub pointing up. Put it
  above the columns it groups. Defaults to 140x26.

`Width` and `Height` set the span and the depth: for a `Horizontal` brace the height is
how far it reaches and the width is how deep the nub cuts. `Density` sets the pen weight.

### WireframeShape

A hand-drawn shape. Every shape is inscribed in the widget's box, so `Width` and `Height`
stretch it -- an `Ellipse` in a wide box is a wide ellipse.

```xml
<WireframeShape Shape="Ellipse" Text="Start" Filled="true" Color="Green" />
<WireframeShape Shape="Diamond" Text="Valid?" Width="120px" Height="90px" />
<WireframeShape Sides="7" />
```

| Prop     | Type               | Default   | Values                                                |
|----------|--------------------|-----------|-------------------------------------------------------|
| `Shape`  | WireframeShapeKind | Rectangle | Rectangle, Ellipse, Triangle, Diamond, Pentagon, Hexagon, Octagon, Star, Cross, Parallelogram |
| `Sides`  | int                | 0         | 3 or more draws a regular polygon and ignores `Shape` |
| `Text`   | string             |           | Optional centered label                                |
| `Filled` | bool               | false     | Washes the interior with the color                     |
| `Color`  | Colors             | Black     | Any color from the Colors enum                         |

Defaults to 140x100. `Density` sets the pen weight and label size. Use `Sides` for the
shapes the enum does not name -- a heptagon is `Sides="7"`, and a high count reads as a
hand-drawn circle.

### WireframeMockup

A hand-drawn device or browser frame that **wraps real content**. Unlike the other
wireframe widgets it takes children, and lays them out inside the frame's screen area.

```xml
<WireframeMockup Variant="Mobile">
  <StackLayout Padding="3">
    <TextBlock Variant="H4">Inbox</TextBlock>
    <Button Title="Compose" Width="Full" />
  </StackLayout>
</WireframeMockup>

<WireframeMockup Variant="Website" Url="https://example.com">
  <TextBlock Variant="H3">Product Name</TextBlock>
</WireframeMockup>

<WireframeMockup Variant="Desktop" Title="Finder">
  <TextBlock Variant="H4">Dashboard</TextBlock>
</WireframeMockup>
```

| Prop      | Type          | Default | Values                                            |
|-----------|---------------|---------|---------------------------------------------------|
| `Variant` | MockupVariant | Mobile  | Mobile, Website, Tablet, Desktop                  |
| `Title`   | string        |         | Window title, Desktop variant only                |
| `Url`     | string        |         | Address bar text, Website variant only            |
| `Color`   | Colors        | Black   | Any color from the Colors enum                    |

`Desktop` is a macOS-style application window -- rounded shell, title bar with traffic
lights, content directly beneath. `Website` is a browser window: toolbar with back,
forward, reload and an address pill, no tab strip.

Default sizes: Mobile 300x600, Tablet 520x700, Desktop 660x500, Website 660x460. Override
with `Width`/`Height`; the screen area shrinks with the frame. Content that overflows the
screen is clipped, exactly as it would be on the device.

### WireframeTransform

Renders its children under a transform. Takes children like `WireframeMockup` does.

```xml
<WireframeTransform Rotate="-4">
  <WireframeNote Text="Pinned at an angle" />
</WireframeTransform>

<WireframeTransform Scale="0.5" Origin="TopLeft">
  <WireframeMockup Variant="Mobile" />
</WireframeTransform>
```

| Prop             | Type            | Default | Notes                                        |
|------------------|-----------------|---------|----------------------------------------------|
| `Rotate`         | double          | 0       | Clockwise degrees                            |
| `Scale`          | double          | 1       | Uniform scale                                |
| `ScaleX`         | double          |         | Overrides `Scale` horizontally               |
| `ScaleY`         | double          |         | Overrides `Scale` vertically                 |
| `SkewX`, `SkewY` | double          | 0       | Skew in degrees                              |
| `OffsetX`, `OffsetY` | double      | 0       | Nudge in pixels                              |
| `FlipHorizontal` | bool            | false   | Mirrors horizontally                         |
| `FlipVertical`   | bool            | false   | Mirrors vertically                           |
| `Origin`         | TransformOrigin | Center  | Center, TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight |
| `Fit`            | bool            | false   | Shrink the layout box to the transformed bounds |
| `Opacity`        | double          | 1       | Fades the children                           |

Transforms are applied in the order translate, rotate, scale, skew.

**The transform is visual only.** Children keep the layout box they started with, so a
rotated or scaled child does not push its neighbours around and may overlap them. Put the
transform on a `CanvasLayout`, or leave room around it, when the transformed bounds matter.

Or set `Fit="true"`, which sizes the widget to the transformed bounds so it takes up the
room it visually occupies and flows normally in a layout. That is usually what you want
when scaling a mockup down to sit inline:

```xml
<WireframeTransform Scale="0.5" Fit="true">
  <WireframeMockup Variant="Mobile" />
</WireframeTransform>
```

With `Fit` on, `Origin` no longer changes anything -- the bounds are recentred either way.

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

## Widget Defaults

Many widgets have built-in defaults. **Do not set an attribute if it matches the default** -- it adds noise without changing behavior. Use `ivyml docs <widget>` to see all defaults for a specific widget.

| Widget       | Prop          | Default      |
|--------------|---------------|--------------|
| StackLayout  | Orientation   | Vertical     |
| StackLayout  | RowGap        | 4            |
| StackLayout  | ColumnGap     | 4            |
| GridLayout   | RowGap        | 4            |
| GridLayout   | ColumnGap     | 4            |
| Button       | Variant       | Primary      |
| Button       | IconPosition  | Left         |
| Button       | BorderRadius  | Rounded      |
| Badge        | IconPosition  | Left         |
| TextBlock    | Variant       | Literal      |
| Separator    | Orientation   | Horizontal   |
| Card         | HoverVariant  | None         |
| Box          | BorderRadius  | Rounded      |
| Box          | BorderStyle   | Solid        |
| Box          | Padding       | 2            |

For example, these two are equivalent:

```xml
<!-- Redundant -- don't do this -->
<StackLayout Orientation="Vertical" RowGap="4" ColumnGap="4">
  <Button Title="Submit" Variant="Primary" />
</StackLayout>

<!-- Clean -- use this -->
<StackLayout>
  <Button Title="Submit" />
</StackLayout>
```

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

Composed example -- a bordered box with header, badges, progress bar, and buttons:
```xml
<Box Width="400px" Padding="6">
  <StackLayout>
    <TextBlock Content="Project Status" Variant="H3" />
    <TextBlock Content="Sprint 12 is 75% complete." Variant="Muted" />
    <Separator />
    <StackLayout>
      <Badge Title="Auth service" Variant="Success" />
      <Badge Title="Dashboard UI" Variant="Warning" />
      <Badge Title="API docs" Variant="Info" />
    </StackLayout>
    <Progress Value="75" />
    <StackLayout Orientation="Horizontal">
      <Button Title="View Details" Variant="Outline" />
      <Button Title="Mark Complete" Variant="Success" />
    </StackLayout>
  </StackLayout>
</Box>
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
