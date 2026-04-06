# DOM and Rendering Quirks

## Canvas-Based Rendering (DataTable, Glide Data Grid)

- DataTable uses Glide Data Grid which renders as `<canvas>` elements, NOT HTML `<table>`
- `AsQueryable().ToDataTable()` renders using glide-data-grid — a virtualized grid where cells have `role="gridcell"` but may not be "visible" (outside virtual viewport). Use `toBeAttached()` instead of `toBeVisible()` for these cells.
- Canvas elements intercept real mouse events — `.click()` and `.click({ force: true })` both fail on gridcells. Use `.dispatchEvent("click")` to bypass.

## Virtualized Grid Rendering

- Cells are `<td role="gridcell">` elements that Playwright considers "hidden" even when data is present
- Column headers (`role="columnheader"`) are also "hidden" in virtual rendering — use `toBeAttached()` not `toBeVisible()`
- Reading cell values requires React fiber introspection since DOM text assertions don't work on canvas

## React Fiber Introspection

- Find a `canvas` element, walk `__reactFiber` tree upward to find a component with `getCellContent` prop
- Call `getCellContent([col, row])` to get `{ kind, data, displayData }`
- For ECharts: walk fiber tree to find `memoizedProps.option` for chart data access
- `__reactFiber$` prefixed keys on DOM elements provide entry to the fiber tree

## Inline Styles vs CSS Classes

- **CodeBlock syntax tokens use inline styles, not CSS classes** — `react-syntax-highlighter` (Prism mode) applies styles via inline `style` attributes on `<span>` elements inside `<pre><code>`. Do NOT use class-based selectors like `span.token.comment`.
- **`Html` component inline styles are largely invisible** — `new Html(...)` with CSS properties like `background-color`, `border`, `color` are NOT reliably applied in the actual DOM. CSS custom properties (`var(--foreground)`, etc.) resolve to nothing inside the Html iframe.
- **Tailwind `space-y-*` spacing verification** — `space-y` uses `> :not([hidden]) ~ :not([hidden])` sibling selector, NOT direct `margin-top` on child elements. `getComputedStyle(li).marginTop` returns `0`. Verify spacing via bounding box gaps: `box2.y - (box1.y + box1.height) > 0`.

## Radix UI Component Patterns

- Radix Toast renders individual toasts as `<li>` elements with `data-state="open"`
- Radix Select trigger is `button[role="combobox"]`, options are `[role="option"]`
- Radix Popover content renders via Portal at document root
- Radix Slider renders with `role="slider"`, keyboard-interactive
- Radix Checkbox renders as `<button role="checkbox">` — NOT native `<input type="checkbox">`
- Radix Switch renders as `<button role="switch">`

## Role Attribute Conflicts

- **Xml/Json widget toggle buttons use `role="button"`** — expandable tree nodes render as `<div role="button">`. This means `getByRole("button", { name: "0" })` can match both actual `<button>` elements AND XML toggle nodes. Always use `{ exact: true }`.
- **Confetti Click trigger wrapper uses `role="button"`** — wraps children in `<div role="button" tabindex="0">`
- **Badge renders as `<div>`**: Badge.tsx renders `<div>`, not `<span>` — target `<div>` elements in selectors
- **Badge CVA base class**: Changed from `flex` to `inline-flex` — `inline-flex` on flex items computes to `flex` (CSS spec behavior)

## CSS and Layout Rendering

- **Flex stretch overrides `aspect-ratio`**: In `Layout.Horizontal()`, flex `align-items: stretch` causes all children to share the tallest height, partially overriding the visual aspect ratio
- **`ivy-widget` height is 0**: Custom element has no display:block — measure content via `iw.querySelector(":scope > div")?.children[0]?.getBoundingClientRect()`
- **Layout `|` operator is left-associative**: `Layout.Vertical() | Layout.Horizontal() | a | b` adds a,b to vertical, not horizontal. Must use parens: `(Layout.Horizontal() | a | b)`
- **CSS blockification**: `inline-flex` on flex items computes to `flex` — this is CSS spec behavior, not a bug. Only assert `inline-flex` computed style for badges in block contexts (table cells)

## Sidebar and Chrome Navigation Patterns

- Chrome sidebar renders app names as `role="button"` elements
- Sidebar items may be outside the viewport in headless mode — use `dispatchEvent("click")` instead of `.click()`
- Sidebar search + `dispatchEvent("click")` pattern works reliably for navigating to apps
- `UseChrome()` renders a hidden sidebar search `<input type="search">` that is the first `input` in the DOM

## Clipboard Testing

- Clipboard `writeText` fails in headless Chromium with "Write permission denied" — this is expected
- Use `context.grantPermissions(["clipboard-read", "clipboard-write"])` for clipboard testing
- For CodeMirror: use `page.evaluate(async (t) => { await navigator.clipboard.writeText(t); }, text)` then `Ctrl+V`
