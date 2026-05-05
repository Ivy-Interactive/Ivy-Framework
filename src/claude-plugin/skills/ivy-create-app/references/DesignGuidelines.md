# Design Guidelines

## 1. Page Layout

### Centered Max-Width (default)
```csharp
return Layout.TopCenter()
    | (Layout.Vertical().Width(Size.Full().Max(200)).Margin(10)
        | content);
```
- Always use `Layout.TopCenter()` — without it, content hugs left edge.
- Never use `Layout.Vertical().AlignContent(Align.Center)` as top-level — it centers children but doesn't constrain width.

### Full Center (rare)
```csharp
return Layout.Center()
    | (Layout.Vertical().Gap(2) | urlInput | submitButton);
```
Reserve for truly minimal UIs (login, empty state, single action). Most apps should use `TopCenter()`.

### Full-Height: Header + Content + Footer
Only apply `.Height(Size.Full())` to the content row. Header/footer auto-size:
```csharp
Layout.Vertical().Height(Size.Full())
    | (Layout.Horizontal().AlignContent(Align.Center) | title | dropdown)     // auto
    | (Layout.Horizontal().Height(Size.Full()) | editor | preview)     // fills
    | (Layout.Horizontal().AlignContent(Align.Center) | downloadButton)       // auto
```

### Full-Width Panels (editor + preview, input + output)
```csharp
return Layout.TopCenter()
    | (Layout.Vertical().Width(Size.Full().Max(300)).Margin(10)
        | Text.H1("Title")
        | Text.Lead("Description")
        | (Layout.Horizontal().Gap(6)
            | (Layout.Vertical().Width(Size.Full()) | leftPanel)
            | (Layout.Vertical().Width(Size.Full()) | rightPanel)));
```
- **Never** add `.Width(Size.Full())` to `Layout.TopCenter()` — it defeats centering and removes margin.
- Use `.Width(Size.Full().Max(300))` on the inner container if panels need more room than the default max-width.

## 2. Multi-Column & Panels

### Split Panels
Use plain `Layout.Vertical()` columns with headings — not `Card` wrappers (cards add borders/padding → heavy "card-in-card" feel):
```csharp
Layout.Horizontal()
    | (Layout.Vertical().Width(Size.Full()) | Text.H3("Schema") | schema)
    | (Layout.Vertical().Width(Size.Full()) | Text.H3("Preview") | preview)
```

### Blades
`UseBlades()` is for CRUD/master-detail drill-down only. Not for dashboards or full-width layouts.

### TabsLayout vs Separate Apps
Separate apps = distinct entities with own CRUD. TabsLayout = views of same data (chart/table, settings categories, wizard steps).

## 3. Grid
```csharp
// Wrong: Layout.Grid(3) — "3" renders as text child
Layout.Grid().Columns(3).Gap(4)  // Correct
```

## 4. Visual Hierarchy & Spacing

- Use `new Separator()` between major sections to prevent blending.
- Add `.BottomMargin(2)` on the wrapping `LayoutView` between search/filter inputs and their content. Note: `BottomMargin` is a `LayoutView` method — do not call it on `Field` or input types.
- Never use `Colors.White` for borders (invisible on white bg). Use `Colors.Gray` or `Colors.Slate`.
- Gap default is 4 — do not add `.Gap(4)`.
- Padding is rarely needed — layouts have appropriate defaults.

## 5. Inputs & Validation

### Input Labels
Use `.WithField().Label()` — never call `.Label()` directly (resolves to `AxisExtensions.Label<T>()`, causes CS0311):
```csharp
nameState.ToTextInput().WithField().Label("Name");
ageState.ToNumberInput().WithField().Label("Age");
colorState.ToSelectInput(options).WithField().Label("Color");
```
Applies to all input types: SelectInputBase, NumberInputBase, TextInputBase, DateTimeInputBase, BoolInputBase, ColorInputBase, FileInputBase, FeedbackInputBase, CodeInputBase.

### Validation & Errors

**Forms**: Use `form.Validate()` and display errors inline. Example:
```csharp
Callout.Error("Name is required");
```

**API/Service Errors**: Wrap error messages in callouts:
```csharp
Callout.Error($"Could not save: {ex.Message}. Check your input and try again.");
```

**Streaming Response Errors** (IChatClient, etc.): When catching exceptions in streaming tasks, set the state to a `Callout.Error()` widget, NOT plain markdown. Example:
```csharp
catch (Exception ex)
{
    errorState.Set(Callout.Error(ex.Message));
}
// Then conditionally render: errorState.Value ?? new Markdown(responseText.Value)
```

Use `Callout.Error()` / `Callout.Warning()` — do not repurpose display widgets.

### Text Input vs Textarea
Use `.ToTextareaInput()` for multi-line content (descriptions, notes, pasted text). `.ToTextInput()` is single-line and truncates.

## 6. Content Display

### CodeInput vs CodeBlock
- **Read-only:** `new CodeBlock(value, Languages.Json)`
- **Editable:** `state.ToCodeInput()` — `.ToCodeInput()` is on `IAnyState`, not `string` (CS1929)

### Html with Inline Styles
Chain `.DangerouslyAllowScripts()` when Html has `style="..."` — sanitizer strips inline styles without it:
```csharp
new Html($"<span style=\"color: green\">{ch}</span>").DangerouslyAllowScripts();
```

### MetricView for KPIs
```csharp
Layout.Grid().Columns(3)
    | new MetricView("Sales", Icons.DollarSign,
        ctx => ctx.UseQuery("sales", () => Task.FromResult(new MetricRecord("$84.3K", 0.21, 0.21, "$400K"))))
```
`MetricRecord`: `MetricFormatted` (string), `TrendComparedToPreviousPeriod` (double?, 0.21 = +21%), `GoalAchieved` (double?, 0-1), `GoalFormatted` (string?). Abbreviate large numbers.

### Progressive Disclosure
Use `Expandable` for secondary content when items have >4-5 properties:
```csharp
Layout.Vertical()
    | Text.H4(item.Name)
    | Text(item.Summary).Color(Colors.Gray)
    | new Expandable("Details", Layout.Vertical() | Text(item.FullDescription))
```

## 7. UX Interaction

- **Toast after actions:** confirm create/delete/update succeeded:
  ```csharp
  var client = UseService<IClientProvider>();
  client.Toast($"{item.Name} added!");
  ```
- **No duplicate info:** If Sheet title shows item name, don't repeat it in body.
- **Confirm destructive actions:** Use `.WithConfirm()` — never delete on single click:
  ```csharp
  Button("Delete", _ => { items.Remove(item); UseService<IClientProvider>().Toast($"{item.Name} deleted"); },
      variant: ButtonVariant.Destructive)
      .WithConfirm($"Delete {item.Name}?", title: "Confirm Delete", confirmLabel: "Delete", destructive: true)
  ```
- **Empty states:** Show guidance toward first action, not blank area:
  ```csharp
  if (!items.Any())
      yield return Layout.Center()
          | (Layout.Vertical().Gap(2).AlignContent(Align.Center)
              | Icons.Inbox.ToIcon().Color(Colors.Gray)
              | Text("No items yet").Color(Colors.Gray)
              | Button("Add your first item").OnClick(ShowAddForm));
  ```
- **Multi-step flows:** Show step progress (`Step {n} of {total}: {title}`) with `Separator` between header and content.

## 8. Labeling and User Expectations

- Only use "(editable)", "(optional)", or similar hints when they accurately describe the behavior
- If content is read-only (using Table, Details, CodeBlock, or display-only widgets), do not label it as "(editable)"
- Use "(read-only)" or no label at all for non-editable content
- If you plan to make content editable later, implement the editing first, then add the label

## 9. App Attribute - Group Parameter

The `[App]` attribute's `group` parameter is nullable and has a runtime default value of `["Apps"]` (applied in `AppHelpers.cs`).

**Best Practice**:
- Omit the `group` parameter when using the default "Apps" group
- Explicitly specify `group` only for non-default groups
- Valid group names include: "Apps" (default), "Tools", "Settings", or custom group names

**Examples**:

Default group (omit parameter):
```csharp
[App(icon: Icons.Calculator)]
public class CalculatorApp : ViewBase
{
    // group defaults to ["Apps"] at runtime
}
```

Non-default group (must specify explicitly):
```csharp
[App(icon: Icons.Settings, group: ["Settings"])]
public class PreferencesApp : ViewBase
{
    // explicitly in Settings group
}
```

Multiple groups (specify explicitly):
```csharp
[App(icon: Icons.Database, group: ["Apps", "Data"])]
public class DataExplorerApp : ViewBase
{
    // appears in both Apps and Data groups
}
```

**CRITICAL**: When a specification explicitly defines a non-default `group` value (e.g., `group: ["Tools"]`, `group: ["Settings"]`), you MUST include it in the implementation. Omitting a non-default group value is an implementation error that fails to meet the specification requirements.

**Reference**: See `ChatApp.cs` (line 9) for an example that correctly omits the default group parameter.
