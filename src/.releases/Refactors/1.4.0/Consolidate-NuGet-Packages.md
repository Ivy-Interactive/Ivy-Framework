# Consolidate NuGet Packages Into Ivy, Ivy.Auth and Ivy.Desktop

## Summary

Ivy now publishes **three** NuGet packages instead of 23: `Ivy`, `Ivy.Auth` and `Ivy.Desktop`.
Everything that used to be its own package is now compiled into one of those three. **Namespaces and
type names are unchanged**, so source code keeps compiling once the old `PackageReference`s are
removed.

## What Changed

### Retired package IDs (20)

| Retired package | Now shipped in | Namespace |
| --- | --- | --- |
| `Ivy.Widgets.ActivityHeatmap` | `Ivy` | `Ivy.Widgets.ActivityHeatmap` |
| `Ivy.Widgets.AnimatedStatusLabel` | `Ivy` | `Ivy.Widgets.AnimatedStatusLabel` |
| `Ivy.Widgets.DiffView` | `Ivy` | `Ivy.Widgets.DiffView` |
| `Ivy.Widgets.Leaflet` | `Ivy` | `Ivy.Widgets.Leaflet` |
| `Ivy.Widgets.QRCode` | `Ivy` | `Ivy.Widgets.QRCode` |
| `Ivy.Widgets.ScreenshotFeedback` | `Ivy` | `Ivy.Widgets.ScreenshotFeedback` |
| `Ivy.Widgets.Tiptap` | `Ivy` | `Ivy.Widgets.Tiptap` |
| `Ivy.Widgets.Xterm` | `Ivy` | `Ivy.Widgets.Xterm` |
| `Ivy.Agent.Filter` | `Ivy` | `Ivy.Agent.Filter` |
| `Ivy.Hooks.Pty` | `Ivy` | `Ivy.Hooks.Pty` |
| `Ivy.Analyser` | `Ivy` (`analyzers/dotnet/cs/`) | n/a |
| `Ivy.Plugin.Abstractions` | `Ivy` (`lib/net10.0/`) | `Ivy.Plugins` |
| `Ivy.Auth.Auth0` | `Ivy.Auth` | `Ivy.Auth.Auth0` |
| `Ivy.Auth.Authelia` | `Ivy.Auth` | `Ivy.Auth.Authelia` |
| `Ivy.Auth.Clerk` | `Ivy.Auth` | `Ivy.Auth.Clerk` |
| `Ivy.Auth.GitHub` | `Ivy.Auth` | `Ivy.Auth.GitHub` |
| `Ivy.Auth.Google` | `Ivy.Auth` | `Ivy.Auth.Google` |
| `Ivy.Auth.MicrosoftEntra` | `Ivy.Auth` | `Ivy.Auth.MicrosoftEntra` |
| `Ivy.Auth.Sliplane` | `Ivy.Auth` | `Ivy.Auth.Sliplane` |
| `Ivy.Auth.Supabase` | `Ivy.Auth` | `Ivy.Auth.Supabase` |

`Ivy` and `Ivy.Desktop` keep their IDs and contents (plus, for `Ivy`, everything above).

### Assemblies that no longer exist

`Ivy.Widgets.*.dll`, `Ivy.Agent.Filter.dll` and `Ivy.Hooks.Pty.dll` are gone — their types are
compiled into `Ivy.dll` under the same namespaces. This is **source-compatible but
binary-breaking**: anything compiled directly against those assemblies must be recompiled. No facade
assemblies are shipped.

`Ivy.Plugin.Abstractions.dll` still exists as a separate assembly with the same public surface; only
its delivery vehicle changed.

### Analyzer is now on by default

`Ivy.Analyser` used to be opt-in. It now ships inside `Ivy` under `analyzers/dotnet/cs/`, so its
diagnostics apply to every project that references `Ivy`. Rules that report as **errors** (the Rules
of Hooks diagnostics) can fail a build that previously succeeded. Fix the violations, or suppress
specific IDs in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.IVYHOOK001.severity = warning
```

### `Ivy.Auth` dependency surface

`Ivy.Auth` is the union of the eight provider packages, so installing it pulls in every provider's
dependencies — Auth0, Azure.Identity, Microsoft.Graph, Microsoft.Identity.Client,
Microsoft.IdentityModel.Protocols, Microsoft.Kiota.Abstractions and Supabase — even if you only use
one provider. This is the deliberate trade-off for a single signed package.

## Migration Path

1. **Remove the retired `PackageReference`s.** A stale reference resolves the same assembly from two
   packages (or an assembly that no longer exists), which surfaces as `NU1605`/duplicate-type or
   `MissingMethodException` errors:

   ```diff
   - <PackageReference Include="Ivy.Widgets.Xterm" Version="1.0.0" />
   - <PackageReference Include="Ivy.Hooks.Pty" Version="1.3.21" />
   - <PackageReference Include="Ivy.Plugin.Abstractions" Version="1.3.21" />
   - <PackageReference Include="Ivy.Analyser" Version="1.3.21" />
     <PackageReference Include="Ivy" Version="1.4.0" />
   ```

2. **Replace per-provider auth packages with `Ivy.Auth`:**

   ```diff
   - <PackageReference Include="Ivy.Auth.Auth0" Version="1.3.21" />
   - <PackageReference Include="Ivy.Auth.Google" Version="1.3.21" />
   + <PackageReference Include="Ivy.Auth" Version="1.4.0" />
   ```

3. **Leave `using` directives alone.** Every namespace is unchanged, including
   `Ivy.Widgets.Xterm.Terminal` and the per-provider auth namespaces.

4. **Plugin abstraction packages** that referenced `Ivy.Plugin.Abstractions` now reference `Ivy`
   instead. The type-identity split still holds — keep `Ivy` types out of your base abstractions
   assembly — but the base package now carries `Ivy` as a build-time dependency. See
   [Host Abstractions](../../../Ivy.Docs.Shared/Docs/01_Onboarding/02_Concepts/27_Plugins/03_HostAbstractions.md).

5. **Fix or suppress any new analyzer errors** (see above).

## Version

Minor bump to **1.4.0**. `CheckSharedAssemblyCompatibility` in `PluginLoader` uses the minor version
to reject incompatible old plugins gracefully instead of failing at runtime, so plugins built against
1.3.x are gated rather than crashing.

`Ivy`'s `PackageValidationBaselineVersion` stays at 1.2.67: the change is purely additive to
`Ivy.dll`'s public surface.
