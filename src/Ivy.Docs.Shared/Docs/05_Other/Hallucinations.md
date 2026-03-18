# Ivy Framework Hallucinations

Known cases where the agent hallucinated Ivy Framework APIs. Use this as a reference when debugging build errors in agent sessions.

## HandleSubmit / Handle* — renamed event handler methods

**Hallucinated API:**
```csharp
input.ToTextInput().HandleSubmit(() => Save())
button.HandleClick(() => DoSomething())
input.HandleBlur(() => Validate())
```

**Error:** `does not contain a definition for 'HandleSubmit'` (or `HandleClick`, `HandleBlur`, etc.)

**Correct API:**
```csharp
input.ToTextInput().OnSubmit(() => Save())
button.OnClick(() => DoSomething())
input.OnBlur(() => Validate())
```

All `Handle*` event handler extension methods were renamed to `On*` in v1.2.17 (Ivy-Framework#2459, #2510): `HandleClick` → `OnClick`, `HandleSubmit` → `OnSubmit`, `HandleChange` → `OnChange`, `HandleSelect` → `OnSelect`, `HandleClose` → `OnClose`, `HandleBlur` → `OnBlur`, `HandleRowAction` → `OnRowAction`, `HandleCardMove` → `OnCardMove`, `HandleExpand` → `OnExpand`, `HandleCollapse` → `OnCollapse`, `HandlePageChange` → `OnPageChange`, `HandleUpload` → `OnUpload`, `HandleDownload` → `OnDownload`. **Auto-fixed:** The refactoring service automatically rewrites all `Handle*` calls to `On*`.

**Found In:**
(multiple sessions — agent uses old API names from training data)

## InputBase.Label() — AxisExtensions method used on input

**Hallucinated API:**
```csharp
// NumberInputBase
stockAdjustment.ToNumberInput().Label("Adjustment amount")

// DateTimeInputBase
dateState.ToDateInput().Label("Birthdate")
```

**Error:** `The type 'Ivy.NumberInputBase' cannot be used as type parameter 'T' in the generic type or method 'AxisExtensions.Label<T>(T, string)'` (same CS0311 error for `DateTimeInputBase`, `TextInputBase`, `SelectInputBase`, `BoolInputBase`, etc.)

**Correct API:**
```csharp
// Use .WithField().Label() to wrap the input in a labeled field:
stockAdjustment.ToNumberInput().WithField().Label("Adjustment amount")
dateState.ToDateInput().WithField().Label("Birthdate")

// Or use Text.Label() as a separate element above the input:
Layout.Vertical()
    | Text.Label("Adjustment amount")
    | stockAdjustment.ToNumberInput()

// Or use a form with .Label() on the form builder:
state.ToForm().Label(m => m.Amount, "Adjustment amount")
```

`.Label()` is an `AxisExtensions` method for chart axes, not for inputs. This applies to ALL input types (`NumberInputBase`, `DateTimeInputBase`, `TextInputBase`, `SelectInputBase`, `BoolInputBase`, etc.). The preferred way to label an input is `.WithField().Label("...")`, which wraps the input in a `Field` with a label.

**Found In:**
f20dced8-1689-4289-a2d8-ee67136eb6ce
2e91e9c7-9c03-4b86-a9d2-c0417bcf715f
7a9aadf3-097e-448d-8d5c-bc86152710a6

## Badge.Color(Colors.X) — non-existent fluent method

**Hallucinated API:**
```csharp
new Badge(match.Value).Color(Colors.Green)
new Badge("No match").Color(Colors.Red)
```

**Correct API:**
```csharp
// Via constructor variant parameter:
new Badge(match.Value, BadgeVariant.Success)

// Via fluent shortcut methods:
new Badge(match.Value).Success()
new Badge("No match").Destructive()

// Via explicit Variant() method:
new Badge(match.Value).Variant(BadgeVariant.Info)
```

Available `BadgeVariant` values: `Primary`, `Destructive`, `Secondary`, `Outline`, `Success`, `Warning`, `Info`. The agent confused `LabelExtensions.Color(Label, Colors)` (which exists for `Label`) with a Badge method. Badge uses `BadgeVariant`, not `Colors`.

**Found In:**
3c507fb4-71e1-4136-9d40-8eca6590250d
ce144de9-0688-490a-bef6-b2766e323154
642d3167-790d-48c4-a381-bfab78f928cc

## BorderRadius.Medium — non-existent enum value

**Hallucinated API:**
```csharp
BorderRadius.Medium
BorderRadius.Large
BorderRadius.Small
```

**Error:** `'BorderRadius' does not contain a definition for 'Medium'`

**Correct API:**
```csharp
BorderRadius.None     // no rounding
BorderRadius.Rounded  // standard rounded corners
BorderRadius.Full     // fully rounded (pill shape)
```

Valid `BorderRadius` values: `None`, `Rounded`, `Full`. The agent hallucinates Tailwind-style size variants (`Small`, `Medium`, `Large`, `Xl`) that don't exist.

**Found In:**
050136ca-9275-4e1d-9740-e393b544c1b5
8a776329-6dc7-474f-aa4d-c8b4da753a25 (BorderRadius.Large)
4e59e443-3579-4df9-af4b-765b7b7d61c8 (BorderRadius.Small — via IvyMcp hallucination)

## Button("text", Icons.X) — icon as constructor argument

**Hallucinated API:**
```csharp
new Button("Add Item", Icons.Plus)
```

**Error:** `Argument 2: cannot convert from 'Ivy.Icons' to 'System.Func<Ivy.Event<Ivy.Button>, System.Threading.Tasks.ValueTask>?'`

**Correct API:**
```csharp
new Button("Add Item").Icon(Icons.Plus)
```

The `Button` constructor signature is `Button(string label, Func<Event<Button>, ValueTask>? onClick = null, ...)`. The second parameter is a click handler, not an icon. Use the `.Icon(Icons.X)` fluent method to set an icon on a button.

**Found In:**
f20dced8-1689-4289-a2d8-ee67136eb6ce
7a9aadf3-097e-448d-8d5c-bc86152710a6

## AppAttribute — PascalCase properties and invented parameters

**Hallucinated API:**
```csharp
[App(Icon = Icons.Bot, Group = "Apps", Chrome = UseDefaultAppChrome)]
[App(Icon = Icons.Waves)]
```

**Errors:**
- `CS0655: 'Icon' is not a valid named attribute argument` — PascalCase property used instead of constructor parameter
- `CS0246: The type or namespace name 'Group' could not be found` — parameter doesn't exist
- `CS0246: The type or namespace name 'Chrome' could not be found` — parameter doesn't exist

**Correct API:**
```csharp
[App(icon: Icons.Bot, path: new[] { "Apps" })]
```

The `AppAttribute` uses **lowercase named constructor parameters**, not PascalCase named properties. C# attributes with nullable property types cause CS0655 when accessed via `PropertyName = value` syntax. Use `parameterName: value` syntax instead.

Available parameters: `id`, `title`, `icon`, `description`, `path`, `isVisible`, `order`, `groupExpanded`, `documentSource`, `searchHints`. There is NO `group` or `chrome` parameter — use `path:` for navigation grouping, and configure chrome in `Program.cs` via `server.UseDefaultApp(typeof(MyApp))`.

See: Ivy-Framework#2587 (plans to rename `path` to `group`)

**Found In:**
7c547408-00b3-47e1-976e-59c9357c1e74
d6a5f377-bc84-404d-acca-71164d3754d4

## SelectInputBase.Options() — chained options method

**Hallucinated API:**
```csharp
defaultBehavior.ToSelectInput().Options(["Refused", "Allowed", "Ignored"])
```

**Error:** `'SelectInputBase' does not contain a definition for 'Options'`

**Correct API:**
```csharp
defaultBehavior.ToSelectInput(new[] { "Refused", "Allowed", "Ignored" }.ToOptions())
```

Options are passed as `IEnumerable<IAnyOption>` to `ToSelectInput(options)`, not chained via a `.Options()` method. Use the `.ToOptions()` extension method on a string array to convert to the correct type.

**Found In:**
4eb1799f-39b2-4325-a0bd-37b769a33432``

https://github.com/Ivy-Interactive/Ivy-Framework/issues/2271

## ToastVariant — non-existent enum

**Hallucinated API:**
```csharp
client.Toast("Error!", ToastVariant.Destructive)
```

**Error:** `The name 'ToastVariant' does not exist in the current context`

**Correct API:**
```csharp
client.Toast("Success message");       // neutral toast
client.Toast("Done!", "Title");        // with title
client.Error("Something went wrong."); // error toast
```

`ToastVariant` does not exist. The `IClientProvider.Toast()` method takes `(string message)` or `(string message, string title)`. For error toasts, use `client.Error(message)` instead.

**Found In:**
d90474ac-78b9-48c7-8317-3860ff36b9dd (sub-tasks 002–006, appeared in ALL sub-tasks)

## DateTimeVariant — wrong enum name

**Hallucinated API:**
```csharp
date.ToDateTimeInput().Variant(DateTimeVariant.Date)
```

**Error:** `The name 'DateTimeVariant' does not exist in the current context`

**Correct API:**
```csharp
date.ToDateInput()
// or:
date.ToDateTimeInput().Variant(DateTimeInputVariant.Date)
```

The enum is `DateTimeInputVariant` (singular), not `DateTimeVariant` (missing "Input") or `DateTimeInputVariants` (old plural name). All input variant enums were renamed from plural to singular in Ivy-Framework#2546. Values: `DateTime`, `Date`, `Time`, `Month`, `Week`. **Auto-fixed:** The refactoring service automatically rewrites both `DateTimeVariant` and `DateTimeInputVariants` to `DateTimeInputVariant`.

**Found In:**
d90474ac-78b9-48c7-8317-3860ff36b9dd (sub-tasks 002–006, appeared in ALL sub-tasks)

## TextBuilder.Style() — non-existent styling method

**Hallucinated API:**
```csharp
Text.P("🐶").Style("font-size: 48px")
```

**Error:** `'TextBuilder' does not contain a definition for 'Style'`

**Correct API:**
```csharp
Text.P("🐶").Large()
Text.P("text").Medium()
Text.P("text").Small()
```

`TextBuilder` does not have a `.Style()` method for arbitrary CSS. Use `.Large()`, `.Medium()`, or `.Small()` fluent modifiers. The agent invented a CSS-style `.Style()` method similar to JSX `style` props. Variant of the documented `WithFontSize()` hallucination.

Also hallucinated: `Text.Code(expr).FontSize(24)` — CS1929: `.FontSize()` is an extension on `LabelList`, not `TextBuilder`.

**Found In:**
88e4f0bb-d358-4b34-9458-bc7eb98845e5, 625c285f-068b-4de3-b01c-ae2f7286a5d8

## TextBuilder.AlignCenter() / .Centered() — use .Center()

**Hallucinated API:**
```csharp
Text.H1("$0.00").AlignCenter()
Text.H1("title").Centered()
```

**Error:** `CS1061: 'TextBuilder' does not contain a definition for 'AlignCenter'` / `'Centered'`

**Correct API:**
```csharp
Text.H1("$0.00").Center()
```

`TextBuilder` now has a `.Center()` method (returns `Align(TextAlignment.Center)`). The agent sometimes hallucinates `.AlignCenter()` or `.Centered()` instead. The correct method name is `.Center()`.

**Found In:**
713546f7-32fb-4961-ab78-def91e7c010d, 5d2202d2-9d6b-4198-9922-c3763534aca5

## Table\<T\> — non-generic type used with type arguments

**Hallucinated API:**
```csharp
new Table<MyRecord>(items)
```

**Error:** `The non-generic type 'Table' cannot be used with type arguments`

**Correct API:**
```csharp
items.ToTable()
```

`Table` is non-generic. Use the `IEnumerable<T>.ToTable()` builder pattern to create a table from a collection. The type is inferred from the collection.

**Found In:**
a9ee3993-1cfb-4cba-9322-80a60b56c8d2

## LayoutView.MaxWidth() — non-existent method

**Hallucinated API:**
```csharp
Layout.Vertical().MaxWidth(Size.Lg)
```

**Error:** `'LayoutView' does not contain a definition for 'MaxWidth'`

**Correct API:**
```csharp
Layout.Vertical().Width(Size.Lg)
```

`LayoutView` does not have a `.MaxWidth()` method. Use `.Width(Size)` instead.

**Found In:**
a9ee3993-1cfb-4cba-9322-80a60b56c8d2

## LayoutView.SpaceBetween() — non-existent method

**Hallucinated API:**
```csharp
Layout.Horizontal().SpaceBetween()
```

**Error:** `'LayoutView' does not contain a definition for 'SpaceBetween'` (CS1061)

**Correct API:**
```csharp
Layout.Horizontal(Align.SpaceBetween)
```

`SpaceBetween` is an `Align` enum value passed to the layout constructor, not a fluent method. The same applies to `SpaceAround` and `SpaceEvenly`.

**Found In:**
f6d6e841-9a14-4475-9fa5-0791be30e578

## Callout constructor — wrong constructor + invented enum

**Hallucinated API:**
```csharp
new Callout("No to-do items.", CalloutType.Info)
```

**Error:** `The typeound`

**Correct API:**
```csharp
Callout.Info("No to-do items.")
```

`Callout` uses static factory methods: `Callout.Info()`, `Callout.Warning()`, `Callout.Error()`, `Callout.Success()`. The `CalloutType` enum does not exist.

**Found In:**
bd5f45ac-569d-4be8-8ef8-882451e608a1

## Callout.Destructive() — fluent method on constructor instance

**Hallucinated API:**
```csharp
new Callout("Error message").Destructive()
```

**Error:** `'Callout' does not contain a definition for 'Destructive'`

**Correct API:**
```csharp
Callout.Error("Error message")
```

`Callout` uses static factory methods (`Callout.Info()`, `Callout.Warning()`, `Callout.Error()`, `Callout.Success()`), not a constructor + fluent style chain. `.Destructive()` is a `Button` style method — the agent confused the two APIs. No auto-fix is possible because the intent (error vs warning vs info) is ambiguous.

**Found In:**
d9116efb-830e-484a-a258-fc3193769158

## Callout.Error() as instance method — static method called on instance

**Hallucinated API:**
```csharp
new Callout(errorMessage).Error()
```

**Error:** `CS0176: Member 'Callout.Error(string?, string?)' cannot be accessed with an instance reference; qualify it with a type name instead`

**Correct API:**
```csharp
Callout.Error(errorMessage)
```

`Callout.Error()`, `.Warning()`, `.Info()`, `.Success()` are **static** factory methods, not instance methods. The agent constructed a Callout instance and then tried to call the static method on it. This is a variant of the documented `CalloutType` and `Callout.Destructive()` hallucinations — all stem from the agent not understanding that Callout uses static factories.

**Auto-fix:** `new Callout({arg}).Error()` → `Callout.Error({arg})`

**Found In:**
600cee71-6d3b-45a3-ac18-86e2a98e79d1

## TextInputBase.OnEnter() — invented fluent method

**Hallucinated API:**
```csharp
newItemText.ToTextInput().Placeholder("Add a new to-do...").OnEnter(AddTodo)
```

**Error:** `'TextInputBase' does not contain a definition for 'OnEnter'`

**Correct API:**
`.OnEnter()` does not exist on `TextInput`. Use `OnSubmit()` to handle enter-key submission:
```csharp
text.ToTextInput().OnSubmit(() => DoSomething())
```

**Found In:**
bd5f45ac-569d-4be8-8ef8-882451e608a1

## TextInputVariants — old plural enum name

**Hallucinated API:**
```csharp
new TextInput(text.Value, e => text.Set(e.Value)).Variant(TextInputVariants.Textarea)
```

**Error:** `The name 'TextInputVariants' does not exist in the current context`

**Correct API:**
```csharp
text.ToTextInput().Variant(TextInputVariant.Textarea)
```

The enum is `TextInputVariant` (singular), not `TextInputVariants` (plural). All input variant enums were renamed from plural to singular in Ivy-Framework#2546 (e.g., `TextInputVariants` → `TextInputVariant`, `ColorInputVariants` → `ColorInputVariant`, etc.). **Auto-fixed:** The refactoring service automatically rewrites `TextInputVariants` → `TextInputVariant`. Values: `Text`, `Textarea`, `Email`, `Tel`, `Url`, `Password`, `Search`.

**Found In:**
4a94f8f6-865d-4663-8f4c-d4c09913398f

## Event<T,E>.Data — non-existent property

**Hallucinated API:**
```csharp
args.Data.Id
args.Data.Tag
```

**Error:** `'Event<DataTable, RowActionClickEventArgs>' does not contain a definition for 'Data'`

**Correct API:**
```csharp
args.Value.Id
args.Value.Tag
```

`Event<TSender, TValue>` uses `.Value` to access the event args, not `.Data`. The agent likely confused this with other event patterns from different frameworks (e.g., WPF `DataContext`, JavaScript `event.data`).

**Found In:**
f20dced8-1689-4289-a2d8-ee67136eb6ce

## UseState\<T?\>(null) — ambiguous overload call

**Hallucinated API:**
```csharp
var selectedItem = UseState<InventoryItem?>(null);
```

**Error:** `The call is ambiguous between 'ViewBase.UseState<T>(T?, bool)' and 'ViewBase.UseState<T>(Func<T>, bool)'`

**Correct API:**
```csharp
// Best: omit the null argument — the default is already null:
var selectedItem = UseState<InventoryItem?>();
// Or cast null to the explicit type:
var selectedItem = UseState<InventoryItem?>((InventoryItem?)null);
// Or use a lambda:
var selectedItem = UseState(() => (InventoryItem?)null);
```

When `T` is a reference type, `null` matches both `T?` and `Func<T>`, causing overload ambiguity. The simplest fix is to omit the `null` argument entirely — the default parameter is already `null`/`default`. Alternatively, cast null to the explicit type or wrap it in a lambda.

**Note:** Unlike `IState<T>.Set(null)` (which was fixed via `[OverloadResolutionPriority(1)]`), `UseState` cannot use the same approach because T is inferred from the argument — C# 10+ lambda natural types cause the `T?` overload to steal ALL lambda calls when given higher priority, breaking `UseState(() => expr)` throughout the codebase.

**Found In:**
f20dced8-1689-4289-a2d8-ee67136eb6ce

## Tab.Content() — non-existent fluent method

**Hallucinated API:**
```csharp
new Tab("Customer Info").Content(
    Layout.Vertical() | ...
)
```

**Error:** `'Tab' does not contain a definition for 'Content' and the best extension method overload 'ButtonExtensions.Content(Button, object)' requires a receiver of type 'Ivy.Button'`

**Correct API:**
```csharp
new Tab("Customer Info", Layout.Vertical() | ...)
```

`Tab` takes content as the second constructor parameter: `Tab(string title, object? content = null)`. There is no `.Content()` fluent method. This is the same pattern as `ListItem.Content()` — the agent invents fluent `.Content()` methods on widgets that accept content through constructors.

**Note:** The IvyQuestion MCP tool also hallucinated this same API, returning `.Content()` as valid in two separate answers, reinforcing the agent's mistake.

**Found In:**
41ae072b-2845-46f1-bd0b-a4a6370c6807

## Layout.Tabs() | Tab — pipe operator on TabView

**Hallucinated API:**
```csharp
Layout.Tabs()
    | customerInfoTab
    | yourInfoTab
```

**Error:** `Operator '|' cannot be applied to operands of type 'TabView' and 'Tab'`

**Correct API:**
```csharp
Layout.Tabs(customerInfoTab, yourInfoTab)
```

The `|` pipe operator works on `LayoutView` (for composing children) but does NOT exist on `TabView`. Tabs must be passed as constructor arguments via `Layout.Tabs(params Tab[] tabs)`.

**Found In:**
41ae072b-2845-46f1-bd0b-a4a6370c6807

## FormBuilder.Header() — non-existent method

**Hallucinated API:**
```csharp
entity.ToForm()
    .Header("Edit Fund")
    .Field(f => f.Name)
```

**Error:** `'FormBuilder<T>' does not contain a definition for 'Header'`

**Correct API:**
```csharp
entity.ToForm()
    .Field(f => f.Name)
    .ToSheet(title: "Edit Fund")
```

`FormBuilder` does not have a `.Header()` method. The title/header is set when converting the form to a dialog or sheet via `.ToDialog(title:)` or `.ToSheet(title:)`. The agent confused this with `Card.Header()` or `BladeHeader`.

**Found In:**
d90474ac-78b9-48c7-8317-3860ff36b9dd (sub-tasks 002, 003)

## Callout.Color(Colors.X) — non-existent fluent method

**Hallucinated API:**
```csharp
new Callout("Error message").Color(Colors.Destructive)
```

**Error:** `'Callout' does not contain a definition for 'Color' and the best extension method overload 'LabelExtensions.Color(Label, Colors)' requires a receiver of type 'Ivy.Label'`

**Correct API:**
```csharp
Callout.Error("Error message")
Callout.Warning("Warning message")
Callout.Info("Info message")
Callout.Success("Success message")
```

`Callout` uses static factory methods, not a constructor + `.Color()` chain. This is a variant of the documented `Callout.Destructive()` hallucination — both stem from the agent trying to apply fluent styling to Callout instead of using the static factory pattern. To change variant after creation, use `.Variant(CalloutVariant.Warning)`.

**Found In:**
3c507fb4-71e1-4136-9d40-8eca6590250d

## Spacer(int) constructor — non-existent constructor overload

**Hallucinated API:**
```csharp
new Spacer(6)
new Spacer(2)
new Spacer(4)
```

**Error:** `'Spacer' does not contain a constructor that takes 1 arguments`

**Correct API:**
```csharp
new Spacer().Height(Size.Units(6))
// or
new Spacer().Width(Size.Units(6))
```

Spacer has only a parameterless constructor. Use fluent `.Height()` or `.Width()` to set size.

**Found In:**
276d383f-696e-4d67-bc6e-14502c59734b

## Button.Color(Colors.X) — non-existent fluent method

**Hallucinated API:**
```csharp
new Button(label).Color(colors[i])
```

**Error:** `'Button' does not contain a definition for 'Color' and the best extension method overload 'LabelExtensions.Color(Label, Colors)' requires a receiver of type 'Ivy.Label'`

**Correct API:**
Button doesn't have `.Color()`. Use `.Variant(ButtonVariant.X)` or fluent shortcuts like `.Primary()`, `.Destructive()`. `.Color()` only exists on `Label` via `LabelExtensions`. Variant of documented `Badge.Color()` and `Callout.Color()` patterns.

**Found In:**
276d383f-696e-4d67-bc6e-14502c59734b

## UseAlert().ShowInfo() — wrong API usage

**Hallucinated API:**
```csharp
var alert = UseAlert();
alert.ShowInfo("title", "message");
```

**Error:** `'(IView? alertView, ShowAlertDelegate showAlert)' does not contain a definition for 'ShowInfo'`

**Correct API:**
```csharp
var (alertView, showAlert) = UseAlert();
showAlert("message", result => { }, "title", AlertButtonSet.Ok);
```

`UseAlert()` returns a tuple `(IView? alertView, ShowAlertDelegate showAlert)`, not an object with methods. Destructure the tuple and call the delegate directly. The `alertView` must be included in the returned view tree.

Also seen as `showAlert.Error("message")` — the agent confuses `ShowAlertDelegate` methods with `IClientProvider` toast methods. For simple error/success notifications, use `IClientProvider.Error()` or `IClientProvider.Toast()` instead of `UseAlert`.

**Found In:**
276d383f-696e-4d67-bc6e-14502c59734b, 0a42123e-a489-433a-93e5-87d4de7075eb

## Size.Flex() — non-existent static method

**Hallucinated API:**
```csharp
new Spacer().Width(Size.Flex())
.Height(Size.Flex())
```

**Error:** `'Size' does not contain a definition for 'Flex'`

**Correct API:**
```csharp
new Spacer().Width(Size.Grow())
```

The agent confused CSS flexbox terminology with Ivy's API.

**Found In:**
276d383f-696e-4d67-bc6e-14502c59734b

## RefreshToken.Version — non-existent property

**Hallucinated API:**
```csharp
refreshToken.Version
```

**Error:** `'RefreshToken' does not contain a definition for 'Version'`

**Correct API:**
`RefreshToken` has these members: `Token` (Guid), `ReturnValue` (object?), `IsRefreshed` (bool), `Refresh()`, `ToTrigger()`. There is no `Version` property. Pass `refreshToken` directly as a dependency to `UseQuery`, or use `refreshToken.Token` if you need a changing value.

Source: `D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Hooks\UseRefreshToken.cs`

**Found In:**
a224c9f6-94b2-4b9f-9d5c-6a9ba67d5b3b (traces 002-005, 008-009)

## QueryResult\<T\>.Data — wrong property name

**Hallucinated API:**
```csharp
queryResult.Data
```

**Error:** `'QueryResult<T>' does not contain a definition for 'Data'`

**Correct API:**
`queryResult.Value` — The property is `.Value`, not `.Data`. `QueryResult<T>` is a record with: `Value` (T?), `Loading` (bool), `Validating` (bool), `Previous` (bool), `Mutator` (QueryMutator<T>), `Error` (Exception?).

Source: `D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Hooks\UseQuery.cs`

**Found In:**
a224c9f6-94b2-4b9f-9d5c-6a9ba67d5b3b (traces 002, 004)

## QueryResult\<T\>.IsLoading — wrong property name

**Hallucinated API:**
```csharp
queryResult.IsLoading
```

**Error:** `'QueryResult<T>' does not contain a definition for 'IsLoading'`

**Correct API:**
`queryResult.Loading` — The property is `.Loading`, not `.IsLoading`. Similarly, `.Validating` not `.IsValidating`, and `.Previous` not `.IsPrevious`.

**Found In:**
a224c9f6-94b2-4b9f-9d5c-6a9ba67d5b3b (traces 002, 004)

## ListItem.Description / ListItem.Meta / ListItem.Actions — non-existent members

**Hallucinated API:**
```csharp
ListItem.Description("text")
ListItem.Meta("text")
ListItem.Actions(button1, button2)
```

**Error:** `'ListItem' does not contain a definition for 'Description'/'Meta'/'Actions'`

**Correct API:**
`ListItem` is a record with constructor parameters: `title`, `subtitle`, `onClick`, `icon`, `badge`, `tag`, `items`. Use `subtitle` for descriptions. There are no `.Description()`, `.Meta()`, or `.Actions()` methods. The only extension method is `.Content(child)`.

Source: `D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Widgets\Lists\ListItem.cs`

**Found In:**
a224c9f6-94b2-4b9f-9d5c-6a9ba67d5b3b (traces 008, 009)

## Size.Sm — non-existent member

**Hallucinated API:**
```csharp
Size.Sm
```

**Error:** `'Size' does not contain a definition for 'Sm'`

**Correct API:**
`Size` does not have Tailwind-style size aliases like `Sm`, `Md`, `Lg`. Use `Size.Units(n)` for specific pixel values, or `Size.Full()`, `Size.Grow()`, `Size.Fit()` for relative sizing.

**Found In:**
a224c9f6-94b2-4b9f-9d5c-6a9ba67d5b3b (traces 008, 009)

## String literal as Icons? — wrong type

**Hallucinated API:**
```csharp
// Using string literals like "edit", "delete", "trash" where Icons? is expected
new RowAction("Edit", icon: "edit")
```

**Error:** `Cannot implicitly convert type 'string' to 'Ivy.Icons?'`

**Correct API:**
Always use the `Icons` enum: `Icons.Pencil`, `Icons.Trash2`, `Icons.Plus`, etc. There is no implicit conversion from string to Icons. The refactoring service already handles invalid Icons enum values via LLM-based matching, but it cannot fix string-to-enum type mismatches.

**Found In:**
a224c9f6-94b2-4b9f-9d5c-6a9ba67d5b3b (traces 002, 003, 005, 008)

## Text.Small("text") — static factory confusion

**Hallucinated API:**
```csharp
Text.Small(frequencyText).Muted()
```

**Error:** `No overload for method 'Small' takes 1 arguments`

**Correct API:**
```csharp
Text.P(frequencyText).Small().Muted()
// or
Text.Block(frequencyText).Small().Muted()
```

`Small()` is an instance modifier on `TextBuilder` (returns `Scale(Ivy.Scale.Small)`), not a static factory. The static factories are `Text.P()`, `Text.H1()`, `Text.H2()`, `Text.H3()`, `Text.H4()`, `Text.Block()`, `Text.Label()`, etc. Chain `.Small()` after creating the text.

**Found In:**
ce144de9-0688-490a-bef6-b2766e323154

## Box.BorderRadius(int) — wrong argument type

**Hallucinated API:**
```csharp
new Box(content).BorderRadius(8)
```

**Error:** `'Box' does not contain a definition for 'BorderRadius'` (CS1929 — no extension matches `Box.BorderRadius(int)`)

**Correct API:**
```csharp
new Box(content).BorderRadius(BorderRadius.Rounded)
```

`Box.BorderRadius()` takes a `BorderRadius` enum (`None`, `Rounded`, `Full`), not an integer. The agent ignored the IvyQuestion MCP response and used an int literal instead.

**Found In:**
ce144de9-0688-490a-bef6-b2766e323154

## GridView.Background() — non-existent method

**Hallucinated API:**
```csharp
Layout.Grid(items).Columns(8).Gap(1).Background(Colors.Slate)
```

**Error:** `'GridView' does not contain a definition for 'Background'`

**Correct API:**
```csharp
new Box(
    Layout.Grid(items).Columns(8).Gap(1)
).Color(Colors.Slate)
```

`GridView` does not have a `.Background()` method. To add a background color to a grid, wrap it in a `Box` and use `.Color()` on the Box. This pattern applies to any view that needs a background color — `Box` is the universal container for adding visual styling.

**Found In:**
7e97011f-41b3-42d3-98ea-3b7faad347c2

## GridView.AddChildren() / GridView.Children() — non-existent methods

**Hallucinated API:**
```csharp
var grid = new GridView();
grid.AddChildren(widget1, widget2);
// or
grid.Children(widget1, widget2);
```

**Error:** `CS1061: 'GridView' does not contain a definition for 'AddChildren'/'Children'`

**Correct API:**
```csharp
// Use the pipe operator to add children to a GridView:
var grid = new GridView(columns: 8);
grid | widget1 | widget2;
// Or pass children in constructor:
new GridView(columns: 8, children: new[] { widget1, widget2 });
```

**Found In:**
5c9cfb70-c9f5-4642-8de6-480be8f5ee85

## OnClick() on non-clickable widgets — extension method receiver mismatch

**Hallucinated API:**
```csharp
myCustomView.OnClick(e => ...)
new LayoutView().OnClick(e => ...)
```

**Error:** `CS1929: 'MyView' does not contain a definition for 'OnClick' and the best extension method overload requires a receiver of type 'Card'/'Button'/'Badge'`

**Correct API:**
```csharp
// OnClick is only available on specific widgets: Card, Button, Badge, Image, Box
// For custom click handling, wrap in a Box or use a Button:
new Box(myCustomView).OnClick(e => ...)
// Or use a Card:
new Card(myCustomView).OnClick(e => ...)
```

**Found In:**
5c9cfb70-c9f5-4642-8de6-480be8f5ee85

## Size.Pixels() — wrong method name

**Hallucinated API:**
```csharp
Size.Pixels(280)
```

**Error:** `'Size' does not contain a definition for 'Pixels'`

**Correct API:**
```csharp
Size.Px(280)
```

The method is `Size.Px()`, not `Size.Pixels()`. The agent expanded the abbreviated name. **Auto-fixed:** The refactoring service automatically rewrites `Size.Pixels(...)` → `Size.Px(...)`.

**Found In:**
7c51c481-c48e-4398-8db3-60cfac6379d5 (trace 002)

## string.ToCodeInput() — wrong receiver type

**Hallucinated API:**
```csharp
responseBody.Value.ToCodeInput().Language(Languages.Json)
responseHeaders.Value.ToCodeInput().Language(Languages.Text)
```

**Error:** `'string' does not contain a definition for 'ToCodeInput' and the best extension method overload 'CodeInputExtensions.ToCodeInput(IAnyState, ...)' requires a receiver of type 'Ivy.IAnyState'`

**Correct API:**
```csharp
// For read-only display of code, use CodeBlock:
new CodeBlock(stringValue, Languages.Json)

// For editable code input, bind to state first:
var editableState = UseState(stringValue);
editableState.ToCodeInput().Language(Languages.Json)
```

`.ToCodeInput()` is an extension on `IAnyState`, not on `string`. For display-only code, use `CodeBlock` instead of a code input. Only use `.ToCodeInput()` when the user needs to edit the code, and bind the string to state first.

**Found In:**
535f38d4-b9d5-43bf-a3d9-b4b17e6ecbb0

## State\<T\> — non-existent type

**Hallucinated API:**
```csharp
private State<List<Player>> _players;
```

**Error:** `The type or namespace name 'State<>' could not be found`

**Correct API:**
```csharp
var players = UseState(new List<Player>());
```

`State<T>` does not exist. `UseState<T>()` returns `IState<T>`. State is created inside `Build()` via hooks, not stored as fields.

**Found In:**
84faf65a-c7df-4b5a-888b-4c49255c50ab (traces 004, 006, 008)

## IRefreshToken — non-existent interface

**Hallucinated API:**
```csharp
private readonly IRefreshToken _refreshToken;
```

**Error:** `The type or namespace name 'IRefreshToken' could not be found`

**Correct API:**
```csharp
var refreshToken = UseRefreshToken();
```

`IRefreshToken` does not exist. `UseRefreshToken()` returns a `RefreshToken` class. Like all hooks, call inside `Build()`.

**Found In:**
84faf65a-c7df-4b5a-888b-4c49255c50ab (traces 004, 005, 006)

## DataTable\<T\> — non-generic type used with type arguments

**Hallucinated API:**
```csharp
new DataTable<Player>(players)
```

**Error:** `The non-generic type 'DataTable' cannot be used with type arguments`

**Correct API:**
```csharp
players.ToDataTable()
```

`DataTable` is non-generic. Use `.ToDataTable()` extension method on `IEnumerable<T>` or `IQueryable<T>`.

**Found In:**
84faf65a-c7df-4b5a-888b-4c49255c50ab (trace 004)

## Shrink(int) — method takes no arguments

**Hallucinated API:**
```csharp
Text.P("vs").Shrink(1)
```

**Error:** `No overload for method 'Shrink' takes 1 arguments`

**Correct API:**
```csharp
Text.P("vs").Shrink()
```

`.Shrink()` takes no arguments. It is a simple fluent modifier.

**Found In:**
84faf65a-c7df-4b5a-888b-4c49255c50ab (traces 004, 007)

## Card.Padding() — non-existent method

**Hallucinated API:**
```csharp
new Card(content).Padding(20)
```

**Error:** `'Card' does not contain a definition for 'Padding'`

**Correct API:**
```csharp
new Box(content).Padding(20)
```

`Card` has no `.Padding()` method. Cards have built-in padding. For custom padding, wrap content in a `Box`.

**Found In:**
84faf65a-c7df-4b5a-888b-4c49255c50ab (trace 004)

## TextBuilder.Padding() — non-existent method

**Hallucinated API:**
```csharp
Text.Block(content).Padding(16)
Text.P(content).Padding(4)
```

**Error:** `CS1929: 'TextBuilder' does not contain a definition for 'Padding'`

**Correct API:**
```csharp
// Wrap text in a Box for padding:
new Box(Text.Block(content)).Padding(16)

// Or wrap in a layout:
Layout.Vertical().Padding(16)
    | Text.Block(content)
```

`TextBuilder` does not have `.Padding()`. Padding is available on container widgets (`Box`, `LayoutView`, `TabView`, `GridView`). To add padding around text, wrap it in a `Box` or layout. This is a variant of the `TextBuilder.AlignCenter()` and `TextBuilder.Style()` hallucinations — the agent applies container-level styling to text elements.

**Found In:**
7c547408-00b3-47e1-976e-59c9357c1e74

## MetricCard — non-existent class name

**Hallucinated API:**
```csharp
new MetricCard("Title", "Value", Icons.Activity)
```

**Error:** `CS0246: The type or namespace name 'MetricCard' could not be found`

**Correct API:**
```csharp
new MetricView("Title", "Value", icon: Icons.Activity)
```

`MetricCard` does not exist. The correct class is `MetricView`. Constructor: `MetricView(string title, string value, string? description = null, Icons? icon = null, IView? chart = null)`.

**Found In:**
c008af27-1cb1-4ab3-b41a-36aa711c6a41

## Disposable.Create() — missing using statement

**Hallucinated usage (missing using):**
```csharp
return Disposable.Create(() => timer?.Dispose());
```

**Error:** `CS0103: The name 'Disposable' does not exist in the current context`

**Fix:** Add the using statement — the package IS available as a transitive dependency:
```csharp
using System.Reactive.Disposables;

return Disposable.Create(() => timer?.Dispose());
```

`System.Reactive` is a transitive dependency of Ivy Framework. The error occurs because the agent omits the `using System.Reactive.Disposables;` directive, not because the package is missing.

**Found In:**
fb184b5b-8254-4a1f-b8f2-ab8e8657fdbc

## Button.Visible() / Widget.Visible() — removed conditional rendering method

**Hallucinated API:**
```csharp
new Button("Reset").Visible(hasDate)
```

**Error:** `'Button' does not contain a definition for 'Visible'` (CS1061)

**Correct API:**
```csharp
// Use a simple if statement for conditional rendering:
if (hasDate)
    yield return new Button("Reset");

// Or use a ternary:
var resetButton = hasDate ? new Button("Reset") : null;
```

The `.Visible()` extension method was removed from `WidgetBase` (commit f869df302). `LayoutView.Visible()` was also removed. The only remaining `.Visible()` is `FormBuilder<TModel>.Visible(field, predicate)` which controls form field visibility — not widget rendering. The agent confuses this with the old WidgetBase API or UI frameworks like WPF/WinForms that have a `Visible` property. In Ivy, conditional rendering is done with standard C# control flow (`if`, ternary, etc.) like in React.

**Found In:**
18763683-ff01-4f76-8dc5-6f0bfe750e4a

## Card.Secondary() — Badge extension used on Card

**Hallucinated API:**
```csharp
new Card(...).Secondary()
```

**Error:** `CS1929: 'Card' does not contain a definition for 'Secondary' and the best extension method overload 'BadgeExtensions.Secondary(Badge)' requires a receiver of type 'Ivy.Badge'`

**Correct API:**
```csharp
// Cards don't have variants. To style card content, style the children:
new Card(new Text("Content").Secondary())
// Or use a Box with background:
new Box(content).Background(Colors.Gray100)
```

**Found In:**
ab38eba1-af47-4003-905b-4fe9cea8ba4f

## Card.Children() — MenuItem extension used on Card

**Hallucinated API:**
```csharp
new Card().Children(child1, child2)
```

**Error:** `CS1929: 'Card' does not contain a definition for 'Children' and the best extension method overload 'MenuItemExtensions.Children(MenuItem, params MenuItem[])' requires a receiver of type 'Ivy.MenuItem'`

**Correct API:**
```csharp
// Use the constructor or pipe operator:
new Card(child1 / child2)
// Or:
var card = new Card();
card | (child1 / child2);
```

**Found In:**
ab38eba1-af47-4003-905b-4fe9cea8ba4f

## Card.Child — Non-existent property

**Hallucinated API:**
```csharp
Card.Child(content)
// or
new Card { Child = content }
```

**Error:** `CS0117: 'Card' does not contain a definition for 'Child'`

**Correct API:**
```csharp
// Use the constructor, pipe operator, or .Content():
new Card(content)
new Card() | content
new Card().Content(content)
```

**Found In:**
2e18b175-94ec-459c-94a5-8f28b81ecfdc

## Card.Background() — Box extension used on Card

**Hallucinated API:**
```csharp
new Card(...).Background(Colors.Gray100)
```

**Error:** `CS1929: 'Card' does not contain a definition for 'Background' and the best extension method overload 'BoxExtensions.Background(Box, Colors)' requires a receiver of type 'Ivy.Box'`

**Correct API:**
```csharp
// Wrap in a Box for background color:
new Box(new Card(content)).Background(Colors.Gray100)
// Or use Card's built-in styling via content:
new Card(content)
```

Similar to the GridView.Background() hallucination — `.Background()` is a Box-only extension.

**Found In:**
ab38eba1-af47-4003-905b-4fe9cea8ba4f

## Button.ColSpan() — non-existent grid span method

**Hallucinated API:**
```csharp
new Button("=").ColSpan(2)
```

**Error:** `CS1061: 'Button' does not contain a definition for 'ColSpan'`

**Correct API:**
```csharp
// Grid column spanning is not set on child widgets.
// Use GridLayout column definitions to control spans,
// or use multiple grid cells for the same widget.
```

**Found In:**
ab38eba1-af47-4003-905b-4fe9cea8ba4f

## IState\<T\>.ToTextArea() — incorrect textarea method name

**Hallucinated API:**
```csharp
var text = UseState("");
text.ToTextArea()
```

**Error:** `CS1061: 'IState<string>' does not contain a definition for 'ToTextArea'`

**Correct API:**
```csharp
var text = UseState("");
text.ToTextareaInput()
// or equivalently:
text.ToTextInput().Multiline()
```

The method is `ToTextareaInput()`, not `ToTextArea()`. Alternatively use `ToTextInput().Multiline()`. See `Docs/02_Widgets/04_Inputs/02_TextInput.md` for full textarea documentation.

**Found In:**
19ec33cf-3e86-409e-806c-babf0d20730f

## IState\<T\>.ToSelect() — incorrect select method name

**Hallucinated API:**
```csharp
var format = UseState("Option1");
format.ToSelect(options)
```

**Error:** `CS1061: 'IState<string>' does not contain a definition for 'ToSelect'`

**Correct API:**
```csharp
var format = UseState("Option1");
format.ToSelectInput(new[] { "Option1", "Option2" }.ToOptions())
```

The method is `ToSelectInput()`, not `ToSelect()`. Options are passed as `IEnumerable<IAnyOption>` — use `.ToOptions()` on a string array to convert.

**Found In:**
19ec33cf-3e86-409e-806c-babf0d20730f

## Card.When() — non-existent conditional rendering method

**Hallucinated API:**
```csharp
new Card(outputText).When(hasOutput)
```

**Error:** `CS1061: 'Card' does not contain a definition for 'When'`

**Correct API:**
```csharp
// Use standard C# control flow for conditional rendering:
if (hasOutput)
{
    new Card(outputText);
}
```

There is no `.When()` method on any widget. Ivy uses standard C# `if` statements for conditional rendering, similar to React's conditional rendering pattern. See also the existing `.Visible()` hallucination entry.

**Found In:**
19ec33cf-3e86-409e-806c-babf0d20730f

## Card.Style() / Card.ClassName() / Card.WithStyle() — non-existent CSS methods

**Hallucinated API:**
```csharp
new Card(...).Style("background: green")
new Card(...).ClassName("my-class")
new Card(...).WithStyle(new { Background = "green" })
```

**Error:** `CS1061: 'Card' does not contain a definition for 'Style'/'ClassName'/'WithStyle'`

**Correct API:**
```csharp
// Cards don't support direct CSS styling. To add a colored background, wrap in a Box:
new Box(new Card(content)).Background(Colors.Green)
// Or use a Box directly instead of Card when you need full styling control:
new Box(content).Background(Colors.Green).Padding(20).Rounded()
```

**Found In:**
5c9cfb70-c9f5-4642-8de6-480be8f5ee85

## Card.Border() — Box extension used on Card

**Hallucinated API:**
```csharp
new Card(...).Border(1)
```

**Error:** `CS1929: 'Card' does not contain a definition for 'Border'`

**Correct API:**
```csharp
// Cards have a built-in border. For custom borders, wrap in a Box:
new Box(new Card(content)).Border(1)
```

**Found In:**
5c9cfb70-c9f5-4642-8de6-480be8f5ee85

## Card.Color() — non-existent method on Card

**Hallucinated API:**
```csharp
new Card(...).Color(Colors.Green)
```

**Error:** `CS1061: 'Card' does not contain a definition for 'Color'`

**Correct API:**
```csharp
// Cards don't have a Color method. Use Box for colored containers:
new Box(content).Background(Colors.Green)
```

**Found In:**
5c9cfb70-c9f5-4642-8de6-480be8f5ee85

## Card.Align() — non-existent method on Card

**Hallucinated API:**
```csharp
new Card(...).Align(Align.Center)
```

**Error:** `CS1929: 'Card' does not contain a definition for 'Align'`

**Correct API:**
```csharp
// Use a Layout to control alignment of card content:
Layout.Vertical(Align.Center) | new Card(content)
```

**Found In:**
5c9cfb70-c9f5-4642-8de6-480be8f5ee85

## Nested Layout | operator without parentheses

**Hallucinated pattern:**
```csharp
Layout.Vertical()
    | Layout.Horizontal().Gap(4)
        | child1
        | child2
    | otherContent;
```

**Problem:** C# evaluates `|` left-to-right. Without parentheses, `child1` and `child2` are added to the outer `Vertical` layout, not the inner `Horizontal`. The indentation is misleading — C# ignores indentation.

**Correct pattern:**
```csharp
Layout.Vertical()
    | (Layout.Horizontal().Gap(4)
        | child1
        | child2)
    | otherContent;
```

Always wrap nested layouts in parentheses `(Layout.Horizontal() | child1 | child2)` to ensure children are added to the correct parent layout.

**Found In:**
19ec33cf-3e86-409e-806c-babf0d20730f

## Edge — Non-existent margin edge enum

**Hallucinated API:**
```csharp
widget.Margin(Edge.Top, 4)
```

**Error:** `CS0103: The name 'Edge' does not exist in the current context`

**Correct API:**
```csharp
// Use WithMargin with positional int parameters (left, top, right, bottom):
widget.WithMargin(0, 4, 0, 0) // top margin of 4

// Or use Layout.Margin:
Layout.Vertical().Margin(0, 4, 0, 0) | widget
```

**Found In:**
2e18b175-94ec-459c-94a5-8f28b81ecfdc

## WithMargin(top: 4) — Named parameters don't exist

**Hallucinated API:**
```csharp
widget.WithMargin(top: 4)
```

**Error:** `CS7036: There is no argument given that corresponds to the required parameter 'left' of 'LayoutExtensions.WithMargin(object, int, int, int, int)'`

**Correct API:**
```csharp
// WithMargin has three overloads, all with positional parameters:
widget.WithMargin(4)            // uniform margin
widget.WithMargin(4, 2)         // horizontal, vertical
widget.WithMargin(0, 4, 0, 0)   // left, top, right, bottom
```

**Found In:**
2e18b175-94ec-459c-94a5-8f28b81ecfdc

## Margin(new Thickness(...)) — Margin takes int, not Thickness

**Hallucinated API:**
```csharp
layout.Margin(new Thickness(0, 4, 0, 0))
```

**Error:** `CS1503: Argument 1: cannot convert from 'Ivy.Thickness' to 'int'`

**Correct API:**
```csharp
// Margin() takes int parameters directly:
layout.Margin(4)              // uniform
layout.Margin(4, 2)           // horizontal, vertical
layout.Margin(0, 4, 0, 0)    // left, top, right, bottom
```

**Found In:**
2e18b175-94ec-459c-94a5-8f28b81ecfdc

## Form() — internal constructor

**Hallucinated API:**
```csharp
new Form()
new Form(children)
```

**Error:** `CS1729: 'Form' does not contain a constructor that takes 0 arguments`

**Correct API:**
```csharp
// Forms are created from state objects:
state.ToForm()
    .Field(f => f.Name)
    .Field(f => f.Email)
```

`Form` constructors are `internal`. Forms must be created using the `.ToForm()` extension method on `IState<T>`. The agent should never use `new Form()` directly.

**Found In:**
5d2202d2-9d6b-4198-9922-c3763534aca5

## new TextInput() — parameterless constructor

**Hallucinated API:**
```csharp
new TextInput()
```

**Error:** `CS1729: 'TextInput' does not contain a constructor that takes 0 arguments`

**Correct API:**
```csharp
var text = this.UseState("");
// ...
text.ToTextInput()
```

All input widgets should be created from state variables using `.To*Input()` extension methods, not via constructors. This applies to `TextInput`, `NumberInput`, `DateTimeInput`, `SelectInput`, `BoolInput`, `ColorInput`, etc.

**Found In:**
dee4652c-ff8a-4ca4-a354-de8a3f9ddd75

## TextInputBase.Css() — non-existent styling method

**Hallucinated API:**
```csharp
text.ToTextInput().Css("...")
```

**Error:** `CS1061: 'TextInputBase' does not contain a definition for 'Css'`

**Correct API:**
Use `Style()` for inline styles or the standard sizing/layout extensions:
```csharp
text.ToTextInput().Width(300)
```

**Found In:**
dee4652c-ff8a-4ca4-a354-de8a3f9ddd75

## AnimationType.Spin / AnimationType.None — non-existent enum values

**Hallucinated API:**
```csharp
box.WithAnimation(AnimationType.Spin);
box.WithAnimation(AnimationType.None);
```

**Error:** `CS0117: 'AnimationType' does not contain a definition for 'Spin'`

**Correct API:**
```csharp
box.WithAnimation(AnimationType.Rotate);
```

`AnimationType` values: Rotate, SlideIn, FadeIn, ZoomIn, SlideOut, FadeOut, ZoomOut, Bounce, Shake, Flip, Stagger, Wave, Pulse, Spring, Hover. There is no `Spin` or `None` value.

**Found In:**
7aaec87b-1189-4439-863e-3ee0c219a5d1

## Card.BackgroundColor() / Box.BackgroundColor() — non-existent method

**Hallucinated API:**
```csharp
card.BackgroundColor(Colors.Primary)
box.BackgroundColor(Colors.Primary)
```

**Error:** `CS1061: 'Card'/'Box' does not contain a definition for 'BackgroundColor'`

**Correct API:**
```csharp
new Box(content).Background(Colors.Primary)
```

The method is `Background`, not `BackgroundColor`. It is available on `Box` (not on `Card`). To set background on a Card, wrap it in a Box.

**Found In:**
7aaec87b-1189-4439-863e-3ee0c219a5d1

## Card.MinWidth() — non-existent method

**Hallucinated API:**
```csharp
card.MinWidth(Size.Units(48))
```

**Error:** `CS1061: 'Card' does not contain a definition for 'MinWidth'`

**Correct API:**
```csharp
Layout.Vertical(card).Width(Size.Units(48))
```

No widget in Ivy has a `MinWidth` method. To constrain width, wrap the widget in a Layout with `.Width()`.

**Found In:**
7aaec87b-1189-4439-863e-3ee0c219a5d1

## Align.End / Align.Start — CSS-inspired enum values

**Hallucinated API:**
```csharp
Align.End
Align.Start
Align.FlexEnd
Align.FlexStart
```

**Error:** `'Align' does not contain a definition for 'End'` (CS0117)

**Correct API:**
```csharp
Align.Right   // instead of Align.End or Align.FlexEnd
Align.Left    // instead of Align.Start or Align.FlexStart
```

Valid `Align` values: `TopLeft`, `TopRight`, `TopCenter`, `BottomLeft`, `BottomRight`, `BottomCenter`, `Left`, `Right`, `Center`, `Stretch`, `SpaceBetween`, `SpaceAround`, `SpaceEvenly`.

The agent draws from CSS `justify-content: flex-end` / `align-items: flex-end` terminology. **Auto-fixed:** The refactoring service automatically rewrites `Align.End` → `Align.Right`, `Align.Start` → `Align.Left`, etc.

**Found In:**
DecisionMatrixApp.cs (two occurrences of `Align.End`)

## TextInput.Grow() — Box-only extension called on TextInput

**Hallucinated API:**
```csharp
new TextInput(query).Grow()
```

**Error:** `CS1929: 'TextInput' does not contain a definition for 'Grow'`

**Correct API:**
```csharp
query.ToTextInput().Width(Size.Grow())
```

`Grow()` was originally defined only as a `Box`-specific extension method in `Box.cs`. It is not available on `TextInput` or other widget types. Use `.Width(Size.Grow())` directly, or note that `Grow()` has since been promoted to a generic `WidgetBase<T>` extension and is now available on all widgets.

**Found In:**
7a9aadf3

## Progress.Max() — non-existent method

**Hallucinated API:**
```csharp
new Progress(value).Max(100)
```

**Error:** `'Progress' does not contain a definition for 'Max'` (CS1929)

**Correct API:**
```csharp
new Progress(value) // value is 0-100 (percentage)
```

The `Progress` widget always uses percentage values (0-100). There is no `.Max()` or `.Min()` method. If you need a custom range, normalize the value to 0-100 before passing it. Note: `ProgressBuilder<T>` (used in DataTable column builders) does have `.Min()` and `.Max()`, but these are not available on the `Progress` widget itself.

**Found In:**
ec6b51cb-29aa-4b6c-89dc-24d1e7bba68f

## using Ivy.Icons — enum used as namespace

**Hallucinated API:**
```csharp
using Ivy.Icons;
```

**Error:** `CS0138: A 'using namespace' directive can only be applied to namespaces; 'Icons' is a type not a namespace`

**Correct API:**
```csharp
using static Ivy.Icons;
// Then use: Icons.FileCode
// Or without the using: Ivy.Icons.FileCode
```

`Icons` is an enum (`Ivy.Icons`), not a namespace. Use `using static Ivy.Icons;` if you want unqualified access to icon values, or reference them as `Icons.FileCode` / `Ivy.Icons.FileCode`.

**Found In:**
0a42123e-a489-433a-93e5-87d4de7075eb

## CodeInput.ReadOnly() — non-existent method

**Hallucinated API:**
```csharp
codeInput.ReadOnly()
```

**Error:** `CS1061: 'CodeInputBase' does not contain a definition for 'ReadOnly'`

**Correct API:**
```csharp
codeInput.Disabled(true)
```

`CodeInput` does not have a `.ReadOnly()` method. Use `.Disabled(true)` to make a CodeInput non-editable. The agent draws from HTML `readonly` attribute or other UI frameworks. In Ivy, the `Disabled` extension is the standard way to prevent editing on all input widgets.

**Found In:**
0a42123e-a489-433a-93e5-87d4de7075eb

## View / IComponent — non-existent base types

**Hallucinated API:**
```csharp
public class MyApp : View { }
public class MyApp : IComponent { }
```

**Error:** `CS0246: The type or namespace name 'View' could not be found` / `CS0246: The type or namespace name 'IComponent' could not be found`

**Correct API:**
```csharp
public class MyApp : ViewBase { }
```

The base class for all Ivy views/apps is `ViewBase`, not `View` or `IComponent`. The agent confuses Ivy's naming with React (`Component`), Blazor (`ComponentBase`), or generic UI patterns (`View`).

**Found In:**
0a42123e-a489-433a-93e5-87d4de7075eb

## TableHeader — non-existent class

**Hallucinated API:**
```csharp
new TableHeader("Name", "Age", "Email")
```

**Error:** `The type or namespace name 'TableHeader' could not be found`

**Correct API:**
There is no `TableHeader` class. Use `TableRow` with `.IsHeader()`:
```csharp
new Table(
    new TableRow(new TableCell("Name"), new TableCell("Age"), new TableCell("Email")).IsHeader(),
    new TableRow(new TableCell("Alice"), new TableCell("30"), new TableCell("alice@example.com"))
)
```

**Found In:**
1e59a9a1-4d98-4491-84d9-6f6e74bcbdad
## Table.Header() — non-existent fluent method

**Hallucinated API:**
```csharp
new Table().Header(new TableRow(...))
```

**Error:** `'Table' does not contain a definition for 'Header'`

**Correct API:**
`Table` has no `.Header()` method. Pass all rows (including the header row) directly to the constructor:
```csharp
new Table(headerRow, dataRow1, dataRow2)
```
Mark the header row with `.IsHeader()` on the `TableRow`.

**Found In:**
1e59a9a1-4d98-4491-84d9-6f6e74bcbdad
## Text.Secondary("text") — non-existent static factory

**Hallucinated API:**
```csharp
Text.Secondary("some text")
```

**Error:** `CS1501: No overload for method 'Secondary' takes 1 arguments`

**Correct API:**
```csharp
// Use Text.Muted() for secondary/muted appearance:
Text.Muted("some text")
// Or use Text.P() with .Muted() chained:
Text.P("some text").Muted()
// Or use Text.P() with Colors.Secondary color:
Text.P("some text").Color(Colors.Secondary)
```

`Text.Secondary()` does not exist as a static factory method. The static factories on `Text` are: `H1`, `H2`, `H3`, `H4`, `H5`, `H6`, `P`, `Inline`, `Block`, `Blockquote`, `Monospaced`, `Lead`, `Label`, `Muted`, `Strong`, `Bold`, `Danger`, `Warning`, `Success`, `Code`, `Markdown`, `Json`, `Xml`, `Html`, `Latex`, `Display`, `Literal`, `Rich`. The agent likely confused `Secondary` from `ButtonVariant.Secondary` / `Button.Secondary()` or `BadgeVariant.Secondary` / `Badge.Secondary()` with the `Text` API. `.Secondary()` is a fluent method on `Button` and `Badge`, not on `Text`.

**Found In:**
(session not yet recorded)

## SelectInput<T>.Width() — generic constraint mismatch

**Hallucinated API:**
```csharp
language.ToSelectInput(options).Width(Size.Px(200))
```

**Error:** `CS0311: The type 'Ivy.SelectInput<string>' cannot be used as type parameter 'T' in the generic type or method 'WidgetBaseExtensions.Width<T>(T, Size?)'`

**Correct API:**
```csharp
// Cast to SelectInputBase first:
(SelectInputBase)language.ToSelectInput(options).Width(Size.Px(200))
// Or wrap in a Box with width:
new Box(language.ToSelectInput(options)).Width(Size.Px(200))
```

`SelectInput<T>` inherits from `SelectInputBase : WidgetBase<SelectInputBase>`, not `WidgetBase<SelectInput<T>>`. The `Width<T>()` extension requires `T : WidgetBase<T>`, which `SelectInput<T>` doesn't satisfy.

### Found In
852f6bec-756c-48f8-93da-ad426af73fab

## FileUploadStatus.Completed — non-existent enum value

**Hallucinated API:**
```csharp
if (upload.Status == FileUploadStatus.Completed)
```

**Error:** `'FileUploadStatus' does not contain a definition for 'Completed'`

**Correct API:**
```csharp
if (upload.Status == FileUploadStatus.Finished)
```

`FileUploadStatus` values are: `Pending`, `Aborted`, `Loading`, `Failed`, `Finished`. There is no `Completed` value. **Auto-fixed:** The refactoring service automatically rewrites `FileUploadStatus.Completed` → `FileUploadStatus.Finished`.

**Found In:**
(session not yet recorded)

## UseDownload — ambiguous overload between sync and async

**Hallucinated API:**
```csharp
UseDownload(() => bytes, "file.txt", "text/plain")
```

**Error:** `CS0121: The call is ambiguous between 'ViewBase.UseDownload(Func<byte[]>, string, string)' and 'ViewBase.UseDownload(Func<Task<byte[]>>, string, string)'`

**Correct API:**
```csharp
// For sync: explicitly type the delegate
UseDownload((Func<byte[]>)(() => bytes), "file.txt", "text/plain")

// Or use a named method:
byte[] GetBytes() => bytes;
UseDownload(GetBytes, "file.txt", "text/plain")
```

When using `UseDownload` with a lambda, you must explicitly cast to `Func<byte[]>` or `Func<Task<byte[]>>` to avoid ambiguity.

**Found In:**
(session not yet recorded)

## Server.OnReady / Server.OnStartup — non-existent lifecycle callbacks

**Hallucinated API:**
```csharp
server.OnReady(() => { /* seed data */ });
server.OnStartup(() => { /* initialize */ });
```

**Error:** `CS1061: 'Server' does not contain a definition for 'OnReady'`

**Correct API:**
```csharp
// Seed data via the context factory pattern:
var connection = server.UseConnection<MyDbContext>(options =>
    options.ContextFactory = () =>
    {
        var ctx = new MyDbContext();
        ctx.Database.EnsureCreated();
        SeedData(ctx);
        return ctx;
    });

// Or resolve services directly in Program.cs:
var myService = server.Services.GetRequiredService<IMyService>();
myService.Initialize();
```

The `Server` class does not have `OnReady`, `OnStartup`, or similar lifecycle callback methods. To run initialization code (e.g., database seeding), use the connection's context factory pattern — seed data in the factory's `CreateContext` method or use `server.Services` to resolve and call services directly in `Program.cs`.

**Found In:**
(session not yet recorded)

## Fragment.Empty — non-existent static member

**Hallucinated API:**
```csharp
return Fragment.Empty;
```

**Error:** `'Fragment' does not contain a definition for 'Empty'`

**Correct API:**
```csharp
// Use ViewBase.Empty:
return ViewBase.Empty;

// Or return an empty Fragment:
return new Fragment();

// Or just return null:
return null;
```

`Fragment` does not have an `Empty` static member. To return nothing from a view, use `ViewBase.Empty`, `new Fragment()`, or `null`.

**Found In:**
(session not yet recorded)

## Server Configuration

| Hallucinated API | Correct API |
|-----------------|-------------|
| `server.UseSingleApp()` | `server.UseDefaultApp(typeof(AppType))` |
| `server.UseNoChrome()` | `server.UseDefaultApp(typeof(AppType))` — omit `UseChrome()` instead |
| `server.UseDefaultApp<T>()` | `server.UseDefaultApp(typeof(T))` — takes Type, not generic |

## UploadDelegate — wrong parameter count

**Hallucinated API:**
```csharp
UseUpload((file) => { ... });
UseUpload((file, ct) => { ... });
```

**Error:** `UploadDelegate` signature mismatch — agent assumed 1 or 2 parameters.

**Correct API:**
```csharp
UseUpload((FileUpload fileUpload, Stream stream, CancellationToken ct) => { ... });
```

`UploadDelegate` takes **three** parameters: `(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)`. The `Stream` parameter contains the file data. Use `MemoryStreamUploadHandler.Create(state)` for the common case of reading bytes into state.

## FileUpload.ReadAllBytesAsync — non-existent method

**Hallucinated API:**
```csharp
UseUpload((file, stream, ct) => {
    var bytes = await file.ReadAllBytesAsync();
});
```

**Error:** `FileUpload` does not contain a definition for `ReadAllBytesAsync`.

**Correct API:**
```csharp
UseUpload(async (file, stream, ct) => {
    using var ms = new MemoryStream();
    await stream.CopyToAsync(ms, ct);
    var bytes = ms.ToArray();
});
```

`FileUpload` does not have a `ReadAllBytesAsync` method. Read the file content from the `Stream` parameter of `UploadDelegate`.

## QueryMutator.MutateAsync, .Loading, .Error — non-existent members

**Hallucinated API:**
```csharp
var mutation = UseMutation<byte[]>("key", async () => await FetchData());
mutation.MutateAsync();
if (mutation.Loading) { ... }
if (mutation.Error != null) { ... }
```

**Error:** `QueryMutator<T>` does not contain definitions for `MutateAsync`, `Loading`, or `Error`.

**Correct API:**
```csharp
var query = UseQuery("key", async () => await FetchData());
var mutator = UseMutation<byte[]>("key");
// Use query.Loading, query.Error for status
// Use mutator.Revalidate() to trigger refetch
```

`QueryMutator<T>` only has `Mutate`, `Revalidate`, and `Invalidate` methods. `Loading` and `Error` are on `QueryResult<T>` returned by `UseQuery`.

## Spinner — non-existent component

**Hallucinated API:**
```csharp
new Spinner()
```

**Error:** The type or namespace name `Spinner` could not be found.

**Correct API:**
```csharp
new Progress().Indeterminate()
```

There is no `Spinner` component. Use `Progress` with `.Indeterminate()` for an indeterminate loading indicator.

## Color — singular instead of Colors enum

**Hallucinated API:**
```csharp
Ivy.Color.Red
// or
using Ivy;
Color myColor = Color.Red;
```

**Error:** `CS0234: The type or namespace name 'Color' does not exist in the namespace 'Ivy'`

**Correct API:**
```csharp
Ivy.Colors.Red
// or
using Ivy;
Colors myColor = Colors.Red;
```

The color enum in Ivy is `Colors` (plural), not `Color` (singular). This is a common confusion with `System.Drawing.Color`. All color references should use `Colors.X`.

## Widget — non-existent base type

**Hallucinated API:**
```csharp
Widget CreateCell() { ... }
// or
List<Widget> cells = new();
```

**Error:** `CS0246: The type or namespace name 'Widget' could not be found`

**Correct API:**
```csharp
WidgetBase CreateCell() { ... }
// or
List<WidgetBase> cells = new();
```

There is no `Widget` type in Ivy. The base class for all widgets is `WidgetBase`. Views (like `GridView`, `StackView`) inherit from `ViewBase`, not `WidgetBase`. When you need a common return type, use `WidgetBase` for widgets or `IView` for views.
## IRef\<T\> — now supported

`IRef<T>` was previously a hallucinated interface. It has since been added to the framework as `IRef<T> : IState<T>`. Both `UseRef<T>()` return types are now `IRef<T>`, while `UseState<T>()` continues to return `IState<T>`. The two interfaces are interchangeable — `IRef<T>` is a marker subtype used for clarity.

## LayoutView.Border() — now supported

LayoutView supports `.Border(color, thickness)` for adding borders. Example:

```csharp
new LayoutView()
    .Border(Colors.Gray, 1)
    .Padding(4)
    .Vertical(content);
```

Individual properties are also available: `.BorderColor()`, `.BorderThickness()`, `.BorderStyle()`, `.BorderRadius()`.

Note: `.Border()` expects a `Colors` enum as the first argument, not a string. Thickness accepts `int` (uniform) or `Thickness` struct — do NOT pass `Ivy.Thickness` where `int` is expected.

## Code — non-existent widget

**Hallucinated API:**
```csharp
Code("var x = 1;")
```

**Error:** `The name 'Code' does not exist in the current context` (CS0246)

**Correct API:**
```csharp
CodeBlock("var x = 1;", Languages.Csharp)
```

`Code` is not a widget. Use `CodeBlock` for displaying code snippets. It takes the code string and a `Languages` enum value.

**Found In:**
0a42123e-a489-433a-93e5-87d4de7075eb

## Languages.Shell / Languages.Bash — non-existent enum values

**Hallucinated API:**
```csharp
CodeBlock("pip install reflexify", Languages.Shell)
```

**Error:** `'Languages' does not contain a definition for 'Shell'` (CS0117)

**Correct API:**
```csharp
CodeBlock("pip install reflexify", Languages.Text)
```

`Languages.Shell` and `Languages.Bash` do not exist. For shell/terminal commands, use `Languages.Text`.

Available languages: `Csharp, Javascript, Typescript, Python, Sql, Html, Css, Json, Dbml, Markdown, Text, Xml, Yaml, Csv`

**Found In:**
0a42123e-a489-433a-93e5-87d4de7075eb

## using Ivy.Icons — enum used as namespace

**Hallucinated API:**
```csharp
using Ivy.Icons;
```

**Error:** `CS0138: A 'using namespace' directive can only be applied to namespaces; 'Icons' is a type not a namespace`

**Correct API:**
```csharp
using static Ivy.Icons;
// Then use: Icons.FileCode
// Or without the using: Ivy.Icons.FileCode
```

`Icons` is an enum (`Ivy.Icons`), not a namespace. Use `using static Ivy.Icons;` if you want unqualified access to icon values, or reference them as `Icons.FileCode` / `Ivy.Icons.FileCode`.

**Found In:**
0a42123e-a489-433a-93e5-87d4de7075eb

## CodeInput.ReadOnly() — non-existent method

**Hallucinated API:**
```csharp
codeInput.ReadOnly()
```

**Error:** `CS1061: 'CodeInputBase' does not contain a definition for 'ReadOnly'`

**Correct API:**
```csharp
codeInput.Disabled(true)
```

`CodeInput` does not have a `.ReadOnly()` method. Use `.Disabled(true)` to make a CodeInput non-editable. The agent draws from HTML `readonly` attribute or other UI frameworks. In Ivy, the `Disabled` extension is the standard way to prevent editing on all input widgets.

**Found In:**
0a42123e-a489-433a-93e5-87d4de7075eb

## View / IComponent — non-existent base types

**Hallucinated API:**
```csharp
public class MyApp : View { }
public class MyApp : IComponent { }
```

**Error:** `CS0246: The type or namespace name 'View' could not be found` / `CS0246: The type or namespace name 'IComponent' could not be found`

**Correct API:**
```csharp
public class MyApp : ViewBase { }
```

The base class for all Ivy views/apps is `ViewBase`, not `View` or `IComponent`. The agent confuses Ivy's naming with React (`Component`), Blazor (`ComponentBase`), or generic UI patterns (`View`).

**Found In:**
0a42123e-a489-433a-93e5-87d4de7075eb


## View — generic "view" return type

**Hallucinated API:**
```csharp
private View BuildHeader() { ... }
private View BuildTimeline() { ... }
```

**Error:** `CS0246: The type or namespace name 'View' could not be found`

**Correct API:**
```csharp
private IView BuildHeader() { ... }
private IView BuildTimeline() { ... }
```

Agent hallucinates a concrete `View` type for UI composition. The correct return type for view builder methods is `IView` (interface). There is no `View` class in the framework.

**Found In:**
401a4efa-30f5-40d5-9c2f-7d6c769de075 (5 occurrences in Build #1)

## Div — HTML-like container element

**Hallucinated API:**
```csharp
new Div().Add(children)
Div.Create(content)
```

**Error:** `CS0246: The type or namespace name 'Div' could not be found`

**Correct API:**
```csharp
// Use Box for styled containers:
new Box() | children

// Use Layout.Vertical() or Layout.Horizontal() for grouping:
Layout.Vertical() | child1 | child2

// Use Card for semantic grouping with borders:
new Card() | content
```

Agent hallucinates HTML-like `Div` elements. Ivy Framework uses `Box` for styled containers, `Layout` for composition, and `Card` for semantic grouping. There is no `Div` type.

**Found In:**
401a4efa-30f5-40d5-9c2f-7d6c769de075 (Build #2)

## SelectItem<> / ToSelect — non-existent select item types

**Hallucinated API:**
```csharp
var timezones = new List<SelectItem<string>> { ... };
timezone.ToSelect()
```

**Error:** `CS0246: The type or namespace name 'SelectItem<>' could not be found`; `CS1061: 'IState<string>' does not contain a definition for 'ToSelect'`

**Correct API:**
```csharp
// For SelectInput options:
var timezones = new[] { "UTC", "EST", "PST" }.ToOptions();
timezone.ToSelectInput(timezones)

// Or for enum options:
Option.FromEnum<MyEnum>()

// Or for custom options:
new[] {
    new Option("UTC+00:00", "UTC"),
    new Option("UTC-05:00", "EST")
}
```

Agent hallucinates `SelectItem<T>` type and `.ToSelect()` method. Use `Option` and `.ToOptions()` for select input options. SelectInput takes `IEnumerable<IAnyOption>`, not a custom `SelectItem` type.

**Found In:**
401a4efa-30f5-40d5-9c2f-7d6c769de075 (Build #2)

## TextBuilder.Class() — CSS class method

**Hallucinated API:**
```csharp
Text.P("content").Class("font-bold")
```

**Error:** `CS1061: 'TextBuilder' does not contain a definition for 'Class'`

**Correct API:**
```csharp
// Use semantic methods:
Text.P("content").Bold()
Text.P("content").Large()

// Or use variants:
Text.H1("Heading")  // inherently bold and large
Text.Label("Label")
```

Agent hallucinates a CSS-like `.Class()` method on TextBuilder. Ivy Framework uses fluent semantic methods like `.Bold()`, `.Large()`, `.Medium()`, `.Small()`, `.Muted()` instead of arbitrary CSS class names.

**Found In:**
401a4efa-30f5-40d5-9c2f-7d6c769de075 (Build #2)

## ViewBase.Min() — Min() on wrong type

**Hallucinated API:**
```csharp
workStart.ToNumberInput().Min(0)
workEnd.ToNumberInput().Min(0)
```

**Error:** `CS1929: 'ViewBase' does not contain a definition for 'Min'`

**Correct API:**
```csharp
// For NumberInput constraints:
workStart.ToNumberInput().WithField().Min(0).Max(23)

// Or using SetConstraints:
workStart.ToNumberInput().SetConstraints(min: 0, max: 23)
```

`.Min()` and `.Max()` exist on **Field wrappers** (via `WithField()`) and specialized builders like `ProgressBuilder` and `Size`, but NOT directly on input widgets themselves. Agent tried to use `.Min()` directly on `NumberInputBase` which extends `ViewBase`, causing CS1929 error.

The GetTypeInfo search for "Min" found it on Size and chart elements, confusing the agent about where it can be used.

**Found In:**
401a4efa-30f5-40d5-9c2f-7d6c769de075 (Build #3)

## Badge.Tooltip() — Tooltip on wrong widget type

**Hallucinated API:**
```csharp
new Badge(hour.ToString()).Tooltip("Available")
```

**Error:** `CS1929: 'Badge' does not contain a definition for 'Tooltip'`

**Correct API:**
```csharp
// Badge does not support tooltips. Use meaningful badge text instead:
new Badge("Available").Success()

// Or use a tooltip-enabled wrapper:
new Box()
    | new Badge(hour.ToString())
    | new Tooltip("Available")  // if Tooltip widget exists

// Tooltip() exists on:
// - Chart widgets (BarChart, LineChart, PieChart, etc.)
// - MenuItem
```

Agent hallucinates `.Tooltip()` on Badge after searching GetTypeInfo for "Tooltip" and finding it on chart widgets and MenuItem. `.Tooltip()` is NOT available on Badge. Badge doesn't support tooltips — use descriptive badge text or alternative UI patterns.

**Found In:**
401a4efa-30f5-40d5-9c2f-7d6c769de075 (Build #3)

## Justify — layout alignment enum

**Hallucinated API:**
```csharp
Layout.Horizontal().Justify(Justify.SpaceBetween)
```

**Error:** `CS0103: The name 'Justify' does not exist in the current context`

**Correct API:**
```csharp
Layout.Horizontal().Align(Align.SpaceBetween)
```

Agent hallucinates a separate `Justify` enum for layout justification. Ivy Framework uses the `Align` enum for all alignment operations including `SpaceBetween`, `SpaceAround`, `Center`, `Start`, `End`. There is no `Justify` type.

**Found In:**
142df25d-520b-4320-a84f-a5e549d6698c (FinanceDashboard, line 30)

## If() — conditional widget function

**Hallucinated API:**
```csharp
If(condition, () => new Text("content"))
If(items.Any(), () => items.ToTable())
```

**Error:** `CS0103: The name 'If' does not exist in the current context`

**Correct API:**
```csharp
// Use C# ternary operator:
condition ? new Text("content") : null

// Or standard C# if:
if (items.Any())
    return items.ToTable();
else
    return null;

// For conditional pipeline:
| (condition ? content : null)
```

Agent hallucinates a React-like `If()` helper function for conditional rendering. Ivy Framework uses standard C# conditional expressions (`?:`, `if/else`) for conditional rendering. There is no `If()` widget or function.

**Found In:**
142df25d-520b-4320-a84f-a5e549d6698c (FinanceDashboard, line 49)

## new SelectInput<T>() — constructor usage

**Hallucinated API:**
```csharp
new SelectInput<string>()
    .Value(state.Value)
    .Options(optionsList)
    .OnChange(state.Set)
```

**Error:** `CS1729: 'SelectInput<string>' does not contain a constructor that takes 0 arguments`

**Correct API:**
```csharp
// Use ToSelectInput extension on state:
state.ToSelectInput(optionsList)

// For options from enum:
state.ToSelectInput(Option.FromEnum<MyEnum>())

// For custom options:
state.ToSelectInput(new[] {
    new Option("value1", "Label 1"),
    new Option("value2", "Label 2")
})
```

Agent hallucinates constructor-based instantiation of SelectInput. SelectInput should be created via the `.ToSelectInput()` extension method on `IState<T>`, which automatically wires up the state binding. There is no public constructor.

**Found In:**
142df25d-520b-4320-a84f-a5e549d6698c (FinanceDashboard, line 43)

## UseState<T>(new()) — explicit generic with target-typed new

**Hallucinated API:**
```csharp
var stockData = UseState<List<StockData>>(new());
var companyHistory = UseState<List<StockData>>(new());
```

**Error:** `CS1729: 'Func<List<StockData>>' does not contain a constructor that takes 0 arguments`

**Correct API:**
```csharp
// Use explicit object construction:
var stockData = UseState(new List<StockData>());

// Or use type inference:
var stockData = UseState<List<StockData>>(new List<StockData>());

// For empty collections, prefer explicit:
var stockData = UseState(new List<StockData>());
```

Agent combines explicit generic type parameter with target-typed `new()`. When `UseState<T>` is called with an explicit generic, the compiler prefers the `Func<T>` overload (for lazy initialization), and `new()` tries to construct `Func<List<StockData>>` with no arguments, causing CS1729.

Use either:
1. **Type inference**: `UseState(new List<StockData>())` — compiler infers `T` from argument
2. **Explicit type with explicit constructor**: `UseState<List<StockData>>(new List<StockData>())`

Never combine explicit generic with target-typed `new()` on UseState.

**Found In:**
142df25d-520b-4320-a84f-a5e549d6698c (FinanceDashboard, lines 17, 19)
