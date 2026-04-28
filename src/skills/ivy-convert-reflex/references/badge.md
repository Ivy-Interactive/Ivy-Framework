# Badge

Badges are used to highlight an item's status for quick recognition, displaying small pieces of information such as counts or statuses in a compact form.

## Reflex

```python
rx.badge("New")

# With variant and color
rx.badge("Sale", variant="solid", color_scheme="red", size="2")

# With icon inside
rx.badge(
    rx.flex(rx.icon("bell", size=16), "Notification", gap="1"),
    variant="surface",
)
```

## Ivy

```csharp
new Badge("Primary")

// With variant
new Badge("Destructive", variant: BadgeVariant.Destructive)

// With icon
new Badge("Notification", icon: Icons.Bell)

// Icon on the right
new Badge("Download").Icon(Icons.Download, Align.Right)
```

## Parameters

| Parameter      | Documentation                                                                 | Ivy                                                                  |
|----------------|-------------------------------------------------------------------------------|----------------------------------------------------------------------|
| text / title   | The text content displayed in the badge                                       | `Title` (string), passed as first constructor argument               |
| variant        | Visual style: `"solid"`, `"soft"`, `"surface"`, `"outline"` (default `soft`) | `Variant` (BadgeVariant): Primary, Destructive, Outline, Secondary, Success, Warning, Info |
| size           | Size of the badge: `"1"` or `"2"` (default `"1"`)                           | `Scale` (Scale?) for scaling; `Height`/`Width` for explicit sizing   |
| color_scheme   | Sets a specific color (e.g. `"tomato"`, `"red"`, `"blue"`)                  | Controlled via `Variant` enum (no free-form color scheme)            |
| high_contrast  | Bool to increase color contrast between text and background                   | Not supported                                                        |
| radius         | Border radius: `"none"`, `"small"`, `"medium"`, `"large"`, `"full"`         | Not supported                                                        |
| icon           | Achieved by nesting `rx.icon()` inside the badge                             | `Icon` (Icons?) — first-class parameter with `IconPosition` (Align)  |
| visible        | Controlled via `rx.cond` or conditional rendering                            | `Visible` (bool) — built-in property                                 |
