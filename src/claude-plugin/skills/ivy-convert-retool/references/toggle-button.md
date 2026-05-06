# Toggle Button

A binary state button that can be toggled between true and false values, used to trigger different actions or represent on/off settings.

## Retool

```toolscript
{{toggleButton1.value}} // Access current boolean state

// Configure via properties
toggleButton1.setValue(true)
toggleButton1.toggle()
toggleButton1.resetValue()
```

## Ivy

```csharp
var isFavorite = UseState(false);
return isFavorite.ToToggleInput(Icons.Heart)
                 .Label("Add to Favorites");
```

## Parameters

| Parameter        | Documentation                                           | Ivy                                                        |
|------------------|---------------------------------------------------------|------------------------------------------------------------|
| value            | Current boolean state (default: `false`)                | `UseState(false)` bound to `BoolInput`                     |
| text             | Primary button text                                     | `.Label("...")`                                            |
| disabled         | Prevents interaction (default: `false`)                 | `.Disabled` property / constructor param                   |
| hidden           | Controls visibility (default: `false`)                  | `.Visible` property                                        |
| loading          | Display a loading indicator (default: `false`)          | Not supported                                              |
| iconForTrue      | Icon when value is true                                 | `.Icon(Icons.X)` (single icon, no per-state distinction)   |
| iconForFalse     | Icon when value is false                                | `.Icon(Icons.X)` (single icon, no per-state distinction)   |
| styleVariant     | Visual style: `solid` or `outline`                      | `BoolInputs` variant: `Checkbox`, `Switch`, `Toggle`       |
| horizontalAlign  | Alignment: `left`, `center`, `right`                    | Not supported                                              |
| ariaLabel        | Accessible label for screen readers                     | Not supported                                              |
| margin           | External spacing (default: `4px 8px`)                   | Not supported                                              |
| isHiddenOnMobile | Hide on mobile layout (default: `true`)                 | Not supported                                              |
| isHiddenOnDesktop| Hide on desktop layout (default: `false`)               | Not supported                                              |
| loaderPosition   | Loader placement: `auto`, `replace`, `left`, `right`    | Not supported                                              |
| onChange         | Event triggered on value change                         | `.OnChange` event handler                                  |
| onTrue           | Event triggered when value becomes true                 | Not supported (use `OnChange` and check value)             |
| onFalse          | Event triggered when value becomes false                | Not supported (use `OnChange` and check value)             |
| setValue()       | Method to set current value                             | State is set via `UseState` binding                        |
| toggle()         | Method to switch between true/false                     | State is toggled via `UseState` binding                    |
| resetValue()     | Method to restore default value                         | Not supported                                              |
| clearValue()     | Method to remove current value                          | Nullable bool via `UseState((bool?)null)`                  |
| description      | Not available                                           | `.Description("...")` property                             |
| invalid          | Not available                                           | `.Invalid("...")` validation message                       |
