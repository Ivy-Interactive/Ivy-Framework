# Migrating to Ivy 1.2.34

This release introduces several breaking API changes as namespaces and APIs were consolidated and modernized. When updating projects to use Ivy 1.2.34, follow these instructions to reproduce, fix, and verify the changes.

## 1. Unified Namespace
**Change:** Sub-namespaces like `Ivy.Apps`, `Ivy.Auth`, `Ivy.Chrome`, `Ivy.Views`, `Ivy.Widgets` etc. have been removed. All framework types are now consolidated under the single `Ivy` namespace.
**Fix:**
- Remove any `global using Ivy.*` (e.g., `global using Ivy.Apps;`) from your `GlobalUsings.cs` or other files.
- Ensure `global using Ivy;` is present.

## 2. FormBuilder Submission Handling
**Change:** The `HandleSubmit` method on `FormBuilder<TModel>` has been renamed to `OnSubmit` to better align with the new event syntax.
**Fix:** 
- Find any `.HandleSubmit(async r => ...)` called on `Request.ToForm()` or `new FormBuilder(...)`.
- Change to `.OnSubmit(async r => ...)`.

## 3. ChromeSettings renamed to AppShellSettings
**Change:** `ChromeSettings` and the `server.UseChrome()` extension have been updated to reflect the new App Shell architecture.
**Fix:**
- Rename `ChromeSettings` to `AppShellSettings`.
- Rename `server.UseChrome(chromeSettings)` to `server.UseAppShell(chromeSettings)`.
- Existing builder methods like `.DefaultAppId()` and `.UseTabs(preventDuplicates: true)` function the same way.

## 4. SelectInput Constructor & State Binding
**Change:** The public constructors for `SelectInput` and `SelectInput<T>` take a different shape or are internal. 
**Fix:**
- Use the `.ToSelectInput(...)` extension method directly on your state.
- Before: `new SelectInput<string>(activeTab.Value, e => activeTab.Set(e.Value), scenarioOptions.ToOptions())`.
- After: `activeTab.ToSelectInput(scenarioOptions)`. (The `.ToOptions()` call is also no longer necessary if `scenarioOptions` is a simple string collection).

## 5. Text API Changes for Inline Code
**Change:** `Text.InlineCode()` has been removed in favor of `Text.Monospaced()` or `Text.Code()`.
**Fix:**
- Replace `Text.InlineCode(content)` with `Text.Monospaced(content)` for inline references.

## Verification
- Run `dotnet build` to verify no CS0246 (type not found) or CS1061 (method not found) compilation errors persist.
- Run `dotnet test` to ensure basic binding logic works.
