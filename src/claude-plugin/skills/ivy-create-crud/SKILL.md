---
name: ivy-create-crud
description: >
  Create a CRUD app with list, detail, create, and edit views for an Ivy project.
  Use when the user asks for CRUD views, master-detail views, data management screens,
  entity management, or wants to build views for database tables. Handles blades,
  dialogs, sheets, foreign key lookups, search, pagination, parent-child relationships,
  and async select inputs.
allowed-tools: Bash(dotnet:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[entities to create CRUD views for]"
---

# ivy-create-crud

Create a CRUD app with list, detail, create, and edit views for an Ivy project.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-crud.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Reference Files

Read these before implementing:
- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference (widgets, hooks, layouts, inputs, colors)
- [references/ProductsApp.cs](references/ProductsApp.cs) -- simple app entry point with UseBlades
- [references/OrdersApp.cs](references/OrdersApp.cs) -- app entry point for entity with child relationships
- [references/ProductListBlade.cs](references/ProductListBlade.cs) -- list blade with search, UseQuery, FuncView
- [references/ProductDetailsBlade.cs](references/ProductDetailsBlade.cs) -- details blade with ToDetails, delete, edit trigger
- [references/ProductCreateDialog.cs](references/ProductCreateDialog.cs) -- create dialog with ToForm, validation
- [references/ProductEditSheet.cs](references/ProductEditSheet.cs) -- edit sheet with ToForm, Remove, revalidation
- [references/OrderListBlade.cs](references/OrderListBlade.cs) -- list blade with FK resolution via Include
- [references/OrderDetailsBlade.cs](references/OrderDetailsBlade.cs) -- details blade with related child cards
- [references/OrderCreateDialog.cs](references/OrderCreateDialog.cs) -- create dialog with async select for FK
- [references/OrderEditSheet.cs](references/OrderEditSheet.cs) -- edit sheet with FK fields
- [references/OrderLinesBlade.cs](references/OrderLinesBlade.cs) -- child relationship blade with table
- [references/OrderLineCreateDialog.cs](references/OrderLineCreateDialog.cs) -- child create dialog with parent FK
- [references/OrderLineEditSheet.cs](references/OrderLineEditSheet.cs) -- child edit sheet with async select

## Workflow

This skill guides you through creating complete CRUD applications:

1. **Plan** - Analyze the data models and propose CRUD views for selected entities
2. **Review** - Present the plan for user approval (accept / request changes / cancel)
3. **Implement** - Generate all CRUD files following reference patterns, one entity at a time

## Step 1: Plan the CRUD App

If the user has specified a database connection, read the connection context using `ivy cli explain connections/{ConnectionName}`. If not, ask the user which connection to use.

Ask the user which entities they want CRUD views for (suggest all top-level entities as options).

For each selected entity, determine:
- **Singular name** and **plural name**
- **Lucide icon** (PascalCase, e.g. `ShoppingBag`, `Users`, `FileText`)
- Whether it is a **top-level entity** (gets its own App with list/details/create/edit) or a **child entity** (shown as a relationship blade on a parent's details view)
- Child entities are those that primarily exist as children of another entity via a required foreign key (e.g., OrderLines belong to Orders)

Detect **one-to-many relationships** from navigation properties and foreign keys:
- A parent entity's details blade should show a related card linking to child blades
- Child entities get: RelationshipBlade (table view), RelationshipCreateDialog, RelationshipEditSheet

### Plan Format

```
# Plan

## Create App: [PluralName]

**Namespace**: `{ProjectNamespace}.Apps`
**Connection**: `[ConnectionName]`
**Entity**: `[SingularName]`
**Icon**: `[LucideIcon]`
**Group**: `Apps`
**Layout**: `Blades`
**Files**: `Apps\[PluralName]App.cs`

Opens [Singular]ListBlade.

### Create View: [Singular]ListBlade

**Files**: `Apps\[PluralName]\[Singular]ListBlade.cs`
**Namespace**: `{ProjectNamespace}.Apps.[PluralName]`

List view with search. Shows [key fields]. Click opens [Singular]DetailsBlade. Create button opens [Singular]CreateDialog.

### Create View: [Singular]DetailsBlade

**Files**: `Apps\[PluralName]\[Singular]DetailsBlade.cs`
**Namespace**: `{ProjectNamespace}.Apps.[PluralName]`

Detail view showing [interesting fields]. Edit button opens [Singular]EditSheet. Delete button with confirmation.
[If has children: Related card with links to child blades with count badges.]

### Create View: [Singular]CreateDialog

**Files**: `Apps\[PluralName]\[Singular]CreateDialog.cs`
**Namespace**: `{ProjectNamespace}.Apps.[PluralName]`

Create dialog with required fields. [List the fields.]

### Create View: [Singular]EditSheet

**Files**: `Apps\[PluralName]\[Singular]EditSheet.cs`
**Namespace**: `{ProjectNamespace}.Apps.[PluralName]`

Edit sheet with all editable fields. Removes Id, CreatedAt, UpdatedAt. [List notable field customizations.]

### Create Relationship View: [ParentSingular][ChildPlural]Blade

**Files**: `Apps\[PluralName]\[ParentSingular][ChildPlural]Blade.cs`
**Namespace**: `{ProjectNamespace}.Apps.[PluralName]`

Table view of [ChildPlural] for a given [ParentSingular]. Shows [key columns]. Add button opens [ParentSingular][ChildPlural]CreateDialog.

### Create Relationship View: [ParentSingular][ChildPlural]CreateDialog

**Files**: `Apps\[PluralName]\[ParentSingular][ChildPlural]CreateDialog.cs`
**Namespace**: `{ProjectNamespace}.Apps.[PluralName]`

Create dialog for adding a [ChildSingular] to a [ParentSingular]. Parent ID passed via constructor.

### Create Relationship View: [ParentSingular][ChildPlural]EditSheet

**Files**: `Apps\[PluralName]\[ParentSingular][ChildPlural]EditSheet.cs`
**Namespace**: `{ProjectNamespace}.Apps.[PluralName]`

Edit sheet for a [ChildSingular]. Takes child entity ID as parameter.
```

Repeat the `## Create App` block for each top-level entity selected.

### Chrome Decision
- If the plan has **only one `## Create App` block** (single entity) and no multi-app navigation needs, add a `### Update Program.cs` section that replaces `server.UseAppShell(...)` with `server.UseDefaultApp(typeof(TheAppClass));`.
- If the plan has **multiple `## Create App` blocks** or the user explicitly wants sidebar navigation, add a `### Update Program.cs` section that ensures `server.UseAppShell(new AppShellSettings().DefaultApp<FirstAppClass>().UseTabs(preventDuplicates: true));` is present.

Present the plan to the user and ask them to **Accept**, **Request Changes**, or **Cancel**.

## Step 2: Handle Changes

If the user requests changes, apply them to the plan and present the updated plan again.

## Step 3: Implement

Once approved, implement each entity's views in order.

### Namespace Conventions

- App files (`[PluralName]App.cs`): `namespace {ProjectNamespace}.Apps;`
- View files (all blades, dialogs, sheets): `namespace {ProjectNamespace}.Apps.[PluralName];`

### Required Using Directives

Every generated `.cs` file must include explicit `using` statements. Do NOT rely on global usings.

**Connection usings (when using a database connection):**
- `using {ProjectNamespace}.Connections.{ConnectionName};` -- for context factory
- `using {ProjectNamespace}.Connections.{ConnectionName}.Models;` -- for entity classes (only if project uses a Models subfolder)
- `using Microsoft.EntityFrameworkCore;` -- for ToListAsync, FirstOrDefaultAsync, Include, etc.

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

Include only the namespaces your file actually uses.

### Data Access Pattern

Always use the context factory pattern:
```csharp
var factory = UseService<{ConnectionName}ContextFactory>();
await using var db = factory.CreateDbContext();
```

### Program.cs Chrome Configuration

Check if the approved plan includes a `### Update Program.cs` section. If it does:
- If it specifies `UseDefaultApp`: Open `Program.cs` and replace the `server.UseAppShell(...)` line with `server.UseDefaultApp(typeof(AppClassName));`. Remove the `UseAppShell` line entirely.
- If it specifies `UseAppShell`: Ensure `server.UseAppShell(new AppShellSettings().DefaultApp<AppClassName>().UseTabs(preventDuplicates: true));` is present in `Program.cs`.
- Follow the approved plan exactly.

### Per-View-Type Instructions

#### App (`[PluralName]App.cs`)

```csharp
using {ProjectNamespace}.Apps.[PluralName];

namespace {ProjectNamespace}.Apps;

[App(icon: Icons.[Icon], group: ["[Group]"])]
public class [PluralName]App : ViewBase
{
    public override object? Build()
    {
        return UseBlades(() => new [Singular]ListBlade(), "Search");
    }
}
```

#### ListBlade (`[Singular]ListBlade.cs`)

- Define a `private record [Singular]ListRecord(int Id, ...)` with the key display fields. **For FK properties (e.g., `CustomerId`, `PartnerId`), do NOT include the raw ID. Instead, include a resolved name field (e.g., `string? CustomerName`) and project it via the navigation property in the `.Select()`.**
- Use `UseRefreshToken()` for create/edit refresh flow.
- `UseEffect` on `refreshToken` to pop, revalidate, and push to DetailsBlade.
- `onItemClicked` pushes `[Singular]DetailsBlade(id)`.
- Create button: `Icons.Plus.ToButton(...).Ghost().Tooltip("Create [Singular]").ToTrigger(isOpen => new [Singular]CreateDialog(isOpen, refreshToken))`.
- Two UseQuery methods: `Use[Singular]ListRecords` (filtered list) and `Use[Singular]ListRecord` (single item for FuncView).
- List query: filter with `.Where()`, order by `.OrderByDescending(e => e.CreatedAt)`, `.Take(50)`, tags: `[typeof([Entity][])]`.
- Single item query: `RevalidateOnMount = false`, `initialValue: record`, tags: `[(typeof([Entity]), record.Id)]`.
- Search input + create button in a horizontal header layout.
- If entity has FK properties, ALWAYS `.Include()` the navigation property and project the human-readable name into the record -- NEVER expose raw FK IDs as display columns. Explicitly type as `IQueryable<Entity>` to avoid IIncludableQueryable type issues.

#### DetailsBlade (`[Singular]DetailsBlade.cs`)

- Constructor: `(int [entityCamelCase]Id)`.
- Query the entity with `.Include()` for navigation properties.
- Show loading skeleton: `if (query.Loading) return Skeleton.Card();`
- Show not found callout if null.
- Delete button with `.WithConfirm()`, pops and revalidates `typeof([Entity][])`.
- Edit button with `.ToTrigger(isOpen => new [Singular]EditSheet(isOpen, [entityCamelCase]Id))`.
- Details card using anonymous type `.ToDetails().RemoveEmpty().Builder(e => e.Id, e => e.CopyToClipboard())`.
- Only include "interesting" fields. Exclude CreatedAt, UpdatedAt.
- If entity has one-to-many children: add a related card with `ListItem` entries that push child blades with count badges.

#### CreateDialog (`[Singular]CreateDialog.cs`)

- Constructor: `(IState<bool> isOpen, RefreshToken refreshToken)`.
- Define a `private record [Singular]CreateRequest` with `[Required]` attributes on required **nullable** fields (`string`, `int?`, `Guid?`, etc.). Do NOT add `[Required]` on non-nullable value types (`int`, `decimal`, `DateTime`, `bool`, etc.).
- **CRITICAL: CreateRequest MUST NEVER include these auto-managed fields:**
  - `Id` -- auto-generated by the database
  - `CreatedAt` -- set in OnSubmit: `CreatedAt = DateTime.UtcNow`
  - `UpdatedAt` -- set in OnSubmit: `UpdatedAt = DateTime.UtcNow`
  - Any other auto-increment or computed fields
- Use the SAME types as in the target entity to avoid type mismatches.
- `UseState(() => new [Singular]CreateRequest())`.
- `.ToForm().OnSubmit(OnSubmit).ToDialog(isOpen, title: "Create [Singular]", submitTitle: "Create")`.
- Use `.Builder(e => e.ForeignKeyId, e => e.ToAsyncSelectInput(...))` for foreign key fields.
- OnSubmit creates entity, sets CreatedAt/UpdatedAt if they exist, saves, returns ID via `refreshToken.Refresh(id)`.

#### EditSheet (`[Singular]EditSheet.cs`)

- Constructor: `(IState<bool> isOpen, int [entityCamelCase]Id)`.
- Query entity by ID.
- Loading state: `Skeleton.Form().ToSheet(isOpen, "Edit [Singular]")`.
- `.ToForm().Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt)` - ONLY remove properties that actually exist on the entity.
- Use `.Builder()` for custom inputs (TextArea for long text, AsyncSelect for FKs, etc.).
- OnSubmit sets `UpdatedAt = DateTime.UtcNow` if property exists, updates entity, revalidates tags.

#### RelationshipBlade (`[Parent][ChildPlural]Blade.cs`)

- Constructor: `(int [parentCamelCase]Id)`.
- Query child entities filtered by parent ID with `.Where(e => e.[ParentId] == [parentCamelCase]Id)`.
- For FK properties on child entities, `.Include()` the navigation property and project the resolved name -- never show raw FK IDs as table columns.
- Use `.ToTable()` with `.Totals()` for numeric columns and `.RemoveEmptyColumns()`.
- Each row has Delete button with `.WithConfirm()` and Edit trigger to `[Parent][ChildPlural]EditSheet`.
- Add button triggers `[Parent][ChildPlural]CreateDialog(isOpen, refreshToken, [parentCamelCase]Id)`.
- UseEffect on refreshToken to revalidate both child list and parent tags.
- Empty state: show Callout with info variant.

#### RelationshipCreateDialog (`[Parent][ChildPlural]CreateDialog.cs`)

- Constructor: `(IState<bool> isOpen, RefreshToken refreshToken, int [parentCamelCase]Id)`.
- Parent ID is passed via constructor, NOT part of the form.
- CreateRequest record MUST only have user input fields -- no parent FK, no Id, no CreatedAt, no UpdatedAt.
- Set parent FK directly when creating entity: `[ParentId] = [parentCamelCase]Id`.

#### RelationshipEditSheet (`[Parent][ChildPlural]EditSheet.cs`)

- Constructor: `(IState<bool> isOpen, RefreshToken refreshToken, int [childCamelCase]Id)`.
- For regular child entities with their own ID, only the child ID is needed.
- For junction tables with composite keys, accept ALL key fields.
- `.Remove()` the parent FK and ID fields that shouldn't be editable.

### Pattern Sources
- Use ONLY the reference documents in the `references/` folder and `ivy docs` / `ivy ask` for API patterns and code examples.
- Do NOT read existing app files in the project's `Apps/` directory for patterns.

### Critical Code Generation Rules

#### Use Modern C# Features
- Use file-scoped namespaces, primary constructors, collection expressions.
- Use `record` for DTOs with `{ get; init; }` properties.

#### Icon Usage
Only use icons from the Icons enum. Common ones: `Icons.Pencil`, `Icons.Trash`, `Icons.Plus`, `Icons.ChevronRight`, `Icons.Search`, `Icons.Filter`.

#### Entity Property Verification
- ALWAYS check entity definitions before using any property.
- Don't assume properties exist (CreatedAt, UpdatedAt, FullName, etc.).
- Match property types exactly (int vs int?, DateTime vs string).
- If navigational properties don't exist in the POCOs, don't use `.Include()`.

#### ToForm() Rules
- Only `.Remove()` properties that actually exist on the model.
- Only `.Builder()` with builders you've seen in the reference examples.
- ToMoneyInput MUST be followed by `.Currency("USD")` or appropriate currency.
- ToAsyncMultiSelectInput does NOT exist.
- Always provide explicit input builder to avoid CS0411 errors: `.Builder(e => e.Field, e => e.ToTextInput())`.

#### Available Form Builders by Type

**string:** `ToTextInput()`, `ToTextareaInput()`, `ToPasswordInput()`, `ToEmailInput()`, `ToUrlInput()`, `ToTelInput()`, `ToColorInput()`, `ToCodeInput()`
**DateTime, DateTime?:** `ToDateInput()`, `ToDateTimeInput()`. Never use `ToTextareaInput()` for dates.
**int, long, decimal, double, float:** `ToNumberInput()`, `ToFeedbackInput()`, `ToMoneyInput()` (MUST chain `.Currency()`)
**Foreign keys:** `ToAsyncSelectInput(searchFn, lookupFn, placeholder)` -- single selection only

#### Async Select Pattern
For foreign key dropdowns, create two static methods:
- `Use[Entity]Search(IViewContext context, string query)` - returns `QueryResult<Option<TKey?>[]>`
- `Use[Entity]Lookup(IViewContext context, TKey? id)` - returns `QueryResult<Option<TKey?>?>`

**CRITICAL: Return type MUST be `QueryResult<>`, NOT `Task<>`.** Always use `context.UseQuery()` which returns `QueryResult<>`.

#### FK Validation in EditSheet

When editing, entity FK properties are non-nullable (`int`, `Guid`). If the user clears a required FK field, the value becomes `0` or `Guid.Empty`. Add explicit validation:
```csharp
if (request.CustomerId == default)
{
    client.Error("Please select a customer.");
    return;
}
```

#### IQueryable Type with Include
When using `.Include()` followed by `.Where()`, explicitly type as `IQueryable<Entity>` to avoid type mismatch:
```csharp
IQueryable<Order> query = db.Orders.Include(o => o.Customer);
query = query.Where(o => o.StoreName.Contains(filter));
```

### After Implementation
After writing all files for each entity, summarize what was created.

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-crud.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
