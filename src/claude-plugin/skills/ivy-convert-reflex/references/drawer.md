# Drawer

An overlay panel that slides in from the edge of the screen to display additional content (navigation, forms, details) without navigating away from the current page. The user can dismiss it by clicking outside, pressing Escape, or using a close button.

## Reflex

```python
rx.drawer.root(
    rx.drawer.trigger(rx.button("Open Drawer")),
    rx.drawer.overlay(z_index="5"),
    rx.drawer.portal(
        rx.drawer.content(
            rx.flex(
                rx.drawer.close(rx.box(rx.button("Close"))),
                align_items="start",
                direction="column",
            ),
            top="auto",
            right="auto",
            height="100%",
            width="20em",
            padding="2em",
            background_color="#FFF",
        )
    ),
    direction="left",
)
```

## Ivy

```csharp
new Button("Open Sheet").WithSheet(
    () => Layout.Vertical().Gap(2)
        | Text.P("Sheet content goes here")
        | new Button("Save").Variant(ButtonVariant.Primary),
    title: "My Sheet",
    description: "Additional details or forms",
    width: Size.Fraction(1 / 2f)
);
```

## Parameters

| Parameter            | Reflex                                                                 | Ivy                                                          |
|----------------------|------------------------------------------------------------------------|--------------------------------------------------------------|
| Open state           | `open` (bool) on `rx.drawer.root`                                     | Managed via `UseState` + conditional rendering, or `.WithSheet()` extension |
| Direction            | `direction` ("left", "right", "top", "bottom") on `rx.drawer.root`    | Not supported (slides in from the right only)                |
| Modal                | `modal` (bool) on `rx.drawer.root`                                    | Not supported (always modal)                                 |
| Dismissible          | `dismissible` (bool) on `rx.drawer.root`                              | Built-in close via `onClose` callback                        |
| Title                | Manual via content layout                                              | `title` (string) on `Sheet` constructor                      |
| Description          | Manual via content layout                                              | `description` (string) on `Sheet` constructor                |
| Width                | CSS `width` on `rx.drawer.content`                                     | `Width` property (`Size.Rem()`, `Size.Fraction()`, `Size.Full()`) |
| Height               | CSS `height` on `rx.drawer.content`                                    | `Height` property                                            |
| Snap points          | `snap_points` (Sequence) on `rx.drawer.root`                          | Not supported                                                |
| Close threshold      | `close_threshold` (float) on `rx.drawer.root`                         | Not supported                                                |
| Overlay              | `rx.drawer.overlay` sub-component                                      | Built-in (automatic backdrop)                                |
| On open/close        | `on_open_change` event on `rx.drawer.root`                            | `OnClose` event on `Sheet`                                   |
| On animation end     | `on_animation_end` event on `rx.drawer.root`                          | Not supported                                                |
| Nested close button  | `rx.drawer.close` sub-component                                       | Automatic close button in header                             |
| Trigger element      | `rx.drawer.trigger` sub-component                                      | `.WithSheet()` extension on any `Button`                     |
| Portal rendering     | `rx.drawer.portal` sub-component                                       | Not supported (handled internally)                           |
