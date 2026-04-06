# Critical Gotchas and Crash Patterns

## Critical: ValueTuple Crash Pattern

- **NEVER** use C# ValueTuple syntax `Layout.Vertical() | (item1, item2, ...)` when the tuple contains widgets — `DefaultContentBuilder.Format()` calls `ValueTuple.ToString()` which triggers `PrintMembers()` → `get_Id()` on uninitialized widgets, causing `InvalidOperationException`
- **Always** use `|` operator chaining instead: `Layout.Vertical() | item1 | item2 | item3`
- This is a Framework bug in `DefaultContentBuilder.Format()` (line 171)
- Affects ALL widget types (Field, TextInput, Button, etc.) when placed in tuples

## Critical: Layout.Vertical with IEnumerable

- `Layout.Vertical(items.Select(...), otherWidget)` where the first arg is `IEnumerable<T>` causes Ivy's content builder to render it as a data table instead of expanding children
- **Always** materialize enumerables before passing to Layout: `.Select<T, object?>(...).Append<object?>(otherWidget).ToArray()`
- Alternative: build layout with `|` operator chaining instead of passing collections

## Decimal Columns in DataTable

- `decimal` values in DataTable grid render as `00000000000000000` (framework bug)
- Root cause: `useRowData.ts` reads raw Arrow Decimal128 values without applying `10^scale` division
- Workaround: format values as strings in the `.Header()` call

## Process Locking Issues

- Stale `Test.*.exe` processes can lock EXE and prevent rebuilding
- Always kill app processes after test runs on Windows
- When DLLs are locked, spawn pre-built exe directly instead of `dotnet run`

## Frontend Build Timing Issues

- C# backend can compile successfully while TypeScript frontend fails
- "Unknown component type: Ivy.WidgetName" means frontend bundle is missing the widget
- Always verify frontend build before testing widgets
- **CRITICAL**: Frontend rebuild is MANDATORY when testing frontend commits — dist built before fix commit won't work

## UseEffect Timing with CodeBlock

- `new CodeBlock(state.Value, language)` where state is populated via `UseEffect` renders EMPTY initially
- Must trigger a state change first (e.g., type in an input, move a slider) to force UseEffect to fire
- Wait 500-1000ms before asserting after state change

## Enum ToOptions() Display Text

- `typeof(MyEnum).ToOptions()` converts PascalCase to spaced text: `V1` → "V 1", `V3` → "V 3"
- `getByText("V1")` will NOT match — must use `getByText("V 1", { exact: true })`
- Same pattern for all enum values where PascalCase splitting inserts spaces

## Layout.Grid Column Count Bug

- `Layout.Grid(3).Gap(4)` renders the column count "3" as visible text content at the top
- Grid items appear stacked vertically (single column) instead of in a 3-column layout
- Workaround: use `Layout.Horizontal()` with equal-width children, or `Layout.Grid().Columns(N)` (`.Columns(8)` works)

## Html Widget Invisibility

- `new Html(...)` with CSS custom properties (`var(--foreground)`, `var(--border)`) renders completely invisible
- Html component appears to render in an iframe without CSS custom property inheritance
- Hardcoded colors or native Ivy components are recommended instead

## Card Content Invisibility

- `new Card().Title(widget).Content(widget)` renders invisible card body content — title renders but body is empty
- Workaround: use `Layout.Vertical()` with manual `.Padding()`, `.BorderThickness()`, `.BorderColor()` styling

## Timer / UseEffect Race Conditions

- `System.Timers.Timer` in `UseEffect`: timer `Elapsed` events can fire after `isRunning` state is false — always add `if (!isRunning.Value) return;` guard
- Real-time state updates via `UseEffect` + Timer are NOT reliably observable in Playwright — verify format and static state instead

## UseState Ambiguity

- `UseState<T>(T?)` vs `UseState<T>(Func<T>)` is ambiguous when T is nullable reference type — use explicit typed variables

## API and External Service Patterns

- External API calls in async button handlers can crash the server process — always wrap in try/catch
- When external APIs are expected to fail, use `Promise.race()` in tests to detect either data load OR error
- `ToAsyncSelectInput` with nullable state types: delegates MUST use `Option<int?>` (not `Option<int>`) — nullable mismatch causes `InvalidCastException`

## Webhook Limitations

- `UseWebhook` only accepts GET and POST — PUT/DELETE return non-200
- Webhook URLs require internal `state` query parameter — server-side calls without it return 400
- `UseArgs<T>` does NOT work with external URL navigation — reads from WebSocket, not browser URL
- `IHttpContextAccessor.HttpContext` is null in WebSocket context

## Miscellaneous Gotchas

- `Layout.Horizontal().Gap()` takes `bool`, not `Size` — use `Gap(true)` or omit
- **Stale obj cache**: When switching branches in worktrees, `src/Ivy/obj/` can reference old frontend asset filenames. Delete obj/bin and rebuild.
- `test.use({ video })` CANNOT be inside `test.describe()` — must be top-level in the file
- `Dictionary<TKey, TValue>.ToDetails()` crashes with `TargetParameterCountException` — convert to anonymous object first
- `DataTableBuilder.Header(r => r.Values[colIndex], ...)` crashes — `GetNameFromMemberExpression` only handles simple member access, not indexers
