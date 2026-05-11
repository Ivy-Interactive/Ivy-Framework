# Callout

A callout displays a short, prominent message designed to capture user attention. It is typically used for informational notices, warnings, errors, and success confirmations.

## Reflex

```python
import reflex as rx

rx.callout.root(
    rx.callout.icon(),
    rx.callout.text("You will need admin privileges to install and access this application."),
    size="2",
    variant="soft",
    color_scheme="blue",
)
```

## Ivy

```csharp
new Callout("You will need admin privileges to install and access this application.",
    variant: CalloutVariant.Info)

// Or using the static factory method:
Callout.Info("You will need admin privileges to install and access this application.")
```

## Parameters

| Parameter      | Documentation                                                                 | Ivy                                                                 |
|----------------|-------------------------------------------------------------------------------|---------------------------------------------------------------------|
| text/description | The message content to display                                              | `description` (object) — main content passed as first argument      |
| icon           | Icon identifier shown alongside text                                          | `icon` (Icons?) — custom icon override; defaults based on variant   |
| size           | Controls component size (`"1"`, `"2"`, etc.)                                  | `Scale` (Scale?) — scaling factor                                   |
| variant        | Visual styling (`"soft"`, `"surface"`, `"outline"`)                           | `variant` (CalloutVariant) — `Info`, `Success`, `Warning`, `Error`  |
| color_scheme   | Color assignment (`"tomato"`, `"red"`, `"blue"`, etc.)                        | Not supported (color is determined by variant)                      |
| high_contrast  | Increases visual contrast (bool)                                              | Not supported                                                       |
| as_child       | Renders as child element for structural composition                           | Not supported                                                       |
| title          | Not supported                                                                 | `title` (string) — optional heading above the description           |
| Width          | Supported via style props                                                     | `Width` (Size)                                                      |
| Height         | Supported via style props                                                     | `Height` (Size)                                                     |
| Visible        | Supported via conditional rendering                                           | `Visible` (bool)                                                    |
