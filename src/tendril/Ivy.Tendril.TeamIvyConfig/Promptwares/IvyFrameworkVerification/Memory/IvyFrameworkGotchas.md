# Ivy Framework Gotchas

Common mistakes and issues encountered when working with the Ivy Framework during feature testing and development.

## Hook Rules (Critical)

### Hook Ordering — IVYHOOK005
**All Ivy hooks MUST be called at the very top of `Build()` method, before ANY other statements.**

❌ **WRONG — Causes IVYHOOK005 warning:**
```csharp
public override object? Build() {
    var state1 = UseState<int>(() => 0);
    var value = state1.Value * 2;        // ❌ Logic between hooks
    var state2 = UseState<string>("");   // ❌ Hook after logic
    return Layout.Vertical() | ...;
}
```

✅ **CORRECT — All hooks first:**
```csharp
public override object? Build() {
    var state1 = UseState<int>(() => 0);
    var state2 = UseState<string>(() => "");
    var service = UseService<IClientProvider>();

    var value = state1.Value * 2;
    return Layout.Vertical() | ...;
}
```

**Why**: Ivy's hook system relies on consistent call order across renders. Conditional or out-of-order hooks break state tracking.

### Extension Hooks on IViewContext (UseLoading, UseAlert, etc.)
❌ **`UseLoading()`** — direct call from ViewBase fails with CS0103
✅ **`this.Context.UseLoading()`** — extension methods on `IViewContext` must be called via `this.Context`
📝 **Why**: `ViewBase` wraps hooks like `UseState`, `UseEffect` as protected methods. But extension methods on `IViewContext` aren't wrapped.

### TryUseService Requires Context Prefix
❌ **`TryUseService<T>(out var x)`** — CS0103 from ViewBase
✅ **`Context.TryUseService<T>(out var x)`**
📝 `UseService<T>()` IS available directly on `ViewBase` (without `Context.`).

## Serialization & CamelCase Patterns

### DataKey String Values Are NOT CamelCased
Ivy's `WidgetSerializer` uses `JsonNamingPolicy.CamelCase` for property names and dictionary keys, but string VALUES (like `DataKey`) are sent as-is.

- Data `new { Height = 165 }` serializes to `{"height": 165}`
- `XAxis("Height").DataKey` serializes as the string `"Height"`
- Frontend: `d["Height"]` is `undefined` because the key is `"height"`

📝 **Pattern**: Always use case-insensitive lookups when mapping C#-serialized data to frontend chart properties. This affects any chart widget using DataKey for value lookup.

### Enum PascalCase vs Frontend Lowercase
C# enums serialize as PascalCase strings (e.g., `"Week"`). Frontend libraries often expect lowercase. Always normalize with `.toLowerCase()` when receiving C# enum values on the frontend.

📝 Any C# enum prop that the frontend expects as a number needs a string-to-number conversion, because Ivy's `JsonEnumConverter` always serializes enums as their string name.

### Enum Display Names — PascalCase Split
`EnumHelper.GetDescription()` (used by `typeof(MyEnum).ToOptions()`) calls `StringHelper.SplitPascalCase()`:
- `SciFi` → "Sci Fi", `ExtraLarge` → "Extra Large"

❌ In Playwright: `getByText('SciFi')` won't find the label
✅ Match the split name: `getByText('Sci Fi')`
📝 `enum.ToString()` still returns the raw member name (e.g., `SciFi`).

## Calendar Widget

### Enum PascalCase vs lowercase in Frontend
C# enum `CalendarDisplayMode.Week` serializes as `"Week"` (PascalCase), but the frontend CalendarView type uses lowercase `'week'`. Always normalize with `.toLowerCase()` when receiving C# enum values on the frontend.

### Widget Children Pattern (Calendar/Kanban)
To pass structured children to a widget on the frontend:
1. Add child type filter in `WidgetRenderer.tsx` (both memoized and external paths)
2. Use `widgetNodeChildren` prop in the widget component for metadata
3. Use `slots.default[index]` for rendered React content at matching index
4. Register both parent and child widgets in `widgetMap.ts`

### WidgetRenderer.tsx File Casing Issue
Git tracks this file as `widgetRenderer.tsx` (lowercase) but the file on disk is `WidgetRenderer.tsx` (PascalCase). Existing imports use lowercase (`@/widgets/widgetRenderer`). This pre-existing mismatch causes TS1261 errors when the file is modified. Use `npx vite build` directly to bypass the `tsc -b` check if needed.

### Slot Content vs Title in Child Widgets (Calendar/Kanban)
When a child widget (e.g. CalendarEvent) has no custom content children, `slots?.default?.[index]` still contains a rendered widget component (empty React Fragment). This is truthy, so any `event.content ? ... : event.title` check incorrectly takes the content branch and renders an empty div instead of the title.

- Check `widgetNode.children && widgetNode.children.length > 0` before using slot content
- `content: hasChildren ? (slots?.default?.[index] || null) : null`

This pattern applies to any widget that uses the children/slots pattern and has a fallback text display.

## FunnelChart DataKey CamelCase Mismatch

### FunnelChartWidget.tsx Hardcoded Property Names
**Problem**: `FunnelChartWidget.tsx` line 71 hardcoded `d.measure`/`d.dimension` for data mapping, which only works with `PieChartData` format. When `FunnelChartData` (with `Stage`/`Value` properties) is used via `ToFunnelChart()`, the serialized data has camelCase keys `stage`/`value` but the frontend looks for `measure`/`dimension`.

❌ **Original code (broken for FunnelChartData):**
```typescript
data.map(d => ({ value: d.measure, name: d.dimension as string }))
```

✅ **Fixed code:**
```typescript
// Derive keys from funnel config's dataKey/nameKey with camelCase conversion
const valKey = firstFunnel?.dataKey ? camelCase(firstFunnel.dataKey) : 'measure';
const nameKey = firstFunnel?.nameKey ? camelCase(firstFunnel.nameKey) : 'dimension';
data.map(d => ({ value: record[valKey] ?? d.measure, name: record[nameKey] ?? d.dimension }))
```

📝 **This is another instance of the DataKey camelCase mismatch pattern** documented in the Serialization section above. Any chart widget that hardcodes property names instead of using config-provided keys will break when data uses non-standard property names.

## RadarChart Explicit Radar Config CamelCase Bug (FIXED)

### Case-sensitive property lookup in RadarChartWidget.tsx
**Problem**: When using explicit `.Radar("values")` config, `RadarChartWidget.tsx` line 122 used `item[ind.name]` (case-sensitive) to look up indicator values. Since C# serializes properties to camelCase (`sales`, `marketing`) but indicator names are PascalCase (`Sales`, `Marketing`), all values resolved to 0 — rendering an empty radar polygon.

**Note**: The default path (no explicit Radar config, line 112) correctly used `getPropertyValue(item, ind.name)` (case-insensitive).

✅ **Fix applied**: Changed line 122 from `item[ind.name]` to `getPropertyValue(item, ind.name)`.

📝 **Another instance of the DataKey camelCase mismatch pattern.** Always use case-insensitive lookups when mapping C#-serialized data to frontend chart properties.

## Enum Display Names (PascalCase Split)

### Enum values are auto-split for display labels
Ivy's `EnumHelper.GetDescription()` (used by `typeof(MyEnum).ToOptions()`) calls `StringHelper.SplitPascalCase()` on enum member names.

- `SciFi` → "Sci Fi"
- `ExtraLarge` → "Extra Large"
- `OnlyChoice` → "Only Choice"

❌ **In Playwright tests, don't match enum member name directly**: `getByText('SciFi')` won't find the label
✅ **Match the split display name**: `getByText('Sci Fi')` or `locator('label').filter({ hasText: 'Sci Fi' })`

📝 **Note**: `enum.ToString()` still returns the raw member name (e.g., `SciFi`), so state feedback text like `$"Selected: {state.Value}"` will show `SciFi`, not `Sci Fi`.

### SelectInput State Binding
❌ **`new SelectInput<T>(options).Value(x).OnChange(handler)`** — `Value()` and `OnChange()` are not available as extension methods on `SelectInputBase`
✅ **Use state binding**: `state.ToSelectInput(options).Radio()` — state changes are automatic
✅ **For side effects on change**: Use `UseEffect(() => { ... }, state)` to react to state changes

## DayOfWeek Enum Serialization (FIXED)

### FirstDayOfWeek Prop — String vs Number Mismatch
**Problem**: C# `DayOfWeek` enum serializes as string ("Monday", "Sunday", etc.) via `JsonEnumConverter`, but the frontend `react-day-picker` `weekStartsOn` prop expects a number (0-6).

❌ **Before fix**: Setting `.FirstDayOfWeek(DayOfWeek.Monday)` crashed the calendar with `RangeError: Invalid time value`
✅ **After fix**: Added `resolveDayOfWeek()` in `DateTimeInputWidget.tsx` and `DateRangeInputWidget.tsx` to convert string enum names to numeric values

📝 **Pattern**: Any C# enum prop that the frontend expects as a number needs a string-to-number conversion on the frontend side, because Ivy's `JsonEnumConverter` always serializes enums as their string name (e.g., `"Monday"` not `1`).

## react-day-picker v9 Date Restriction API

### fromDate/toDate are v8 props — use disabled + startMonth/endMonth in v9
❌ **`<Calendar fromDate={minDate} toDate={maxDate} />`** — `fromDate`/`toDate` are react-day-picker v8 props, silently ignored in v9
✅ **Use `disabled` with matchers + `startMonth`/`endMonth` for navigation restriction:**
```tsx
const disabledMatcher: Matcher[] = [];
if (minDate) disabledMatcher.push({ before: minDate });
if (maxDate) disabledMatcher.push({ after: maxDate });

<Calendar
  disabled={disabledMatcher.length > 0 ? disabledMatcher : undefined}
  startMonth={minDate}
  endMonth={maxDate}
/>
```

📝 **Why**: react-day-picker v9 removed `fromDate`/`toDate` props. Date disabling uses the `disabled` prop with `DateBefore`/`DateAfter` matchers. Navigation restriction uses `startMonth`/`endMonth`.

## react-day-picker DOM Structure

### Calendar uses flex layout, NOT `<table>`
❌ **`page.locator("table thead th")`** — react-day-picker v9 does NOT use HTML tables
✅ **`page.locator(".rdp-weekdays .rdp-weekday")`** — use RDP CSS classes to find weekday headers
✅ **`page.locator(".rdp-day button")`** — use for clicking day buttons

## Badge TestId Not Rendered in DOM

### TestId on Badge does NOT produce data-testid attribute
❌ **`new Badge("text").TestId("my-id")`** — compiles but does NOT render `data-testid` in the DOM
✅ **Use `getByText()` for text content verification** — more reliable than TestId on badges
✅ **Buttons DO render data-testid** — `getByTestId()` works for buttons

📝 **Why**: Badge may not extend WidgetBase in a way that enables data-testid rendering in the frontend widget. Buttons use `<button>` elements that receive the attribute.

## Float Formatting Locale Issues

### C# float formatting uses system locale
❌ **`$"{volume.Value:F2}"`** — on European locales produces `"0,50"` instead of `"0.50"`
✅ **`volume.Value.ToString("F2", CultureInfo.InvariantCulture)`** — always produces dot separator
📝 **Why**: The Ivy server runs with the system's locale. On Windows with European regional settings, `float.ToString("F2")` uses comma as decimal separator. Always use `CultureInfo.InvariantCulture` when the formatted text needs to be matched in Playwright tests.

## DateTimeInput Popover Click Target

### DateInput is NOT a `<button>` — it's a Popover trigger div
❌ **`page.getByTestId('my-date').locator('button').first().click()`** — times out because the clickable area is not a `<button>` element
✅ **`page.getByTestId('my-date').click()`** — click the testid element directly to open the calendar popover

📝 **Why**: DateVariant, MonthVariant, YearVariant, and WeekVariant use Radix `<PopoverTrigger asChild>` wrapping a styled div, not a button. The `<button>` locator finds nothing. TimeVariant uses `<input type="time">` directly.

### Disabled DateInput has no `<button>` to check
❌ **`page.getByTestId('disabled-date').locator('button').first()` → `toBeDisabled()`** — element not found
✅ **Just verify visibility**: `await expect(page.getByTestId('disabled-date')).toBeVisible()` — the disabled state renders as reduced opacity/non-interactive div

## Navigation in chrome=false Mode

### NavigateSignal requires Chrome wrapper
❌ **`navigator.Navigate(beacon, entity)` in chrome=false mode** — does NOT redirect to the target app page
✅ **Navigation only works with Chrome wrapper** — the `NavigateSignal` is `[Signal(BroadcastType.Chrome)]`, meaning it's consumed by the Chrome sidebar component
📝 **Why**: In `chrome=false` mode, there is no Chrome component to receive and act on the navigation signal. The signal fires but nothing handles it.

**Testing implications:**
- ❌ Don't test actual page navigation (URL change, new page content) in `chrome=false` mode
- ✅ Test state feedback before navigation (click counters, action logs)
- ✅ Test beacon discovery and availability (UseNavigationBeacon returns non-null)
- ✅ Test target apps by navigating directly via URL: `page.goto(\`http://localhost:\${port}/app-id?chrome=false\`)`
- ✅ Test button enabled/disabled state based on beacon availability

### Beacon AppId Must Match Full Registered ID
❌ **`AppId: "customer-details"`** — won't match if the app's registered ID includes a namespace prefix
✅ **`AppId: "my-namespace/customer-details"`** — use `dotnet run -- --describe` to find the exact registered app ID
📝 **Why**: Ivy auto-generates app IDs from the namespace + class name in kebab-case. The beacon's AppId must match exactly.

## DataTable Canvas Locator & Click Target

### DataTable uses Glide Data Grid canvas, NOT HTML table
❌ **`page.locator('[data-testid="gdg-canvas"]')`** — this testid does NOT exist
✅ **`page.locator('canvas').first()`** or **`page.locator('[data-testid="data-grid-canvas"]')`** — correct locators for the DataTable grid
📝 **Note**: The actual `data-testid` on the canvas element is `"data-grid-canvas"`, not `"gdg-canvas"`

### Canvas click intercepted by scroller overlay
❌ **`page.locator('canvas').first().click()`** — blocked by `<div class="dvn-scroller">` which intercepts pointer events
✅ **`page.locator('.dvn-scroller').first().click({ position: { x, y } })`** — click the overlay div instead
📝 **Why**: Glide Data Grid uses a `dvn-scroller` overlay div for scroll handling that intercepts all pointer events above the canvas.

## WidgetSerializer Strips Default Enum Values (FIXED)

### Number columns have `type` omitted from JSON
**Problem**: `WidgetSerializer.AddDefaultValueComparison` compares each property against a default instance. `DataTableColumn.ColType` defaults to `ColType.Number` (enum value 0), so Number columns have `type` stripped from the serialized JSON.

**Impact**: Frontend receives `undefined` for `column.type` on all Number-type columns. Any code doing `column.type.toLowerCase()` crashes.

**Fix applied**: Added null guards in `calculateAutoWidth.ts` (`(column.type ?? 'text')`) and `columnHelpers.ts` (`(col.type ?? 'text')`).

📝 **Pattern**: Any `DataTableColumn` property whose value matches the parameterless constructor default will be stripped from JSON. Watch for similar issues with `Align` (defaults to `Left`), `Sortable` (defaults to `true`), etc.

## DataTable Custom Header Icons — Three Bugs (FIXED)

### Bug 1: mapColumnIcon() discards custom icon names
❌ **`mapColumnIcon()` default case returned `GridColumnIcon.HeaderString`** — custom icon names like "CustomTag" were replaced with a built-in enum value
✅ **Fix**: Changed default case to `return col.icon` to preserve custom names for SpriteMap lookup
📝 **File**: `src/frontend/src/widgets/dataTables/utils/columnHelpers.ts`

### Bug 2: showColumnTypeIcons gate blocks explicit icons
❌ **`icon: showColumnTypeIcons ? mapColumnIcon(col) : undefined`** — columns with explicit `.Icon()` get no icon when `showColumnTypeIcons=false`
✅ **Fix**: Always show icon when `col.icon` is set; only use `showColumnTypeIcons` toggle for auto-detected type icons
📝 **File**: `src/frontend/src/widgets/dataTables/utils/columnHelpers.ts`

### Bug 3: CamelCase mismatch between dictionary keys and Icon values
❌ **`config.CustomHeaderIcons["CustomTag"]` → serialized key `"customTag"` but `column.Icon` stays `"CustomTag"`** — SpriteMap key doesn't match column icon name
✅ **Fix**: In `generateHeaderIcons()`, store custom icons under both camelCased key and PascalCase variant
📝 **File**: `src/frontend/src/widgets/dataTables/utils/headerIcons.ts`
📝 **Root cause**: Ivy's `WidgetSerializer` uses `DictionaryKeyPolicy = JsonNamingPolicy.CamelCase` which camelCases dictionary keys, but string VALUES (like column.Icon) are sent as-is. This is the same class of bug as the DataKey camelCase mismatch documented above.

## VideoPlayer Widget — Id/TestId Not Rendered as HTML Attributes

### `.TestId()` and `.Id()` don't produce predictable HTML IDs
❌ **`new VideoPlayer(url).TestId("my-video")`** — `data-testid` attribute is NOT rendered in the DOM
❌ **`new VideoPlayer(url).Id("my-video")`** — the `id` HTML attribute is set to an Ivy-generated short hash (e.g., `fueuz635nb`), NOT the value passed to `.Id()`
✅ **Use positional locators**: `page.locator('video').nth(0)` for HTML5 videos, `page.locator('iframe').nth(0)` for YouTube embeds
✅ **Use text-based navigation**: Find surrounding headings with `getByText()` then locate the nearby `video` element
📝 **Why**: Ivy's widget system wraps components in `<ivy-widget>` custom elements and generates its own short-hash IDs. The `.Id()` extension sets the widget-level ID which gets transformed by the framework before rendering. VideoPlayerWidget.tsx receives `id` from props but it's the framework-generated ID.

## Video PlaybackRate — Browser Resets During Media Load (FIXED)

### useEffect alone is insufficient for setting playbackRate
❌ **Setting only `videoElement.playbackRate` in useEffect** — the browser's media load algorithm resets `playbackRate` to `defaultPlaybackRate` (1.0) during source loading, overwriting the useEffect
✅ **Set both `defaultPlaybackRate` AND `playbackRate`** — `defaultPlaybackRate` persists across media loads
✅ **Also re-apply in `onLoadedData` handler** — safety net for race conditions
📝 **Why**: The HTML spec's media load algorithm (triggered when `src` is set) includes: "Set playbackRate to defaultPlaybackRate". Since `defaultPlaybackRate` defaults to 1.0, any `playbackRate` set before load completes gets overwritten. This pattern applies to ANY video/audio property set via useEffect that the browser resets during load.

## SelectInput Multi-Select Uses CMDK, Not Radix Select

### Multi-select Select variant has different DOM structure
❌ **`page.locator('button[role="combobox"]')`** — multi-select Select variant does NOT use Radix Select's native combobox button
✅ **`page.getByPlaceholder('placeholder text')`** — click the input area by placeholder to open the dropdown
✅ **`page.locator('[cmdk-item]').filter({ hasText: 'Option' })`** — select options using CMDK item attribute
📝 **Why**: Multi-select Select variant uses CMDK (Command Menu) with a Popover, not the Radix Select primitive. The trigger is a div with an input, not a `<button role="combobox">`. Single-select DOES use `button[role="combobox"]`.

**Key differences:**
- Single-select: `button[role="combobox"]` trigger, `[role="option"]` items
- Multi-select: `getByPlaceholder()` trigger, `[cmdk-item]` items
- Multi-select popover stays open after selecting (allowing multiple picks), close with `Escape`

## SignatureInput OnChange Not Wired (FIXED)

### State-bound constructor must wire OnChange
❌ **`OnChange => null`** — property-body returning null means OnChange is never set, so `InvokeEventAsync` returns false
✅ **`OnChange { get; }`** with constructor wiring: `OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });`
📝 **Why**: Unlike auto-properties `{ get; }` which have backing fields, expression-body `=> null` is a computed getter. The `InvokeEventAsync` reflection finds the property but `GetValue()` returns null, so the event is silently ignored. All state-bound input constructors MUST set OnChange.
📝 **Pattern check**: `FileInput` has the same `=> null` — but FileInput uses upload handlers instead of OnChange, so it's OK there.

### Base64 data URL vs raw base64 for byte[] serialization
❌ **`eventHandler('OnChange', id, [canvas.toDataURL('image/png')])`** — sends `data:image/png;base64,...` prefix which breaks C# `System.Text.Json` byte[] deserialization
✅ **Strip prefix**: `const base64 = dataUrl.split(',')[1]; eventHandler('OnChange', id, [base64]);`
❌ **`img.src = value`** when value is raw base64 from C# — img.src needs data URL prefix
✅ **Add prefix**: `img.src = value.startsWith('data:') ? value : \`data:image/png;base64,\${value}\``
📝 **Why**: C# `System.Text.Json` serializes byte[] as raw base64 strings (no prefix). Frontend `canvas.toDataURL()` returns a data URL with prefix. These two formats are incompatible and must be converted at the boundary.

## SVG xmlns Required for Data URI / SpriteMap Usage

### SVG strings used as image sources MUST include xmlns
❌ **`<svg width="24" height="24" viewBox="0 0 24 24" ...>`** — missing xmlns causes "source image cannot be decoded" when used as data URI in `<img>` or SpriteMap
✅ **`<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" ...>`** — always include xmlns
📝 **Why**: When SVG is inline in HTML, the namespace is inherited from the document context. But when SVG is used as an image source (data URI, Blob URL, or glide-data-grid SpriteMap), the browser parses it as a standalone document. Without `xmlns`, the parser doesn't know it's SVG and the image fails silently.

## WidgetSerializer Strips Default Enum Values — Ongoing Pattern

### column.type null guard needed in ALL DataTable utility files
The WidgetSerializer strips properties that match the parameterless constructor default. `DataTableColumn.ColType` defaults to `ColType.Number` (enum value 0), so Number columns have `type: undefined` in the frontend.

**Files that need `(col.type ?? 'text')` null guard:**
- `columnHelpers.ts` — `mapColumnIcon()` ⚠️ (guard still missing as of Apr 2026)
- `calculateAutoWidth.ts` — `calculateAutoWidth()`
- `cellContent.ts` — already guarded
- `DataTableFilterOption.tsx` — already guarded

📝 **Warning**: When rewriting any of these files, always preserve null guards. Any new widget with enum props where value 0 is the "active" mode will hit this bug.

### Dictionary Key vs String Value CamelCase Mismatch
Ivy's `WidgetSerializer` uses `DictionaryKeyPolicy = JsonNamingPolicy.CamelCase` which camelCases dictionary keys, but string VALUES (like column.Icon) are sent as-is. When a dictionary key is used to look up a string value, the casing won't match.

## Widget Extension Method CS1660 Pattern

### OnSave/OnCancel/OnLineClick — Property Shadows Extension Method
When a widget record has an `[Event]` property (e.g., `OnSave`) that is a `Func<Event<T,V>, ValueTask>?`, calling `.OnSave(lambda)` resolves to **delegate invocation** (the property), not the extension method.

❌ **`.OnSave(() => {...})`** — CS1660 on ScreenshotFeedback, DiffView, and similar widgets
✅ **Use `with` expression:**
```csharp
new DiffView().Diff(diff) with
{
    OnLineClick = e => { state.Set(e.Value); return ValueTask.CompletedTask; }
}
```

📝 **Affected widgets**: ScreenshotFeedback (OnSave, OnCancel), DiffView (OnLineClick), and any widget with `Func<Event<T,V>, ValueTask>?` properties. The zero-arg overload works because the delegate requires one parameter and doesn't match.

## TestId / DOM Structure Issues

### TestId Only Works on WidgetBase Types
✅ **Works on**: `WidgetBase` types (inputs, buttons, cards)
❌ **Doesn't work on**: `TextBuilder`, `LayoutView`, and other non-widget types — `TextBuilder` does NOT extend `WidgetBase<TextBuilder>`, so `.TestId()` causes CS0311
✅ **Workaround**: Use `getByText()` for text content verification

### Widgets Where TestId Does NOT Render in DOM

| Widget | Workaround |
|--------|------------|
| Badge | `getByText()` for text content |
| ColorInput | `page.locator("input[type='color']")` |
| VideoPlayer | `page.locator('video').nth(0)` or `page.locator('iframe').nth(0)` |

📝 VideoPlayer's `.Id()` sets an Ivy-generated short hash, not the value you pass.

### NumberInput TestId Is on the `<input>`, Not a Wrapper
❌ **`page.getByTestId('my-number').locator('input')`** — testid IS the input
✅ **`page.getByTestId('my-number')`** — directly references the `<input>`
📝 The clear (X) button is NOT inside the testid element — walk up the DOM to find it.

### `ivy-widget` Custom Elements Have Height 0
❌ **`el.closest("ivy-widget")?.getBoundingClientRect().height`** — always 0
✅ **Measure content div inside**: `iw.querySelector(":scope > div")?.children[0]?.getBoundingClientRect()`

### Badge Renders as `<div>`, Not `<span>`
❌ **`document.querySelectorAll("span")` for badges** — Badge renders `<div>`
📝 CSS `inline-flex` gets blockified to `flex` inside flex containers. Check `className` instead of computed style.

## DataTable Gotchas

### Canvas-Based Grid — NOT HTML Table
❌ **`page.locator('[data-testid="gdg-canvas"]')`** — does not exist
✅ **`page.locator('canvas').first()`** or **`page.locator('[data-testid="data-grid-canvas"]')`**

### Canvas Click Intercepted by Scroller Overlay
❌ **`page.locator('canvas').first().click()`** — blocked by `dvn-scroller`
✅ **`page.locator('.dvn-scroller').first().click({ position: { x, y } })`**

### Dual Data Path (Cell Rendering vs Data Loading)
- `convertArrowTableToData()` — used for column metadata, row counts (NOT cell display)
- `useRowData()` — reads raw Arrow table for cell rendering

📝 **When fixing data conversion bugs**, the fix must be in `useRowData.ts`, not `convertArrowTableToData()`.

### Filter Panel Overlaps HeaderLeft Slot
❌ **Buttons in `.HeaderLeft()` unclickable when `AllowFiltering=true`** (default)
✅ **Workaround**: `.Config(config => config.AllowFiltering = false)` if filtering not needed

### TableBuilder `.Builder()` — No `.Custom()` Method
❌ **`f.Custom(val => widget)`** — does not exist
✅ **`f.Func((string val) => (object)widget)`**
📝 Available: `Default()`, `Text()`, `Link()`, `CopyToClipboard()`, `Func<TIn>()`, `Progress()`

## Component API Quick Reference

| Widget/Type | Missing API | Use Instead |
|-------------|-------------|-------------|
| Card | `.Default()` | `new Card(content)` |
| Size | `.Vh()`, `.Pixels()` | `.Px()` for pixels, `.Full()` for 100% |
| IState | `.Pipe()` | Inline expressions or local variable |
| AppBase | — | Use `ViewBase` for Ivy views |
| ButtonVariant | `.Default` | Use `Primary`, `Secondary`, `Ghost`, etc. |
| SelectInput | `.Value()`, `.OnChange()` | `state.ToSelectInput(options)` with UseEffect for side effects |
| MemoryStreamUploadHandler | `.Create()` zero-arg | `.Create(state)` with `IState<FileUpload<byte[]>?>` |
| Layout.Gap() | `Size` param | `bool` or `int` param |
| RefreshToken | `.Trigger()` | `.Refresh()` or `.Refresh(returnValue)` |
| CodeInput | Direct instantiation | `.ToCodeInput()` on `IState<string>`, or use `Markdown` with fenced code block |

### App Attribute: `group` Not `path`
❌ **`[App(path: new[] { "Tests" })]`** — CS1739
✅ **`[App(group: new[] { "Tests" })]`**

### Icons Enum
- ❌ `Icons.AlignCenter` → ✅ `Icons.AlignCenterHorizontal`
- 📝 Lucide renamed some icons (e.g., `AlertTriangle` → `TriangleAlert`)
- 📝 Chart icons: Use `Icons.ChartBarStacked` (not `Icons.BarChart`, `Icons.BarChart3`)

## Frontend Build Issues

### Stale Frontend Assets
When testing commits that change TypeScript files, the server may serve old bundled JS because assets are embedded into `Ivy.dll`.
✅ Always rebuild frontend before testing: `cd src/frontend && npm run build`

### WidgetRenderer.tsx File Casing Issue
Git tracks as `widgetRenderer.tsx` (lowercase) but disk is `WidgetRenderer.tsx`. Existing imports use lowercase. This causes TS1261 errors on modification. Use `npx vite build` to bypass `tsc -b` check if needed.

## Project Setup Issues

### Missing `using Ivy;` Directive
Compilation errors for `ViewBase`, `Icons`, etc. → add `using Ivy;`

### App Registration Required
Ivy does NOT auto-discover apps. Required in Program.cs:
```csharp
server.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
```

### Nullable Enable Required
CS8632 warnings → add `<Nullable>enable</Nullable>` to `.csproj`

### Ivy.Analyser Reference
```xml
<ProjectReference Include="...\Ivy.Analyser.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

## Chrome Behavior

### Direct URL Routing Not Available With Chrome
`UseChrome()` active → `/app-id` shows "App Not Found". Apps must be accessed via sidebar clicks. `?chrome=false` also shows "App Not Found".

### `?chrome=false` Hides the Sidebar
Don't use `?chrome=false` when testing sidebar labels or navigation items.

### NavigateSignal Requires Chrome Wrapper
❌ `navigator.Navigate(beacon, entity)` in chrome=false mode — does NOT redirect
📝 `NavigateSignal` is `[Signal(BroadcastType.Chrome)]` — consumed by Chrome sidebar component only.

**Testing navigation:**
- ✅ Test state feedback before navigation (click counters, action logs)
- ✅ Test beacon discovery (`UseNavigationBeacon` returns non-null)
- ✅ Test target apps by navigating directly: `page.goto(\`http://localhost:\${port}/app-id?chrome=false\`)`

### Beacon AppId Must Match Full Registered ID
❌ `AppId: "customer-details"` → ✅ `AppId: "my-namespace/customer-details"`
📝 Use `dotnet run -- --describe` to find exact registered app ID.

## Playwright-Specific Gotchas

### DateTimeInput Click Target
❌ **`page.getByTestId('my-date').locator('button').first().click()`** — DateInput is a Popover trigger div, not a button
✅ **`page.getByTestId('my-date').click()`**
📝 TimeVariant uses `<input type="time">` directly.

### Clicking Buttons Behind Modal Overlays
❌ **`page.getByTestId('btn').click()`** — overlay intercepts pointer events
❌ **`click({ force: true })`** — Ivy's event handler doesn't process it
✅ **Use `page.evaluate` with `dispatchEvent`:**
```typescript
await page.evaluate((id) => {
  const btn = document.querySelector(`[data-testid="${id}"]`) as HTMLElement;
  if (btn) btn.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
}, 'my-button-testid');
```

### SelectInput Multi-Select Uses CMDK, Not Radix Select
| Variant | Trigger | Items |
|---------|---------|-------|
| Single-select | `button[role="combobox"]` | `[role="option"]` |
| Multi-select | `getByPlaceholder()` | `[cmdk-item]` |

Multi-select popover stays open after selecting. Close with `Escape`.

### Float Formatting Locale Issues
❌ **`$"{volume.Value:F2}"`** — European locales produce `"0,50"`
✅ **`volume.Value.ToString("F2", CultureInfo.InvariantCulture)`**

### react-day-picker v9 Changes
- ❌ `fromDate`/`toDate` — v8 props, silently ignored in v9
- ✅ Use `disabled` with `DateBefore`/`DateAfter` matchers + `startMonth`/`endMonth`
- ❌ `page.locator("table thead th")` — v9 uses flex layout, NOT `<table>`
- ✅ `page.locator(".rdp-weekdays .rdp-weekday")` for weekday headers

### react-markdown v10 — No `inline` Prop on Code Component
❌ **`if (inline)`** — always `undefined` in v10
✅ **`!className`** to detect inline code (no language = inline)
✅ **`!el.closest('pre')`** — inline code has no `<pre>` wrapper

### Image Widget MinContent Default — Hidden in Playwright
❌ **`new Image(url)`** with default sizing — 0x0 until image loads
✅ **Set explicit dimensions**: `Width = Size.Px(200), Height = Size.Px(150)`

## Miscellaneous

### Namespace Conflicts with External Widget Types
❌ **`namespace ScreenshotFeedback`** when using `Ivy.Widgets.ScreenshotFeedback.ScreenshotFeedback`
✅ Use different namespace (e.g., `namespace ScreenshotFeedbackTest`)

### Ivy.Widgets.Xterm.Terminal Ambiguity with Ivy.Terminal
❌ `using Ivy; using Ivy.Widgets.Xterm;` — `Terminal` is ambiguous
✅ Use fully qualified extension methods: `Ivy.Widgets.Xterm.TerminalExtensions.OnInput(terminal, data => ...)`
✅ For non-event fluent methods: `using XTerminal = Ivy.Widgets.Xterm.Terminal;` works

### SVG xmlns Required for Data URI / SpriteMap Usage
❌ `<svg width="24" ...>` — missing xmlns causes silent decode failure
✅ `<svg xmlns="http://www.w3.org/2000/svg" ...>` — always include xmlns

### PathToAppIdMiddleware Intercepts Custom File Extensions
If you add middleware serving files with a custom extension (e.g., `.md`), add that extension to `staticFileExtensions` in `src/frontend/src/routing-constants.json`.

### Widget Children Pattern (Calendar/Kanban)
When passing structured children to widgets:
1. Add child type filter in `WidgetRenderer.tsx` (both paths)
2. Use `widgetNodeChildren` for metadata, `slots.default[index]` for rendered content
3. Register both parent and child in `widgetMap.ts`
4. Check `widgetNode.children && widgetNode.children.length > 0` before using slot content (empty fragments are truthy)

### Ivy.csproj Rust Binary Build — MSBuild Glob Issue
❌ `dotnet build` fails with MSB3024 for `*rustserver*.dll` glob
✅ Create CI/CD artifacts path: `mkdir -p src/RustServer/artifacts/native/win-x64 && cp src/RustServer/target/release/rustserver.dll src/RustServer/artifacts/native/win-x64/`

## Historical Issues (Fixed)

These bugs have been fixed. Kept for reference in case of regressions.

### RadarChart CamelCase Lookup (FIXED)
`RadarChartWidget.tsx` line 122 used case-sensitive `item[ind.name]` for explicit `.Radar()` config. Fixed to use `getPropertyValue(item, ind.name)` (case-insensitive).

### DayOfWeek Enum Serialization (FIXED)
C# `DayOfWeek` serialized as string but `react-day-picker` `weekStartsOn` expects a number. Fixed with `resolveDayOfWeek()` in `DateTimeInputWidget.tsx` and `DateRangeInputWidget.tsx`.

### Video PlaybackRate Browser Reset (FIXED)
Setting only `videoElement.playbackRate` in useEffect was reset during media load. Fixed by setting both `defaultPlaybackRate` and `playbackRate`, plus re-applying in `onLoadedData` handler.

### SignatureInput OnChange Not Wired (FIXED)
`OnChange => null` (expression-body) meant OnChange was never set. Fixed to `OnChange { get; }` with constructor wiring. Also: frontend must strip `data:` prefix from `canvas.toDataURL()` before sending to C# byte[] deserialization.

### DataTable Custom Header Icons — Three Bugs (FIXED)
1. `mapColumnIcon()` discarded custom icon names (returned `GridColumnIcon.HeaderString`)
2. `showColumnTypeIcons` gate blocked explicit icons
3. CamelCase mismatch between dictionary keys and Icon values

### FileDialog Upload Mode Prop Stripped (FIXED)
`FileDialogMode.Upload` (enum value 0) was stripped by WidgetSerializer. Fixed with `mode = 'Upload'` default in `FileDialogWidget.tsx`.

### WidgetSerializer Default Enum Value Stripping (FIXED)
Number columns had `type: undefined` because `ColType.Number` (enum 0) was stripped. Fixed with null guards in `calculateAutoWidth.ts` and `cellContent.ts`. See "Ongoing Pattern" in Serialization section for remaining guard needs.

## Future Gotchas

As we encounter more issues, add them with:
- ❌ **What doesn't work**
- ✅ **What does work** (solution)
- 📝 **Why** (explanation when helpful)
