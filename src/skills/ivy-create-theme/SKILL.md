---
name: ivy-create-theme
description: >
  Create or modify a custom Ivy theme. Use when the user asks to create a theme, change
  colors, update typography, modify the look and feel, customize the UI appearance, set
  dark mode colors, or adjust border radius, spacing, or other visual styling in their
  Ivy project.
allowed-tools: Bash(dotnet:*) Bash(ivy:*) Read Write Edit Glob Grep
effort: medium
---

# ivy-create-theme

Create or modify a custom theme in an Ivy project.

## Getting Started

1. Use `ivy docs` to read about theming (query: "theming" or "custom theme").
2. Check if the user's `Program.cs` already has a `UseTheme(...)` call.
   - If it does, follow the **Modify an Existing Theme** path.
   - If it does not, follow the **Create a New Theme** path.

## Path A: Create a New Theme

1. If it is not clear from context, ask the user for details about the theme they want to create (colors, typography, dark/light preferences, etc.).

2. Create a file at `Themes/[ThemeName].cs` with a class that extends `Theme`:

```csharp
using Ivy;

namespace [ProjectNamespace].Themes;

public class MyTheme : Theme
{
    public MyTheme()
    {
        Name = "MyTheme";
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors { /* ... */ },
            Dark = new ThemeColors { /* ... */ }
        };
        ...
    }
}
```

3. In `Program.cs`, add `.UseTheme(new MyTheme())` to the server builder chain. This MUST be BEFORE `.Run()`. The framework auto-detects the subclass and creates a factory via `Activator.CreateInstance` for hot reload.

### Inline Configuration Approach

If the user prefers inline configuration instead of a separate class:

In `Program.cs`, add `.UseTheme(theme => { ... })` to the server builder chain. This MUST be BEFORE `.Run()`. The framework wraps the callback in a factory for hot reload.

```csharp
.UseTheme(theme =>
{
    theme.Name = "MyTheme";
    theme.Colors = new ThemeColorScheme
    {
        Light = new ThemeColors { /* ... */ },
        Dark = new ThemeColors { /* ... */ }
    };
})
```

## Path B: Modify an Existing Theme

1. Find the existing `UseTheme(...)` call in `Program.cs`. Determine which pattern is used:
   - `UseTheme(new MyTheme())` -- find and edit the theme subclass file
   - `UseTheme(theme => { ... })` -- edit the inline callback in Program.cs
   - `UseTheme(SomeTheme.Create)` -- find and edit the factory method

2. Ask the user what they want to change (colors, typography, border radius, etc.) if it is not already clear from context.

3. Apply the changes to the theme definition.

## Verification

1. Run `dotnet build` to verify everything compiles. Fix any errors.

2. If the user asks to commit, create a git commit with a descriptive message.

3. Present a short summary to the user describing what was created or changed.
