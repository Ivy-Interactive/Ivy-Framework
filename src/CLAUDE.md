# Plugin API Compatibility

Plugins share the host's `Ivy` and `Ivy.Plugin.Abstractions` assemblies at runtime via a shared
`AssemblyLoadContext`. Old plugin binaries run against newer host assemblies. **Every public type,
method, property, constructor, and enum value in these assemblies is part of the plugin contract.**
A signature change can cause `MissingMethodException` for deployed plugins.

API compatibility is enforced in CI via `EnablePackageValidation`. If your change breaks the API
surface, the pack step will fail. See "Handling intentional breaks" below.

## Rules

### NEVER (binary-breaking for deployed plugins):

- Remove or rename any public type, method, property, or enum value
- Add required parameters to existing constructors or method signatures
- Change return types of existing methods or properties
- Remove interface implementations from a public type
- Change generic arity (e.g. `IState<T>` to `IState<T, TMeta>`)
- Renumber or reorder enum values (compiled IL uses the integer value)
- Add abstract members to base classes plugins may inherit (e.g. `ViewBase`)

### Widget constructors — growth must be additive:

Plugins construct widgets directly (`new Button(...)`, `new Card(...)`, etc.).

```csharp
// WRONG — breaks plugins that pass 2 args
public Button(string label, Action onClick, ButtonStyle style) { }

// RIGHT — new overload, old call sites still resolve
public Button(string label, Action onClick) : this(label, onClick, ButtonStyle.Default) { }
public Button(string label, Action onClick, ButtonStyle style) { }

// ALSO RIGHT — init property for new optional feature
public class Button {
    public ButtonStyle Style { get; init; } = ButtonStyle.Default;
}
```

### Hooks — signatures are frozen:

```csharp
// Return types and parameter types cannot change. New overloads are safe.
IState<T> UseState<T>(T initial);                        // frozen
IState<T> UseState<T>(T initial, StateOptions options);  // new overload — safe
```

### Interfaces plugins IMPLEMENT — require Default Interface Methods:

`IIvyPlugin`, `IMessagingChannel`, and any interface a plugin class implements:

```csharp
// WRONG — old plugins lack this, runtime failure
Task OnEventAsync(PluginEvent evt);

// RIGHT — DIM keeps old binaries working
Task OnEventAsync(PluginEvent evt) => Task.CompletedTask;
```

### Interfaces plugins CALL INTO — safe to extend:

Context interfaces (`IIvyPluginContext`, `IIvyExtendedPluginContext`, `ITendrilPluginContext`, etc.)
are received by plugins, not implemented. New members can be added freely.

### Records/classes plugins construct — init-only growth:

Types like `MenuItem`, `AppDescriptor`, `PluginManifest`:
- Never add required constructor parameters
- Add new features as `init` properties with defaults
- Prefer new constructor overloads over growing existing parameter lists

### Extension methods are the safest growth path:

They have no binary coupling to the target type. Old plugins are completely unaffected.

## Handling intentional breaks

If a breaking change is truly necessary:
1. Rebuild with `/p:ApiCompatGenerateSuppressionFile=true` to generate/update the
   `CompatibilitySuppressions.xml` in the affected project
2. Document the migration in `src/.releases/Refactors/`
3. Bump the minor version so `CheckSharedAssemblyCompatibility` in PluginLoader rejects
   incompatible old plugins gracefully rather than crashing at runtime
