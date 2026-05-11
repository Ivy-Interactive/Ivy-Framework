# Toggle Link

A toggle link to trigger different actions when clicked. It displays as a styled hyperlink that toggles between `true` and `false` states, supporting custom text, icons, underline styles, and event handlers for state changes.

## Retool

```toolscript
// Set the default value
toggleLink1.setValue(true);

// Toggle the current value
toggleLink1.toggle();

// React to changes via event handlers:
// - Change: fires when value changes
// - True: fires when set to true
// - False: fires when set to false
```

## Ivy

```csharp
// BoolInput with Toggle variant is the closest equivalent
var isActive = new State<bool>(false);

isActive.ToToggleInput()
    .Label("Enable feature")
    .Icon(Icons.Link);

// Or using the Switch variant for a similar on/off experience
isActive.ToSwitchInput(Icons.ToggleOn)
    .Label("Enable feature");

// React to value changes via state binding
// The state object automatically tracks changes
```

## Parameters

| Parameter              | Documentation                                                    | Ivy                                                  |
|------------------------|------------------------------------------------------------------|------------------------------------------------------|
| `value`                | The current boolean value (`true`/`false`). Default: `false`     | State value via `State<bool>` or `State<bool?>`      |
| `text`                 | The display text for the link                                    | `.Label()`                                           |
| `disabled`             | Whether interaction is disabled. Default: `false`                | `.Disabled()`                                        |
| `tooltipText`          | Tooltip text displayed on hover                                  | `.Description()`                                     |
| `events`               | Event handlers for Change, True, False events                    | State binding (reactive by default)                  |
| `horizontalAlign`      | Horizontal alignment (`left`, `center`, `right`, `stretch`)      | Not supported                                        |
| `iconPosition`         | Icon position relative to text (`left`, `right`, `replace`)      | Not supported                                        |
| `iconType`             | Icon type (`caret`, `plusMinus`, `arrow`)                        | `.Icon()` (uses `Icons` enum)                        |
| `showUnderline`        | Underline display (`always`, `hover`, `never`)                   | Not supported (toggle/switch variant has no underline)|
| `underlineStyle`       | Underline style (`solid`, `dashed`, `dotted`)                    | Not supported                                        |
| `allowWrap`            | Whether content can wrap to multiple lines. Default: `true`      | Not supported                                        |
| `style`                | Custom style options                                             | Not supported                                        |
| `margin`               | Margin spacing (`4px 8px` or `0`)                                | Not supported                                        |
| `isHiddenOnDesktop`    | Hide on desktop layout. Default: `false`                         | `.Visible()` (single property for all layouts)       |
| `isHiddenOnMobile`     | Hide on mobile layout. Default: `true`                           | `.Visible()` (single property for all layouts)       |
| `maintainSpaceWhenHidden` | Keep space when hidden. Default: `false`                      | Not supported                                        |
| `showInEditor`         | Show in editor when hidden. Default: `false`                     | Not supported                                        |
| `Variant`              | N/A (Retool has separate toggle/switch/checkbox components)      | `.Variant()` — Checkbox, Switch, or Toggle           |
| `Nullable`             | N/A                                                              | `.Nullable()` — supports `bool?` for tri-state       |
| `Invalid`              | N/A                                                              | `.Invalid()` — validation state                      |
