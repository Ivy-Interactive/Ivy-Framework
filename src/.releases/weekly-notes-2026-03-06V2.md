
# Ivy Framework Weekly Notes - Week of 2026-03-06

This week brings a massive wave of improvements across the entire framework, spanning new widgets, vast enhancements to existing ones, API overhauls, and better developer experience setups. 

## 💥 Breaking Changes & Major API Refactors

### Consistent Event Handlers (`On*` replace `Handle*`)
We have completely refactored widget event handlers to use a unified `On*` naming convention. Extension methods now match the property names, delivering a cleaner and more intuitive developer experience (e.g., `OnClick` instead of `HandleClick`, `OnSubmit` instead of `HandleSubmit`).
*(Note: `MenuItem.HandleSelect` remains unchanged, as it sets an `OnSelect` property, rather than behaving as a direct widget event handler.)*

### Hook Syntax Simplification
The `.releases` docs and syntax have been modernized. The `this.` prefix is no longer required for hooks. 
- **Before:** `this.UseState(0)`
- **After:** `UseState(0)`

### Naming Standardization
- Changed `MultiLine` to `Multiline` across builders and inputs.
- Changed `TextArea` to `Textarea` for HTML spec alignment across `TextInputVariants`.
- Renamed `AudioRecorder` widget to `AudioInput`.
- Consolidated input type enums to a consistent `*InputVariant` naming convention.

## ✨ New Features & Widget Enhancements

### SelectInput & Variants
The `SelectInput` and all of its variants (List, Toggle, Radio, MultiSelect) saw massive enhancements:
- **Search & Limits:** Native search, loading state, and selection limit support.
- **Disabled Options:** Per-item `.Disabled()` support on `Option<T>`.
- **Ghost Styling:** A new `.Ghost()` API removes borders and background fills for minimal, inline designs.

### Interactive Components 
- **Tree Widget:** Added the new `Tree` widget for hierarchical data! Tree nodes can now be expanded on label clicks, and `Tree.HandleRowAction` was added for triggering item action menus.
- **Badge & Box:** Both widgets now support `.OnClick()` event handlers. `Box` also now supports `HoverVariant` properties to create interactive regions.
- **Card:** `Card` widgets now support `.Disabled()`, visually dimming them and disabling clicks.
- **Expandable:** Added `.Icon()` support to `Expandable` headers.
- **Sheet:** Added `Side` API allowing sheets to slide in from `.Side(SheetSide.Top)` or `Left`, `Right`, `Bottom`.
- **Progress:** Added `.Indeterminate()` support for animated looping when percentages are unknown, and standardized color properties.
- **Html:** You can safely opt-in to script tag executions inside HTML widgets via `.DangerouslyAllowScripts()`.
- **Separator:** Added `TextAlign` property to align labels `Left`, `Center`, or `Right`.

### Layouts & Data Presentation
- **SidebarLayout:** Fully dynamic dragging enabled via `.Resizable()`. Using the Size API, you can easily set min and max constraints (e.g., `.Resizable().SidebarWidth(Size.Min("200px").Max("600px"))`).
- **Table:** Added a new `.Progress()` builder for rendering inline progress bars in Table cells.
- **CodeBlock:** (Renamed from Code widget to CodeBlock). Added `.WrapLines()` to prevent horizontal scrolling on lengthy snippets, and `.StartingLineNumber()` to correctly offset line counts in excerpts.
- **Markdown:** Added `TextAlignment` property and fixed missing borders on code blocks.
- **TextBlock:** Added `TextAlignment` support.
- **Spacer:** A bare `new Spacer()` now intelligently defaults to full `flex-grow` rendering without explicitly needing `.Width(Size.Grow())`.
- **DataTable:** Addressed a large suite of features: Refresh token support for explicit re-rendering, automatic data refresh on data source changes, and dynamic scrollbars that only appear when actually needed.
- **Grid Container:** Full-bleed mode without DOM hacks was reintroduced.
- **ListWidget:** Removed parent padding hack.

### Text & Number Inputs
- `NumberInput` now supports **Prefix** and **Suffix** properties, automatically placing currency symbols or unit labels inside the field.
- `TextInput` now natively supports `.OnSubmit()` events for Enter-key handling, removing the need for `Form` wrappers in simple search or quick-add configurations, and now supports `.MinLength()` validation.
- `FileInput` gets `.MinFileSize()` validation to safely reject empty/erroneous files.

## 🛠 Framework & Internal Fixes

### General Framework Updates
- **Default Theme:** The default framework theme has been changed from `light` to `system`, instantly respecting the user's OS-level dark/light mode preference.
- **Auth Integrations:** Added support for Sliplane Authentication seamlessly via the new `SliplaneAuthProvider` (including mapping for the `/v0/me` endpoint).
- **ErrorSheet:** Improved error display styling. Call stacks are now properly scroll-scoped within the error code block to keep headers sticky.
- **Dompurify Update:** Updated the internal `dompurify` dependency to version `3.3.2`.
- **Issues fixed:** Avoided overlaying of kanban cards, conditional prop rendering fixes, and various bag of warnings securely handled.

### Documentation & CLI
- Listed all widgets correctly in the Widget Library documentation table.
- Added comprehensive MCP getting-started documentation with external server links.
- Added CLI upgrade docs page.
- Added detailed examples for the `UseQuery` hook and other advanced API usages.
