---
name: ivy-create-app
description: >
  Create a custom Ivy app with any combination of views. Use when the user asks for
  an app that is not a standard dashboard or CRUD screen -- chat interfaces, tools,
  custom layouts, SOAP/WSDL integrations, streaming UIs, wizards, or any bespoke
  application. Handles layout composition, connections, streaming chat with IChatClient,
  and custom view hierarchies.
allowed-tools: Bash(dotnet:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[description of the app to create]"
---

# ivy-create-app

Create an ad hoc Ivy app with any combination of views.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-app.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Reference Files

Read these before implementing:
- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference (widgets, hooks, layouts, inputs, colors)
- [references/ChatApp.cs](references/ChatApp.cs) -- streaming chat app with IChatClient, ImmutableArray messages
- [references/OpenAIConnection.cs](references/OpenAIConnection.cs) -- IConnection implementation for external services
- [references/DesignGuidelines.md](references/DesignGuidelines.md) -- layout patterns, spacing, UX interaction rules

## Workflow

This skill guides you through creating custom Ivy applications:

1. **Plan** - Understand the user's request and design the app structure
2. **Review** - Present the plan for user approval (accept / request changes / cancel)
3. **Implement** - Generate all app files following reference patterns

## Step 1: Plan the App

If the app requires data from a database, use `ivy cli explain connections/{ConnectionName}` to get the connection schema first.

If the user's prompt includes a `.wsdl` URL or mentions SOAP/WSDL, the app MUST use a SOAP connection (`IConnection`) rather than hardcoding HTTP calls. Any external service access must go through an `IConnection` implementation.

### Analysis

- Determine what views and components the app needs.
- **Multiple entities = multiple apps.** If the request involves distinct data entities (e.g., Contacts, Deals, Activities), plan a **separate app for each entity** rather than combining them into tabs within a single app. A Dashboard app may reference data from multiple entities, but each entity's CRUD/management screen should be its own app.
- **TabsLayout** is for showing different views or aspects of the **same** data (e.g., chart vs. table view, settings categories, wizard steps) -- not for combining unrelated entity screens.
- Choose a Lucide icon for the app (PascalCase, e.g. `ShoppingBag`, `LayoutDashboard`, `FileText`, `Users`).

### Plan Format

```
# Plan

## Create App: [AppName]

**Connection**: `[ConnectionName or "None"]`
**App Attribute**: `[App(icon: Icons.[LucideIcon])]`
**Note**: The `group` parameter defaults to `["Apps"]` at runtime. Only specify `group` explicitly when assigning the app to a different group (e.g., `group: ["Tools"]`, `group: ["Settings"]`).
**Layout**: `[Blades / Dashboard / Custom]`
**Chrome**: `[UseAppShell / UseDefaultApp]`
**Files**: `Apps\[AppName]App.cs`
**Namespace**: `{ProjectNamespace}.Apps`

[Description of the app entry point and layout.]

### Create View: [ViewName]

**Files**: `Apps\[AppFolder]\[ViewName].cs`
**Namespace**: `{ProjectNamespace}.Apps.[AppFolder]`

[Description of what this view does, what data it shows, what interactions it supports.]
```

### Chrome Decision
- If the app has **only one app class** and no multi-app navigation needs, set Chrome to `UseDefaultApp`. This skips the sidebar.
- If the app has **multiple app classes** or the user explicitly wants sidebar navigation, set Chrome to `UseAppShell`.
- Include a `### Update Program.cs` section at the end of the plan.

### Data File Ordering
If the plan includes files with large static datasets (lookup tables, seed data, icon/emoji lists, etc.):
- These files should be created **last**, after all UI and logic files.
- Prefer programmatic generation or embedded resources over inlined literals.

Present the plan to the user and ask them to **Accept**, **Request Changes**, or **Cancel**.

## Step 2: Handle Changes

If the user requests changes, apply them to the plan and present the updated plan again.

## Step 3: Implement

Once approved, implement all app files.

### Namespace Conventions
- App files (`[AppName]App.cs`): `namespace {ProjectNamespace}.Apps;`
- View files: `namespace {ProjectNamespace}.Apps.[AppFolder];`

### Required Using Directives

Every generated `.cs` file must include explicit `using` statements. Do NOT rely on global usings.

**Connection usings (when using a database connection):**
- `using {ProjectNamespace}.Connections.{ConnectionName};` -- for context factory
- `using {ProjectNamespace}.Connections.{ConnectionName}.Models;` -- for entity classes (only if project uses a Models subfolder)
- `using Microsoft.EntityFrameworkCore;` -- for LINQ extensions

**Common Ivy usings by view type:**

| View Type | Required Namespaces |
|---|---|
| All views | `Ivy`, `Ivy.Core.Hooks`, `Ivy.Hooks`, `Ivy.Shared`, `Ivy.Views` |
| App entry point | + `Ivy.Apps` |
| Blades | + `Ivy.Views.Blades` |
| Details views | + `Ivy.Views.Builders` |
| Forms (dialogs, sheets) | + `Ivy.Views.Forms`, `System.ComponentModel.DataAnnotations` |
| Tables | + `Ivy.Views.Tables` |
| Alerts/Callouts | + `Ivy.Views.Alerts` |
| Async select inputs | + `Ivy.Widgets.Inputs` |
| Charts | + `Ivy.Views.Charts` |
| Metrics/Dashboard | + `Ivy.Views.Dashboards` |

### App Entry Point

Use `[App(icon: Icons.[Icon], group: ["Apps"])]` attribute. The icon and group are parameters **inside** the `[App(...)]` attribute -- they are NOT separate attributes.

Common Hallucinations -- Do NOT Use:
- **`[Icon("...")]`** -- NOT a valid Ivy attribute
- **`[Group("...")]`** -- NOT a valid Ivy attribute
- **`[Description("...")]`** -- NOT a valid Ivy attribute
- All metadata belongs as named parameters on the single `[App(...)]` attribute

### App Attribute - Group Parameter

The `[App]` attribute's `group` parameter is nullable and has a runtime default value of `["Apps"]`.
- Omit the `group` parameter when using the default "Apps" group
- Explicitly specify `group` only for non-default groups
- **CRITICAL**: When a specification explicitly defines a non-default `group` value, you MUST include it in the implementation

### Program.cs Chrome Configuration
- If `UseDefaultApp`: Open `Program.cs` and **remove** the `server.UseAppShell(...)` line entirely, then add `server.UseDefaultApp(typeof(AppClassName));`. Do NOT keep both.
- If `UseAppShell`: Ensure `server.UseAppShell(new AppShellSettings().DefaultApp<AppClassName>().UseTabs(preventDuplicates: true));` is present.
- Follow the approved plan exactly.

### Critical Code Rules
- Use modern C# features: file-scoped namespaces, primary constructors, collection expressions.
- Write UI, logic, and app entry-point files **before** any data/seed files.
- If a file will contain >50 static records, prefer programmatic generation or embedded JSON resources over inline C# arrays.

### When NOT to Use Html Widget
- Do NOT use the `Html` widget to build interactive canvases, drawing surfaces, or diagram editors. The `Html` widget renders static HTML -- it cannot handle click/drag events on individual elements within the HTML.
- Simple HTML usage for display purposes is fine.
- If the user needs interactive canvas, drawing, or diagram functionality, recommend using the `/ivy-create-external-widget` skill to create a React-based external widget instead.

### Pattern Sources
- Use ONLY the reference documents in the `references/` folder and `ivy docs` / `ivy ask` for API patterns and code examples.
- Do NOT read existing app files in the project's `Apps/` directory for patterns.

### After Implementation
After writing all files, summarize what was created and verify the build succeeds.

## Reference: Design Guidelines

### Page Layout

**Centered Max-Width (default):**
```csharp
return Layout.TopCenter()
    | (Layout.Vertical().Width(Size.Full().Max(200)).Margin(10)
        | content);
```
- Always use `Layout.TopCenter()` -- without it, content hugs left edge.

**Full Center (rare):**
```csharp
return Layout.Center()
    | (Layout.Vertical().Gap(2) | urlInput | submitButton);
```
Reserve for truly minimal UIs (login, empty state, single action).

**Full-Width Panels (editor + preview):**
```csharp
return Layout.TopCenter()
    | (Layout.Vertical().Width(Size.Full().Max(300)).Margin(10)
        | Text.H1("Title")
        | (Layout.Horizontal().Gap(6)
            | (Layout.Vertical().Width(Size.Full()) | leftPanel)
            | (Layout.Vertical().Width(Size.Full()) | rightPanel)));
```

### Visual Hierarchy & Spacing
- Use `new Separator()` between major sections
- Gap default is 4 -- do NOT add `.Gap(4)`
- Padding is rarely needed

### Inputs & Validation
Use `.WithField().Label()` -- never call `.Label()` directly:
```csharp
nameState.ToTextInput().WithField().Label("Name");
```

### MetricView for KPIs
```csharp
new MetricView("Sales", Icons.DollarSign,
    ctx => ctx.UseQuery("sales", () => Task.FromResult(new MetricRecord("$84.3K", 0.21, 0.21, "$400K"))))
```

### UX Interaction
- **Toast after actions:** `client.Toast($"{item.Name} added!");`
- **Confirm destructive actions:** Use `.WithConfirm()`
- **Empty states:** Show guidance toward first action, not blank area

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-app.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
