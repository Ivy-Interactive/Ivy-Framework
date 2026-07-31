---
searchHints:
  - plugin-compatibility
  - minimumhostversion
  - binary-compatibility
  - default-interface-methods
  - package-validation
  - plugin-versioning
  - shared-assembly-version
---

# Version Compatibility

<Ingress>
Plugins are compiled against one version of your abstractions and run against another. Discipline is important to avoid plugin load failures or runtime crashes.
</Ingress>

## The Problem

Shared assemblies are resolved **by simple name, ignoring version**. A plugin built against version 1.2 of your abstractions package runs against the host's 1.4 copy — that is deliberate, and it is what lets plugins keep working as your host evolves.

The consequence is that **every public member of every shared assembly is part of your plugin contract**. Remove a method and a plugin that calls it throws `MissingMethodException` at runtime, with nothing at compile time to warn you, because the plugin was never recompiled.

## The Gates the Framework Enforces

Three checks run before a plugin's code executes. Failing any of these checks will result in the plugin entering a failed state, with a reason you can show the user.

### Minimum Host Version

A plugin can declare a floor:

```csharp
public PluginManifest Manifest { get; } = new()
{
    Id = "Acme.Plugin.Exporter",
    Title = "CSV Exporter",
    MinimumHostVersion = new Version(2, 1),
};
```

On an older host the plugin is refused with `"Requires host version 2.1 but current is 2.0.0.0"`. The host version is whatever you passed to `UsePlugins`, defaulting to the entry assembly's version. It is up to the plugin author to voluntarily bump their `MinimumHostVersion` whenever they start using a newly-added context member.

### Shared Assembly Versions

Before loading, the framework reads the plugin's `.deps.json`, picks out every package that matches a shared assembly name, and compares the version the plugin was built against with the version the host actually has:

```text
Requires Acme.Plugin.Abstractions >= 2.0.0 but host provides 1.4.0
```

The comparison is on **major and minor only** — patch and revision are ignored, so a plugin built against 1.4.3 loads happily on a host with 1.4.0.

<Callout Type="warning">
**Bump the minor version of your abstractions package whenever you make a breaking change.**"
</Callout>

Two cases skip the check: a plugin with no `.deps.json`, and a host assembly version of `0.0.0.0` — which is what a source build with `GenerateAssemblyInfo=false` produces. Local development therefore does not exercise this gate.

### Type Identity

If a plugin still fails to instantiate — because it was built against a genuinely different shape of the abstractions — the loader reports `"Incompatible plugin type …"` and moves on rather than taking the host down. Separately, a plugin whose declared `TContext` isn't satisfied by the host's context gets a message naming both types.

Both of these are backstops. If you are seeing them, one of the two gates above should have caught the problem earlier.

## Rules for Evolving Your Abstractions

These are the rules the framework follows for its own plugin surface. Apply them to yours.

### Never

- Remove or rename a public type, method, property, or enum value.
- Add a required parameter to an existing constructor or method.
- Change the return type of an existing method or property.
- Remove an interface implementation from a public type.
- Change generic arity.
- Renumber or reorder enum values — compiled IL uses the integer, not the name.
- Add an abstract member to a type a plugin might inherit.

### Grow Constructors Additively

Plugins construct your types directly. Add an overload, don't extend the existing signature:

```csharp
// Wrong — breaks every plugin passing two arguments
public ExportOptions(string delimiter, bool includeHeader, Encoding encoding) { }

// Right — a new overload; old call sites still resolve
public ExportOptions(string delimiter, bool includeHeader)
    : this(delimiter, includeHeader, Encoding.UTF8) { }
public ExportOptions(string delimiter, bool includeHeader, Encoding encoding) { }
```

### Interfaces Plugins Implement Grow Only Through Default Methods

A plugin *implements* your channel, handler, and provider interfaces. Adding an abstract member to one breaks every plugin binary that already exists. Give new members a default implementation:

```csharp
public interface IExporter
{
    string Format { get; }
    Task ExportAsync(Stream target, CancellationToken ct = default);

    // Added in 1.5 — a default keeps existing plugins loading
    bool SupportsStreaming => false;
}
```

Write that expectation into the interface's own doc comment, so the next person to touch it knows:

```csharp
/// COMPATIBILITY NOTE: All future members added to this interface MUST provide
/// a default implementation to avoid breaking existing plugin implementations.
```

### Interfaces Plugins Receive Grow Freely

Your context interfaces are received by plugins, never implemented by them. Add members whenever you like — a plugin that doesn't call them is unaffected. This asymmetry is a good reason to keep contributions flowing through a context rather than through interfaces plugins implement.

### Records Plugins Construct Grow Through Optional `init` Properties

Never add a `required` member to a type plugins construct. The framework applies this to itself — `PluginManifest` carries the rule as a comment:

```csharp
/// COMPATIBILITY NOTE: Only optional (nullable, non-required) properties may be added
/// to this record going forward. Adding new 'required' properties is a breaking change
/// for all existing plugins.
```

### Extension Methods Are the Safest Growth Path

An extension method has no binary coupling to the type it extends. Old plugins are completely unaffected by one being added. Where a new capability can be expressed as an extension method over `Services`, `Config`, or your context, prefer that.

## Designing for Forward Compatibility

The gates above handle old plugin against new host. The reverse — a plugin sending your host a value the host doesn't understand yet — needs a decision in the type itself.

Give such types a fallback the older host can use:

```csharp
public abstract record ExportField
{
    /// Plain-text fallback for hosts that don't recognize this field type.
    public virtual string? FallbackText => null;
}
```

An older host that meets an `ExportField` subtype it has never seen renders the fallback instead of throwing.

The same reasoning argues against enums at extension points. An enum cannot gain a member safely from the consumer's side — a host switching on it hits the default branch with no idea what it was given. A polymorphic type with a fallback degrades; an enum does not.

## Verify It Mechanically

Turn on package validation in your abstractions projects so an accidental break fails the build rather than a customer's host:

```xml
<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <PackageValidationBaselineVersion>1.4.0</PackageValidationBaselineVersion>
</PropertyGroup>
```

The pack step then compares your API surface against the published baseline. Deliberate breaks are recorded in a `CompatibilitySuppressions.xml` next to the csproj, which doubles as a written record of every break you have made. Both `Ivy` and `Ivy.Plugin.Abstractions` are built this way.

## A Checklist for Releasing Abstractions

- Base and extended packages carry the **same version** and ship together.
- Patch releases are additive only — new members with defaults, new overloads, new extension methods.
- A minor bump accompanies anything the shared-assembly check should catch.
- Deliberate breaks are documented where plugin authors will actually read it, with the minimum version that has the change.
- Package validation passes, and any new suppression was added on purpose.

## See Also

- [Host Abstractions](./03_HostAbstractions.md)
- [Hosting Plugins](./02_HostingPlugins.md)
- [Writing Plugins](./04_WritingPlugins.md)
- [Plugins Overview](./01_PluginsOverview.md)
