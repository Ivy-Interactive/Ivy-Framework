# Kbd

Represents keyboard input or a hotkey. Displays keyboard shortcuts or key combinations with proper styling to help users identify commands.

## Reflex

```python
rx.text.kbd("Shift + Tab")
```

## Ivy

```csharp
new Kbd("Ctrl + C")
```

## Parameters

| Parameter | Documentation                                                                 | Ivy           |
|-----------|-------------------------------------------------------------------------------|---------------|
| content   | The text content to display inside the keyboard indicator                      | `content` (constructor argument) |
| size      | `"1"` - `"9"` — Controls text size with appropriate line height and letter spacing | Not supported |
| height    | Not supported                                                                 | `Height` (read-only `Size`) |
| width     | Not supported                                                                 | `Width` (read-only `Size`) |
| scale     | Not supported                                                                 | `Scale` (read-only `Scale?`) |
| visible   | Not supported                                                                 | `Visible` (read-only `bool`) |
