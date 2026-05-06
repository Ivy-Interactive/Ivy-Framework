# JSON Editor

An interface to edit and validate JSON data. Provides syntax highlighting, formatting, and validation feedback.

## Retool

```toolscript
jsonEditor1.stringValue = '{ "a": { "b": [1,2,3], "c": { "d": false }, "e": "hi" } }'
jsonEditor1.valid // true or false
```

## Ivy

```csharp
var json = UseState("{}");

json.ToCodeInput()
    .Language(Languages.Json)
    .Width(Size.Full())
    .Height(Size.Screen(50));
```

## Parameters

| Parameter                | Documentation                                          | Ivy                                                        |
|--------------------------|--------------------------------------------------------|------------------------------------------------------------|
| `stringValue`            | Current or default JSON string value                   | `Value` (via `UseState`)                                   |
| `valid`                  | Whether the current value is valid JSON (read-only)    | `Invalid` (string error message, inverse approach)         |
| `hidden`                 | Whether the component is visible                       | `Visible` (bool, inverse)                                  |
| `isHiddenOnDesktop`      | Hide on desktop layout                                 | Not supported                                              |
| `isHiddenOnMobile`       | Hide on mobile layout                                  | Not supported                                              |
| `maintainSpaceWhenHidden`| Reserve canvas space when hidden                       | Not supported                                              |
| `showInEditor`           | Visibility in editor mode                              | Not supported                                              |
| `margin`                 | External spacing around component                      | Not supported (layout-driven)                              |
| `style`                  | Custom CSS styling                                     | `Scale`, `Variant`                                         |
| `formDataKey`            | Key for form component data                            | Not supported (state-driven binding)                       |
| `events.change`          | Triggered when JSON value is modified                  | `OnChange`                                                 |
| N/A                      | N/A                                                    | `Language` (supports JSON and 10+ other languages)         |
| N/A                      | N/A                                                    | `Placeholder` (hint text when empty)                       |
| N/A                      | N/A                                                    | `Disabled` (read-only mode)                                |
| N/A                      | N/A                                                    | `ShowCopyButton` (copy-to-clipboard toggle)                |
| N/A                      | N/A                                                    | `OnBlur` (fires when focus is lost)                        |
| N/A                      | N/A                                                    | `Nullable` (allows null values)                            |
| N/A                      | N/A                                                    | `Width` / `Height` (dimensional sizing)                    |
