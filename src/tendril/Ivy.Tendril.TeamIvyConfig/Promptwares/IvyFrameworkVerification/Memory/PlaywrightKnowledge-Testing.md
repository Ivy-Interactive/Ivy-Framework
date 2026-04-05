# Testing Best Practices and Verification

## Playwright Test Project Setup

- Tests live in `.ivy/tests/` within the Ivy project
- Use `package.json` with `@playwright/test` dependency
- `playwright.config.ts` targets Chromium only, single worker, no retries
- Config uses `process.env.APP_PORT` for base URL
- **Viewport configuration**: Use `{ width: 1920, height: 1920 }` for square screenshots. Must be set in BOTH `use` and `projects[0].use` to override device preset defaults.

## Video Recording

- Enable via `video: { mode: 'on', dir: './videos' }` in both `use` and `projects[0].use` config
- Videos are saved as `.webm` files in the configured directory
- Use `test.afterEach` to rename videos with descriptive names matching test titles
- Access video path via `page.video()?.path()` — only available after the page context closes
- Videos add ~1-2 seconds overhead per test but provide invaluable debugging evidence
- `test.use({ video })` CANNOT be inside `test.describe()` — must be top-level in the file

## Test Structure Patterns

- `test.beforeAll` runs once per `test.describe` block. Multiple `test.describe` blocks = multiple `beforeAll` executions = multiple server spawns and potential DLL lock conflicts.
- For shared server: put all tests in one `test.describe`, or use top-level `test()` calls outside describe blocks which share the file-level `beforeAll`.
- Always kill app processes after test runs on Windows — lingering processes lock DLLs
- Screenshot/log path resolution: use `path.resolve(__dirname)` in spec files, NOT `process.cwd()`

## Common Test Categories

1. **Visibility tests** — verify UI elements render correctly
2. **Interaction tests** — click buttons, toggle switches, move sliders
3. **State change tests** — verify UI updates after interactions
4. **Output validation** — check generated/computed values appear correctly

## General Testing Tips

- Ivy apps may take a few seconds to start; 30s timeout is safe
- Use `shell: true` in spawn options on Windows
- Password/random generation tests: compare two outputs rather than asserting exact values
- Switch/toggle labels include the label text, use `getByText()` to find them
- After clicking a button, use `waitForTimeout(500)` or `waitFor()` before asserting changes
- Badges with strength/status text may appear multiple times; use `.first()`
- `UseDefaultApp()` means no Chrome sidebar — navigate directly to `/<app-id>?shell=false`
- `UseDefaultApp(typeof(App))` with single app — no Chrome, access via `/<app-id>?shell=false`
- For API-dependent tests, use `Promise.race()` with both success and error element waiters

## Widget-Specific Test Helpers

- `safeClick` pattern: try `.click({ timeout: 3000 })`, catch and fall back to `.dispatchEvent("click")` — handles both in-viewport and out-of-viewport elements
- `clickToggleOption` helper: check `[role="checkbox"]` count, then `[role="switch"]`, then fallback `dispatchEvent("click")` — robust across all BoolInput variants
- Sidebar search + `dispatchEvent("click")` pattern works reliably for Chrome tabs navigation

## Widget Library Testing

- Widget library projects have `.samples/` subfolder with the runnable app — tests must use `samplesDir` not `projectRoot` for `dotnet run`
- `[ExternalWidget]` components show "Unknown component type" when JS bundle isn't served — Framework issue
- External widget testing requires TWO frontend rebuilds: host app frontend AND the external widget frontend

## Clipboard Testing Limitations

- Clipboard `writeText` fails in headless Chromium with "Write permission denied" — this is expected and not an app bug
- Use `context.grantPermissions(["clipboard-read", "clipboard-write"])` for clipboard testing
- Verify clipboard content via `page.evaluate(() => navigator.clipboard.readText())`

## File Dialog Testing

- Chromium's `showOpenFilePicker`/`showSaveFilePicker`/`showDirectoryPicker` cannot be intercepted by Playwright — disable via `addInitScript` to fall back to `<input type="file">`
- Folder dialog `<input webkitdirectory>` requires passing directory path to `fileChooser.setFiles()`
- Save dialog fallback uses `<a download>` — capture with `page.waitForEvent('download')`

## Verifying CSS Properties

- Walk the DOM tree from text content upward to find elements with specific computed styles (e.g., `window.getComputedStyle(el).aspectRatio`)
- This is more reliable than data-testid selectors for layout/style verification
- For bounding box measurements: `element.getBoundingClientRect()` via `page.evaluate()`

## PNG Generation for Tests

- To generate valid PNG images for Playwright tests: use `zlib.deflateSync()` on raw pixel data (filter byte 0 + RGB per row), then construct PNG chunks (IHDR, IDAT, IEND) with proper CRC32 checksums
- Invalid/minimal base64 PNGs will cause `InvalidImageContentException`

## Fake Camera Testing

- Chromium fake device flags produce `videoWidth=0` and `videoHeight=0` in headless mode
- Upload-dependent assertions cannot be verified with fake cameras — test UI state transitions instead
- Set config: `permissions: ['camera']` and launch args for fake media stream

## Run History

### 2026-03-17 — UseFileDialog/UseSaveDialog/UseFolderDialog Hooks
- File dialog hooks with browser API detection (File System Access API → fallback input)
- **BUG FOUND**: `FileDialogMode.Upload` (enum value 0) stripped by WidgetSerializer → `mode` undefined on frontend → upload silently skipped. Fixed with `mode = 'Upload'` default in `FileDialogWidget.tsx`
- 6 tests, 3 fix rounds, all passed

### 2026-03-13/15 — FirstDayOfWeek (DateTimeInput & DateRangeInput) — FIXED
- `DayOfWeek` enum serializes as string but picker expects numeric
- react-day-picker v9 uses flex layout, NOT `<table>`
- 8 tests, 3 fix rounds, all passed

### 2026-03-15 — ExpressionNameHelper RadarChart Fix
- ECharts indicator labels on canvas, NOT in DOM
- Access chart data via React fiber `memoizedProps.option`
- 5 tests, 4 fix rounds, all passed

### 2026-03-16 — FeedbackInputMax
- Star rating SVG locator: use `page.locator('button svg')`
- Decimal formatting locale: use `CultureInfo.InvariantCulture`
- 20 tests, 4 fix rounds, all passed

### 2026-03-16 — Copy Page (Ivy Docs)
- Ivy Docs at `src/Ivy.Docs/` — use directly for docs testing
- PathToAppIdMiddleware rewrites ALL non-static paths
- All passed

### 2026-03-18 — Optimistic Rendering (All Input Widgets)
- Input widget `.TestId()` wraps Field container, not inner element
- Range inputs use tuple state
- Rapid interaction tests work reliably with optimistic rendering

### 2026-03-26 — LineChart Empty Render Bug
- Canvas presence ≠ visual content — charts can render empty
- Case-insensitive dataKey matching now applied to LineChart/BarChart
- 12 tests, all passed

### 2026-03-26 — Markdown OnLinkClick Event
- Fires for ALL URL types (http, https, app://, relative, root-relative, anchor)
- Lambda overload ambiguity: explicitly type parameter
- 20 tests, 1 fix round, all passed

### 2026-03-26 — Markdown Local Image Support (FIXED)
- `.DangerouslyAllowLocalFiles()` preserves file:// URLs
- Browser blocking of file:// is expected behavior
- 18 tests, all passed

### 2026-03-29 — Icons.X in Markdown Inline Code (BUG FOUND)
- react-markdown v10 removed `inline` prop — icon detection never triggers
- Fix: replace `if (inline)` with `if (!className)`
- 11 tests, 2 fix rounds, all passed

### 2026-03-30 — Badge inline-flex Fix in Tables
- Badge CVA base changed from `flex` to `inline-flex`
- `Layout.Horizontal().Gap()` takes `bool`
- 12 tests, 4 fix rounds, all passed

### 2026-03-30 — NumberInput/ColorInput Height Alignment
- ColorInput `.TestId()` not rendered — use `input[type='color']`
- Layout `|` operator is left-associative
- 14 tests, 6 fix rounds, all passed

### 2026-03-30 — DiffView External Widget JSX Runtime Fix
- External widget testing requires TWO frontend rebuilds
- DiffView OnLineClick: use `with { OnLineClick = ... }` expression
- 11 tests, 0 fix rounds, all passed

### 2026-03-31 — Markdown Popover Links
- `[text](## "content")` syntax renders `<span role="button">` with Radix Popover
- Only one popover visible at a time
- 16 tests, 1 fix round, all passed

### 2026-03-31 — Nested Code Block Rendering Fix
- `normalizeNestedFences` preprocessor handles nested backtick fences
- 18 tests, 1 fix round, all passed

### 2026-03-31 — AsyncSelectInput State Update Bug Fix
- Opens Sheet with search input and ListItem options
- OnBlur test: use `.focus()` + `Tab` instead of click
- 17 tests, 1 fix round, all passed
