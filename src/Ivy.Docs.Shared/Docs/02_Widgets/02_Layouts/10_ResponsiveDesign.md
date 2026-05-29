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
Build adaptive layouts that respond to viewport size using the built-in breakpoint system. Vary width, height, visibility, columns, orientation, gap, padding, and density at different screen sizes.
</Ingress>

Ivy's responsive design system lets you vary widget properties by viewport size using `Responsive<T>` values with mobile-first cascading.

## Basic Usage

A typical first step is a [grid](03_GridLayout.md) whose column count depends on the viewport: chain `.At()` and `.And()` on the value you pass to `.Columns()`:

```csharp
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 3))
    | new Card("A")
    | new Card("B")
    | new Card("C")
```

```csharp demo
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 3))
    .Gap(4)
    | new Card("A")
    | new Card("B")
    | new Card("C")
```

## Breakpoints

| Breakpoint | Max Width | Typical Device |
|---|---|---|
| `Mobile` | 640px | Phones |
| `Tablet` | 768px | Tablets |
| `Desktop` | 1024px | Laptops |
| `Wide` | 1280px | Large screens |

Values **cascade upward**: when you set a value at a smaller breakpoint, it applies to all larger breakpoints unless overridden. This is called **mobile-first** design — you start with the smallest screen and layer on changes for bigger viewports.

```csharp
// Setting columns at Mobile means it applies to Mobile, Tablet, Desktop, and Wide
// unless a larger breakpoint overrides it
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile)        // 1 col on Mobile AND Tablet (cascades up)
        .And(Breakpoint.Desktop, 3))         // 3 cols on Desktop AND Wide
```

## The Responsive type

A plain `T` converts implicitly to `Responsive<T>` with only `Default` set, so existing call sites stay valid. For per-breakpoint values, chain `.At(breakpoint)` on a supported type, then `.And(breakpoint, value)` for further overrides:

```csharp
// Plain value → implicit Responsive<T> (Default only)
Layout.Grid().Columns(3)

// Breakpoint overrides — cascade from Mobile unless a larger breakpoint sets its own
Layout.Grid().Columns(1.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 3))

Size.Full().At(Breakpoint.Mobile)
    .And(Breakpoint.Tablet, Size.Units(80))
    .And(Breakpoint.Desktop, Size.Half())
```

The `.At()` extension method is available for the following types:

| Type | Example | Used For |
|---|---|---|
| `Size` (class) | `Size.Full().At(Breakpoint.Mobile)` | Width, Height |
| `int` | `1.At(Breakpoint.Mobile)` | Gap, Columns |
| `Orientation` | `Orientation.Vertical.At(Breakpoint.Mobile)` | Layout orientation |
| `Density` | `Density.Large.At(Breakpoint.Mobile)` | Touch target size |
| `bool` | `true.At(Breakpoint.Mobile)` | Visibility |

## Responsive properties

Most layout and widget APIs accept `Responsive<T>` values built with `.At()` and `.And()`. Use `.HideOn()` and `.ShowOn()` for visibility, and construct `Responsive<Thickness?>` with object initializers when setting padding (there is no `.At()` for `Thickness`). The demo below shows width, height, grid columns, stack orientation, gap, padding, density, and badges in a single view.

```csharp demo-below
Layout.Vertical()
    .Gap(2.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
    .Padding(new Responsive<Thickness?>
    {
        Mobile = new Thickness(8),
        Desktop = new Thickness(24)
    })
    | new Badge("Large screens").HideOn(Breakpoint.Mobile, Breakpoint.Tablet)
    | new Badge("Phones").ShowOn(Breakpoint.Mobile)
    | (Layout.Horizontal()
        .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
            .And(Breakpoint.Desktop, Orientation.Horizontal))
        | new Button("Density")
            .Density(Density.Large.At(Breakpoint.Mobile)
                .And(Breakpoint.Desktop, Density.Medium))
        | new Box(Text.P("Sized box"))
            .Width(Size.Full().At(Breakpoint.Mobile)
                .And(Breakpoint.Desktop, Size.Half()))
            .Height(Size.Units(56).At(Breakpoint.Mobile)
                .And(Breakpoint.Desktop, Size.Units(40)))
            .Background(Colors.Primary))
    | (Layout.Grid()
        .Columns(1.At(Breakpoint.Mobile)
            .And(Breakpoint.Tablet, 2)
            .And(Breakpoint.Desktop, 3))
        | new Card("One")
        | new Card("Two")
        | new Card("Three"))
```

## API Reference

Summary of all responsive-capable properties:

| Property | Widget/Layout | Type | Fluent Method |
|---|---|---|---|
| Width | All widgets | `Responsive<Size>` | `.Width()` |
| Height | All widgets | `Responsive<Size>` | `.Height()` |
| Visible | All widgets | `Responsive<bool?>` | `.HideOn()` / `.ShowOn()` |
| Density | All widgets | `Responsive<Density?>` | `.Density()` |
| Columns | GridLayout | `Responsive<int?>` | `.Columns()` |
| Orientation | StackLayout | `Responsive<Orientation?>` | `.Orientation()` |
| Gap | StackLayout, GridLayout | `Responsive<int?>` | `.Gap()` |
| Padding | StackLayout | `Responsive<Thickness?>` | `.Padding()` |

## Examples

<Details>
<Summary>
Collapsing Sidebar
</Summary>
<Body>
Hide the sidebar on phones only. Prefer `.ShowOn(Breakpoint.Tablet)` here: `.HideOn(Breakpoint.Mobile)` sets visibility false at `Mobile`, and that value **cascades** to tablet unless you override it—so the sidebar would stay hidden on tablet too.

```csharp demo-tabs
public class CollapsingSidebarExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal()
            .Height(Size.Units(36))
            | new Box(Text.P("Sidebar"))
                .Width(Size.Units(60))
                .ShowOn(Breakpoint.Tablet)
                .Background(Colors.Muted)
            | new Box(Text.P("Main Content"))
                .Width(Size.Grow())
                .Background(Colors.Primary);
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Responsive Dashboard
</Summary>
<Body>
Combine a responsive grid, conditional visibility, and stack orientation for a layout that works across device sizes.

```csharp demo-tabs
public class ResponsiveDashboardExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | new Badge("Dashboard").HideOn(Breakpoint.Mobile)
            | (Layout.Grid()
                .Columns(1.At(Breakpoint.Mobile)
                    .And(Breakpoint.Desktop, 3))
                | new Card("Revenue")
                | new Card("Users")
                | new Card("Orders"))
            | (Layout.Horizontal()
                .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
                    .And(Breakpoint.Desktop, Orientation.Horizontal))
                | new Box(Text.P("Chart Area")).Width(Size.Grow()).Background(Colors.Muted)
                | new Box(Text.P("Activity Feed")).Width(Size.Units(60).At(Breakpoint.Desktop)).HideOn(Breakpoint.Mobile).Background(Colors.Muted));
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Mobile-First Form
</Summary>
<Body>
Keep the form full width on phones and cap width plus gap on larger breakpoints. Use [`.ToForm()`](../../01_Onboarding/02_Concepts/08_Forms.md) on state so fields and validation match a real model.

```csharp demo-tabs
public class MobileFirstFormExample : ViewBase
{
    public record ContactModel(string Name, string Email);

    public override object? Build()
    {
        var contact = UseState(() => new ContactModel("", ""));

        return Layout.Vertical()
            .Width(Size.Full().At(Breakpoint.Mobile)
                .And(Breakpoint.Desktop, Size.Fraction(0.5f)))
            .Gap(4.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
            | contact.ToForm()
                .Required(m => m.Name, m => m.Email);
    }
}
```

</Body>
</Details>
