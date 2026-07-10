# Ivy Framework Weekly Notes - Week of 2026-07-03

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release focuses on enabling dynamic self-signed certificate configuration for macOS and Windows to support HTTPS locally, introducing a new Tools slot to Field inputs, resolving several Dependabot security vulnerabilities, fixing a layout crash on the Details docs page, and improving copy button sizing.

## New Features

### Core & CLI
- **Desktop HTTPS Support**: Enabled HTTPS on macOS/Windows by dynamically generating and configuring a local self-signed certificate for desktop app hosting, ensuring fully secured local dev environments automatically.

### UI & Layout
- **Field Tools Slot**: Added an optional `Tools` slot to the `Field` input wrapper. This enables developer-defined widgets (like clear buttons, copy widgets, or tooltips) to render adjacent to fields. In `LabelPosition.Top` mode, they align to the right of the label; in `LabelPosition.Left` they render directly under the label.

  ```csharp
  TextInput.Name.WithField()
      .Label("Name")
      .Tools(new Button("Clear", click: () => { ... }))
  ```

## Bug Fixes and Improvements

### Core & Server
- **Details Docs Page Crash**: Fixed a `TargetInvocationException` crash (issue #4550) on the Details docs page. The `DetailsBuilder` now correctly excludes `IView` and `ViewBase` types from navigation property classification, passing them directly to the renderer via `DefaultBuilder`.
- **Serialization Performance**: Optimized widget serialization speed by caching compiled lambda getters within the `WidgetSerializer`.

### Security
- **NuGet Dependency Audit**: Removed unused `Microsoft.AspNetCore.OpenApi` and `Microsoft.OpenApi` NuGet dependencies, resolving Dependabot alert #238 (high-severity vulnerability).
- **Chart Security Update**: Upgraded frontend dependency `echarts` from `6.0.0` to `6.1.0` to address vulnerability alerts #239 and #240 (moderate-severity XSS).

### UI & Layout
- **Copy Button Sizing**: Fixed `CopyToClipboardButton` rendering issues by using `controlSize` instead of `controlHeight` and removing custom padding to ensure a clean, square aspect ratio.
- **Kbd Primitive Cleanup**: Refactored the `Kbd` component's `Content` property from `string?` to a non-nullable `string` defaulting to `string.Empty` for consistency with other primitive text controls.
