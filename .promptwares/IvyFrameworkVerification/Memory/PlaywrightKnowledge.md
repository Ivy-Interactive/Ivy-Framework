# Ivy Playwright Test Knowledge Base

## Ivy Framework Basics

- Ivy apps are .NET web apps started with `dotnet run -- --port <port>`
- The frontend client is in `src/frontend/` and built with `vp build` into `src/frontend/dist/`. These assets are embedded into the Ivy.dll via `<EmbeddedResource>`. If testing a commit that changed frontend `.ts` files, you MUST rebuild the frontend (`cd src/frontend && vp build`) before running the dotnet project, otherwise the old bundled JS is served.
- If `vp build` fails due to pre-existing TS errors in unrelated files, run vite directly: `npx vite build` — this skips tsc and still produces a working bundle. (Note: `node ./node_modules/vite/bin/vite.js` may fail with MODULE_NOT_FOUND if vite is hoisted; use `npx` instead)
- The server prints `Ivy is running on http://localhost:<port>` when ready
- Apps are decorated with `[App(icon: Icons.X, path: ["Apps"])]` and inherit `ViewBase`
- The `Build()` method returns the UI tree
- State is managed via `UseState<T>()` which returns reactive state objects
- Services are injected via `UseService<T>()`

## DataTable Testing Patterns

- DataTable uses Glide Data Grid which renders as `<canvas>` elements, NOT HTML `<table>`
- Canvas locator: `page.locator('[data-testid="data-grid-canvas"]')` or `page.locator('canvas').first()`
- **Click target**: Use `page.locator('.dvn-scroller').first().click({ position: { x, y } })` — the `dvn-scroller` overlay div intercepts pointer events over the canvas
- Wait for table: `await page.waitForSelector('canvas', { timeout: 15000 })`; then wait 1s for render
- Stale process cleanup: Always kill previous `DataTableAutoWidth.exe` processes before running tests — they lock DLLs and cause build failures
- Use `--no-build` in test spawn after pre-building: `spawn('dotnet', ['run', '--no-build', '--', '--port', port])`

## Standalone Test Project Setup

- The server class is `Server` (NOT `IvyServer`). Entry point: `var server = new Server(); await server.RunAsync();`
- Apps must be explicitly registered: `server.AddAppsFromAssembly(Assembly.GetExecutingAssembly());` — Ivy does NOT auto-discover apps
- Use ProjectReference to local Ivy: `<ProjectReference Include="D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Ivy.csproj" />`
- The csproj needs `<Nullable>enable</Nullable>` to avoid CS8632 warnings
- `ButtonVariant` values: Primary, Destructive, Outline, Secondary, Success, Warning, Info, Ghost, Link (NO `Default`)
- `Card` has no `.Default()` method — use `new Card(content)` or `Layout.Vertical()` containers instead
- `Icons` enum: use PascalCase Lucide icon names. Not all names exist (e.g. `AlignCenter` doesn't exist, use `AlignCenterHorizontal`). Lucide renamed some icons: `AlertTriangle` → `TriangleAlert`
- `AppContext` is ambiguous with `System.AppContext` — always use `Ivy.AppContext` in standalone test projects (e.g. `UseService<Ivy.AppContext>()`)
- For `IDisposable` return in `UseEffect`, use `System.Reactive.Disposables.Disposable.Empty` (available via Ivy's dependency on System.Reactive)

## Ivy UI Components

- `Layout.Center()`, `Layout.Vertical()`, `Layout.Horizontal()` — layout containers
- `Text.H2()`, `Text.P()`, `Text.Label()`, `Text.InlineCode()` — text elements
- `Slider` — range input, configured with `.Min()`, `.Max()`, `.Step()`
- `Switch` — toggle input, configured with `.Label()`
- `Button` — click handler, configured with `.Icon()`, `.Variant()`, `.Disabled()`
- `Badge` — status label with variant (Destructive, Warning, Success, Info, Secondary)
- `Card` — container card component. Note: Card supports pipe `|` for children but Text elements (H3, P) added directly as Card children may not render; use `Layout.Vertical()` inside Card for mixed content
- `Json` — displays formatted JSON with syntax highlighting and collapsible tree. Constructors: `Json(JsonNode)`, `Json(object)` (auto-serializes), `Json(string)`. Props: `Expanded` (null=collapsed, N=depth, -1=fully expanded). Default is collapsed — use `{ Expanded = -1 }` for testing to make content visible
- `Xml` — displays formatted XML with syntax highlighting and collapsible tree. Constructors: `Xml(XObject)`, `Xml(string)`. Props: `Expanded` (null=collapsed, 0=collapsed, N=depth, -1=fully expanded). Same behavior as Json widget. `.TestId()` does NOT render `data-testid` in the DOM — use text-based or heading-relative locators instead
- `Toast` — notification via `IClientProvider.Toast(message, type)`
- State bindings: `state.ToSliderInput()`, `state.ToSwitchInput()`

## Playwright Test Patterns for Ivy

### Project Setup
- Tests live in `.ivy/tests/` within the Ivy project
- Use `package.json` with `@playwright/test` dependency
- `playwright.config.ts` targets Chromium only, single worker, no retries
- Config uses `process.env.APP_PORT` for base URL
- **Viewport configuration**: Use `{ width: 1920, height: 1920 }` for square screenshots that capture sufficient page content without being excessively tall. Must be set in BOTH `use` and `projects[0].use` to override device preset defaults (e.g., `...devices['Desktop Chrome']` sets 1280x720 which is too short)

### Video Recording
- Enable via `video: { mode: 'on', dir: './videos' }` in both `use` and `projects[0].use` config
- Videos are saved as `.webm` files in the configured directory
- Use `test.afterEach` to rename videos with descriptive names matching test titles
- Access video path via `page.video()?.path()` — only available after the page context closes
- Videos add ~1-2 seconds overhead per test but provide invaluable debugging evidence

### App Lifecycle in Tests
- `beforeAll`: find free port via `net.createServer()`, spawn `dotnet run -- --port <port>`, wait for HTTP 200
- `afterAll`: kill the spawned process
- `beforeEach`: navigate to `http://localhost:<port>`
- Use `cwd: process.cwd().replace(/[/\\]\.ivy[/\\]tests$/, "")` to resolve project root from test dir
- Wait for server with polling loop (500ms interval, 30s timeout)

### Locator Strategies
- Prefer `page.getByText()` for visible text content
- Use `page.getByRole("button", { name: /pattern/i })` for buttons
- Use `page.locator("code")` for inline code elements
- Use `.first()` when multiple matches are possible
- Use `waitFor({ state: "visible", timeout: 5000 })` for async content
- **Flexible text matching**: When matching formatted values (currency, percentages), use flexible regex patterns like `/Value:.*50.*200/` instead of strict patterns like `/Value: \$50\.00 - \$200\.00/` to handle formatting variations
- **Heading role conflicts**: `getByRole('heading', { name: 'Text' })` can match multiple headings if names overlap (e.g., "Percent Format" matches both "Currency & Percent Formatting" H1 and "Percent Format" H3). Always use `{ exact: true }` when heading names are substrings of other headings

### Common Test Categories
1. **Visibility tests** — verify UI elements render correctly
2. **Interaction tests** — click buttons, toggle switches, move sliders
3. **State change tests** — verify UI updates after interactions
4. **Output validation** — check generated/computed values appear correctly

### Gotchas & Tips
- **CodeBlock syntax tokens use inline styles, not CSS classes** — `react-syntax-highlighter` (Prism mode) applies styles via inline `style` attributes on `<span>` elements inside `<pre><code>`. Do NOT use class-based selectors like `span.token.comment`. To find comment spans, match by text content (starts with `//`, `/*`, `#`, `--`, `<!--`) within `pre code span` elements, then check `getComputedStyle(el).fontStyle` or other computed properties.
- **Scope element locators to widget containers** — `page.locator('ul')` or `page.locator('ol')` can match Ivy shell elements (e.g., toast viewport `<ol>`). Scope to the widget container: `.markdown-widget ul`, `.markdown-widget ol`, etc.
- **Tailwind `space-y-*` spacing verification** — `space-y` uses `> :not([hidden]) ~ :not([hidden])` sibling selector, NOT direct `margin-top` on child elements. `getComputedStyle(li).marginTop` returns `0`. Verify spacing via **bounding box gaps**: `box2.y - (box1.y + box1.height) > 0`.
- **Xml/Json widget toggle buttons use `role="button"`** — the expandable tree nodes in Xml and Json widgets render as `<div role="button">` elements. This means `getByRole("button", { name: "0" })` can match both actual `<button>` elements AND XML toggle nodes whose tag text contains "0". Always use `{ exact: true }` when targeting real buttons on pages that also contain Xml/Json widgets.
- **Confetti Click trigger wrapper uses `role="button"`** — `.WithConfetti(AnimationTrigger.Click)` wraps children in `<div role="button" tabindex="0">`. This means `getByRole("button", { name: "X" })` matches both the wrapper div (containing all child text) AND the actual `<button>` inside. Always use `{ exact: true }` when targeting buttons inside a Click-trigger Confetti wrapper.
- **Ivy layout doesn't render widgets as direct DOM siblings of headings** — `h2.nextElementSibling` won't contain the widget that follows the heading in the Ivy tree. Use full-page `body` text assertions or specific locators instead of DOM sibling traversal.
- Ivy apps may take a few seconds to start; 30s timeout is safe
- Use `shell: true` in spawn options on Windows
- Password/random generation tests: compare two outputs rather than asserting exact values
- Switch/toggle labels include the label text, use `getByText()` to find them
- After clicking a button, use `waitForTimeout(500)` or `waitFor()` before asserting changes
- Badges with strength/status text may appear multiple times; use `.first()`
- `state.ToSelectInput().Variant(SelectInputVariants.Toggle)` renders as radio buttons — use `getByRole("radio", { name: "OptionName" })` to click them, NOT `getByText()` (the option text may appear in headings/descriptions too)
- `IClientProvider.Toast()` renders both a visible `<div>` and an `aria-live` status `<span>` — use `getByText("message", { exact: true })` to avoid strict mode violations
- **Toast DOM locator**: Radix Toast renders individual toasts as `<li>` elements with `data-state="open"`. Use `page.locator('li[data-state="open"]')` to find all visible toast elements. Do NOT use `[role="status"]` — that matches the aria-live region, not individual toasts. The toast viewport is an `<ol>` element with `gap-2` for spacing.
- **`Html` component inline styles don't render as expected** — `new Html(...)` with inline CSS properties like `background-color`, `border`, `color`, `width`, `height` are NOT applied to the actual DOM. Do NOT use `page.locator('div[style*="..."]')` selectors to target Html content. Instead use text-based assertions (`page.content().includes(...)`) or `getByText()` locators. Also do NOT check for hex color codes in `page.content()` — they won't appear in the rendered HTML.
- **Stale server processes**: The `taskkill` in `afterAll` may not reliably kill all dotnet processes. After multiple test runs, stale `Test.*.exe` processes can lock the EXE and prevent rebuilding. The launcher script (`IvyFrameworkVerification.ps1`) now kills processes with executables in `artifacts/sample/bin/` before starting, but the `afterAll` kill should still be done as best-effort cleanup. Use `taskkill //im <name>.exe //f` as a fallback.
- `state.ToTextareaInput()` renders as a standard `<textarea>` element, locatable via `page.locator("textarea").first()`; disabled output textarea is the `.nth(1)`
- Clipboard `writeText` fails in headless Chromium with "Write permission denied" — this is expected and not an app bug
- `state.ToSliderInput()` renders as a Radix UI slider with `role="slider"`, NOT a native `<input type="range">` — use `page.getByRole("slider")` and keyboard interaction (ArrowRight/ArrowLeft to increment/decrement by step, Home/End for min/max)
- `UseChrome()` renders a hidden sidebar search `<input type="search" data-testid="sidebar-search">` that is the first `input` in the DOM but outside the viewport — `page.locator("input").first()` will target it instead of app inputs. Use `input[type='text']` or label-based locators to target app inputs
- **Single-app Chrome auto-selection**: When `UseChrome().UseTabs()` is enabled and there's only ONE app registered, Chrome automatically opens that app's tab on page load — no need to click the sidebar nav item. Clicking it may cause a re-navigation that times out.
- **Chrome URL routing**: When `UseChrome()` is active, direct URL navigation (e.g., `/basic-app` or `/basic-app?shell=false`) shows "App Not Found". Apps must be accessed by navigating to `/` first, then clicking sidebar items. In Playwright tests, always navigate to `http://localhost:PORT/` and use sidebar clicks.
- **SelectInput trigger displays all option labels**: `SelectInput<T>` with many options renders ALL option display names concatenated in the trigger button (combobox). This makes `getByText("OptionName")` ambiguous — it matches both the trigger and the dropdown item. To click a dropdown option reliably: open the dropdown, then use `page.evaluate` to find the option element by text content + bounding rect (y > trigger height), and click by coordinates. The dropdown container has `role="listbox"`.
- **SelectInput dropdown structure**: Ivy SelectInput dropdown uses Radix-like components. The trigger is a `<button role="combobox">`. The dropdown panel is `<div role="listbox">`. Items inside have NO `role="option"` — they are plain `<div>` elements with text content. Use `page.evaluate` + coordinate clicking for reliable selection when text-based locators are ambiguous.
- **Sidebar nav button name conflicts**: Chrome sidebar renders app names as `role="button"` elements. A button with text "C" will conflict with "Calculator" sidebar button when using `getByRole("button", { name: "C" })` — always use `{ exact: true }` for single-character button names
- **`waitForServer` must use `http` module**, not `fetch` — `fetch` may not be available in all Node.js versions used by Playwright. Use `http.get()` with polling loop instead
- `state.ToBoolInput().Variant(BoolInputVariants.Checkbox)` renders as `<button role="checkbox">` (Radix UI), NOT a native `<input type="checkbox">` — `getByRole("checkbox", { name: /.../ })` and `getByLabel()` do NOT work for locating these; use `page.locator('[role="checkbox"]').nth(N)` by index order instead
- `new Card(content, header: Text.H3("Title"))` — the card header text (e.g., "Monthly Revenue") will match `getByText()` along with any content containing the same substring (e.g., "Total Monthly Revenue:"), causing strict mode violations. Always use `getByRole("heading", { name: "Title" })` for card headers
- `new Card(content).Title("X")` — Card `.Title()` does NOT render as an HTML heading element (`<h1>`-`<h6>`), so `getByRole("heading", { name: "X" })` will fail. Use `getByText("X", { exact: true }).first()` instead. Only explicit `Text.H2()`/`Text.H3()` passed as card header render as headings.
- **NumberInput** (`state.ToNumberInput()`) renders as a regular text `<input>` in the DOM, NOT `<input type="number">`. `page.locator('input[type="number"]')` will find nothing. Use `page.locator('input[value="200"]')` to locate by current value, or find inputs relative to their label text.
- **NumberRangeInput** (`state.ToNumberRangeInput()`) renders as a dual-handle slider with `role="slider"` attributes (2 sliders per range input for lower/upper bounds). Supports currency formatting (`.ToMoneyRangeInput()`), percent formatting, prefix/suffix (text and icons), and keyboard navigation. Range constraints (lower ≤ upper) are enforced automatically.
- **CodeInput copy buttons**: `.ShowCopyButton()` and code editors add `aria-label="Copy to clipboard"` icon buttons. Combined with explicit "Copy" `Button` widgets, `getByRole("button", { name: "Copy" })` may match multiple elements — always use `{ exact: true }` for the explicit Copy button.
- **Webhooks require state parameter**: Ivy webhook URLs require an internal `state` query parameter. Server-side `HttpClient` calls to `webhook.BaseUrl` without this parameter return 400 "The 'state' query parameter is required." — Test Endpoint features using server-side HTTP calls to webhooks will fail.
- `state.ToMoneyInput().Currency("USD").Precision(0)` renders as a text input with formatted currency (e.g., "$850,000") — values are displayed with `$` prefix and comma separators
- `.ToDetails()` on an anonymous object renders key-value pairs with PascalCase property names converted to spaced labels (e.g., `MonthlyNetBurn` → "Monthly Net Burn")
- **Kanban column headers render as `<h3>` headings** with card counts (e.g., "To Do (2)"). Use `getByRole('heading', { name: /To Do/ })` to locate them. If the app also has instruction text containing the same words, `getByText()` will cause strict mode violations — always use heading role selectors for Kanban column headers.

## DataTable Gotchas

- **DataTable uses virtualized grid rendering** — cells are `<td role="gridcell">` elements that Playwright considers "hidden" even when data is present. Use `page.locator('[role="gridcell"]').first()` with `.toBeAttached()` instead of `.toBeVisible()` to verify data loaded.
- **`.Remove()` on positional records: crash fixed but data alignment broken** — Commit `e08a55d6` fixes the crash (fills removed params with defaults in constructor). However, the DataTable serializes ALL property values (including defaults for removed fields) but maps them by index to only the visible column headers, causing a systematic data shift. The "Name" column shows `0` (default Id) instead of the actual name. This is a framework serialization bug.
- **Decimal columns display as `0000000000000000`** — `decimal` values in DataTable grid render incorrectly (framework bug). Root cause: `useRowData.ts` reads raw Arrow Decimal128 values via `column.get(i)` without applying `10^scale` division. Fix was applied to `convertArrowTableToData()` but that function is only used for column metadata, not cell rendering.
- **Reading DataTable cell values in Playwright**: Since DataTable renders on `<canvas>`, DOM text assertions don't work. Use React fiber introspection: find a `canvas` element, walk `__reactFiber` tree upward to find a component with `getCellContent` prop, then call `getCellContent([col, row])` to get `{ kind, data, displayData }`. Column 0 = first visible column.
- **DataTable shows ALL model properties, not just those with `.Header()` calls**: The `.Header()` method only renames a column label and optionally sets a `ValueAccessor`. All properties of the model type appear as columns in the grid. Column order follows the model's property definition order, not the order of `.Header()` calls. When looking up columns by index, use the React fiber `columns` prop to find column titles and map to indices dynamically.
- **One ValueAccessor per root property**: When using navigation/ternary expressions in `.Header()`, `GetNameFromMemberExpression` extracts the root property name (e.g., `Address` from `e.Address.City`). Two `.Header()` calls targeting different sub-properties of the same root property (e.g., `Address.City` and `Address.Region.Name`) share one column — only the first `ValueAccessor` is stored (the `if (column.Column.ValueAccessor == null)` guard in `GetColumn` prevents overwriting). Avoid using multiple complex expressions on the same root property in test apps.

## Layout.Vertical with IEnumerable (Critical)

- `Layout.Vertical(items.Select(...), otherWidget)` where the first arg is `IEnumerable<T>` causes Ivy's content builder to render it as a data table instead of expanding children
- **Always** materialize enumerables before passing to Layout: `.Select<T, object?>(...).Append<object?>(otherWidget).ToArray()`
- Alternative: build layout with `|` operator chaining instead of passing collections

## Dialog and Sheet Locators (Critical)

- Ivy dialogs (`.ToDialog()`) and confirmation dialogs (`.WithConfirm()`) render as `<div role="dialog">`, NOT HTML `<dialog>` elements
- **NEVER** use `page.locator("dialog")` — it won't match. Always use `page.getByRole("dialog", { name: "Dialog Title" })` or `page.locator("[role='dialog']")`
- Ivy sheets (`.ToSheet()`) also render with `role="dialog"` — same locator pattern applies
- Form fields inside dialogs use labels like "Title *" (with asterisk for required) — `getByLabel("Title")` may not match. Use `dialog.getByRole("textbox").nth(N)` to target fields by position within the dialog
- Edit sheets use "Save" button (not "Submit") for form submission
- Create dialogs reuse the entity action name as the submit button text (e.g., "Create")

## WithConfirm Dialog

- `button.WithConfirm("message", "title")` renders a custom Dialog with "Cancel" (outline) and "Ok" (primary) buttons
- The confirm button text is always **"Ok"** — use `getByRole("button", { name: "Ok" })` to click it
- The dialog title and message are customizable, but button labels are hardcoded in `WithConfirmView`

## Critical: ValueTuple Crash Pattern

- **NEVER** use C# ValueTuple syntax `Layout.Vertical() | (item1, item2, ...)` when the tuple contains widgets — `DefaultContentBuilder.Format()` calls `ValueTuple.ToString()` which triggers `PrintMembers()` → `get_Id()` on uninitialized widgets, causing `InvalidOperationException`
- **Always** use `|` operator chaining instead: `Layout.Vertical() | item1 | item2 | item3`
- This is a Framework bug in `DefaultContentBuilder.Format()` (line 171) — note as external issue for planning
- Affects ALL widget types (Field, TextInput, Button, etc.) when placed in tuples

## Test Structure Patterns

- `test.beforeAll` runs once per `test.describe` block. Multiple `test.describe` blocks = multiple `beforeAll` executions = multiple server spawns and potential DLL lock conflicts.
- For shared server: put all tests in one `test.describe`, or use top-level `test()` calls outside describe blocks which share the file-level `beforeAll`.
- Always kill app processes after test runs on Windows — lingering processes lock DLLs and prevent rebuild on next run. Use `powershell.exe -NoProfile -Command 'Get-Process -Name "AppName" -ErrorAction SilentlyContinue | Stop-Process -Force'`
- When stale processes lock DLLs and `dotnet run` fails repeatedly, spawn the pre-built exe directly: `spawn(path.join(projectRoot, "bin", "Debug", "net10.0", "AppName.exe"), ["--port", port], ...)`
- Screenshot/log path resolution: use `path.resolve(__dirname)` in spec files, NOT `process.cwd()` — Playwright may resolve cwd differently than expected, causing silent file write failures
- Clicking `ListItem` in Ivy lists: extract the visible item title text from the page (e.g., via regex on body text) and use `getByText(name, { exact: true }).dispatchEvent("click")`. Filtering parent divs by `hasText` matches too many ancestors
- `page.goBack()` in Ivy SPA may not reliably restore blade state — prefer re-navigating or keeping blade context

## VideoPlayer Widget

- `<track>` elements inside `<video>` are NOT accessible via Playwright locators (`page.locator('video track')` returns 0 results). Use `page.evaluate()` to query track elements: `document.querySelectorAll('video')[N].querySelectorAll('track')`
- When subtitles are configured, the frontend sets `crossOrigin="anonymous"` on the video element. This means the video source URL MUST support CORS headers, otherwise the video will error out and show an error div instead
- Non-CORS video sources (e.g., w3schools) will fail when subtitles are present. Use CORS-compatible sources like `https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4`
- Track elements have `kind="subtitles"`, `src`, `label`, and `default` (first track is default)
- The `textTracks` API on the video element can also be used to detect subtitle tracks

## Ivy App Construction

- `ViewBase.Build()` is `public override`, NOT `protected override` — using `protected` causes CS0507
- Ivy apps MUST have a parameterless constructor — `AppDescriptor.CreateApp()` uses `Activator.CreateInstance` without DI
- Do NOT use primary constructor injection like `MyApp(IClientProvider client) : ViewBase` — causes `MissingMethodException` at runtime
- Instead, use `var client = UseService<IClientProvider>()` inside the `Build()` method

## SelectInput Multi-Select vs Single-Select Locators

- **Single-select** (Select variant): Uses Radix Select — trigger is `button[role="combobox"]`, options are `[role="option"]`
- **Multi-select** (Select variant): Uses CMDK (Command Menu) in a Popover — trigger is a div with input, options are `[cmdk-item]`
- To open multi-select: `page.getByPlaceholder('placeholder text').click()`
- To select option in multi-select: `page.locator('[cmdk-item]').filter({ hasText: 'Option' }).click()`
- Multi-select popover stays open after selecting — close with `page.keyboard.press('Escape')`
- Visual group separators appear between option groups in the dropdown (Select variant only)
- Group separator count = number of groups - 1

## SelectInput Slider Variant

- `state.ToSelectInput(options).Slider()` renders a discrete range slider using Radix UI `<Slider>` with `role="slider"`
- Use `page.getByRole('slider')` to locate; keyboard interaction with ArrowRight/ArrowLeft (step), Home/End (min/max)
- The slider fires `OnChange` on value commit (not during drag), providing the selected option value
- End labels (first/last option) and current value label are rendered below the track
- The slider shows a numeric index above the thumb (Radix built-in) — this is cosmetic, not the option label
- Does NOT support `selectMany` — logs a warning and falls back to single-select
- Works with string options (`.ToOptions()`), enum options (auto-generated), and supports Disabled/Density

## Ivy Hooks Rules (Enforced by Ivy.Analyser)

- **IVYHOOK005**: ALL Ivy hooks (`UseState`, `UseEffect`, `UseService`, etc.) MUST be called at the very top of the `Build()` method, before any other statements or logic
- Hooks cannot be called conditionally or after value extraction — they must be the first lines in `Build()`
- Correct pattern:
  ```csharp
  public override object? Build() {
      // All hooks first
      var state1 = UseState<int>(() => 0);
      var state2 = UseState<string>(() => "");
      var service = UseService<IClientProvider>();

      // Then other logic
      var value = state1.Value * 2;
      return Layout.Vertical() | ...;
  }
  ```

## Kanban Widget

- Kanban column headers include card counts, e.g., "Todo (2)", "In Progress (3)", "Done (1)" — `getByText("Todo", { exact: true })` won't match. Use regex patterns like `/^Todo/` or `/Todo.*\(\d+\)/`
- `ToKanban()` builder groups items by a selector and renders cards in columns
- Cards are rendered inside individual column containers
- The `group:` parameter (not `path:`) is used in the `[App]` attribute for nav grouping

## Calendar Widget

- Calendar renders using react-big-calendar with toolbar (Today, <, >, Month/Week/Day/Agenda)
- Events display on correct dates with color support (string colors like "Blue", "Green", "Red", "Purple")
- All-day events span across multiple date cells
- `OnEventClick` handler receives the event ID — verify via event log text
- Calendar display modes: `CalendarDisplayMode.Month`, `.Week`, `.Day`, `.Agenda`
- `.ShowToolbar(false)` hides the navigation toolbar

## Card Component

- `new Card(content).Title("X")` renders the title as a `<span>`, NOT as a heading element — use `getByText("X", { exact: true })` to locate card titles, not `getByRole("heading")`
- Card title text can cause strict mode violations if the same substring appears elsewhere — always use `{ exact: true }`
- Note: `new Card(content, header: Text.H3("X"))` renders a heading (see Skyline entry), but `.Title("X")` does not

## Form (.ToForm / .ToDialog / .ToSheet) Patterns

- `.ToForm()` with `[Required]` fields renders labels as "FieldName *" (with asterisk suffix) — `getByText("Code", { exact: true })` won't match "Code *". Use input element locators (`input[type='text']`, `input[type='number']`) instead of label text for asserting form field presence
- `.ToDialog(isOpen, title, submitTitle)` renders a dialog with custom title and submit button text
- `.ToSheet(isOpen, title)` renders a slide-in sheet with Save/Cancel buttons

## ECharts (Bar/Line/Area/Pie Charts) Locators

- Ivy charts use **echarts-for-react** which renders as **SVG** (not canvas) by default — `page.locator('canvas')` will NOT find chart elements
- Locate ECharts instances via: `page.locator('[_echarts_instance_]')` — this attribute is set on the chart container div
- The `onChartReady` callback sets `data-chart-rendered="true"` on the container ONLY when the chart initializes successfully. If ECharts silently fails (e.g., `yAxisIndex` references a non-existent axis), the attribute is still set but the chart area is blank
- For screenshot-based verification of charts, use longer waits (3s+) since chart rendering is async
- **Screenshot path in test specs**: `projectRoot` resolves to the `sample/` dir; screenshots go to `path.resolve(projectRoot, '..', 'screenshots')` (one level up, not two)

## Ivy Component Test Patterns

- `AsQueryable().ToDataTable()` renders using glide-data-grid — a virtualized grid where cells have `role="gridcell"` but may not be "visible" (outside virtual viewport). Use `toBeAttached()` instead of `toBeVisible()` for these cells.
- **glide-data-grid cell clicking**: gridcell elements are hidden behind the canvas overlay. `.click()` and `.click({ force: true })` both fail. Use `.dispatchEvent("click")` to bypass visibility — but note this does NOT trigger Ivy's `OnCellClick` handler (the canvas intercepts real mouse events). Sheet/detail panels triggered by `OnCellClick` cannot be reliably tested via automated clicks on glide-data-grid.
- **`decimal` type in DataTable**: `decimal` columns in `AsQueryable().ToDataTable()` render as `00000000000000000` (zero-padded) instead of formatted numbers. This is a framework/glide-data-grid issue. Workaround: format values as strings in the `.Header()` call.
- `state.ToNumberInput().Variant(NumberInputVariants.Slider)` renders similarly to `ToSliderInput()` — a Radix slider with `role="slider"`.
- `state.ToSelectInput(options).Variant(SelectInputVariants.Toggle)` with string arrays renders radio buttons — use `getByRole("radio", { name: "OptionName" })`.
- `state.ToSelectInput([new Option<T>("Label", Value), ...])` (without Toggle variant) renders as a dropdown with `role="combobox"` — click to open, then `getByRole("option", { name: "Label" }).click()` to select
- `.ToTable()` on anonymous object collections renders as a standard HTML `<table>` — buttons in table cells are locatable via `page.locator("table button")`

## DateRangeInput (ToDateRangeInput) Calendar Popover

- `state.ToDateRangeInput()` renders as a `<button data-slot="calendar">` trigger with a two-month calendar popover and preset shortcuts (Today, Yesterday, Last 7 Days, Last 30 Days, etc.)
- The `data-testid` attribute (from `.TestId()`) is placed directly on the trigger button itself, NOT on a wrapper div. So `page.locator('[data-testid="my-id"]')` IS the button — do NOT look for a child `button` inside it.
- **Date selection in range mode**: After clicking the start date, react-day-picker disables dates before the start. For reliable testing, use the sidebar presets (e.g., `page.getByText('Last 7 Days', { exact: true }).click()`) instead of clicking individual calendar days.
- **Placeholders**: Supports `Placeholder`, `StartPlaceholder`, `EndPlaceholder`. When `StartPlaceholder`/`EndPlaceholder` are set, shows `"{start} - {end}"` format. Falls back to default `"Pick a date range"` for missing sides.
- **Card constructors**: `Card.Default(content)` does NOT exist. Use `new Card().Content(widget)` or `new Card().Header("title").Content(widget)` pattern instead.

## DateInput (ToDateInput) Calendar Popover

- `state.ToDateInput()` renders as a `<button data-slot="calendar">` trigger that opens a Radix Popover with a react-day-picker Calendar
- It is NOT a native `<input type="date">` — do NOT use `input[type="date"]` selectors
- The trigger button shows the formatted date or placeholder text, with a CalendarIcon
- **Calendar navigation**: The calendar caption has a `MonthYearInput` component with two `<input>` fields:
  - Month input: `input[placeholder="M"]` (width w-6)
  - Year input: `input[placeholder="YYYY"]` (width w-10)
  - Fill month number and year, press Enter to navigate to that month/year
- **Day selection**: Day buttons are inside `div[data-slot="calendar"]` (the calendar root, NOT the trigger button). Use `page.locator('div[data-slot="calendar"]').locator("button").filter({ hasText: /^15$/ }).first().click()`
- The popover content renders via Radix Portal at document root — selectors like `input[placeholder="M"]` work globally
- The calendar trigger button selector: `page.locator('button[data-slot="calendar"]').first()`
- After selecting a day, the popover auto-closes and the trigger button shows the formatted date
- `nullable` dates show a clear (X) button when a value is set

- `state.ToSelectInput(options)` WITHOUT `.Variant(SelectInputVariants.Toggle)` renders as a Radix dropdown (not native `<select>`) — click the trigger text to open, then `getByText("Option", { exact: true }).first().click()` to select.
- `page.goto()` with `waitUntil: "networkidle"` hangs on Ivy apps because WebSocket connections keep the network active — use `waitUntil: "domcontentloaded"` instead.
- `UseChrome().UseTabs(preventDuplicates: true)` with a single app auto-opens the tab — no sidebar click needed for navigation.
- `state.ToBoolInput().Label("X")` WITHOUT explicit `.Variant()` renders as `role="checkbox"` (NOT `role="switch"`) — clicking the label text does NOT toggle the checkbox. Use `page.getByRole("checkbox").nth(N)` to click by index order. Note: `getByRole("checkbox")` without `{ name }` DOES work; `getByRole("checkbox", { name: "X" })` does NOT (Playwright can't resolve the accessible name from the Ivy label association).

## CodeBlock and UseEffect Timing

- `new CodeBlock(state.Value, language)` where `state` is populated via `UseEffect` will render with EMPTY `<code>` element on initial page load — the UseEffect hasn't fired yet
- The `<code>` element exists but has no text content; `page.locator("pre code").first().textContent()` returns `""`
- To test CodeBlock content: trigger a state change first (e.g., type in an input, move a slider) to force UseEffect to fire, then wait 500-1000ms before asserting
- Alternative: the app could initialize the state with the default value directly instead of relying on UseEffect

## Enum ToOptions() Display Text

- `typeof(MyEnum).ToOptions()` converts PascalCase enum names to spaced display text: `V1` → "V 1", `V3` → "V 3", `V4` → "V 4", etc.
- This means `getByText("V1")` will NOT match — must use `getByText("V 1", { exact: true })` with the space
- Same pattern applies to all enum values where PascalCase splitting inserts spaces between uppercase letters and digits

## Layout.Grid Column Count Bug

- `Layout.Grid(3).Gap(4)` renders the column count "3" as visible text content at the top of the grid container
- The grid items appear stacked vertically (single column) instead of in a 3-column layout
- This is a framework rendering bug — `Layout.Grid(N)` does not produce a proper CSS grid with N columns
- Workaround: use `Layout.Horizontal()` with equal-width children, or `Layout.Grid().Columns(N)` (see ReversiForge.AI entry — `.Columns(8)` worked)

## Camera/Media Testing

- Chromium's `--use-fake-device-for-media-stream` and `--use-fake-ui-for-media-stream` flags produce video streams with `videoWidth=0` and `videoHeight=0` in headless mode
- This means `canvas.toBlob()` after drawing from a 0x0 video produces no valid blob, preventing actual file uploads
- `canvas.captureStream()` in `addInitScript` as a getUserMedia mock also doesn't produce valid dimensions
- The CameraInput widget's state machine (idle -> active -> captured) CAN be tested: clicking the placeholder starts the camera, clicking Capture shows the Retake button
- Upload-dependent assertions (e.g., file upload status changes on the backend) cannot be verified with fake cameras — test the UI state transitions instead
- Set Playwright config: `permissions: ['camera']` and `launchOptions.args: ['--use-fake-ui-for-media-stream', '--use-fake-device-for-media-stream']`

## Ivy Upload/FileUpload Patterns

- `UseUpload(MemoryStreamUploadHandler.Create(photoState), defaultContentType: "image/png")` creates an upload context
- `FileUpload<byte[]>` has: FileName, Length, Content, Status (Pending/Loading/Aborted/Failed/Finished)
- `FileUploadStatus` values: Pending, Aborted, Loading, Failed, Finished (NO `InProgress`)
- `Callout.Success("message")` is the correct static factory — `new Callout("msg", CalloutVariant.Success)` uses named params: `new Callout(description: "msg", variant: CalloutVariant.Success)`

## Frontend Build Issues

### Unknown Component Type Errors

When an Ivy app displays "Unknown component type: Ivy.WidgetName" instead of rendering a widget:

1. **Check frontend build status**: The widget may have TypeScript compilation errors preventing the frontend bundle from building
2. **Rebuild frontend**: Run `cd D:\Repos\_Ivy\Ivy-Framework\src\frontend && vp build` to see compilation errors
3. **Common causes**:
   - Unused variables in the TypeScript widget implementation
   - Type mismatches (e.g., XAxisProps[] not compatible with expected interface)
   - Missing imports or exports in the frontend code
   - Widget not registered in the frontend widget registry
4. **Verification**: A successful frontend build is required before testing any new or modified widgets
5. **Note**: The C# backend can compile successfully while the TypeScript frontend fails, causing this error

This is different from `[ExternalWidget]` "Unknown component type" errors which are Framework bundle serving issues.

## File Dialog Testing (UseFileDialog/UseSaveDialog/UseFolderDialog)

### Disabling File System Access API for Playwright
Chromium supports `showOpenFilePicker` / `showSaveFilePicker` / `showDirectoryPicker`, which Playwright CANNOT intercept. To use the fallback `<input type="file">` path that Playwright CAN intercept via `filechooser` events:
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

### Folder Dialog with webkitdirectory
Playwright's `fileChooser.setFiles()` for `<input webkitdirectory>` requires passing a **directory path**, not individual file paths.

### Save Dialog Fallback
In headless mode with File System Access API disabled, the save dialog uses `<a download>` fallback. Use `page.waitForEvent('download')` to capture it.

## Run History

### 2026-03-17 — UseFileDialog/UseSaveDialog/UseFolderDialog Hooks
- File dialog hooks with browser API detection (File System Access API → fallback input)
- **BUG FOUND**: `FileDialogMode.Upload` (enum value 0) stripped by WidgetSerializer → `mode` undefined on frontend → upload silently skipped. Fixed with `mode = 'Upload'` default in `FileDialogWidget.tsx`
- Must disable File System Access API via `addInitScript` for Playwright to intercept file selection
- Folder dialog `<input webkitdirectory>` requires passing directory path to `fileChooser.setFiles()`
- Save dialog fallback uses `<a download>` → capture with `page.waitForEvent('download')`
- 6 tests, 3 fix rounds (TestId on Text, locator fixes, mode serializer bug), all passed, logs clean

### 2026-03-13 — CameraInput Widget
- Camera input widget with idle/active/captured states, upload via MemoryStreamUploadHandler
- MUST rebuild frontend (`cd src/frontend && vp build`) when testing commits with new frontend widgets — otherwise shows "Unknown component type: Ivy.CameraInput"
- `TextBuilder` does NOT support `.TestId()` — use `Badge` or `Card` wrapper for testable text
- `Icons.AlertTriangle` renamed to `Icons.TriangleAlert` in Lucide
- Fake camera in headless Chromium produces 0x0 video — upload tests must verify UI state machine instead of backend state
- 21 tests, 5 fix rounds (frontend rebuild, API fixes, fake camera workarounds), all passed

### 2026-03-13 — Test.CSSGradientTextGenerator
- Gradient text generator with text input, color pickers, angle/font-size sliders, live preview, and CSS code output
- `new CodeBlock(cssCode.Value)` where `cssCode` is set via `UseEffect` renders EMPTY on initial load — must trigger state change before asserting code content
- `getByText("Preview")` matched both "Preview Text" label and "Preview" heading — use `getByRole("heading", { name: "Preview" })`
- `getByText("Gradient Text")` matched both title "CSS Gradient Text Generator" and preview div — use `{ exact: true }`
- Html inline styles (gradient background, font-size) not rendered in preview (confirmed known limitation)
- 8 tests, 2 fix rounds (strict mode + CodeBlock timing), all passed, logs clean

### 2026-03-13 — Test.LifeInWeeksVisualizer
- 2-step flow: birth date input → life visualization with stats and weeks grid
- `Layout.Grid(3)` renders "3" as text and produces single-column layout instead of 3-column grid
- `new Html(gridHtml)` with CSS grid + inline styles is completely invisible — only `<span>` text content visible (confirmed known Html limitation)
- `UseDefaultApp()` means no Chrome sidebar — navigate directly to `/<app-id>?shell=false`
- "Weeks lived" text appears in both stat label and Html grid legend — use `.first()` for strict mode
- 7 tests, 1 fix round (strict mode violation), all passed, logs clean

### 2026-03-13 — Test.SimpleCRM
- CRM app with TabsLayout (Dashboard, Contacts, Deals, Activities) — tabs navigable via `getByText("TabName", { exact: true }).first().click()`
- `decimal` column in DataTable renders as `00000000000000000` — framework bug, not fixable in project code without changing column to string
- glide-data-grid cell `.dispatchEvent("click")` passes test but does NOT trigger Ivy `OnCellClick` — sheet panels cannot be validated
- MetricView KPI cards render correctly with icons and formatted values
- Bar/Pie charts render within Card components — chart titles via `.Title()` are spans, not headings
- 9 tests, 3 fix rounds (gridcell click visibility issues), all passed

### 2026-03-10 — Tempus.AgeCalc
- `state.ToDateInput()` renders as a `<button data-slot="calendar">` popover trigger, NOT a native date input
- Calendar month/year navigation via `input[placeholder="M"]` and `input[placeholder="YYYY"]` + Enter
- Day buttons inside `div[data-slot="calendar"]` (calendar root) — filter by exact day number regex
- `Card.Title("X")` does NOT render as a heading — `getByRole("heading")` fails; use `getByText("X").last()` to skip sidebar/tab matches
- `getByText("Birthdate")` matched 3 elements (label, placeholder "Select your birthdate", message "Enter your birthdate...") — use `{ exact: true }`

### 2026-03-10 — ReversiForge.AI
- Single-app Chrome tabs with `preventDuplicates: true` auto-opens the app — no sidebar navigation needed
- `page.goto` with `waitUntil: "networkidle"` hangs indefinitely on Ivy apps (WebSocket keeps network active) — always use `"domcontentloaded"`
- `ToSelectInput(enumOptions)` without `.Variant(Toggle)` renders as Radix dropdown, not native `<select>` or radio buttons
- `ToBoolInput()` checkbox buttons: clicking label text still does NOT toggle — must click `[role="checkbox"]` button directly (confirmed again)
- Grid of buttons with `Layout.Grid().Columns(8)` renders fine — empty ghost buttons are invisible (no background/border), only occupied cells visible

### 2026-03-10 — Polyglot.TypeTrainer
- `new Html(...)` component renders raw HTML content but it appears invisible — the word display area is completely blank in all screenshots. Likely renders in an iframe without CSS custom property inheritance (`var(--foreground)` etc. resolve to nothing). Hardcoded colors or native Ivy components would fix this.
- `SelectInput` with options that share a prefix (e.g., "Deutsch" and "Deutsch (Schweiz)", or "5 seconds" and "15 seconds") cause strict mode violations — always use `{ exact: true }` with `getByText()`
- `TextInput` onChange fires with the full new value, not character-by-character — `fill("test")` sends value "test" in one event. The timer start condition `e.Value.Length == 1 && userInput.Value.Length == 0` only triggers with single-char input, so use `keyboard.type("t")` not `fill("t")` for reliability
- `UseEffect` with `async` lambda containing `while(true) + Task.Delay(1000)` works as a polling timer in Ivy

### 2026-03-10 — CineStream.Converter
- `AsQueryable().ToDataTable()` uses glide-data-grid (virtualized) — cells are `role="gridcell"` but may not pass `toBeVisible()` check; use `toBeAttached()` instead
- DataTable with `.Config(c => { c.AllowFiltering = true; c.ShowSearch = true; })` shows filter icon button and search — but these may be above/below the grid viewport
- `state.ToNumberInput().Variant(NumberInputVariants.Slider)` renders as Radix slider, same as `ToSliderInput()`
- Multiple `test.describe` blocks in one file each trigger their own `beforeAll` — caused server re-spawn failure due to DLL locks from first server instance
- Solution: use top-level `test()` (outside `describe`) for tests that need to share the same server lifecycle

### 2026-03-09 20:46 — Parsely.Markflow
- CodeMirror-based code inputs (from `state.ToCodeInput()`) use `.cm-content[contenteditable='true']` as the editable locator
- **IMPORTANT**: Do NOT use `keyboard.type()` or `keyboard.insertText()` for CodeMirror editors — `type()` triggers auto-brackets/auto-complete that corrupts structured input, and `insertText()` does not trigger CodeMirror's change events so Ivy state bindings never update (the server-side state stays stale). Instead, use clipboard paste: `page.evaluate(async (t) => { await navigator.clipboard.writeText(t); }, text)` then `page.keyboard.press("Control+V")`. Requires `permissions: ["clipboard-read", "clipboard-write"]` in playwright config.
- For CodeMirror inputs, the second editor (read-only/disabled output) is accessed via `.cm-content` with `.nth(1)` index selector
- Markdig's `ToHtml` with `UseAdvancedExtensions()` generates heading IDs like `id="hello-world"` — useful for asserting exact HTML output in tests
- `Button.Disabled()` with a reactive condition correctly toggles the HTML `disabled` attribute, testable via `toBeDisabled()`/`toBeEnabled()`
- Multi-line input in CodeMirror: use clipboard paste (see above) — the paste approach handles newlines correctly without needing line-by-line entry
- `Button.Variant(ButtonVariant.Ghost)` and `.Primary()` render as standard `role="button"` elements — no special locator strategy needed
- After Clear button resets state, a `waitForTimeout(1000)` is needed before asserting the UI has updated

### 2026-03-10 — ByteForge.UrlCraft
- `state.ToSelectInput().Variant(SelectInputVariants.Toggle)` renders enum options as radio buttons with `role="radio"` and `aria-label` matching the enum value name
- Toast messages from `client.Toast()` produce duplicate text nodes (visible div + aria-live span) — always use `{ exact: true }` with `getByText` for toast assertions
- `state.ToTextareaInput()` renders as `<textarea>`, output via `new TextInput().Variant(TextInputVariants.Textarea).Disabled()` also renders as `<textarea>` — use `.first()` / `.nth(1)` to distinguish

### 2026-03-10 — Nexus.PasswordForge2
- `state.ToSliderInput()` renders Radix slider (`role="slider"`), not native range input — interact via keyboard: `focus()` then `ArrowRight`/`ArrowLeft` (step by 1), `Home`/`End` for min/max
- `state.ToBoolInput().Variant(BoolInputVariants.Checkbox)` renders `<button role="checkbox">` but `getByRole("checkbox", { name })` and `getByLabel()` fail to locate them — the label `<label for="id">` association doesn't work for Playwright's accessible name resolution; use `page.locator('[role="checkbox"]').nth(N)` by DOM order
- Clicking the checkbox label text via `getByText("Label").click()` also does NOT toggle the checkbox — must click the `<button>` element directly
- `Progress` component with dynamic color shows correctly as a colored progress bar

### 2026-03-10 — Skyline.RunwayCalculator
- `new Card(content, header: Text.H3("X"))` card headers cause strict mode violations with `getByText("X")` when page also contains "Total X:" text — use `getByRole("heading", { name: "X" })` instead
- `state.ToMoneyInput()` renders formatted currency inputs (e.g., "$850,000") — values visible as text with `$` and commas
- `.ToDetails()` converts anonymous object properties to readable labels (PascalCase → spaced) and displays as key-value pairs
- Stale dotnet processes from previous test runs can lock DLLs and prevent rebuilds — kill processes between runs; `taskkill /f` may fail, use `wmic process where "name='X.exe'" delete` as fallback

### 2026-03-09 21:01 — Chromatica.Palettes2
- `Layout.TopCenter()` serves as a top-aligned centered layout container; the app renders at root `/`
- `state.ToTextInput()` renders as a standard `role="textbox"` element, locatable via `page.getByRole("textbox")` and compatible with `.fill()`
- `.Primary()` and `.Outline()` button variants both render as standard `role="button"` elements; `.Disabled()` with reactive boolean correctly toggles HTML `disabled` attribute
- Suggested prompt buttons that both set state and trigger an async action populate the input with lowercase text — assert `.toHaveValue("luxury spa")` not `"Luxury spa"`
- For API-dependent tests, use `Promise.race()` with both success and error element waiters (30s timeout) to handle cases where API keys may not be configured
- `Callout.Error()` renders error text locatable via `page.getByText(/Failed to generate/i)` — no special selector needed
- `new Progress().Goal()` renders a loading indicator; the loading state is brief and may require immediate screenshot capture after click

### 2026-03-10 — Numerix.Statistics
- ValueTuple syntax in `Build()` crashes with `InvalidOperationException: uninitialized WidgetBase Id` — switched to `|` operator chaining
- `.WithField().Label("Numbers")` on textarea also crashes in tuple context — replaced with separate `Text.Label("Numbers")`
- `.ToDetails()` on anonymous object renders clean key-value pair table in a card
- Spawning pre-built exe (`bin/Debug/net10.0/AppName.exe`) avoids `dotnet run` rebuild DLL lock issues
- `Text.Label()` works as a standalone label element outside of `.WithField()`

### 2026-03-10 — Ivy.TextAnnotate (Widget Library)
- Widget library projects have `.samples/` subfolder with the runnable app — tests must use `samplesDir` not `projectRoot` for `dotnet run`
- `[ExternalWidget]` components show "Unknown component type: Namespace.WidgetName" when the embedded JS bundle isn't served to the browser — this is a Framework issue, not a test issue
- Ivy NuGet 1.2.17 is missing `Layout`, `Text`, `ButtonVariant`, `ChromeSettings`, `Server.UseCulture()` — these APIs only exist in source. Use `ProjectReference` to Ivy source if samples require them.
- Generated sample code may use stale API patterns: `Button.Style()` → `.Variant()`, `ButtonStyle` → `ButtonVariant`, `HandleClick` → `.OnClick()`, `HandleChange(state.Set)` → `HandleChange(v => state.Set(v))` (Set returns T, not void)
- Without `UseChrome()`, Ivy SPA doesn't support URL-based app routing — `/app-name` shows the default app. Must enable Chrome for multi-app navigation.
- `powershell -Command "Get-Process -Name 'X' -ErrorAction SilentlyContinue | Stop-Process -Force"` is more reliable than `taskkill /f /im` for killing stale processes on Windows

### 2026-03-10 — Calc.Desktop
- Single-app Chrome auto-opens the only app — clicking sidebar nav item is unnecessary and can cause timeout
- `getByRole("button", { name: "C" })` conflicts with "Calculator" sidebar button — use `{ exact: true }` for short button names
- `Layout.Grid().Columns(4).Gap(4).Width(Size.Units(280))` renders buttons with excessive horizontal spacing — grid cells expand to fill container width
- `GridRowSpan(2)` on a button doesn't produce a visually distinct two-row button in the rendered output
- `http.get()` is more reliable than `fetch()` for server health checks in Playwright test setup

### 2026-03-10 — Nexus.DecisionMatrix
- Card `.Title("X")` does NOT render as HTML heading — `getByRole("heading")` fails; use `getByText("X", { exact: true }).first()` instead
- Badge `.OnClick()` works for removable items — clicking badge text triggers the click handler
- `NumberInput<int>` renders as a standard text input with `placeholder` attribute — locatable via `input[placeholder="1-10"]`
- `.ToNumberInput()` on state renders similarly with placeholder support
- `.WithField().Label("X")` on NumberInput does NOT create an HTML `<label for="">` association — `getByLabel("X")` fails. Use `page.locator("input").first()` or index-based `page.locator("input").nth(N)` instead
- App had no runtime errors — clean first run, only test locator fixes needed

### 2026-03-10 — Nexus.HumanCore
- **Chrome tabs start with NO tab open** — `UseChrome(new ChromeSettings().UseTabs())` renders a sidebar but the content area is blank on initial load. Tests MUST click a sidebar item (e.g., `page.getByText("Dashboard").first().click()`) before asserting content
- Navigation is via sidebar nav items, NOT `role="tab"` — use `page.getByText("AppName").first().click()`. **BUT** sidebar items may be outside the viewport in headless mode — `click()` and `click({ force: true })` both fail with "Element is outside of the viewport". Use `dispatchEvent("click")` instead, or use sidebar search first to filter then `dispatchEvent("click")`
- `Icons.Plus.ToButton().Ghost().Tooltip("Create X").ToTrigger(...)` — the tooltip text does NOT become the button's accessible name in Playwright. `getByRole("button", { name: /Create X/i })` fails. Use `page.locator("button").filter({ has: page.locator("svg") }).first()` instead
- Ivy `ListItem` does not have a `data-ivy-list-item` attribute — fallback selectors are needed to click list items in tests
- CRUD apps with `UseBlades()` pattern: list blade on left, detail blade opens on right when item clicked — the blade push/pop pattern
- `MetricView` renders as cards with title, icon, value, and trend percentage — fully functional with seeded data
- Chart views (LineChart, PieChart, BarChart, AreaChart) wrapped in Card with Skeleton loading — charts may be empty for short date ranges with no matching data
- `HeaderLayout(header, body)` pattern used for Dashboard — header contains the date range toggle, body contains the content
- Lists sorted alphabetically may push expected items below viewport — assert items visible at the TOP of the sorted list, not arbitrary ones

### 2026-03-10 — Nexus.CrmPortal
- Complex CRUD app with 4 apps (Dashboard, Contacts, Deals, Activities), Chrome sidebar + tabs, blade navigation, SQLite EF Core
- All 15 tests passed after 1 fix round (test assertion corrections only, no project fixes needed)
- **List sort order matters**: `UseQuery` with `OrderByDescending(e => e.CreatedAt)` means newest items appear first — don't assume alphabetical sort when asserting visible list items
- `Icons.Plus.ToButton().Ghost().Tooltip("Create X").ToTrigger(...)` — tooltip text does NOT become accessible name; use `page.locator("button").filter({ has: page.locator("svg") }).first()` (confirmed from HumanCore)
- Backend parsing error `Must specify valid information for parsing in the string` appeared once — non-crashing, likely null value in metric calculation
- `SelectInput` with async options (entity dropdowns in create/edit forms) works — opens dialog/sheet with form fields
- Chrome sidebar with multiple apps starts blank — must click sidebar item to open app tab

### 2026-03-10 — Clockwise.MeetingCost
- Primary constructor injection `MyApp(IClientProvider client)` crashes with `MissingMethodException` — Ivy uses `Activator.CreateInstance` (parameterless). Fix: use `UseService<IClientProvider>()` in `Build()`
- Card `.Title("X")` renders `<span>` not heading — use `getByText("X", { exact: true })`, confirmed same as Nexus.DecisionMatrix finding
- `ToSelectInput([new Option<T>()])` (no Toggle variant) renders `role="combobox"` dropdown — open with click, select via `getByRole("option", { name })`
- `.ToTable()` renders standard HTML `<table>` with buttons in cells locatable via `page.locator("table button")`
- `UseEffect` with `System.Threading.Timer` works for real-time updates (1-second polling)

### 2026-03-10 — PawByte.Tamadog
- Simple single-app project with Chrome tabs — clean pass, no runtime errors, no project fixes needed
- Sidebar search + `dispatchEvent("click")` pattern works reliably for navigating to apps in Chrome tabs mode when items are outside viewport
- `Progress` component with `.Color()` renders correctly for stat bars
- `UseEffect` with `System.Threading.Timer` works for periodic stat decay — no issues observed

### 2026-03-10 — Flowcraft.MermaidStudio
- Clean first-pass run — all 11 tests passed, no project fixes needed
- `new Markdown(markdownPreview)` with mermaid code blocks renders SVG diagrams via client-side Mermaid.js — preview updates reactively when code state changes
- `UseDownload(factory, mimeType, fileName)` returns a nullable URL — renders as `role="link"` when non-null, locatable via `getByRole("link", { name: /Download/i })`
- `SelectInput<string>` with `.Placeholder()` and `options.ToOptions()` renders as Radix dropdown — same pattern as other `ToSelectInput()` without Toggle variant

### 2026-03-10 — Patternix.RegexLens
- `ToBoolInput().Label("X")` without explicit Variant renders as `role="checkbox"`, NOT `role="switch"` — clicking label text does NOT toggle; must click the `[role="checkbox"]` element directly
- Dynamic checkbox/switch detection pattern: check `[role="checkbox"]` count, then `[role="switch"]`, then fallback `dispatchEvent("click")` — robust across all BoolInput variants
- `new Html(...)` with inline styles (background-color, color) renders but highlighting is invisible — likely iframe CSS isolation issue; the yellow background match highlighting doesn't display
- `Expandable` component works as expected for collapsible sections — click text to toggle
- `Box` with `.Padding()`, `.BorderThickness()`, `.BorderStyle()` renders bordered containers correctly for match items
- Clean run: 10 tests passed, no project fixes needed, no runtime errors, logs clean

### 2026-03-10 — HoofTrack.StableVault
- CRUD app with `UseBlades()` and Chrome tabs — clean pass, no runtime errors, no project fixes needed
- `Card.Title("Horse Details")` renders as text, NOT `role="heading"` — use `getByText()` not `getByRole("heading")`
- `ListItem` click by parent div `filter({ hasText })` fails — matches too many ancestors. Must extract specific title text and use `getByText(name, { exact: true }).dispatchEvent("click")`
- Screenshot path must use `path.resolve(__dirname)` not `process.cwd()` — the latter silently fails to write files in Playwright context
- `safeClick` pattern: try `.click({ timeout: 3000 })`, catch and fall back to `.dispatchEvent("click")` — handles both in-viewport and out-of-viewport elements
- `page.goBack()` after blade push doesn't restore previous blade — SPA routing doesn't map 1:1 to browser history for blade navigation

### 2026-03-10 — Folio.TextMiner
- File upload app with `UseUpload(MemoryStreamUploadHandler.Create(state))` — `page.locator('input[type="file"]').setInputFiles(path)` works for Playwright file upload testing
- `ToFileInput(upload).Placeholder(...)` renders a dashed dropzone with the placeholder text
- `ToCodeInput().Language(Languages.Text).Disabled()` renders as a read-only CodeMirror editor — `.cm-content` locator works
- Clean pass, no runtime errors, no project fixes needed

### 2026-03-10 — Pinnacle.StockGrid
- `YahooFinanceApi` v2.3.3 NuGet package returns 401 Unauthorized from Yahoo Finance v7 download API — package likely outdated/broken
- Unhandled async exceptions in button click handlers (`async () => await FetchStockData()`) crash the server process with `IsTerminating: True` — always wrap external API calls in try/catch, not just try/finally
- Fix: added `catch (Exception ex)` with `errorMessage` state + `Callout.Error()` display — app stays stable on API failure
- When external APIs are expected to fail, use `Promise.race()` pattern in tests to detect either data load OR error callout, with `test.skip()` for data-dependent tests

### 2026-03-10 — Archiva.ZipForge
- `FileUpload<byte[]>.ToTable()` with `.Remove(e => e.Content)` throws `KeyNotFoundException` — `byte[]` properties are not registered in `TableBuilder`'s field dictionary. Fix: remove the `.Remove(e => e.Content)` call
- `ToFileInput(upload).Placeholder(...)` renders a dashed dropzone with hidden `input[type="file"]` — `page.locator('input[type="file"]').setInputFiles({name, mimeType, buffer})` works for Playwright testing (confirmed from Folio.TextMiner pattern)
- `FileUpload<T>` exposes `FileName`, `ContentType`, `Length`, `Progress`, `Id`, `Content` — `.ToTable()` creates columns for all simple-type properties; `byte[]` Content is excluded from the field dictionary but other fields like ContentType and the upload Status enum still show
- 8 tests passed after 1 fix round (project fix only)

### 2026-03-10 — Pixelforge.AsciiCraft
- `Layout.Tabs(new Tab("Name", view).Icon(Icons.X), ...)` renders tab UI — tabs are clickable via `getByText("Tab Name").first().click()`
- `UseDownload()` with sync factory renders as `role="link"` (anchor tag with href), not `role="button"` — use `getByRole("link", { name: /Download/i })`
- `new CodeBlock(content, Languages.Text).ShowCopyButton()` renders with a copy icon button and `<code>` element for the content
- To generate valid PNG images for Playwright tests: use `zlib.deflateSync()` on raw pixel data (filter byte 0 + RGB per row), then construct PNG chunks (IHDR, IDAT, IEND) with proper CRC32 checksums. Invalid/minimal base64 PNGs will cause `InvalidImageContentException`
- After `dotnet build` + code changes, `dotnet run` may need a rebuild that exceeds 30s — set `beforeAll` timeout to 120s with `testInfo.setTimeout(120000)` for safety
- 12 tests passed after 1 fix round (project fix: added try-catch for corrupt image loading in AsciiArtConverter)

### 2026-03-11 — Meridian.StockGrid
- `AsQueryable().ToDataTable()` with empty data renders column headers as `role="columnheader"` elements but they are "hidden" (glide-data-grid virtual rendering) — use `toBeAttached()` not `toBeVisible()` for column headers too, not just gridcells
- When `StockDataService` catches all per-ticker exceptions and returns empty list, `UseQuery` resolves with empty data (not null, not error) — app enters "data loaded" path with empty metrics showing "No data available"
- YahooFinanceApi v2.3.3 confirmed broken again (same as Pinnacle.StockGrid) — all tickers fail silently
- No project fixes needed — app handles empty data gracefully without crashes
- 8 tests passed after 1 fix round (test fix only: column header visibility assertion)

### 2026-03-11 — StockFlow.Inventory
- Complex CRUD app with 6 apps (Dashboard, Categories, Products, Orders, Suppliers, Warehouses), Chrome sidebar + tabs, blade navigation, SQLite EF Core with Bogus seeder
- **`ToAsyncSelectInput` with nullable state types**: When form property is `int?` (nullable), the search/lookup delegates MUST use `Option<int?>` (not `Option<int>`). Using `Option<int>` causes `InvalidCastException: Null object cannot be converted to a value type` at runtime. The lookup delegate must also handle null input: `if (id == null) return null;`
- `data-testid="list-item"` does NOT exist in Ivy list rendering — click list items by visible text content using `getByText(name, { exact: true }).first().dispatchEvent("click")`
- Regex-based text extraction from `page.textContent("body")` for supplier names is fragile — regex can match too much text. Prefer regex locators like `page.locator("text=/\\w+ Inc$/").first()` for pattern-matching visible elements
- Chrome sidebar with multiple apps starts blank (no auto-open) — confirmed same as HumanCore/CrmPortal pattern
- `HeaderLayout(header, body)` pattern used in Dashboard — header contains date range toggle
- Dashboard `SelectInput<DateRange>` with `Variant(SelectInputVariant.Toggle)` renders as radio buttons — same Toggle variant pattern
- All 15 tests passed after 1 project fix round (3 files fixed: OrderCreateDialog, OrderItemCreateDialog, OrderShipmentCreateDialog)

### 2026-03-11 — Meridian.ClockFace
- `new Html(clockFaceHtml)` with CSS custom properties (`var(--foreground)`, `var(--border)`, etc.) renders completely invisible in the iframe — confirmed same pattern as Polyglot.TypeTrainer and Patternix.RegexLens
- **Real-time state updates via `UseEffect` + `System.Threading.Timer` are NOT reliably observable in Playwright**: `page.locator("h3").textContent()` polled every 500ms for 5+ seconds returned the same value despite the timer updating state every 1000ms. Screenshots confirm the time displays correctly, so it's a Playwright DOM snapshot timing issue, not an app bug. Avoid asserting real-time clock/timer updates in tests — instead verify format, timezone correctness, and static state.
- `ToSelectInput(options)` without Toggle variant renders as dropdown with `role="option"` — `getByRole("option", { name: "Tokyo" })` works for selection (confirmed again)
- `ToSwitchInput().Label(running.Value ? "Running" : "Stopped")` — label text changes reactively with state, testable via `getByText("Stopped")`
- Clean run: 12 tests passed, no project fixes, no runtime errors, logs clean

### 2026-03-11 — NexTask.TodoSync
- CRUD Todo app with UseBlades, SQLite EF Core, Chrome tabs (single app, auto-opens)
- **`WithConfirm` dialog uses "Ok" as the confirm button text** — NOT "Continue", "Confirm", or "Delete". Use `getByRole("button", { name: "Ok" })` to click the confirm button
- Form `.ToDialog()` renders field labels (e.g., "Title *") but `getByText("Title", { exact: true })` may fail — use input element locators (`input[type='text']`) instead for reliability
- `.ToDetails().Multiline(e => e.Description).RemoveEmpty().Builder(e => e.Id, e => e.CopyToClipboard())` renders clean key-value pairs with copy button on Id
- `ListItem` with `onClick` and `tag` — click items via `getByText(title, { exact: true }).first().dispatchEvent("click")` (confirmed pattern)
- Clean run: 12 tests passed after 2 test-only fix rounds, no project fixes, no runtime errors, logs clean

### 2026-03-11 — LinguaFlow.Translator
- Single-app translator using GTranslate NuGet for Google Translate, Chrome tabs (single app, auto-opens)
- `UseEffect` with async translation triggered reactively by inputText and selectedLanguage states — translation happens automatically on state change, no submit button needed
- `ToSelectInput(languageOptions)` without Toggle variant renders as Radix dropdown with 100+ language options — confirmed `getByRole("option", { name: "French", exact: true })` needed because "French" and "French (Canada)" both exist (same pattern as Polyglot.TypeTrainer prefix collisions)
- `Layout.Vertical().Background(Colors.White)` for text boxes inside cards creates low-contrast containers — white-on-white blends with card surface
- Clean run: 12 tests passed after 1 test-only fix round, no project fixes, no runtime errors, logs clean

### 2026-03-11 — GridQuest.Pathfinder
- `UseState<T>(T?)` vs `UseState<T>(Func<T>)` is ambiguous when T is a nullable reference type (e.g., `CellType[][]`, `string?`) — fix by using explicit typed variables: `CellType[][] val = ...; UseState(val);` or switch to non-nullable types (e.g., `string` with `string.IsNullOrEmpty()`)
- `NumberInput<int>.Variant(NumberInputVariant.Slider)` renders identical to `ToSliderInput()` — Radix slider with `role="slider"`, keyboard Home/End/ArrowRight/ArrowLeft work (confirmed)
- `Box` with `.Color()`, `.Size()`, `.BorderThickness()`, `.BorderColor()` renders colored div cells — no special selectors needed, just visual
- Clean first-pass run: 14 tests passed, 1 project fix (UseState build ambiguity), no runtime errors, logs clean

### 2026-03-12 — CourtVision.Analytics
- NBA statistics dashboard with data table (glide-data-grid), position/college dropdown filters, age/salary slider filters, and bar charts
- **CSV with decimal numbers**: The NBA CSV from geeksforgeeks uses `25.0`, `7730337.0` notation. C# `int.TryParse("25.0")` returns false — must use `double.TryParse()` + cast to int for CSV numeric parsing
- Filter boundary bug: `>` / `<` (strict inequality) excluded boundary values (age 18/50, salary 0). Changed to `>=` / `<=`
- `ToSelectInput(options).Searchable()` renders dropdown with search input — works correctly for long option lists (colleges)
- `.ToBarChart().Dimension().Measure().Toolbox()` renders ECharts bar charts with toolbox icons (save, switch chart type)
- 10 tests passed after 2 project fix rounds (ParseInt decimal parsing + filter inequality), no runtime errors, logs clean

### 2026-03-12 — Meridian.ProductVault
- Standard CRUD app with UseBlades, Chrome tabs (single app, auto-opens), SQLite EF Core, Bogus seeder (100 products)
- `.ToForm()` renders field labels with `*` suffix for `[Required]` fields (e.g., "Code *") — `getByText("Code", { exact: true })` fails; use `input[type='text']` locators instead
- `[Required]` on non-nullable `int Quantity` causes value 0 to fail validation (standard .NET DataAnnotations behavior) — not an Ivy bug
- `.ToDetails().RemoveEmpty().Builder(e => e.Id, e => e.CopyToClipboard())` renders clean key-value pairs with copy button on Id field
- `ProductEditSheet` uses `.ToForm().Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt)` — correctly hides non-editable fields
- Clean run: 12 tests passed after 1 test-only fix round, no project fixes, no runtime errors, logs clean

### 2026-03-12 — Test.PomodoroTimer
- `System.Timers.Timer` in `UseEffect` callback: timer `Elapsed` events can fire after `isRunning` state is set to false, because `UseEffect` disposal has a race condition with pending callbacks. Always add `if (!isRunning.Value) return;` guard inside `Elapsed` handler
- `.WithField().Label("X")` on `NumberInput` does NOT create HTML `<label for="">` — `getByLabel("X")` and `getByRole("spinbutton")` both fail. Use `page.locator("input").first()` or `.nth(N)` by index
- Pause/resume timing tests: after clicking Pause, wait 3+ seconds for timer disposal to settle before capturing "paused time" for comparison
- `Expandable` renders as a clickable header with chevron — `page.getByText("Settings").click()` expands it
- 6 tests passed after 1 project fix (timer guard in Elapsed callback), 4 test fix rounds, logs clean

### 2026-03-12 — Test.DataVisualization
- `DataTableBuilder.Header(r => r.Values[colIndex], columns[i])` crashes with `ArgumentException: Invalid expression` — `GetNameFromMemberExpression` (Utils.cs:467) only handles simple member access, not indexer/array expressions
- `DataTableBuilder` does NOT have a `.Remove()` method (unlike `FormBuilder` and `TableBuilder`) — `builder.Remove(expressions)` resolves to `CollectionExtensions.Remove<TKey,TValue>` and fails to compile
- Workaround for dynamic-column DataTable: replace `ToDataTable()` with manual `Layout.Horizontal/Vertical` table using `Text.Label` headers and `Text.P` cells — loses virtualization/row-actions but avoids the API limitation
- Alternative DynRow approach (record with C0-C19 properties + `Header()` rename) compiles and `Header()` works, but can't hide unused columns without `Remove()`, making it impractical
- `FileUpload<byte[]>` with `MemoryStreamUploadHandler` + `UseEffect` on fileState correctly triggers parsing when `Status == FileUploadStatus.Finished`
- `Button.Url(downloadUrl)` with `UseDownload()` renders as a standard button (not `role="link"`) — locatable via `getByRole("button", { name: /Export CSV/i })` or `getByText("Export CSV")`
- `file.setInputFiles({ name, mimeType, buffer })` works for programmatic file upload in Playwright — buffer is `Buffer.from(csvString, "utf-8")`
- 11 tests passed after 1 project fix round (replaced DataTable with manual layout)

### 2026-03-12 — Test.UUIDGenerator2
- `typeof(UuidVersion).ToOptions()` converts enum names with PascalCase splitting: `V1` → "V 1", `V4` → "V 4" — `getByText("V1")` fails, must use `getByText("V 1", { exact: true })`
- `ToSelectInput(options)` without Toggle: `getByRole("combobox").first().click()` opens dropdown, then `getByText("Option Text", { exact: true }).first().click()` to select — `getByRole("option")` does NOT work for Radix dropdown items (confirmed)
- UUID V1/V6/V7 generation with `SwapGuidEndianness` byte-swaps bytes 6-7 containing the version nibble — version identifier appears in non-standard position in string representation (not an app crash, just RFC non-compliance)
- `.ToTable()` on `List<UuidEntry>` renders standard HTML table with copy icon buttons per row — `.Builder(u => u.Uuid, f => f.CopyToClipboard())` works correctly
- `UseDownload()` for TXT/JSON/CSV export renders as `Button.Url()` links — visible and functional
- Clean run: 13 tests passed after test-only fix rounds, no project fixes, no runtime errors, logs clean

### 2026-03-12 — Test.PivotTableBuilder
- **Multi-select Toggle variant** (`IState<string[]>` with `.Variant(SelectInputVariant.Toggle)`) renders as Radix toggle buttons with `aria-pressed` attribute, NOT `role="radio"` or `role="checkbox"`. Use `page.getByText("Option", { exact: true }).first().click()` to click them. The `clickToggleOption` helper pattern (try radio → checkbox → text fallback) works robustly
- When Toggle row field options overlap with dropdown option names (e.g., "Quantity" appears in both row field toggles AND value field dropdown), `getByText("Quantity").first()` matches the TOGGLE button, not the dropdown option. Use `getByRole("option", { name: "Quantity", exact: true })` to select dropdown items when a popover/dropdown is open — this contradicts the UUIDGenerator2 finding that `getByRole("option")` doesn't work. It DOES work when the dropdown renders proper `role="option"` elements (varies by Ivy component)
- `FileUpload<string>` with `MemoryStreamUploadHandler.Create(fileState)` and `.Accept(".csv")` — file content is available as `fileState.Value.Content` (string), not byte array
- `.ToTable().Header(r => r.Prop, "Label").Totals(r => r.Prop)` renders a standard HTML table with a totals footer row — totals value may not be findable by exact number text (use regex pattern like `/7[,.]?850/`)
- `UseDefaultApp(typeof(App))` with single app — no Chrome, access via `/<app-id>?shell=false`
- Clean run: 12 tests passed after 2 test-only fix rounds, no project fixes, no runtime errors, logs clean

### 2026-03-12 — Test.WebhookTester
- `UseWebhook` generates URLs at `/ivy/webhook/<guid>`, NOT `/api/webhook/<guid>` — don't assume API path prefix
- `UseWebhook` only accepts GET and POST requests — PUT and DELETE return non-200 status codes. This is a Framework limitation
- Webhook state (`UseState` with `ImmutableList`) persists server-side per session. Each `page.goto()` creates a new WebSocket session with fresh state (empty list), but also generates a new webhook URL (new GUID)
- `page.request.get/post/put/delete()` from Playwright fires HTTP requests that hit the webhook endpoint. State updates propagate to the page via WebSocket — need 1500-3000ms wait for UI to reflect webhook-received data
- When tests share a server and state accumulates across tests, either: (1) clear state at start of each test using UI buttons, or (2) use regex matchers like `/\d+ request\(s\) received/` instead of exact counts
- Clean run: 11 tests passed after 2 test-only fix rounds, no project fixes, no runtime errors, logs clean

### 2026-03-12 — Test.UTMParameterBuilder
- **`Dictionary<TKey, TValue>.ToDetails()` crashes** with `TargetParameterCountException` — `DetailsBuilder.Item.GetValue()` calls `PropertyInfo.GetValue(obj)` on the dictionary's indexed `this[TKey key]` property which requires a parameter. Workaround: convert to anonymous object with explicit properties, then use `.ToDetails().RemoveEmpty()`
- **Ivy `Layout.Tabs` renders BOTH tab panels in DOM** — inactive tab content is hidden but still exists. `getByText("X").first()` or `locator("input").first()` can match hidden elements from the inactive tab panel. For tab-specific interactions, use placeholder-based or role-based selectors to target elements in the active panel specifically (e.g., `input[placeholder*='unique_text']`)
- `.WithField().Required()` adds `*` suffix to field labels — `getByText("Base URL", { exact: true })` fails; use `getByText("Base URL").first()` without exact match
- Tab switching: `getByRole("tab", { name: "TabName" })` works reliably for clicking Ivy `Layout.Tabs` triggers
- `ToDetails()` on anonymous objects with PascalCase property names renders labels with spaced text: `utm_source` → "Utm Source" (underscore treated as word boundary)
- 13 tests passed after 1 project fix round (Dictionary.ToDetails crash), no runtime errors, logs clean

### 2026-03-12 — Test.TimezoneWorldClock
- **Card `.Title().Content()` renders invisible card body** — `new Card().Title(widget).Content(widget)`, `new Card(content).Title(widget)`, and `new Card(content)` all result in invisible card body content. The title/header text renders, but the body area is completely empty (white space) despite content being in the DOM. Workaround: use `Layout.Vertical()` with manual `.Padding()`, `.BorderThickness()`, `.BorderColor()` styling instead of Card
- **IANA vs Windows timezone IDs on Windows**: `GetSystemTimeZones()` returns Windows-format IDs (e.g., "Eastern Standard Time") but `FindSystemTimeZoneById("America/New_York")` works via ICU conversion and returns `tz.Id = "America/New_York"` (preserving IANA format). To deduplicate, use `tz.StandardName` not `tz.Id` since both map to the same `StandardName`
- **Searchable SelectInput dropdown**: `getByRole("combobox").first().click()` opens the dropdown, `keyboard.type("search")` filters, `getByRole("option").first().click()` selects — this pattern works reliably for `SelectInput.Searchable()`
- `GetDisplayName(tz)` for Windows timezone IDs without `/` in `tz.Id` falls through to `tz.DisplayName` which returns verbose strings like "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
- 12 tests passed after 3 project fix rounds (cross-platform timezone IDs, Card content visibility, timezone deduplication), no runtime errors, logs clean

### 2026-03-12 — Test.APIRequestBuilder
- **`Layout.Vertical(IEnumerable<T>, otherItem)` renders enumerable as debug table**: When `Layout.Vertical()` receives an `IEnumerable<Layout>` from `.Select()` as one of multiple `params object?[]` arguments, the content builder serializes it as a data table (showing internal columns: Context, Id, Key, Call Site, Is Stateless) instead of expanding items as children. Fix: call `.Select<T, object?>().Append<object?>(otherItem).ToArray()` to materialize into a single array before passing to `Layout.Vertical()`
- **Icon-only buttons (`.Destructive()` trash) have no accessible name**: `Button(icon: Icons.Trash).Destructive()` renders without accessible name, text, or `data-variant` attribute. Cannot use `getByRole("button", { name: "" })` (matches wrong elements) or `getByRole("button")` filters. Working approach: iterate all `page.locator("button")` elements, check `innerText().trim() === ""`, then `dispatchEvent("click")` on the match
- `new TextInput(value, onChange).Placeholder("X")` renders with `placeholder="X"` attribute accessible via `getByRole("textbox", { name: "X" })` in Playwright
- `ToSelectInput(["GET", "POST", "PUT", "DELETE"])` without Toggle renders as Radix dropdown with `role="option"` items — confirmed `getByRole("option", { name: "POST" })` works
- `.Multiline().Rows(6)` on TextInput renders as `<textarea>` element
- 13 tests passed after 1 project fix round (IEnumerable in Layout.Vertical), no runtime errors, logs clean

### 2026-03-12 — Test.CountdownTimer
- **`UseArgs<T>` does NOT work with external URL navigation**: `UseArgs` reads from the WebSocket `appArgs` query parameter, NOT from the browser page URL. Ivy's client-side JS does not extract `appArgs` from `window.location.search` and forward it to the WebSocket. This means shareable links using `appArgs` in the page URL won't work — it's a Framework limitation
- **Shareable link URL format**: Apps that build shareable URLs must use `?appArgs={urlEncodedJson}` format (JSON-serialized args object, URL-encoded). The original pattern of individual query params (`?TargetTimestamp=X&EventName=Y`) does not work with Ivy's `UseArgs<T>()`
- **`IHttpContextAccessor.HttpContext` is null in WebSocket context**: Ivy apps render via WebSocket/SignalR, not HTTP requests. `UseService<IHttpContextAccessor>()` returns an accessor where `.HttpContext` is null. Always use `?.` on the accessor itself: `httpContextAccessor?.HttpContext?.Request`
- **`taskkill` in bash on Windows**: `spawn('taskkill', ['/pid', ...], { shell: true })` fails with "Invalid argument/option" because bash interprets `/f` as a path. Use `spawn('cmd', ['/c', 'taskkill', '/pid', pid, '/f', '/t'], { shell: false })` instead
- **`beforeAll` timeout for recompilation**: When C# source is modified between test runs, `dotnet run` triggers a rebuild that can exceed the 30s default timeout. Always use `testInfo.setTimeout(120000)` in `beforeAll`
- 7 tests passed after 3 fix rounds (2 project fixes: shareable URL format + null ref guard), logs clean

### 2026-03-13 — AspectRatio (WidgetBase property)
- **`data-testid` on Box/Card widgets is NOT reliably found by `[data-testid="..."]` selectors** in Playwright. Use text-based locators (`getByText`, `getByRole`) and `page.evaluate()` with DOM tree walking to inspect computed CSS properties instead
- **`baseURL` in `playwright.config.ts` is evaluated at import time**, before `beforeAll` runs. When using dynamic ports, use full URLs in `page.goto()` calls: `page.goto(\`http://localhost:${port}/app?shell=false\`)` instead of relative paths
- **Verifying CSS properties via `page.evaluate()`**: Walk the DOM tree from text content upward to find elements with specific computed styles (e.g., `window.getComputedStyle(el).aspectRatio`). This is more reliable than data-testid selectors for layout/style verification
- **Flex stretch overrides `aspect-ratio`**: In `Layout.Horizontal()`, flex `align-items: stretch` causes all children to share the tallest height, partially overriding the visual aspect ratio. This is standard CSS behavior. In non-flex contexts, aspect-ratio works perfectly
- 6 tests passed after 2 fix rounds (test fixes only: URL pattern + frontend rebuild), no project fixes needed

### 2026-03-13/15 — FirstDayOfWeek (DateTimeInput & DateRangeInput) — FIXED
- **DayOfWeek enum string-to-number conversion (FIXED)**: C# `DayOfWeek.Monday` serializes as `"Monday"` (string) but react-day-picker `weekStartsOn` expects numeric `0-6`. Fix: Added `resolveDayOfWeek()` in `DateTimeInputWidget.tsx` and `DateRangeInputWidget.tsx` to map string names to numbers
- **Calendar weekday header selectors**: react-day-picker v9 uses flex layout, NOT `<table>` elements. Use `.rdp-weekdays .rdp-weekday` for weekday headers, `.rdp-day button` for day buttons
- **`UseState<(DateOnly, DateOnly)?>()` incompatible with `.ToDateRangeInput()`**: Nullable tuple throws error. Use non-nullable `UseState<(DateOnly, DateOnly)>()` instead
- **Badge TestId may not be found by `[data-testid]` locators**: Use `getByText()` for more reliable Badge content verification
- PASSED after bug fix — 8/8 tests, 3 fix rounds (selectors, serialization fix, text-based locators)

### 2026-03-13 — Test.ScatterChart
- **Frontend TypeScript errors block widget rendering**: ScatterChart widget compiles in C# but displays "Unknown component type: Ivy.ScatterChart" due to TypeScript compilation errors in ScatterChartWidget.tsx (4 errors: unused variables, type mismatch on XAxisProps[])
- **Frontend build verification is critical**: Always verify `cd src/frontend && vp build` succeeds before testing widgets, especially newly added ones
- **"Unknown component type" diagnostic approach**: When seeing this error, check frontend compilation first before investigating test code or backend API
- **BadgeVariant has no Default value**: Use `BadgeVariant.Primary` instead (values: Primary, Destructive, Outline, Secondary, Success, Warning, Info)
- **Card has no Variant prop**: Card only has `HoverVariant` of type `CardHoverVariant`, not a general `Variant` property
- **Expandable constructor requires both parameters**: `new Expandable(header, content)` not `.Content(content)` method
- 0 tests executed (blocked by frontend issues), 5 comprehensive test apps and test suites created and ready, backend compiles successfully

### 2026-03-15 — ExpressionNameHelper RadarChart Fix
- **ECharts indicator labels are rendered on canvas, NOT in DOM text**: `page.innerText('body')` and `getByText()` cannot find chart axis/indicator labels. They are part of the ECharts canvas rendering
- **Accessing ECharts option data via React fiber**: The echarts-for-react component stores the `option` prop (including `radar[].indicator[]`) in React fiber's `memoizedProps`. Access pattern:
  1. Find a `<canvas>` element
  2. Walk up to parent elements looking for `__reactFiber$` prefixed keys
  3. Walk the fiber tree (`fiber.return`) until finding `memoizedProps.option.radar`
  4. Extract `indicator[].name` from the radar config
- **`__ec_instance__` and `_ec_` are NOT reliable**: These properties may not be set on the container div in echarts-for-react — the global `echarts.getInstanceByDom()` and `div[_echarts_instance_]` attribute selector both returned empty results
- **Radar option.radar can be object or array**: When accessed via fiber, `radar` may be a single object rather than an array. Always normalize: `Array.isArray(data) ? data : [data]`
- 5 tests passed, 4 test fix rounds (all ECharts detection), no project fixes, logs clean

### 2026-03-16 — FeedbackInputMax
- **Star rating SVG locator**: Use `page.locator('button svg')` instead of `page.locator('svg')` to target only star rating icons. The page has an extra SVG from the Ivy branding icon (green circle, bottom-right) which throws off index-based locators
- **AllowHalf creates extra SVG elements**: When `AllowHalf()` is enabled, star ratings may render additional overlay SVGs for half-star detection. Don't assert exact `button svg` counts on pages with AllowHalf inputs
- **Star indices per page**: Stars render in DOM order matching visual order. On a page with multiple feedback inputs, indices accumulate: first group (0 to max-1), second group (max to max+max2-1), etc. Navigate fresh for each click test to avoid stale index issues after re-renders
- **Decimal formatting locale**: Use `CultureInfo.InvariantCulture` in C# when formatting decimal/double values that will be matched by Playwright `getByText()` — European locales use `,` instead of `.`
- **Invalid prop visual**: FeedbackInput `.Invalid("message")` renders as a small circle-exclamation icon next to the stars, not as visible text. Don't assert `getByText("message")` for invalid state — just verify the state label
- 20 tests passed, 4 fix rounds (icon name, locale, SVG locator, framework stash), logs clean

### 2026-03-16 — Copy Page (Ivy Docs)
- **Testing Ivy Docs features**: The Ivy.Docs project at `src/Ivy.Docs/` runs the docs server. Use it directly rather than creating a standalone test project. The Ivy.Docs.Shared library contains the Article widget, MarkdownMiddleware, and generated docs.
- **Docs page URL structure**: Pages are at paths like `/onboarding/getting-started/introduction` (derived from app namespace). NOT `/onboarding` (that's not a valid app path).
- **PathToAppIdMiddleware rewrites ALL non-static paths**: Any URL path that doesn't match `staticFileExtensions` in `routing-constants.json` gets rewritten from `/path` to `/?appId=path`. Custom middleware (UseWebApplication) runs AFTER this rewrite. If your middleware needs to intercept specific file extensions (like `.md`), ensure the extension is in `staticFileExtensions`.
- **Vite build runs prettier on source files**: Running `vp build` may reformat source files via prettier. After building, check `git diff` and revert formatting-only changes before committing.
- **Copy Page button location**: Inside ArticleSidebar, only visible on `lg` breakpoint screens (1024px+). Requires `showToc=true` (default for docs pages). Use 1920x1920 viewport.
- **Clipboard testing**: Use `context.grantPermissions(["clipboard-read", "clipboard-write"])` and `page.evaluate(() => navigator.clipboard.readText())` to verify clipboard content.

### 2026-03-18 — Optimistic Rendering (All Input Widgets)
- **Input widget TestId wraps Field container**: `.TestId()` on input widgets (BoolInput, SelectInput, etc.) is applied to the outer Field wrapper, not the inner interactive element. Using `getByTestId('x').locator('button[role="switch"]')` times out. Instead use `page.locator('button[role="switch"]').first()` or `page.locator('button[role="checkbox"]').nth(index)`.
- **Range inputs use tuple state, not two separate states**: `UseState<(DateOnly?, DateOnly?)>()` and `UseState<(int, int)>()` are required for `ToDateRangeInput()` and `ToNumberRangeInput()` — these extend `IAnyState`, not tuples of individual states.
- **Multi-select List variant**: Uses `button[role="checkbox"]` elements. When the same option labels appear in multiple sections (radio, toggle, checkbox), use role-based locators rather than text-based ones to target the correct section.
- **Optimistic rendering verification**: The `useOptimisticValue` hook provides instant feedback. Tests can use short waits (300-500ms) after interactions rather than waiting for server round-trip. Rapid interaction tests (6 toggles in ~1s) work reliably.

### 2026-03-26 — LineChart Empty Render Bug (LineChartBuilder with ToLineChart)
- **Canvas presence ≠ visual content**: Charts can have valid canvas elements (correct dimensions, `data-chart-rendered="true"`) but render completely empty
- **Bug pattern (RESOLVED)**: `data.ToLineChart().Dimension().Measure()` produced empty charts with non-string dimensions (integer years). Two root causes:
  1. `generateDataProps` in `sharedUtils.ts` assumed string-typed dimension keys → fixed in `bbcf22ff`
  2. `generateSeries` used case-sensitive dataKey matching — Ivy JSON serializer camelCases dictionary keys (`"count"`) but Line.dataKey preserves PascalCase (`"Count"`) → fixed in `30fb6214`
- **Chart debugging technique**: Extract LineChartWidget React props via fiber tree to inspect actual data sent from backend. Look at data key casing vs Line/Bar config dataKey casing
- **Test limitation**: Checking for canvas element existence and `data-chart-rendered` attribute is insufficient — axes/gridlines alone produce enough non-white pixels. Verify actual series rendering by inspecting React fiber props or using more targeted pixel sampling
- **Silent failure pattern**: No console errors, no backend exceptions when chart series are empty — ECharts renders axes/grid but no lines. Always visually verify screenshots
- **Case sensitivity pattern**: When charts show axes/labels but no data lines, suspect dataKey case mismatch between serialized data keys and chart config dataKey values. AreaChart, PieChart, FunnelChart already use case-insensitive matching; LineChart and BarChart now do too
- 11 apps tested (5/10/15/20 items × Default/Dashboard + StringDimension + EmptyData + MultiStyle), 12 tests all pass
- Project location: `D:\Temp\IvyFeatureTester\2026-03-26\LineChartEmptyRenderBug\`

### 2026-03-26 — Markdown OnLinkClick Event for All URLs
- **Markdown OnLinkClick event now fires for ALL URL types** when handler is registered: http, https, app://, relative paths, root-relative paths, anchor links
- **Without OnLinkClick handler**: links render normally without special handling (no target="_blank" added by framework)
- **Event signature**: `OnLinkClick((string url) => { ... })` - must explicitly type parameter to disambiguate overload between `Action<Event<Markdown, string>>` and `Action<string>`
- **Lambda overload ambiguity**: Markdown has multiple OnLinkClick overloads. Using untyped lambda `url => { }` causes CS0121 ambiguity error. Fix by explicitly typing: `(string url) => { }`
- **Programmatic URL opening**: `client.OpenUrl(url)` can be called from OnLinkClick handler for custom navigation flows (e.g., confirmation dialogs, analytics tracking)
- **Test pattern**: When markdown headings contain text matching link text (e.g., "HTTPS Link" in both H2 and `<a>`), use `getByRole('link', { name: 'HTTPS Link' })` instead of `getByText('HTTPS Link')` to avoid strict mode violations
- **Heading + link text collision**: `getByText('Relative Link')` matches both `<h2>Relative Links</h2>` (partial match) and `<a>Relative Link</a>` — use role-based selectors for precision
- **Test coverage**: Verified HTTP, HTTPS, custom protocols (app://), relative (./), root-relative (/), all work correctly
- **Non-breaking change**: Existing Markdown widgets without OnLinkClick continue to work as before
- 20 tests, all passed after 1 fix round (test selector fixes), no backend errors, logs clean
- Project location: `D:\Temp\IvyFeatureTester\2026-03-26\MarkdownOnLinkClick\`

### 2026-03-26 — Markdown Widget Local Image Support (FIXED and Re-Tested Successfully)
- **Initial Test (Pre-Fix)**: Commit 1e799a56 added `.DangerouslyAllowLocalFiles()` but feature was NOT working - images showed `src="http://localhost:PORT/ivy/#"` instead of `file://` URLs
- **Root Cause**: The `urlTransform` callback in MarkdownRenderer.tsx was stripping out file:// URLs before they reached the img component
- **Fix Applied (Commit 9906a883)**: Updated urlTransform to check `dangerouslyAllowLocalFiles` flag and preserve file:// URLs and Windows paths when enabled
- **Re-Test Result**: ✅ **ALL 18/18 tests passed** - fix confirmed working
- **CRITICAL LESSON**: Frontend rebuild is MANDATORY when testing frontend commits. The first test failed because the dist was built BEFORE the fix commit. After `npx vite build`, all tests passed.
- **How the fix works**:
  1. Checks `dangerouslyAllowLocalFiles` flag in urlTransform
  2. If enabled AND URL is `file://` or Windows path → preserves/converts it
  3. Windows paths (C:\... or C:/...) are converted to `file:///C:/...` format
  4. Backslashes are normalized to forward slashes
  5. Default behavior (blocking) is maintained for security
- **Browser file:// restrictions**: Modern browsers block file:// URL loading in web context (error: "Not allowed to load local resource"). This is EXPECTED and NOT a bug. The important verification is that the `src` attribute contains the correct file:// URL - the browser security warning is normal.
- **URL format test results**:
  - file:// URLs: ✅ Preserved (e.g., `file:///C:/Windows/System32/shell32.dll`)
  - Windows backslash paths: ✅ Converted to file:/// format
  - Windows forward slash paths: ✅ Converted to file:/// format
  - HTTP/HTTPS URLs: ✅ Pass through unchanged
  - Relative paths: ✅ Pass through unchanged
  - Data URIs: ⚠️ Appear to be blocked (may be intentional)
- **Security default verified**: Without `.DangerouslyAllowLocalFiles()`, local file paths are correctly blocked and converted to `#`
- **Test artifacts**: 4 apps (FileUrlsApp, WindowsPathsApp, SecurityDefaultApp, EdgeCasesApp), 18 tests, 20+ screenshots, 18 videos
- **Project location**: `D:\Temp\IvyFeatureTester\2026-03-26\MarkdownLocalImagesFixRetest\`

### 2026-03-29 — Icons.X in Markdown Inline Code (BUG FOUND)
- **Feature**: `Icons.X` pattern in inline code should render the actual Lucide icon next to the code text
- **BUG**: react-markdown v10 removed the `inline` prop from the `code` component. The implementation checks `if (inline)` which is always `undefined`, so icon detection never triggers. All `Icons.X` code elements render as plain monospaced text without icons.
- **Fix**: Replace `if (inline)` with `if (!className)` — inline code has no className (no language), code blocks have `className="language-xxx"`
- **Icons enum gotcha**: `Icons.AlertTriangle` → `Icons.TriangleAlert`, `Icons.Sliders` → `Icons.SlidersHorizontal` (Lucide renames)
- **Stale obj cache**: When switching branches in worktrees, `src/Ivy/obj/` can reference old frontend asset filenames. Delete obj/bin and rebuild.
- 11 tests, 2 fix rounds (icon names + stale cache), all tests pass (verify current behavior including the bug), logs clean
- **Project location**: `D:\Temp\IvyVerification\2026-03-29\IconsXMarkdown\`

### 2026-03-30 — Badge inline-flex Fix in Tables (CSS variant.ts)
- **Change**: Badge CVA base class changed from `flex` to `inline-flex` in `variant.ts`
- **Badge renders as `<div>`**: Badge.tsx renders `<div>`, not `<span>` — Playwright selectors must target `<div>` elements
- **CSS blockification**: `inline-flex` on flex items (e.g., badges inside `Layout.Horizontal()`) computes to `flex` — this is CSS spec behavior, not a bug. Only assert `inline-flex` computed style for badges in block contexts (table cells)
- **TableBuilder `.Func<TIn>()` not `.Custom()`**: `IBuilderFactory` has `Func<TIn>`, `Text`, `Link`, `CopyToClipboard`, `Progress` — no `Custom` method
- **`Layout.Horizontal().Gap()` takes `bool`**: Not `Size` — use `Gap(true)` or omit
- 12 tests, 4 fix rounds (icon names, Builder API, Gap type, DOM selectors), all passed, logs clean
- **Project location**: `D:\Tendril\Plans\01167-BadgesRenderWeirdlyInTable\artifacts\sample\`

### 2026-03-30 — NumberInput/ColorInput Height Alignment (CSS color-input-variant.ts)
- **Change**: Aligned `colorInputVariant` (h-8→h-7, h-10→h-11) and `colorInputPickerVariant` (w-8→w-7, w-10→w-9, w-12→w-11) with standard `inputVariant` heights
- **ColorInput TestId not rendered**: `.TestId()` on ColorInput does NOT produce `data-testid` — use `input[type='color']` locator or DOM traversal from headings
- **`ivy-widget` height is 0**: Custom element has no display:block — measure content via `iw.querySelector(":scope > div")?.children[0]?.getBoundingClientRect()`
- **Layout `|` operator is left-associative**: `Layout.Vertical() | Layout.Horizontal() | a | b` adds a,b to vertical, not horizontal. Must use parens: `(Layout.Horizontal() | a | b)`
- **Height measurement approach**: Find headings, walk siblings to find row with inputs, measure `div.flex.items-center` (color) vs `div.relative` (number) containers
- 14 tests, 6 fix rounds (all test locator fixes), all passed, logs clean
- **Project location**: `D:\Tendril\Plans\01181-NumberInputColorInputHeightMismatch\artifacts\sample\`

### 2026-03-30 — DiffView External Widget JSX Runtime Fix
- **External widget testing requires TWO frontend rebuilds**: (1) host app frontend (`src/frontend/`) to expose React globals, AND (2) the external widget frontend (`src/widgets/Ivy.Widgets.DiffView/frontend/`) to pick up config changes
- **Worktree-aware testing**: When the fix commit is on a feature branch already checked out in a worktree, use the worktree path for all ProjectReferences and builds. The main repo checkout can't switch to that branch.
- **DiffView OnLineClick has CS1660**: Same as ScreenshotFeedback — use `with { OnLineClick = e => ... }` expression instead of `.OnLineClick(lambda)`
- **External widget renders correctly**: DiffView shows unified and split diffs with proper color-coded added/removed lines, line numbers, revision headers
- **No "Unknown component type" error**: With both frontends rebuilt, the external widget loads and renders correctly via the ExternalWidgetRegistry
- 11 tests, 0 fix rounds (clean first pass excluding 2 compilation fixes: icon name + CS1660), all passed, logs clean
- **Project location**: `D:\Tendril\Plans\01177-FixDiffViewJsxRuntimeErrorInExternalWidget\artifacts\sample\`

### 2026-03-31 — Markdown Popover Links (`[text](## "content")` syntax)
- **PopoverLink component**: Renders as `<span role="button">` with `textDecoration: underline dotted` and `cursor: pointer`. Uses Radix `<Popover>` with click trigger
- **Locating popover triggers**: `page.locator('span[role="button"]')` finds all popover link triggers. Use `getByRole("button", { name: "link text" })` for specific ones
- **Popover content locator**: After clicking trigger, content appears in Radix PopoverContent — use `getByText("popover content text")` to find it
- **List item popovers**: When popover trigger text (e.g., "popover") appears in headings too, scope with `page.locator('li span[role="button"]')` to target list-item popovers specifically
- **Dismiss behavior**: Click `page.locator("body").click({ position: { x: 10, y: 10 } })` to dismiss. Only one popover visible at a time (Radix default)
- **OnLinkClick interaction**: Popover links (`href="##"`) do NOT trigger the `OnLinkClick` handler — they're intercepted before the link handler runs. Regular links still fire OnLinkClick
- **urlTransform preserves `##`**: The `urlTransform` callback returns `##` as-is, preventing it from being sanitized to `#`
- 16 tests, 1 fix round (test locator fixes only), all passed, logs clean
- **Project location**: `D:\Tendril\Plans\01234-MarkdownLinksWithPopoverSupport\artifacts\sample\`

### 2026-03-31 — Nested Code Block Rendering Fix (normalizeNestedFences)
- **normalizeNestedFences preprocessor**: Added to MarkdownRenderer.tsx — increases outer fence backtick counts when inner fences would prematurely close them per CommonMark spec. Applied via `useMemo` before passing content to `ReactMarkdown`
- **Same-length fences handled correctly**: Content with triple-fenced blocks nested inside triple-fenced blocks renders properly — the preprocessor increases outer fence to 4+ backticks
- **Double/triple nesting works**: Escalates fence lengths recursively for deeply nested blocks
- **Plain code blocks unchanged**: Non-nested blocks pass through without modification
- **Markdown OnLinkClick with nested blocks**: Links before and after nested code blocks fire correctly; no interaction issues
- **Link locator issue**: `getByRole("link", { name: "another link" })` timed out even though text was in body — use `page.locator("a").filter({ hasText: "text" }).first()` for more reliable anchor element targeting in Markdown content
- 18 tests, 1 fix round (link locator fix), all passed, logs clean
- **Project location**: `D:\Tendril\Plans\01237-FixNestedCodeBlockRenderingInMarkdownWidget\artifacts\sample\`

### 2026-03-31 — AsyncSelectInput State Update Bug Fix
- AsyncSelectInput opens a **Sheet** (`role="dialog"`) with search input and ListItem options
- The trigger button contains placeholder text as `<span>` — `getByText("placeholder")` matches BOTH the description paragraph and the button span, causing strict mode violation
- **Use `getByRole('button', { name: 'placeholder text' })`** to click the AsyncSelectInput trigger
- Inside the sheet: `sheet.getByPlaceholder('Search')` for the search input, `sheet.getByText('Option', { exact: true }).first()` for options
- After selecting, the sheet auto-closes and state feedback updates — wait 1500ms after click for state propagation
- **OnBlur test**: Clicking the input opens the sheet (overlay blocks subsequent clicks). Use `.focus()` + `keyboard.press('Tab')` to trigger blur without opening the sheet
- `.Invalid("message")` renders as an icon/visual indicator on the button, NOT as visible text — don't assert `getByText("error message")`
- `test.use({ video })` CANNOT be inside `test.describe()` — must be top-level in the file
- 17 tests, 1 fix round (strict mode + video config + onBlur approach), all passed, logs clean
