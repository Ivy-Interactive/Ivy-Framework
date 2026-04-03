# Ivy Framework Gotchas

Common mistakes and issues encountered when working with the Ivy Framework during feature testing and development.

## Component API Issues

### Card Component
- ❌ **No `.Default()` method**: `Card.Default()` does NOT exist
- ✅ **Use**: `new Card(content)` or `Layout.Vertical()` containers

### TestId Support
- ✅ **Works on**: `WidgetBase` types (inputs, buttons, cards)
- ❌ **Doesn't work on**: `TextBuilder`, `LayoutView`, and other non-widget types — `TextBuilder` does NOT extend `WidgetBase<TextBuilder>`, so `.TestId()` causes CS0311 compile error
- ✅ **Workaround**: Use `getByText()` for text content verification instead of TestId
- 📝 **Testing tip**: `getByText()` is often more robust than `getByTestId()` for verifying visible text content

### Icons Enum
- ❌ **Not all Lucide icons available**: Some expected names don't exist (e.g., `Icons.AlignCenter`)
- ✅ **Use correct names**: `Icons.AlignCenterHorizontal` instead
- 📝 **Note**: Lucide renamed some icons (e.g., `AlertTriangle` → `TriangleAlert`)
- 📝 **Chart icons**: Use `Icons.ChartBarStacked` for bar charts (not `Icons.BarChart`, `Icons.BarChart3`, or `Icons.BarChartBig`)

## Hook Rules (Critical)

### Hook Ordering - IVYHOOK005
**All Ivy hooks MUST be called at the very top of `Build()` method, before ANY other statements.**

❌ **WRONG - Causes IVYHOOK005 warning:**
```csharp
public override object? Build() {
    var state1 = UseState<int>(() => 0);
    var value = state1.Value * 2;        // ❌ Logic between hooks
    var state2 = UseState<string>("");   // ❌ Hook after logic
    return Layout.Vertical() | ...;
}
```

✅ **CORRECT - All hooks first:**
```csharp
public override object? Build() {
    // All hooks at the top
    var state1 = UseState<int>(() => 0);
    var state2 = UseState<string>(() => "");
    var service = UseService<IClientProvider>();

    // Then logic
    var value = state1.Value * 2;
    return Layout.Vertical() | ...;
}
```

**Why this matters**: Ivy's hook system relies on consistent call order across renders. Conditional or out-of-order hooks break state tracking and cause runtime errors.

## Frontend Build Issues

### Stale Frontend Assets
**Problem**: When testing commits that change TypeScript files in `src/frontend/`, the server may serve old bundled JS.

**Why**: Frontend assets are built into `src/frontend/dist/` and embedded into `Ivy.dll` via `<EmbeddedResource>`. Running `dotnet run` uses the already-compiled DLL with old assets.

✅ **Solution**: Always rebuild frontend before testing:
```bash
cd /d/Repos/_Ivy/Ivy-Framework/src/frontend
npm run build
```

Then run your test project.

## Project Setup Issues

### Missing using Directive
**Problem**: Compilation errors for `ViewBase`, `Icons`, etc. in test apps.

❌ **WRONG:**
```csharp
namespace MyTest;

[App(icon: Icons.Settings)]
public class MyApp : ViewBase { }  // ❌ ViewBase not found
```

✅ **CORRECT:**
```csharp
using Ivy;  // ✅ Add this

namespace MyTest;

[App(icon: Icons.Settings)]
public class MyApp : ViewBase { }
```

### App Registration
**Problem**: Apps don't appear when running the server.

**Why**: Ivy does NOT auto-discover apps. They must be explicitly registered.

✅ **Required in Program.cs:**
```csharp
var server = new Server();
server.AddAppsFromAssembly(Assembly.GetExecutingAssembly());  // ✅ Required
await server.RunAsync();
```

### App Attribute Parameters
- ❌ **No `path:` parameter**: `[App(icon: Icons.Table, path: ["Apps"])]` → CS1739
- ✅ **Use `group:`**: `[App(icon: Icons.Table, group: ["Widgets"])]`

### Size API
- ❌ **No `Size.Vh()`**: `Size.Vh(80)` does NOT exist → CS0117
- ✅ **Use `Size.Px()`** for fixed heights or `Size.Full()` for 100%

### Nullable Enable Required
**Problem**: CS8632 warnings about nullable reference types.

✅ **Solution**: Add to `.csproj`:
```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

## Common Compilation Errors

### Button Variant
❌ **No `ButtonVariant.Default`** - it doesn't exist
✅ **Use**: `Primary`, `Destructive`, `Outline`, `Secondary`, `Success`, `Warning`, `Info`, `Ghost`, `Link`

### State Pipe Method
❌ **`state.Pipe()` doesn't exist** on `IState<T>`
✅ **Workaround**: Use inline expressions or extract to local variable before passing to layout

### App Attribute: `group` not `path`
- AGENTS.md documentation says `path:` but the actual parameter is `group:`
- `[App(path: new[] { "Tests" })]` causes CS1739
- `[App(group: new[] { "Tests" })]` is correct

### AppBase vs ViewBase
❌ **Don't use `AppBase`** for test apps
✅ **Use `ViewBase`** - it's the correct base class for Ivy views

### BarChartBuilder: No .Tooltip()
❌ **`.Tooltip()` on `BarChartBuilder`** — causes CS1929; `Tooltip` is an `AreaChart` extension
✅ **Use `polish:`** to set tooltip: `polish: chart => chart with { Tooltip = new ChartTooltip().Animated(true) }`
📝 **Note**: `.Toolbox()` IS available on `BarChartBuilder`

### BarChart: Dual-axis via `polish`
❌ **No `bars:` or `YAxis:` parameters on `ToBarChart()`** — these are not constructor args
✅ **Use `polish:`**: `data.ToBarChart(polish: chart => chart with { Bars = [...], YAxis = [...] })`

### Badge.Color() Doesn't Exist
❌ **`new Badge("text").Color(Colors.X)`** — `Color()` extension is for `CalendarEvent`, not `Badge`
✅ **Use**: `new Badge("text")` with variant methods if available, or no color modifier

### MemoryStreamUploadHandler.Create() Requires State
❌ **`MemoryStreamUploadHandler.Create()`** — no zero-arg overload
✅ **Use**: `MemoryStreamUploadHandler.Create(state)` where `state` is `IState<FileUpload<byte[]>?>`

### Ivy.Widgets.Xterm.Terminal Ambiguity with Ivy.Terminal
❌ **`using Ivy; using Ivy.Widgets.Xterm;`** — `Terminal` is ambiguous between `Ivy.Terminal` and `Ivy.Widgets.Xterm.Terminal`
❌ **Type alias `using XTerminal = Ivy.Widgets.Xterm.Terminal;`** — does NOT fix extension method overload resolution (e.g., `.OnInput(data => ...)` still fails with CS1660)
✅ **Use fully qualified extension methods**: `Ivy.Widgets.Xterm.TerminalExtensions.OnInput(terminal, data => ...)` for event handlers
✅ **For non-event fluent methods**: `using XTerminal = Ivy.Widgets.Xterm.Terminal;` with `new XTerminal()` works for `.Background()`, `.Foreground()`, `.InitialContent()`, `.Height()` etc.

### Namespace Conflicts with External Widget Types
❌ **Using namespace matching widget type name** (e.g., `namespace ScreenshotFeedback` when using `Ivy.Widgets.ScreenshotFeedback.ScreenshotFeedback`)
✅ **Use different namespace**: e.g., `namespace ScreenshotFeedbackTest` to avoid `CS0118: 'X' is a namespace but is used like a type`

### ScreenshotFeedback Extension Methods Don't Work (CS1660)
❌ **`.OnSave(() => {...})` and `.OnCancel(() => {...})`** — causes CS1660: "Cannot convert lambda expression to type 'Event<ScreenshotFeedback, AnnotationData>'"
❌ **This also affects the official sample app** in `.samples/Program.cs`
✅ **Use `with` expression instead**:
```csharp
new ScreenshotFeedback() with
{
    IsOpen = isOpen.Value,
    OnSave = e => { /* handle */ return ValueTask.CompletedTask; },
    OnCancel = e => { /* handle */ return ValueTask.CompletedTask; }
}
```
📝 **Why**: The `OnSave`/`OnCancel` properties are `Func<Event<SF, AD>, ValueTask>?`. C# resolves property getter + delegate invocation before extension methods. So `.OnSave(lambda)` is interpreted as "get the OnSave delegate and invoke it with lambda as argument" rather than "call the OnSave extension method". The Button widget avoids this by using `EventHandler<>` wrapper type with `new()` in extensions.

## Ivy.Analyser Integration

### Local Analyser Reference
When adding Ivy.Analyser to test projects:

✅ **Correct ProjectReference:**
```xml
<ProjectReference Include="D:\Repos\_Ivy\Ivy-Framework\src\Ivy.Analyser\Ivy.Analyser.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

**Critical attributes:**
- `OutputItemType="Analyzer"` - Tells MSBuild this is a Roslyn analyzer
- `ReferenceOutputAssembly="false"` - Analyzer runs at compile-time only

### Analyser Catches Issues
The analyser enforces Ivy-specific rules at compile time:
- **IVYHOOK005**: Hook ordering violations (hooks must be called first)
- More rules added over time

**Pro tip**: Run `dotnet clean && dotnet build` to see analyser warnings after adding it.

## Lessons Learned

### 1. Always Check Source Code
Don't assume API methods exist based on patterns from other components. Read the actual source code in `D:\Repos\_Ivy\Ivy-Framework\src\Ivy\` before using any API.

### 2. Frontend Changes Require Rebuild
Testing commits with `.ts` file changes? Frontend rebuild is NOT optional.

### 3. TestId Strategically
Since TestId only works on widgets, plan your test structure around actual widget boundaries, not layouts.

### 4. Use Ivy.Analyser Early
Add the analyser to test projects immediately. It catches issues at compile-time that would otherwise cause runtime errors or subtle bugs.

### 5. Hook Rules Are Strict
The hook ordering rule (all hooks first) is non-negotiable. The analyser will catch violations, but understanding why helps write better code from the start.

## Chrome (UseChrome) Behavior

### Direct URL Routing Not Available With Chrome
- When `UseChrome()` is active, navigating to `/app-id` directly shows "App Not Found"
- Chrome handles routing internally — apps must be accessed via sidebar navigation clicks
- This affects Playwright tests: navigate to `/` first, then click sidebar items to open apps
- `?chrome=false` on direct URLs also shows "App Not Found" for the same reason

### TryUseService Requires Context Prefix
- `TryUseService<T>()` is defined on `ViewContext`, NOT on `ViewBase`
- Calling bare `TryUseService<T>(out var x)` from a `ViewBase` subclass causes CS0103
- Correct: `Context.TryUseService<T>(out var x)`
- Note: `UseService<T>()` IS available directly on `ViewBase` (without `Context.`)

## Serialization / CamelCase Issues

### DataKey String Values Are NOT CamelCased
**Problem**: Ivy's `WidgetSerializer` uses `JsonNamingPolicy.CamelCase` for property names and dictionary keys. But string VALUES (like `DataKey`) are sent as-is.

This means data property names get camelCased during serialization, but the DataKey used to look them up on the frontend stays PascalCase.

- Data `new { Height = 165 }` serializes to `{"height": 165}`
- `XAxis("Height").DataKey` serializes as the string `"Height"`
- Frontend: `d["Height"]` is `undefined` because the key is `"height"`

**Fix applied in ScatterChartWidget.tsx**: Added `resolveValue()` helper that tries both original key and camelCase fallback.

**Note**: This pattern may affect other chart widgets or any widget that uses DataKey to look up values in serialized data objects. Watch for similar issues when testing new chart types.

### Reference Markers (ReferenceDot/Line/Area) — Format Mismatch
**Problem**: C# `ReferenceLine`, `ReferenceArea`, `ReferenceDot` are simple records (`{ x, y, label }`), but the frontend code (both `ScatterChartWidget.tsx` and `sharedUtils.ts`) assumed they arrived as ECharts-native `MarkLine`/`MarkArea` objects (with `data` arrays). Calling `.flatMap(ml => ml.data)` on a C# record fails because there's no `data` property.

**Fix applied in ScatterChartWidget.tsx**: Transform C# records to ECharts format:
- `ReferenceLine { x, y }` → `markLine.data` entries using `xAxis`/`yAxis` keys
- `ReferenceArea { x1, y1, x2, y2 }` → `markArea.data` pairs
- `ReferenceDot { x, y }` → `markPoint.data` with `coord: [x, y]`

**Note**: The SAME bug exists in `sharedUtils.ts` `generateSeries()`, which is used by BarChart, LineChart, and AreaChart. Those widgets' reference markers are also broken and need the same transformation fix.

## RadialBarChart

### RadialBar.Name() doesn't exist
`.Name()` is an `AxisBase<T>` extension, not available on `RadialBar`. Use the constructor's second parameter instead.

- `new RadialBar("dataKey", "Display Name")` - correct
- `new RadialBar("dataKey").Name("Display Name")` - compile error (CS0311)

### RadialBarChart TypeScript circular reference
The `useMemo` callback must NOT reference `option.polar` while building `option`. Build polar config in a separate variable first.

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
- `columnHelpers.ts` — `mapColumnIcon()`
- `calculateAutoWidth.ts` — `calculateAutoWidth()`
- `cellContent.ts` — already has `column.type?.toLowerCase() || 'text'`
- `DataTableFilterOption.tsx` — already has `(col.type ?? ColType.Text)`

📝 **Warning**: When rewriting any of these files (e.g., during plan implementation), always preserve null guards. They've been lost before during file rewrites.

## Extension Hooks on IViewContext (UseLoading, UseAlert, etc.)

### Must use `this.Context.UseXxx()` from ViewBase
❌ **`UseLoading()`** — direct call from ViewBase fails with CS0103 "does not exist in current context"
✅ **`this.Context.UseLoading()`** — extension methods on `IViewContext` must be called via `this.Context`
📝 **Why**: `ViewBase` wraps hooks like `UseState`, `UseEffect` as protected methods delegating to `this.Context`. But extension methods defined on `IViewContext` (like `UseLoading`, `UseAlert`) aren't wrapped, so you must call them through `this.Context` explicitly.

## Clicking Buttons Behind Modal Dialog Overlays (Playwright)

### Dialog overlay intercepts all pointer events
❌ **`page.getByTestId('btn').click()`** — times out, overlay div intercepts pointer events
❌ **`page.getByTestId('btn').click({ force: true })`** — fires click event but Ivy's event handler doesn't process it
✅ **Use `page.evaluate` with `dispatchEvent`:**
```typescript
await page.evaluate((id) => {
  const btn = document.querySelector(`[data-testid="${id}"]`) as HTMLElement;
  if (btn) btn.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
}, 'my-button-testid');
```
📝 **Why**: Ivy's dialog uses a fixed overlay div (`class="fixed inset-0 z-50 bg-black/30"`) that intercepts all pointer events. Playwright's `force: true` dispatches pointer events but they don't propagate through Ivy's websocket-based event system correctly. `dispatchEvent` on the actual DOM element triggers the native click handler which Ivy does process.

## PathToAppIdMiddleware Intercepts Custom File Extensions

### Missing file extensions in routing-constants.json
**Problem**: `PathToAppIdMiddleware` rewrites ALL URL paths to `/?appId=...` unless the path has an extension listed in `staticFileExtensions` (in `src/frontend/src/routing-constants.json`). Custom middleware registered via `UseWebApplication()` runs AFTER this rewrite, so it sees `path="/"` instead of the original URL.

- If you add new middleware that serves files with a custom extension (e.g., `.md`), you MUST add that extension to `staticFileExtensions`
- Without this, the middleware pipeline is: `UseRouting -> UsePathToAppId (rewrites path) -> UseRouting -> user middleware (sees "/" not original path) -> UseFrontend`

## Chrome Parameter and Sidebar Visibility

### `?chrome=false` Hides the Sidebar
- `?chrome=false` disables the Ivy "chrome" (sidebar navigation, tab bar, settings)
- When testing **sidebar labels, navigation items, or app names in the sidebar**, do NOT use `?chrome=false`
- Only use `?chrome=false` when testing app content in isolation without sidebar interference

## FileDialog Upload Mode — mode Prop Stripped by Serializer (FIXED)

### FileDialogMode.Upload is default enum value 0 → stripped from JSON
❌ **`mode` prop arrives as `undefined` on frontend** — `FileDialogMode.Upload` is enum value 0 (the parameterless constructor default), so the WidgetSerializer strips it from the serialized JSON
❌ **Upload silently falls through to PathOnly** — `mode === 'Upload'` is false when mode is undefined, so `handleFiles()` takes the PathOnly branch (fires OnFilesSelected without uploading)
✅ **Fix**: Added default value `mode = 'Upload'` in `FileDialogWidget.tsx` destructuring
📝 **Why**: This is another instance of the WidgetSerializer stripping default enum values (same as DataTable `ColType.Number`). Any new widget with enum props that have value 0 as the "active" mode will hit this bug.

## ColorInput TestId Not Rendered in DOM

### `.TestId()` on ColorInput does NOT produce `data-testid` attribute
❌ **`page.getByTestId('my-color')`** — element not found, `data-testid` is not rendered for ColorInput widgets
✅ **Use `input[type='color']` locator** — `page.locator("input[type='color']")` finds the native color picker
✅ **Use DOM traversal from headings** — find the heading, walk siblings to find the row, then locate the color input container via `.closest(".flex.items-center")`
📝 **Why**: Similar to Badge, ColorInput's widget wrapper does not render `data-testid` in the DOM. Only the internal `<input type="color">` and `<input type="text">` elements are in the DOM without testid attributes.

### `ivy-widget` custom elements have height 0
❌ **`el.closest("ivy-widget")?.getBoundingClientRect().height`** — always returns 0
✅ **Measure the content div inside ivy-widget** — `iw.querySelector(":scope > div")?.children[0]?.getBoundingClientRect()`
📝 **Why**: `ivy-widget` is a custom element without explicit `display: block` styling, so it collapses to 0 height. The actual rendered content is in child divs.

## NumberInput TestId and Clear Button DOM Structure

### TestId is on the `<input>` element, NOT a wrapper
❌ **`page.getByTestId('my-number').locator('input')`** — times out because testid IS the input, there's no child input
✅ **`page.getByTestId('my-number')`** — directly references the `<input>` element
✅ **`page.getByPlaceholder('placeholder text')`** — alternative locator for NumberInput

### Clear (X) button is NOT inside the testid element
❌ **`page.getByTestId('my-number').locator('svg')`** — SVG is not a descendant of the `<input>` element
✅ **Walk up the DOM** to find the SVG clear button:
```typescript
await page.evaluate(() => {
  const input = document.querySelector('[data-testid="my-number"]');
  let container = input?.parentElement;
  let svg: SVGElement | null = null;
  for (let i = 0; i < 5 && container && !svg; i++) {
    svg = container.querySelector('svg');
    if (!svg) container = container.parentElement;
  }
  if (svg) {
    const clickTarget = svg.closest('button') || svg.closest('div[class*="absolute"]') || svg.parentElement || svg;
    (clickTarget as HTMLElement).click();
  }
});
```
📝 **Why**: NumberInput renders `data-testid` directly on the HTML `<input>` element (not an `<ivy-widget>` wrapper). The clear X icon is a sibling element at a higher DOM level (`<div class="relative">` > `<input>` + `<div class="absolute">` containing the SVG). The `<input>` has no children.

## react-markdown v10 Removed `inline` Prop from Code Component

### `inline` prop is always `undefined` in code component
❌ **`if (inline)` in code component** — react-markdown v10 removed the `inline` prop from the `code` component. It is always `undefined`.
✅ **Use `!className` to detect inline code** — inline code has no `className` (no language), while code blocks have `className="language-xxx"`
✅ **Alternative**: Check if the `<code>` element is inside a `<pre>` element: `if (!el.closest('pre'))`
📝 **Why**: react-markdown v9+ changed how inline code vs code blocks are distinguished. In v8, `inline: true` was passed. In v10, inline code is simply a `<code>` element without a `<pre>` wrapper and without a `className`. The project uses react-markdown v10.1.0.

## Badge Renders as `<div>`, Not `<span>`

### Playwright badge locator must target `<div>` elements
❌ **`document.querySelectorAll("span")` for badges** — Badge component renders as `<div>`, not `<span>`
✅ **Use `document.querySelectorAll("div")` or `document.querySelectorAll("td div")`** for badge detection in tables
📝 **Why**: `Badge.tsx` renders `<div className={cn(badgeVariant(...), className)} ...>` — it's an `HTMLDivElement`, not a span.

### CSS `inline-flex` blockified in flex context
❌ **Asserting `computedStyle.display === "inline-flex"` for badges inside `Layout.Horizontal()`** — CSS spec blockifies `inline-flex` to `flex` when element is a flex item
✅ **Assert `inline-flex` only for badges in block context** (e.g., table cells). In flex containers, check the `className` attribute contains "inline-flex" instead of checking computed style
📝 **Why**: CSS Display Level 3 spec: "If a block-level or inline-level box has a flex inner display type, it computes to flex/inline-flex based on outer context."

## TableBuilder `.Builder()` Uses `.Func<TIn>()`, Not `.Custom()`

### No `.Custom()` method on `IBuilderFactory`
❌ **`.Builder(x => x.Prop, f => f.Custom(val => widget))`** — `Custom()` does not exist
✅ **`.Builder(x => x.Prop, f => f.Func((string val) => (object)widget))`** — use `Func<TIn>` with explicit type parameter
📝 **Available methods**: `Default()`, `Text()`, `Link()`, `CopyToClipboard()`, `Func<TIn>()`, `Progress()`

## Layout.Horizontal().Gap() Accepts `bool`, Not `Size`

### Gap parameter type mismatch
❌ **`Layout.Horizontal().Gap(Size.Units(1))`** — CS1503: cannot convert from `Ivy.Size` to `bool`
✅ **`Layout.Horizontal()`** — omit Gap or use `Layout.Horizontal().Gap(true)` / `Layout.Horizontal().Gap(false)` for default gap toggle

## DiffView OnLineClick Extension Method CS1660

### Same pattern as ScreenshotFeedback OnSave/OnCancel
❌ **`.OnLineClick(line => state.Set(line))`** — causes CS1660: "Cannot convert lambda expression to type 'Event<DiffView, int>'"
✅ **Use `with` expression:**
```csharp
new DiffView().Diff(diff) with
{
    OnLineClick = e => { state.Set(e.Value); return ValueTask.CompletedTask; }
}
```
📝 **Why**: Same root cause as ScreenshotFeedback — the `OnLineClick` property is `Func<Event<DiffView, int>, ValueTask>?`. C# resolves property getter + delegate invocation before extension methods.

## Ivy.csproj Rust Binary Build Failure (MSBuild Glob Issue)

### `*rustserver*.dll` glob copy fails with "destination is a folder"
❌ **`dotnet build` fails with MSB3024** — "Could not copy rustserver.dll to bin\Debug\net10.0\runtimes/win-x64/native/, because the destination is a folder instead of a file"
✅ **Workaround**: Create the CI/CD artifacts path to bypass the fallback glob:
```bash
mkdir -p src/RustServer/artifacts/native/win-x64
cp src/RustServer/target/release/rustserver.dll src/RustServer/artifacts/native/win-x64/rustserver.dll
```
📝 **Why**: The fallback `Content Include="../RustServer/target/release/*rustserver*.dll"` with `Link="runtimes/win-x64/native/%(Filename)%(Extension)"` and `CopyToOutputDirectory="PreserveNewest"` produces a destination path ending in `/` which MSBuild interprets as a directory. The CI/CD path (`artifacts/native/win-x64/rustserver.dll`) uses a direct filename in `Link` which works correctly.

## DataTable Dual Data Path (Cell Rendering vs Data Loading)

### Cell values come from useRowData, NOT convertArrowTableToData
**Problem**: DataTable has two separate paths for accessing row data:
1. `convertArrowTableToData()` (in `tableDataMapper.ts`) — called during data loading, converts Arrow table to `{ columns, rows }`. Used for column metadata, row counts, and `hasMore` flag.
2. `useRowData()` (in `dataTableContext/hooks/useRowData.ts`) — called during cell rendering. Reads directly from the raw Arrow table via `column.get(rowIndex)`.

**Impact**: Any fix to data conversion in `convertArrowTableToData()` will NOT affect what the grid displays. The grid cells always read raw values from the Arrow table via `useRowData()`.

📝 **When fixing data conversion bugs** (like Decimal128 scaling), the fix must be applied in `useRowData.ts`, not just in `convertArrowTableToData()`. The `arrowTableRef` holds the raw Arrow table, and `useRowData` is the only path that feeds cell rendering.

📝 **Verification tip**: To check actual cell values in Playwright tests, use React fiber introspection to find `getCellContent` and call it directly — DOM text assertions don't work because DataTable renders on `<canvas>`.

## DataTable Filter Panel Overlaps HeaderLeft Slot Content

### Inline filter panel intercepts pointer events on HeaderLeft widgets
❌ **Buttons in `.HeaderLeft()` are unclickable when `AllowFiltering=true` (default)** — the DataTableOption inline filter panel has a `w-[450px]` container div that overlaps the HeaderLeft area, intercepting pointer events
✅ **Workaround 1**: Disable filtering: `.Config(config => config.AllowFiltering = false)` if filtering is not needed
✅ **Workaround 2**: Use `force: true` on Playwright clicks (but Ivy event handling may not process forced clicks reliably)
📝 **Why**: `DataTableOption` with `displayMode="inline"` and `inlineDirection="right"` renders a collapsible panel to the right of the filter icon. Even when collapsed, the container div (`h-full w-[450px] flex items-center`) remains in the DOM and intercepts pointer events over the adjacent HeaderLeft slot content.

## Size API

### Size.Pixels() doesn't exist
❌ **`Size.Pixels(200)`** — no such method
✅ **`Size.Px(200)`** — correct method for pixel sizes
📝 **Available methods**: `Size.Px(int)`, `Size.Rem(int)`, `Size.Units(int)`, `Size.Fraction(float)`, `Size.Full()`, `Size.Fit()`, `Size.Screen()`, `Size.MinContent()`, `Size.MaxContent()`, `Size.Auto()`, `Size.Grow()`, `Size.Shrink()`, `Size.Half()`, `Size.Third()`, `Size.Percent(int)`

## Image Widget MinContent Default Causes Hidden Elements in Playwright

### Images with default sizing are invisible before HTTP load completes
❌ **`new Image("https://example.com/img.jpg") { Overlay = true }`** — Playwright considers this "hidden" because `Width = MinContent` + `Height = MinContent` defaults produce 0x0 dimensions until the image loads from the network
✅ **`new Image("https://example.com/img.jpg") { Overlay = true, Width = Size.Px(200), Height = Size.Px(150) }`** — explicit pixel dimensions ensure the element is visible immediately
📝 **Why**: `Image` defaults to `Width = Size.MinContent()` and `Height = Size.MinContent()`. Before the browser loads the remote image, `<img>` has intrinsic size 0x0. Playwright's `toBeVisible()` requires non-zero dimensions. Always set explicit `Width`/`Height` in test apps using external image URLs.

## RefreshToken API

### Method is `.Refresh()`, not `.Trigger()`
❌ **`refreshToken.Trigger()`** — no such method; causes CS1501
✅ **`refreshToken.Refresh()`** — correct method to signal data refresh
✅ **`refreshToken.Refresh(returnValue)`** — optional return value parameter
📝 **Why**: `RefreshToken` wraps an `IState<(Guid, object?, bool)>` tuple. `.Refresh()` generates a new GUID to trigger state change. The class implements `IEffectTriggerConvertible` via `.ToTrigger()` for `UseEffect` dependencies.

## Future Gotchas to Document

As we encounter more issues during feature testing, add them here with:
- ❌ **What doesn't work**
- ✅ **What does work** (solution)
- 📝 **Why** (explanation when helpful)
- Code examples showing wrong vs. right approach
