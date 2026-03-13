# Box.Color() Renamed to Box.Background()

The `Color()` attribute and extension methods on the `Box` widget have been renamed to `Background()` to more intuitively reflect what the property controls (the background color of the box).

- Previously: `new Box("Content").Color(Colors.Slate)`
- Now: `new Box("Content").Background(Colors.Slate)`

**Details:**

- The property `Colors? Color` on `Box` is now `Colors? Background`.
- The fluent extension methods `Box.Color(Colors)` and `Box.Color(Colors, float)` are now `Box.Background(Colors)` and `Box.Background(Colors, float)`.
- If you were using `anything.WithCell().Color(...)`, switch to `anything.WithCell().Background(...)`.

**What remains unchanged:**

- `Text.Color(Colors)` and `Icon.Color(Colors)` function as before, changing the text/foreground color.
- `Box.BorderColor()` remains unchanged.
