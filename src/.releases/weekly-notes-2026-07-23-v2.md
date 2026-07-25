# Ivy Framework Weekly Notes - Week of 2026-07-23 (Release 2)

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release simplifies plugin versioning by deriving version metadata dynamically from project/package manifests and improves error capture for incompatible or legacy plugins.

## New Features & Improvements

### Plugins
- **Dynamic Plugin Versioning**: Removed the hardcoded `Version` property from `PluginManifest`. Plugin versions are now resolved automatically at runtime from the `.nuspec` package metadata or `.csproj` file, eliminating version mismatch errors when package versions change.
- **Incompatible Plugin Capture**: Added exception handling for type-compatibility and instantiation errors (e.g., `MissingMethodException` from legacy plugins compiled against older manifest contracts). Incompatible plugins now report detailed failure reasons and render in the UI's failed plugins list.
