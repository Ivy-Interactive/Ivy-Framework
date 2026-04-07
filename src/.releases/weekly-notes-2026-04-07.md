# Ivy Framework Weekly Notes - Week of 2026-04-07

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release covers Ivy Framework changes from late March through early April 2026. Updates that belong only to Tendril apps or internal plan tooling are omitted. Bug fixes focus on server, .NET, and build issues; routine frontend-only fixes are not listed exhaustively.

## Charts

### Dual-axis and axis generation

Bar, Line, Area, and [Scatter](https://docs.ivy.app/widgets/charts/scatter-chart) series support `YAxisIndex` for dual-axis layouts. `generateYAxis` was refined for multi-axis charts (including skipping `largeSpread` when multiple axes are active). Cartesian charts reclaim plot width when X axes are hidden, and grid padding was tuned so hidden axes do not waste space.

### Bar chart

BarChart vertical bar orientation and ECharts axis pairing were corrected. `YAxis.Hide` is honored reliably, and grid padding behaves correctly with hidden axes. Documentation describes the `YAxisIndex` pattern on `Bar` records and dual-axis setups.

### Scatter chart

Scatter avoids category axes where a value axis is required. [ScatterChartApp](https://docs.ivy.app/widgets/charts/scatter-chart) includes a dual-axis sample; the sample uses a numeric X axis (not category) for continuous data. Widget tests cover ScatterChart, and the implementation blocks inappropriate category axis typing for scatter data.

### Line and area series

Line and Area series expose `YAxisIndex` and documentation covers polish callbacks.

### Pie chart

[PieChart](https://docs.ivy.app/widgets/charts/pie-chart) tooltips use a formatter with marker styling for clearer series labels and values.

## DataTable and querying

### Decimal and footer formatting

[DataTable](https://docs.ivy.app/widgets/advanced/data-table) decimal columns handle `valueOf` more reliably with a string fallback. Footer cells format currency and numeric aggregates to match column rules.

### Column expressions

Navigation properties and ternary expressions work more predictably in column expressions.

### Column scaling

Optional auto-exclusion of navigation collection columns from scaling avoids distorted layouts.

### ToDetails and navigation properties

`ToDetails()` no longer shows raw CLR type names for navigation properties.

### Virtual columns

You can define multiple virtual columns from the same root property.

### Sorting and stable order

When `AllowSorting` is false, `ToDataTable` preserves source order. The query processor applies a default `OrderBy` when pagination needs a stable order.

### UseDataTable config

`ViewBase.UseDataTable` accepts a config parameter to forward options from one place.

### Search

Search includes match navigation, highlights, and a progress indicator for large tables.

### Badge and link cells

Badge cells can use per-value colors. Link cells cooperate with `OnCellClick` without double navigation.

### Tooltips on cells

Cells use the shared `withTooltip` wrapper instead of the native `title` attribute.

### Virtual scrolling and height

Virtual scrolling paints rows reliably; height behavior in unconstrained parents was fixed, including a zero-height regression follow-up with container style tests.

### Documentation

UseQuery + DataTable anti-patterns are clarified for authors and AGENTS (prefer `IQueryable` / `ToDataTable()` where appropriate).

## Markdown and tables

The [Markdown](https://docs.ivy.app/widgets/primitives/markdown) widget renders Graphviz diagrams (samples and docs ship in the same period). Images embedded in markdown use a light border. Table widget borders align with markdown-rendered tables. Fenced code blocks without a language render reliably; markdown tables inside fenced code stay literal (not rendered as HTML tables). Default container gap is tighter, with element-specific spacing instead of one uniform gap for every block.

## Image

[Image](https://docs.ivy.app/widgets/primitives/image) supports `Overlay` for lightbox viewing. Arrow keys move between sibling overlays. Earlier in the cycle, `Overlay` was introduced as a boolean on the widget record.

## Sheet

[Sheet](https://docs.ivy.app/widgets/advanced/sheet) has a resizable drag handle, improved width handling (including vs Tailwind variants and follow-up size fixes), and works with explicit width plus resize.

## Dialog and AutoFocus

Dialog and Sheet no longer block AutoFocus on child inputs (with a client `HTMLElement` cast where needed). DialogApp demonstrates AutoFocus.

## Tabs and loading

[TabsLayout](https://docs.ivy.app/widgets/layouts/tabs-layout) adds `OnCloseOthers` (close other tabs), syncs tab order on refresh to avoid flicker, and supports badges on the Content tab variant (secondary, smaller).

## Layout and chrome

Layout.Grid defaults to top-left alignment (use `AlignContent` if you relied on center). [HeaderLayout](https://docs.ivy.app/widgets/layouts/header-layout) and [FooterLayout](https://docs.ivy.app/widgets/layouts/footer-layout) support scroll-triggered drop shadows. The shared `useScrollShadow` hook adds a `direction` parameter, uses `MutationObserver` (batched with `requestAnimationFrame`) for dynamic content, and was extracted from header/footer implementations. Container size measurement retries for nested flex layouts.

## StackedProgress

[StackedProgress](https://docs.ivy.app/widgets/common/progress) is a segmented colored bar with `OnSelect` / `Selected`; ShowLabels turns on automatically when a segment has a label. Samples wrap it in Box with padding and avoid `Client.Toast` from `SampleBase`.

## Terminal

The Terminal widget exposes `Background` and `Foreground`.

## Detail helper

The Detail helper’s `Multiline` option defaults to `false`.

## DiffView

[DiffView](https://docs.ivy.app/widgets/primitives/diff-view) uses a smaller default font.

## Confetti

Confetti uses a shorter duration and fewer particles.

## Buttons and badges

[Button](https://docs.ivy.app/widgets/common/button) badges use the outline chip style (with unit tests). [Tab](https://docs.ivy.app/widgets/layouts/tabs-layout) (Content variant) and [DropDownMenu](https://docs.ivy.app/widgets/common/drop-down-menu) items support badges. [Badge](https://docs.ivy.app/widgets/common/badge) renders nothing for empty text. Menu items support extra color options. ThemeCustomizer improves empty placeholders. `CardHoverVariant` is renamed to `HoverEffect` in shared APIs—update Card, Box, and Image hover calls accordingly.

## Inputs and file uploads

[ContentInput](https://docs.ivy.app/widgets/inputs/content-input) supports attachments, optional `ShortcutKey`, density-scaled sub-widgets, invalid state samples, shared `FileAttachmentList`, `validateFileWithToast`, `useUploadWithProgress`, `XMLHttpRequest` upload progress, and FileDialog integration—plus docs, Playwright patterns, and a three-column CodeBlock-style language grid in related samples. [FolderInput](https://docs.ivy.app/widgets/inputs/folder-input) supports `FolderInputMode` (including full path), full-row click, Enter/Space, and browse `aria-label`. FileInput browse controls expose `aria-label`. Password, email, tel, url, and textarea samples show `ShortcutKey`; textarea supports Ctrl+Enter / Cmd+Enter to submit and blur. TextInput has keyboard tests and cleaner affix shortcut demos. SignatureInput supports dark mode and color tests. `useDictation` gains tests and drops unused surface area; `dictationLanguage` was removed from the TextInput widget where redundant. [Select](https://docs.ivy.app/widgets/inputs/select-input) fixes dropdown placement when both placeholder and items are present.

## Code blocks and languages

The Languages enum carries `Description` attributes for labels. CodeBlock and samples add PowerShell, Bash/Shell, and related FileApp language mapping. The CodeBlock sample uses a three-column language grid in places.

## Accessibility

A broad WCAG pass adds `aria-label`s (including tooltip targets, `role="button"` surfaces, and browse buttons). DataTable uses the design-system tooltip instead of raw `title`.

## Routing, shell, and apps

`?chrome=false` remains compatible with newer shell flags; AppRouter tests live in `Ivy.Test` (internals). Apps may set `allowDuplicateTabs` (e.g. FileApp in samples). Routing dots in the shell were restored. The samples Setup app is renamed Settings with a cogs icon.

## Blades

`IBladeService` is renamed to `IBladeContext`—update DI and `UseService` usages.

## Branding and theming

ivy-green and related brand tokens land in CSS; sidebar can use `bg-secondary`. ThemeCustomizer empty states are clearer.

## Keyboard and shortcuts

Global shortcuts use `event.code` on macOS for reliable Option/Command chords. Modifier shortcuts still fire when focus is inside multi-line text areas. Shortcut helpers consolidate under `@/lib/shortcut`.

## Server, auth, and HTTP

OAuth callbacks use `LocalRedirect`. SignalR hub tests cover `/ivy/messages`. WebApplicationFactory-style HTTP tests and shared Ivy.Integration.Tests infrastructure land alongside Mock HTTP helpers. Ivy.Docs.Shared delegates to Ivy.Docs.Helpers to deduplicate middleware.

## Build, packaging, and native assets

Docker images retain the rustserver binary on clean builds. Embedded resource logical names work cross-platform; Vite EmbeddedResource races (CS1566) are addressed; macOS/Linux 404s and blank screens from assets are fixed; duplicate `UseFrontend`/`UseAssets` and App ID `assets` collisions are resolved. CI fixes NuGet markdown globbing with native targets. `pnpm` `--frozen-lockfile`, lockfile regeneration, NuGet lock files for core packages, and docs embedded in nupkg improve reproducible builds. `KillProcessUsingPort` works on macOS and Linux. `.gitattributes` enforces LF (including widget frontends); `.npmrc` is ignored in frontend and widget trees. Markdown converter cache keys on file content.

## Diagnostics and client logging

Client `logger.info` is downgraded to `logger.debug`. `WidgetTree` `RefreshRequested` and `_RefreshView` wrap try/catch so one bad view does not tear down the tree.

## Tooling, analyzers, and repository hygiene

Roslyn analyzer `IVYSERVICE001` requires `UseService` at the top of `Build()` (see [AGENTS.md](https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/AGENTS.md)). IDE0005 and bulk unused `using` cleanup run across the repo. Pre-commit can scope to frontend paths. Hooks use a barrel export (with duplicate export fixes). Security/code scanning warnings are addressed. Ivy.Agent.Filter.Tests local setup and `ivy` CLI “command not found” Mac notes help onboarding.

## Documentation and AI guidance

Hallucinations / AGENTS docs expand (`IBladeContext`, `Server.StartAsync`, `SelectOption<T>`, compound widgets, UseLoading page, chart polish, IvyFrameworkGotchas trimming, RadialBarChart outdated notes removed). Playwright knowledge splits into focused files; widgets guidance consolidates; obsolete redirect files are removed. Input doc filenames are renumbered so the sidebar order is unique. IvyFrameworkVerification follows the split knowledge layout and documents process timeout guards.

## Breaking changes

### `CardHoverVariant` → `HoverEffect`

The hover enum was renamed and moved to shared APIs. Replace `CardHoverVariant` with `HoverEffect` on Card, Box, and Image.

```csharp
new Card(Text.Block("Hello")).Hover(HoverEffect.Shadow);
new Box(Text.Block("Click me")).Hover(HoverEffect.PointerAndTranslate);
new Image("photo.jpg").Hover(HoverEffect.Pointer);
```

### `IBladeService` → `IBladeContext`

Rename DI registrations and `UseService` types from `IBladeService` to `IBladeContext`.

## Bug fixes

- DataTable: decimal `valueOf` fallback; footer aggregates; navigation/ternary expressions; link cells with `OnCellClick`; source order when sorting is off; default `OrderBy` for paging; virtual columns from one root; virtual row rendering and height in unconstrained layouts.
- ToDetails(): no raw type names for navigation properties.
- Arrow serialization for widget payloads via safer `ToString` paths.
- Markdown converter cache invalidation by file content.
- OAuth: `LocalRedirect` on callback.
- CI / build: NuGet globbing with native targets; Docker rustserver on clean builds; CS1566 EmbeddedResource/Vite; embedded names cross-platform; App ID `assets` collision; duplicate middleware registration.
- C# / tests: compilation and project reference fixes for API moves and packages.
