# Issue 2549: WidgetBase Generic Constraint Mismatch and Width/Height Refactor

## Changes
- Introduced a non-generic `WidgetBase` record to act as the base type for `WidgetBase<T>`.
- Modified `WidgetBaseExtensions` constraints from `where T : WidgetBase<T>` to `where T : WidgetBase`. This fixes the extension resolution for widgets inheriting from generic forms (e.g. `Card`, `SelectInput<T>`).
- Removed `Width(int)`, `Width(float)`, `Height(int)`, `Height(float)` and `Size(int)` from `WidgetBaseExtensions`.

## Migration Guide for LLMs
If a user is upgrading their Ivy Framework project and encounters compilation errors such as:
`error CS1929: '...' does not contain a definition for 'Width' and the best extension method overload requires a receiver of type ...`

This means they were using the old implicit size extension methods.
**Resolution**:
Find usages like:
`.Width(100)` -> Change to `.Width(Size.Units(100))`
`.Height(50)` -> Change to `.Height(Size.Units(50))`
`.Size(200)`  -> Change to `.Size(Size.Units(200))`

For percentages, use `Size.Fraction`:
`.Width("100%")` -> Change to `.Width(Size.Fraction(1f))`
`.Height(0.5f)`  -> Change to `.Height(Size.Fraction(0.5f))`
