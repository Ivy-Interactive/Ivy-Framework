# Close Button

A preset button with a preconfigured close/X icon designed to trigger dismiss actions when clicked. Commonly used to close modals, panels, or dialogs.

## Retool

```toolscript
closeButton1.text = ""
closeButton1.iconBefore = "bold/interface-delete-1"
closeButton1.styleVariant = "outline"
closeButton1.disabled = false
closeButton1.hidden = false

// Event handler
closeButton1.onClick = () => {
  modal1.close()
}
```

## Ivy

Ivy does not have a dedicated Close Button widget. The equivalent is a `Button` configured with a close icon. The `Dialog` widget includes a built-in close button in its `DialogHeader`.

```csharp
// Standalone close button
new Button(onClick: _ => isOpen.Set(false))
    .Icon(Icons.X)
    .Variant(ButtonVariant.Ghost);

// Dialog with built-in close button in header
new Dialog(
    onClose: _ => isOpen.Set(false),
    new DialogHeader("Title"),
    new DialogBody(Text.P("Content")),
    new DialogFooter(
        new Button("Cancel", _ => isOpen.Set(false)).Outline(),
        new Button("Confirm", _ => { /* action */ })
    )
);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `text` | Primary text content displayed on the button | `Title` |
| `disabled` | Toggles whether interaction is disabled (default: `false`) | `Disabled` |
| `hidden` | Controls visibility of the component | `Visible` (inverted logic) |
| `loading` | Displays a loading indicator when `true` | `Loading` |
| `styleVariant` | Style options: `solid`, `outline` | `Variant` (`ButtonVariant`: Primary, Secondary, Outline, Ghost, etc.) |
| `iconBefore` | Prefix icon to display | `Icon` with `IconPosition` set to left |
| `iconAfter` | Suffix icon to display | `Icon` with `IconPosition` set to right |
| `horizontalAlign` | Alignment: `left`, `center`, `right`, `stretch` | Not supported (handled by parent layout) |
| `heightType` | Height mode: `auto` or `fixed` | `Height` (`Size`) |
| `margin` | External margin (default: `4px 8px`) | Not supported (handled by parent layout) |
| `ariaLabel` | Accessible label for screen readers | `Tooltip` (partial equivalent) |
| `allowWrap` | Controls whether content can wrap to multiple lines | Not supported |
| `isHiddenOnDesktop` | Show/hide on desktop layout | Not supported |
| `isHiddenOnMobile` | Show/hide on mobile layout | Not supported |
| `loaderPosition` | Loader placement: `auto`, `replace`, `left`, `right` | Not supported |
| `submit` | Submits form when clicked if `true` | Not supported |
| `clickable` | Indicates if a Click event handler is enabled | Not supported (implicit via `onClick`) |
| `events` | List of configured event handlers | `onClick` callback |
| `setDisabled()` | Method to toggle disabled state | Set `Disabled` property |
| `setHidden()` | Method to toggle visibility | Set `Visible` property |
