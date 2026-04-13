# Widget-Specific Playwright Patterns

## DataTable (Canvas-Based)

DataTable uses Glide Data Grid — renders as `<canvas>`, NOT HTML `<table>`.

**Locators:**
- Canvas: `page.locator('[data-testid="data-grid-canvas"]')` or `page.locator('canvas').first()`
- Click target: `page.locator('.dvn-scroller').first().click({ position: { x, y } })` — the overlay div intercepts pointer events
- Wait: `await page.waitForSelector('canvas', { timeout: 15000 })` then wait 1s for render
- Virtualized elements (`role="gridcell"`, `role="columnheader"`) are "hidden" — use `.toBeAttached()` not `.toBeVisible()`

**Reading cell values:** DOM text assertions don't work on canvas. Use React fiber introspection: walk `__reactFiber` tree from canvas to find `getCellContent` prop, then call `getCellContent([col, row])` → `{ kind, data, displayData }`. Column 0 = first visible column.

**Gotchas:**
- Shows ALL model properties, not just those with `.Header()` calls — `.Header()` only renames the label
- One `ValueAccessor` per root property — two `.Header()` calls on sub-properties of the same root share one column
- `DataTableBuilder` has no `.Remove()` method — resolves to `CollectionExtensions.Remove` and fails
- `.Remove()` on positional records: crash fixed (commit `e08a55d6`) but data alignment broken (index shift)
- `decimal` columns display as `0000000000000000` (framework bug in `useRowData.ts` Decimal128 handling)
- `.dispatchEvent("click")` on gridcells bypasses visibility but does NOT trigger Ivy's `OnCellClick`
- Kill stale `.exe` processes before tests — they lock DLLs
- **Multiple grids on same page**: React fiber walks from different `<canvas>` elements may find the same parent component's `columns` prop. Deduplicate grids by creating a key from column titles (`props.columns.map(c => c.title).join(',')`) and skipping duplicates.
- **Column widths in fiber tree reflect post-grow rendered widths**, not configured widths. The last column always gets `grow: 1` by default. For test assertions: check `grow` factor presence/value, not exact pixel width on grow columns.

## Input Widgets

### SelectInput

**Single-select:** Radix Select — trigger `button[role="combobox"]`, options `[role="option"]`.
**Multi-select:** CMDK in Popover — trigger is div with input, options are `[cmdk-item]`. Popover stays open after selecting — close with `Escape`.
**Trigger gotcha:** Renders ALL option labels concatenated in trigger button — `getByText("Option")` is ambiguous (matches trigger + dropdown item). Use `page.evaluate` + coordinate clicking, or `getByRole("option", { name: "Option", exact: true })`.

**Variants:**
- `.Slider()` — Radix slider with `role="slider"`, keyboard nav (ArrowRight/Left, Home/End). No `selectMany` support.
- `.Variant(SelectInputVariant.Toggle)` — Radix toggle buttons with `aria-pressed`, NOT `role="radio"/"checkbox"`. Use `getByText("Option", { exact: true }).first().click()`.

**AsyncSelectInput:** Opens a Sheet (`role="dialog"`) with search + ListItem options. Trigger: `getByRole('button', { name: 'placeholder text' })`. Wait 1500ms after selection for state update. `.Invalid()` renders as icon, not text.

### NumberInput / MoneyInput

- `state.ToNumberInput()` renders as text `<input>`, NOT `type="number"` — `input[type="number"]` finds nothing
- `state.ToMoneyInput().Currency("USD")` renders formatted currency (e.g., "$850,000")
- `state.ToNumberRangeInput()` renders dual-handle slider with 2 `role="slider"` elements
- `.Variant(NumberInputVariants.Slider)` renders as Radix slider (same as `ToSliderInput()`)
- `.WithField().Label("X")` does NOT create `<label for="">` — `getByLabel("X")` fails; use index-based locators

### BoolInput (Checkbox / Switch)

- Renders as `<button role="checkbox">` (Radix UI), NOT native `<input type="checkbox">`
- `getByRole("checkbox", { name: /.../ })` and `getByLabel()` do NOT work — use `page.locator('[role="checkbox"]').nth(N)`
- Clicking label text does NOT toggle the checkbox — must click the `<button>` directly
- Without explicit `.Variant()`, defaults to `role="checkbox"` (not `role="switch"`)

### Slider

- `state.ToSliderInput()` renders as Radix UI slider with `role="slider"`, NOT `<input type="range">`
- Keyboard: ArrowRight/Left (step), Home/End (min/max)

### ContentInput

- `state.ToContentInput(uploadContext)` — requires `IState<UploadContext>` from `UseUpload(MemoryStreamUploadHandler.Create(filesState))`
- `.ShortcutKey("Ctrl+Enter")` renders `<kbd>` badge in bottom toolbar when not focused
- **macOS shortcut key mapping**: `parseShortcut("Ctrl+X")` maps `Ctrl` to `meta` (Cmd key) on macOS. In Playwright tests, use `Meta+Enter` (not `Control+Enter`) on macOS to trigger `Ctrl+Enter` shortcuts. Detect with `process.platform === 'darwin'`.
- kbd badge locator: `page.locator('kbd')`
- When `shortcutKey` is set, the inline `Ctrl/Cmd+Enter` handler is disabled — only the global `useEffect` listener fires
- `.Invalid("message")` renders as `InvalidIcon` (info icon with `data-invalid-icon="true"`) in top-right corner, NOT visible text. The message is shown in a Radix tooltip on hover. To test: `page.locator('[data-invalid-icon="true"]').first().hover()` then assert tooltip text with `.first()` (Radix creates duplicate tooltip DOM elements). Invalid also adds `border-destructive` CSS class to the wrapper.
- `.Small()` / `.Medium()` / `.Large()` density variants scale textarea size, toolbar padding, paperclip icon, and shortcut badge proportionally

### TextInput / TextareaInput

- `state.ToTextareaInput()` → `<textarea>`, `state.ToTextInput()` → `role="textbox"`
- `.Placeholder("X")` → accessible via `getByRole("textbox", { name: "X" })`
- `.Multiline().Rows(6)` renders as `<textarea>`

### DateInput / DateRangeInput

- `state.ToDateInput()` renders as `<button data-slot="calendar">` trigger with Radix Popover — NOT `<input type="date">`
- Calendar navigation: `input[placeholder="M"]` (month), `input[placeholder="YYYY"]` (year) — fill and Enter
- Day selection: `page.locator('div[data-slot="calendar"]').locator("button").filter({ hasText: /^15$/ }).first().click()`
- Popover auto-closes after day selection; nullable dates show clear (X) button
- react-day-picker v9 uses flex layout, NOT `<table>` — use `.rdp-weekdays .rdp-weekday`, `.rdp-day button`

**DateRangeInput:** Two-month calendar with presets. After clicking start date, dates before it are disabled — use sidebar presets for reliable testing. State: `UseState<(DateOnly, DateOnly)>()` (non-nullable only).

### CodeInput / CodeBlock

- CodeMirror editor locator: `.cm-content[contenteditable='true']`
- **Do NOT use `keyboard.type()`** for CodeMirror — use clipboard: `page.evaluate(async (t) => { await navigator.clipboard.writeText(t); }, text)` then `Control+V`. Requires `permissions: ["clipboard-read", "clipboard-write"]`.
- `CodeInput<TString>` requires type parameter — prefer `.ToCodeInput()` extension or `Markdown` widget
- `CodeBlock` with `UseEffect`-populated state renders empty initially — trigger state change, wait 500-1000ms
- Syntax tokens use inline `style` attributes (react-syntax-highlighter/Prism), not CSS classes
- `.ShowCopyButton()` adds `aria-label="Copy to clipboard"` — use `{ exact: true }`

### FeedbackInput (Star Rating)

- Star SVGs: `page.locator('button svg')` — extra SVGs from Ivy branding exist on page
- `.AllowHalf()` creates extra SVG elements — don't assert exact `button svg` counts
- Use `CultureInfo.InvariantCulture` for decimal values matched by Playwright
- `.Invalid()` renders as icon, not text

## Dialogs, Sheets & Forms

- `.ToDialog()`, `.WithConfirm()`, `.ToSheet()` all render as `<div role="dialog">`, NOT HTML `<dialog>`
- **NEVER** use `page.locator("dialog")` — use `page.getByRole("dialog", { name: "Title" })` or `[role='dialog']`
- **Sheet title strict mode**: `.WithSheet(title: "My Sheet")` renders the title as both button text ("Open My Sheet") and `<h2>` heading inside the sheet. `getByText("My Sheet")` matches both → strict mode violation. Use `getByRole('heading', { name: 'My Sheet' })` for the sheet heading.
- `.WithConfirm("message", "title")` always uses **"Ok"** and **"Cancel"** buttons (hardcoded)
- Edit sheets use "Save" button; create dialogs use entity action name (e.g., "Create")
- `[Required]` fields render labels as "FieldName *" — `getByText("Code", { exact: true })` won't match "Code *"; use input element locators
- `.WithField().Required()` adds `*` suffix; `.WithField().Label("X")` on NumberInput has no `<label for="">` association

## Layout Widgets

### Card

- `.Title("X")` renders as `<span>`, NOT heading — use `getByText("X", { exact: true })`, not `getByRole("heading")`
- `new Card(content, header: Text.H3("X"))` renders heading; `.Title("X")` does not
- `Card.Default(content)` does NOT exist — use `new Card().Content(widget)` pattern
- `.Title().Content()` may render invisible card body — use `Layout.Vertical()` as workaround

### Tabs

- `Layout.Tabs` renders BOTH tab panels in DOM — inactive content is hidden but locators can match it
- Tab switching: `getByRole("tab", { name: "TabName" })`

### Kanban

- Column headers include counts: "Todo (2)" — use regex `/^Todo/` or `/Todo.*\(\d+\)/`

### Calendar

- Uses react-big-calendar with toolbar (Today, <, >, Month/Week/Day/Agenda)
- Color support via string values ("Blue", "Green", "Red", "Purple"); all-day events span date cells
- `OnEventClick` receives event ID; `.ShowToolbar(false)` hides navigation

## Data Display

### Json / Xml

- `Json(object)` auto-serializes; `Xml(XObject)` or `Xml(string)`. Both have `Expanded` prop (null=collapsed, -1=fully expanded)
- Default is collapsed — use `{ Expanded = -1 }` for testing
- Xml's `.TestId()` does NOT render `data-testid` — use text-based locators

### Details

- `.ToDetails()` converts PascalCase to spaced labels ("MonthlyNetBurn" → "Monthly Net Burn")
- `Dictionary<TKey, TValue>.ToDetails()` crashes with `TargetParameterCountException` — convert to anonymous object first

### ECharts

- Uses **echarts-for-react** rendering as **canvas** (not SVG) — `querySelectorAll('text')` won't find axis labels
- Locate via: `page.locator('[_echarts_instance_]')`; `data-chart-rendered="true"` indicates ready
- Use React fiber to read chart options: walk `__reactFiber` tree from `[_echarts_instance_]` element to find `memoizedProps.option`, then inspect `option.xAxis`, `option.yAxis`, etc.
- To verify axis properties (e.g., `show: false` for hidden axes), read the ECharts option object via React fiber — DOM text inspection doesn't work with canvas rendering
- `option.radar` can be object or array — always normalize with `Array.isArray`
- Case sensitivity: axes/labels visible but no data lines → suspect dataKey case mismatch
- Screenshot-based verification needs 3s+ waits

## Media & Files

### VideoPlayer

- `<track>` elements not accessible via Playwright — use `page.evaluate()` with `querySelectorAll('video')[N].querySelectorAll('track')`
- Subtitles require CORS-compatible video sources

### Camera

- Chromium's `--use-fake-device-for-media-stream` produces `videoWidth=0`/`videoHeight=0` in headless — upload assertions unverifiable
- State machine (idle → active → captured) can be tested
- Config: `permissions: ['camera']` + appropriate `launchOptions.args`

### Upload / FileUpload

- `FileUpload<byte[]>`: FileName, Length, Content, Status (Pending/Loading/Aborted/Failed/Finished — NO `InProgress`)
- `ToFileInput(upload).Placeholder(...)` renders dashed dropzone with hidden `input[type="file"]`
- `page.locator('input[type="file"]').setInputFiles({name, mimeType, buffer})` for testing
- `FileUpload<byte[]>.ToTable()` with `.Remove(e => e.Content)` throws `KeyNotFoundException`

### File Dialogs (UseFileDialog / UseSaveDialog / UseFolderDialog)

Disable File System Access API for Playwright:
```typescript
await page.addInitScript(() => {
  Object.defineProperty(window, 'showOpenFilePicker', { value: undefined, configurable: true, writable: true });
  Object.defineProperty(window, 'showSaveFilePicker', { value: undefined, configurable: true, writable: true });
  Object.defineProperty(window, 'showDirectoryPicker', { value: undefined, configurable: true, writable: true });
  delete (window as any).showOpenFilePicker;
  delete (window as any).showSaveFilePicker;
  delete (window as any).showDirectoryPicker;
});
```
- Folder dialog (`<input webkitdirectory>`): `fileChooser.setFiles()` requires **directory path**, not individual files
- Save dialog in headless: uses `<a download>` fallback — `page.waitForEvent('download')` to capture

## Other Widgets

### Markdown

- `OnLinkClick` fires for ALL URL types when handler registered; lambda needs explicit type: `(string url) => { }`
- Popover links (`[text](## "content")`): renders `<span role="button">` with Radix Popover
- Local images: `.DangerouslyAllowLocalFiles()` preserves file:// URLs; browser blocking is expected

### Webhooks (UseWebhook)

- URLs at `/ivy/webhook/<guid>`, NOT `/api/webhook/<guid>`
- GET and POST only — PUT/DELETE return non-200
- Each `page.goto()` creates fresh WebSocket session with new webhook URL
- State updates need 1500-3000ms wait

### UseArgs / UseDownload

- `UseArgs<T>` reads from WebSocket `appArgs`, NOT browser URL — external URL navigation doesn't work
- `IHttpContextAccessor.HttpContext` is null in WebSocket context — always use `?.`
- `UseDownload` returns nullable URL — renders as `role="link"` when non-null
