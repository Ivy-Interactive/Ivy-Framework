# Button Group (Legacy)

A group of buttons to trigger actions when clicked. This is a deprecated Retool component that has been superseded by the newer Button Group component. In Ivy, the equivalent is achieved by combining a horizontal `StackLayout` with multiple `Button` widgets, or by using a `SelectInput` with the `Toggle` variant for selection-style button groups.

## Retool

```toolscript
buttonGroup1.setHidden(false);
buttonGroup1.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

```csharp
// Action-style button group using horizontal StackLayout
new StackLayout([
    new Button("Option A", _ => client.Toast("A clicked")).Secondary(),
    new Button("Option B", _ => client.Toast("B clicked")).Secondary(),
    new Button("Option C", _ => client.Toast("C clicked")).Secondary()
], Orientation.Horizontal, gap: 4)

// Selection-style button group using SelectInput with Toggle variant
var selected = UseState("A");
selected.ToSelectInput(new[] { "A", "B", "C" }.ToOptions())
    .Variant(SelectInputs.Toggle);
```

## Parameters

| Parameter                | Documentation                                              | Ivy                                                                   |
|--------------------------|------------------------------------------------------------|-----------------------------------------------------------------------|
| `alignment`              | Positioning of content (`left`, `center`, `right`)         | `StackLayout` `align` parameter (`Align.Left`, `Center`, `Right`)     |
| `events`                 | Configured event handlers triggering actions/queries        | `Button.OnClick` handler per button                                   |
| `heightType`             | Auto-adjusting or fixed height (`fixed` or `auto`)         | Not supported (layout auto-sizes)                                     |
| `hidden`                 | Controls visibility of the component                       | `Button.Visible` / `StackLayout` conditional rendering                |
| `id`                     | Unique identifier/name                                     | Variable name in C#                                                   |
| `isHiddenOnDesktop`      | Desktop layout visibility toggle                           | Not supported                                                         |
| `isHiddenOnMobile`       | Mobile layout visibility toggle                            | Not supported                                                         |
| `maintainSpaceWhenHidden`| Reserve canvas space when hidden                           | Not supported                                                         |
| `margin`                 | External spacing (`4px 8px`, `0`, etc.)                    | `StackLayout` `margin` parameter                                      |
| `overflowMode`           | Handling overflow: `scroll` or `wrap`                      | Not supported (use `WrapLayout` for wrapping)                         |
| `overflowPosition`       | Index where items move to overflow menu                    | Not supported                                                         |
| `overlayMaxHeight`       | Options list maximum height (pixels)                       | Not supported                                                         |
| `overlayMinWidth`        | Options list minimum width (pixels)                        | Not supported                                                         |
| `showInEditor`           | Visibility in editor when hidden                           | Not supported                                                         |
| `style`                  | Custom style options                                       | `Button.Variant`, `Foreground`, `BorderRadius`, `Scale`               |
| `scrollIntoView()`       | Scrolls container to display component                     | Not supported                                                         |
| `setHidden()`            | Toggle component visibility programmatically               | Set `Visible` property on widgets                                     |
