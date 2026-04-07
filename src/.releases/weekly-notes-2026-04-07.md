# Ivy Framework Weekly Notes - Week of 2026-04-07

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This week’s notes cover **Ivy Framework** changes **excluding** the Tendril apps and tooling. **Frontend-only** fixes are omitted from **Bug fixes** (see that section).

The sections below walk through changes **day by day**; later days are appended as the week progresses.

## Day 1

### [01255] Button badge outline variant

Badges on [**Button**](https://docs.ivy.app/widgets/common/button) now render with the **outline** badge style so they read clearly on both **solid** and **outline** button variants. Unit tests lock in the behavior.

```csharp
new Button("Inbox", OnOpenInbox, variant: ButtonVariant.Primary).Badge("12");
new Button("Updates", OnCheckUpdates, variant: ButtonVariant.Outline).Badge("3");
```

You still use **`.Badge(string)`** on the button; only the default visual treatment of the badge chip changed.

### [01269] DataTable decimal scaling

[**DataTable**](https://docs.ivy.app/widgets/advanced/data-table) column formatting for **decimal** values is more reliable when the runtime exposes values through **`valueOf`**: the pipeline falls back to **string** when needed so scaling and display stay consistent.

### [01278] `CardHoverVariant` renamed to `HoverEffect`

The hover enum was renamed from **`CardHoverVariant`** to **`HoverEffect`** and moved to shared APIs (`Ivy` namespace) so Card, Box, Image, and other widgets share one type.

```csharp
new Card(Text.Block("Hello")).Hover(HoverEffect.Shadow);
new Box(Text.Block("Click me")).Hover(HoverEffect.PointerAndTranslate);
new Image("photo.jpg").Hover(HoverEffect.Pointer);
```

Replace any remaining **`CardHoverVariant`** references with **`HoverEffect`**.

### [01276] Image `Overlay` for lightbox

[**Image**](https://docs.ivy.app/widgets/primitives/image) has a boolean **`Overlay`** property. When **true**, the image opens in a **lightbox-style** full-screen viewer on the client.

```csharp
new Image("https://example.com/diagram.png")
{
    Alt = "Architecture",
    Overlay = true
};
```

Later in the week, keyboard navigation between sibling overlays was added on top of this.

## Day 2

### [01296] Badges on Tab Content and DropdownMenu

When using [**TabsLayout**](https://docs.ivy.app/widgets/layouts/tabs-layout) with the **Content** tab variant, each [**Tab**](https://docs.ivy.app/widgets/layouts/tabs-layout) can show a **badge** (counts, “New”, and so on). [**MenuItem**](https://docs.ivy.app/widgets/common/menu-item) entries used in [**DropDownMenu**](https://docs.ivy.app/widgets/common/drop-down-menu) also **render badges** in the menu UI, aligned with sidebar and button badge patterns.

```csharp
new Tab("Inbox", inboxView).Badge("12");
MenuItem.Default("Exports").Badge("3");
```

### [01321] `?chrome=false` and shell URL compatibility

The **`?chrome=false`** query parameter remains **supported** alongside newer **shell / chrome** URL flags so existing bookmarks and integrations keep working. **Tests** cover chrome and shell parameter combinations. **AppRouter** tests were moved into the **`Ivy.Test`** project so they can use **internals** from the router assembly via **`InternalsVisibleTo`**.

### [01356] Markdown code blocks without a language

Fenced **code blocks** in the [**Markdown**](https://docs.ivy.app/widgets/primitives/markdown) widget that **omit a language** specifier now render reliably instead of breaking or appearing blank.

### [01372] `IBladeService` renamed to `IBladeContext`

The blades API type was renamed from **`IBladeService`** to **`IBladeContext`**. Update dependency injection registrations and **`UseService`** / constructor parameters to the new interface name; documentation and hallucination entries were updated accordingly.

### [01403] Resizable Sheet drag handle

The [**Sheet**](https://docs.ivy.app/widgets/advanced/sheet) widget supports a **resizable drag handle** so users can adjust height (or width, depending on configuration) by dragging.

### [01407] `Languages` enum `Description` attributes

The **Languages** enum used for syntax highlighting and samples now has **`Description`** attributes for **human-readable labels** in UI and docs where the raw enum name is not ideal.

### [01263] Grid default alignment

**Layout.Grid** defaults to **top-left** alignment instead of **center**, which matches typical forms and dashboards. If you relied on centered grid content, set **AlignContent** explicitly.

### [01415] DataTable footer aggregates

[**DataTable**](https://docs.ivy.app/widgets/advanced/data-table) **footer** cells now **format currency and numeric aggregates** consistently with column formatting (for example sums and averages in money and number columns).

### Image overlay keyboard navigation

When [**Image**](https://docs.ivy.app/widgets/primitives/image) **`Overlay`** is enabled, **arrow keys** move between **sibling** overlay images in sequence (for example gallery or documentation figures).

### Branding and shell colors

CSS adds an **`ivy-green`** (and related) **branding variable** for consistent Ivy identity. Follow-up tweaks **hard-code** or wire the **ivy brand** color where theme tokens needed it, and the **sidebar** uses the **`bg-secondary`** semantic variable for its background. **ThemeCustomizer** shows clearer **placeholders** in empty option boxes.

### AutoFocus on buttons and inputs

[**Button**](https://docs.ivy.app/widgets/common/button) and **input** widgets support **`AutoFocus`**, including focus when opening **Dialog** and **Sheet** content so the first field can take focus without an extra click.

### Tooling and security

The **`KillProcessUsingPort`** helper used in dev workflows now runs on **macOS** and **Linux** as well as Windows. **Security**-related analyzer and dependency **warnings** were addressed.

## Buttons, badges, and navigation

- [**Button**](https://docs.ivy.app/widgets/common/button) **badges** use the **outline** variant for clearer contrast; **unit tests** were added.
- [**Badge**](https://docs.ivy.app/widgets/common/badge): **empty** badge text renders **nothing** instead of a tiny pill.
- **Tab** (Content variant) and [**DropDownMenu**](https://docs.ivy.app/widgets/common/drop-down-menu) items support **badges** like other chrome.
- **Menu** items support **additional colors** for richer navigation.
- **Hover effects**: **`CardHoverVariant`** was renamed to **`HoverEffect`** and consolidated in **shared** APIs (used by Card, Box, Image, etc.); update call sites if you still reference the old name.
- **ThemeCustomizer**: visual treatment for **empty** placeholder boxes.
- **Confetti** animation: **shorter duration** and **fewer particles**.

## Image, Markdown, and tables

- [**Image**](https://docs.ivy.app/widgets/primitives/image): **`Overlay`** for **lightbox**-style viewing; **arrow keys** move between sibling images in a group.
- [**Markdown**](https://docs.ivy.app/widgets/primitives/markdown): **light border** on embedded images; **Graphviz** rendering; **samples and docs** for Graphviz; **spacing** refined (including **element-specific** gaps instead of one uniform markdown gap).
- [**Table**](https://docs.ivy.app/widgets/common/table) **border styling** aligned with **Markdown** tables.

## Sheet, dialog, tabs, and loading

- [**Sheet**](https://docs.ivy.app/widgets/advanced/sheet): **resizable drag handle**; **size handling** improvements; wiring for **resizable sheets** with explicit width (see also Bug fixes for non-FE items).
- **Dialog / Sheet** work better with **AutoFocus** on child inputs.
- **TabsLayout**: **`OnCloseOthers`** and **Close other tabs**; **tab order** synced on refresh to reduce flicker.

## Layout and chrome

- **Layout.Grid**: default alignment is **top-left** instead of center (set **AlignContent** if you depended on the old default).
- **HeaderLayout**: **scroll-triggered drop shadow** on the header.
- **FooterLayout**: same **scroll-triggered shadow** pattern.
- Shared **`useScrollShadow`** hook: **direction** parameter; **MutationObserver** + **requestAnimationFrame** optimizations for dynamic content; extracted from layout widgets and reused.
- **Container size**: **retry** of initial measurement for **nested flex** layouts.

## Widgets: progress, terminal, detail, diff

- **StackedProgress**: new **segmented** progress bar widget; **`OnSelect`** / **`Selected`**; labels can **auto-enable** segment labels; samples and layout polish (including wrapping in **Box** where appropriate).
- **Terminal**: **`Background`** and **`Foreground`** for theme-aware terminal colors.
- **Detail**: **`Multiline`** is optional and defaults to **`false`**.
- **DiffView**: **smaller** default font for denser diffs.

## Charts

- **Bar**, **Line**, **Area**, **Scatter**: **`YAxisIndex`** and **dual-axis** support; **`generateYAxis`** updates (including multi-axis and **largeSpread** behavior); **grid padding** and **whitespace** when axes are hidden.
- **BarChart**: vertical bar layout and **axis** behavior refined; **YAxis** **Hide** honored; documentation for **YAxisIndex** and dual-axis **Bar** records.
- **Line / Area / Scatter**: **`YAxisIndex`** on series types; **Scatter** avoids **category** axes where **numeric** axes are required; **ScatterChart** sample **dual-axis** example; **tests** and coverage for **ScatterChart**.
- **PieChart**: **tooltip** uses a **formatter** with **marker** styling.

## DataTable and querying

- **Search**: **match navigation**, **highlights**, and a **progress** indicator for large tables.
- **Columns**: **multiple virtual columns** from the same **root** property; **badge** **colors** in cells.
- **Scaling / schema**: optional **auto-exclusion** of **navigation collection** columns from scaling; **decimal** display improvements (**valueOf** with string fallback).
- **Expressions**: better support for **navigation properties** and **ternary** expressions in column expressions.
- **ToDetails()**: does not surface raw **type names** for navigation properties inappropriately.
- **Footer**: **currency / number** aggregates formatted correctly.
- **Query processor**: **default `OrderBy`** when pagination needs a stable order; **source order** preserved when sorting is disabled; tests and **`ViewBase.UseDataTable`** configuration surface.
- **Docs / agents**: **UseQuery + DataTable** anti-pattern clarified in documentation and agent instructions.
- **Link cells**: interaction when **links** and **`OnCellClick`** are both in play.
- **Tooltips**: **DataTable** uses the shared **tooltip** wrapper instead of relying on the native **`title`** attribute alone.

## Inputs and file uploads

- [**ContentInput**](https://docs.ivy.app/widgets/inputs/content-input): attachments, optional **`ShortcutKey`**, **density** scaling, **invalid** state samples, shared **FileAttachmentList**, **`validateFileWithToast`**, consolidated **upload URL** helpers, **`useUploadWithProgress`**, **client-side progress** via **`XMLHttpRequest`**, **FileDialog** integration, docs and **Playwright** notes.
- [**FolderInput**](https://docs.ivy.app/widgets/inputs/folder-input): **full-row** click to open picker; **`FolderInputMode`** and **full path** mode; **keyboard** (**Enter** / **Space**); **aria-label** on browse control; docs renumbered alongside other input docs.
- **FileInput**: **aria-label** on browse.
- **PasswordInput**, **Email**, **Tel**, **Url**, **Textarea**: **ShortcutKey** demos; **Ctrl+Enter / Cmd+Enter** to submit and blur **textarea** where applicable.
- **TextInput**: **keyboard** interaction **unit tests**; **shortcut** deduplication in affix demos; **dictation** hook cleanup; **modifier shortcuts** respected when focus is inside **text areas**.
- **SignatureInput**: **dark mode** via theme service; **unit tests** for color resolution.
- **Select**: list **placement** when items include placeholders (treated as a **behavior** improvement, not listed under Bug fixes).

## Code blocks and languages

- **Languages** enum: **`Description`** attributes for **display labels**; **Bash/Shell**, **PowerShell** (also in **CodeBlock** samples); **PowerShell** added in related enums/maps where applicable.
- **CodeBlock** sample: language grid layout and sample snippets updated.

## Accessibility

- Broad **WCAG**-oriented pass: **`aria-label`** on **tooltip** targets, **`role="button"`** surfaces, **browse** buttons on **FileInput** / **FolderInput**, and related **keyboard** affordances.
- **Tooltip**-wrapped components expose appropriate **accessible names** where needed.

## Routing, shell, and apps

- **URL**: **`?chrome=false`** **backwards compatibility** with newer shell/chrome flags; **AppRouter** tests moved into **`Ivy.Test`** (internals visibility).
- **Apps**: optional **`allowDuplicateTabs`** (opt out of duplicate-tab prevention) for apps that need multiple instances.
- **Routing dots** / breadcrumb indicator behavior **restored**.
- **Samples**: **Setup** sample renamed to **Settings** with **cogs** icon (where applicable in shared samples).

## Blades

- **`IBladeService`** renamed to **`IBladeContext`**. Update types and registrations; **hallucinations** / docs updated.

## Server, auth, and HTTP

- **OAuth** callback uses **`LocalRedirect`** instead of **`Redirect`** to avoid open-redirect issues.
- **SignalR** / **MessagePack** and hub **integration tests** for **`/ivy/messages`**.
- **WebApplicationFactory**-style **HTTP endpoint** integration tests; shared **test server** infrastructure consolidated (**Ivy.Integration.Tests**).
- **Mock HTTP** helpers shared for tests.

## Build, packaging, and native assets

- **Docker** publish: **rustserver** native binary retained on **clean** builds.
- **Embedded resources**: **logical names** normalized **cross-platform**; **Vite** **EmbeddedResource** race (**CS1566**) addressed; fixes for **macOS/Linux** asset **404s** and **blank screens**; duplicate **`app.UseFrontend` / `UseAssets`** removed; **App ID** **`assets`** collision resolved.
- **CI**: **NuGet** markdown **globbing** with **native target** injection; **pnpm** **`--frozen-lockfile`**; **frontend** **`package-lock.json`** / **`.npmrc`** ignored where appropriate; **widget** frontend **`.gitignore`** updates; **lockfile** regeneration for widget frontends.
- **NuGet**: **lock files** for **Ivy**, **Ivy.Agent.Filter**, **Ivy.Analyser**; **embed** source **docs** in packages so **nupkg** content stays complete after Rust/tooling changes.
- **Markdown** doc pipeline: converter **caching** uses **file content** (not only hash) for invalidation.
- **`KillProcessUsingPort`** supported on **macOS** and **Linux** (dev workflow).

## Diagnostics and client logging

- Client **diagnostic** **`logger.info`** downgraded to **`logger.debug`** to reduce noise.
- **Widget tree** refresh: exceptions in **`RefreshRequested`** and **`_RefreshView`** are caught so one failure does not tear down the tree.

## Tooling, analyzers, and repo hygiene

- **Roslyn**: **`IVYSERVICE001`** enforces calling **`UseService`** at the **top** of **`Build()`**.
- **IDE0005**: **unused `using`** cleanup configured; automated removal in many files.
- **Pre-commit** hook scoped to **frontend** files where appropriate.
- **`.gitattributes`**: **LF** line endings for **frontend** and **widget** frontends; line-ending **normalization** pass.
- **Hooks** directory **barrel** export (frontend); duplicate export fixes in barrel.
- **Security** / **code scanning** warning cleanups.
- **Local development**: **Ivy.Agent.Filter.Tests** setup; **ivy** command-not-found **Mac** troubleshooting doc.

## Documentation and AI guidance

- **Hallucinations** / **AGENTS** docs reorganized and expanded (including **IBladeContext**, **`Server.StartAsync`**, **`SelectOption<T>`**, **session** references, **compound widgets**, **UseLoading** page, chart **polish** callbacks, **BarChart** dual-axis, **Line/Area/Scatter** docs, **IvyFrameworkGotchas** consolidation, **Playwright** knowledge split into **focused files** and **widgets** consolidation, obsolete redirects removed).
- **IvyFrameworkVerification** updated for split Playwright knowledge; **prerequisite** steps and **process timeout** patterns documented.
- **Docs middleware**: **Ivy.Docs.Shared** delegates shared behavior to **Ivy.Docs.Helpers** to remove duplication.
- **Ivy Studio (cloud)**: stability and integration fixes for cloud-oriented Studio workflows.

## Tests (high level)

- **AppRouter** tests in **`Ivy.Test`**; **`Ivy.Tests`** added to the solution where introduced; **QueryProcessor**, **DataTable** container styles, **ScatterChart**, **useScrollShadow** / **MutationObserver**, **SignatureInput**, **useDictation**, **TextInput** keyboard, **shortcut** modules, **SignalR**, **HTTP** integration, **vite** test includes for **`.test.tsx`**, shared **MockState**, **coverlet** cleanup in **Ivy.Test** csproj, and other test **refactors** and **stability** work.

## Bug fixes

The following are **server, .NET, build, or non–UI-layer** fixes. **Frontend-only** widget/CSS/TypeScript fixes are **not** listed here per release policy.

- **DataTable (logic / serialization)**: **decimal** scaling with **`valueOf`** fallback; **footer** aggregates for **currency** and **numbers**; **navigation** properties and **ternary** expressions in column expressions; **link** cells when **`OnCellClick`** is configured; **source order** when sorting is disabled; **default `OrderBy`** for paged queries; **virtual columns** from the same root property behavior.
- **ToDetails()**: no longer shows **type names** for **navigation** properties in place of sensible display.
- **Arrow** serialization: **widget** objects serialize safely via **`ToString`** / sealed patterns where applicable.
- **Markdown pipeline (build/tooling)**: converter **cache** invalidation uses **file content** so edits are picked up reliably.
- **OAuth**: **LocalRedirect** in callback (see Server).
- **CI / build**: **NuGet** markdown globbing with **native** targets; **Docker** image includes **rustserver** binary on clean builds; **CS1566** **EmbeddedResource** race with **Vite**; **embedded resource** logical names **cross-platform**; **App ID** collision for **`assets`**.
- **Server / routing**: duplicate middleware registration for **frontend**/**assets** removed (stability).
- **C# / tests**: various **compilation** and **test project** reference fixes tied to **API** renames and **package** layout (non-FE).
