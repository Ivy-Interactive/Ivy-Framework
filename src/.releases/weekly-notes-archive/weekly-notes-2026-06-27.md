# Ivy Framework Weekly Notes - Week of 2026-06-27

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This week's release focuses on improving the robustness of the Ivy plugin loader and resolving assembly loading conflicts during local plugin development and discovery.

## Bug Fixes and Improvements

### Plugin Loader & Assembly Resolution

- **Plugin Deduplication by Manifest ID**: Fixed an issue where a plugin registered in both `plugin-references.yaml` and located in the plugins folder would load twice and cause duplicate registrations. The loader now deduplicates plugins by their manifest ID, giving priority to explicit references.
- **Deduplication of Debug/Release DLLs**: When a source plugin has both `bin/Debug` and `bin/Release` build directories present, the loader would previously discover both DLLs and reject them due to duplicate `IvyPlugin` attributes. It now deduplicates by filename and prefers `Debug` builds for local loading.
- **Shared Assembly Version Resolution**: Fixed a loader failure where a plugin compiled against a specific NuGet release (e.g. `Ivy.Plugin.Abstractions 1.2.70`) failed to load on a local development host (running version `0.0.0.0`) because assembly resolution via native type loading failed to map back to the Default load context. Shared assemblies are now explicitly resolved and loaded by name from the host's default context.
