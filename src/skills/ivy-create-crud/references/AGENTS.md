# Introduction to the Ivy Framework for LLMs

- Ivy is a declarative full-stack UI framework that allows developers to build user interfaces using a component-based approach very similar to React.
- In Ivy, you only write one application in pure C# and we don't have a BE and FE distinction.
- UI rendering is handled by Ivy.
- When programming in Ivy you focus on building the logical structure of your application using a large set of pre-built widgets and views - you rarely need to specify any styling - Ivy just makes it look good by default.

Terminology:

- "Views" are the main building blocks of the UI, similar to React components.
- "Hooks" are functions that allow views to manage state and side effects.
- "Widgets" are the UI elements that make up the views. e.g., Button, TextBlock, StackPanel...
- "Apps" are the top-level views that represent entire applications.

A view is defined as a class that inherits from `ViewBase` and implements a `Build` method. The `Build` method returns either another view or a widget.

> WARNING: There is NO `AppBase` class. ALL views and apps inherit from `ViewBase`.

Widgets can have multiple children, but views can only return a single object (widget or view). To return multiple widgets from a view, you can use a `Fragment` use the Layout helpers. See below.

public class MyView : ViewBase
{
  public override object? Build()
  {
    var count = UseState(0);
    return Layout.Vertical()
        | new Text($"Count: {count.Value}")
        | new Button("Increment", () => count.Set(count.Value + 1));
  }
}

The topmost view in an Ivy application is called an [App](https://docs.ivy.app/onboarding/concepts/apps.md) and is decorated with the `[App]` attribute. The attribute uses **named constructor parameters** (lowercase):

[App(title: "Customers", icon: Icons.Rocket, group: new[] { "CRM" })]
public class CustomersApp : ViewBase

- `title` is optional — if omitted, it is derived from the class name (e.g. `CustomersApp` → "Customers").
- `icon` uses the `Icons` enum — these are Lucide icons in PascalCase (e.g. `Icons.Link`, `Icons.Settings`, `Icons.Rocket`).
- `group` groups the app in the navigation sidebar (e.g. `group: new[] { "Apps" }`).
- There is no `chrome` parameter. Chrome is configured in `Program.cs` via `server.UseDefaultApp(typeof(MyApp))`. Always ensure `server.AddAppsFromAssembly()` is called before `UseDefaultApp` — without it, `[App]`-attributed classes are not registered and the server throws at runtime.
- Use **lowercase** parameter names (`icon:`, `group:`), NOT PascalCase property names (`Icon =`, `Group =`) — PascalCase causes CS0655.

An app is built into a tree of widgets. This is what's rendered to the screen.

## Application Structure

A typical Ivy project has this folder structure:

MyProject/
├── Program.cs                  # Entry point — configures and starts the Ivy server
├── MyProject.csproj            # Project file
├── Apps/                       # All app classes go here (convention)
│   ├── DashboardApp.cs
│   └── Settings/               # Subfolder namespaces become URL path segments
│       └── UserProfileApp.cs   # → /settings/user-profile
└── Connections/
    └── MyDb/
        ├── MyDbContext.cs
        ├── MyDbContextFactory.cs
        ├── MyDbConnection.cs
        └── Product.cs          # Entity classes

### Dependency Injection

Register services in Program.cs, consume them in views with `UseService<T>()`:

// Program.cs
server.Services.AddSingleton<IMyService, MyService>();

// In a view
var myService = UseService<IMyService>();

## Common Widgets

[Button](https://docs.ivy.app/widgets/common/button.md)
[Card](https://docs.ivy.app/widgets/common/card.md)
[Badge](https://docs.ivy.app/widgets/common/badge.md)
[Sheet](https://docs.ivy.app/widgets/advanced/sheet.md)
[Progress](https://docs.ivy.app/widgets/common/progress.md)
[Expandable](https://docs.ivy.app/widgets/common/expandable.md)
[Tooltip](https://docs.ivy.app/widgets/common/tooltip.md)
[DropDownMenu](https://docs.ivy.app/widgets/common/drop-down-menu.md)
[Table](https://docs.ivy.app/widgets/common/table.md)
[List](https://docs.ivy.app/widgets/common/list.md)
[Details](https://docs.ivy.app/widgets/common/details.md)
[Image](https://docs.ivy.app/widgets/primitives/image.md)
[Avatar](https://docs.ivy.app/widgets/primitives/avatar.md)
[Spacer](https://docs.ivy.app/widgets/primitives/spacer.md)
[Callout](https://docs.ivy.app/widgets/primitives/callout.md)

## Layouts

- Use Layout.Vertical() or Layout.Horizontal() to create stack layouts.
- Layout.Grid()
— Layout.Wrap()
- Add Children: Pipe child elements using the | operator to arrange them top-to-bottom (vertical) or left-to-right (horizontal).
- Layouts can be customized with methods like .Gap(int number) to set spacing between children. Use .Left(), .Center(), or .Right() methods to control alignment.
- The number in Gap(int number) works the same as in Tailwind CSS spacing scale (e.g., 1 = 0.25rem, 2 = 0.5rem, etc.).
- Layouts have a default gap of 4 (1rem). Do NOT add `.Gap(4)` — it is the default and adds unnecessary noise. Only use `.Gap()` when you need a value other than 4.
- `.Padding()` is rarely needed. Layouts and pages already have appropriate padding by default. Only add `.Padding()` when you need extra inner spacing for a specific design reason.

// Basic Vertical Layout
Layout.Vertical()
    | new Badge("Top")
    | new Badge("Middle")
    | new Badge("Bottom");

// Nested layouts with alignment
Layout.Vertical().AlignContent(Align.Center)
    | Text.Label("Header")
    | (Layout.Horizontal()
        | new Button("Previous")
        | new Button("Next")); //NOTE: Parentheses are used to group the horizontal layout - THIS IS REQUIRED

Grids:

Layout.Grid()
  .Columns(2)
  .Rows(2)
    | Text.Block("Cell 1")
    | Text.Block("Cell 2")
    ...

Align values: TopLeft, TopCenter, TopRight, Left, Center, Right, BottomLeft, BottomCenter, BottomRight, Stretch

[Layouts](https://docs.ivy.app/onboarding/concepts/layout.md)

## Text

The Text helper utility is used to create various semantic text elements.

- Text.H1, Text.H2, : For headings.
- Text.Lead: For prominent introductory text.
- Text.P: For standard paragraphs.
- Text.Block: For block-level content (e.g., list items).
- Text.InlineCode: For displaying inline code snippets.
- Test.Muted

Styling Modifiers:
.NoWrap():
.Bold()
.Italic()
.StrikeThrough()
.Color(Colors)

Layout.Vertical()
    | Text.H1("Getting Started")
    | Text.P("This is a paragraph of text.").NoWrap()

[Text](https://docs.ivy.app/widgets/primitives/text-block.md)

## Colors

Ivy.Colors enum has the following values:

Black, White, Slate, Gray, Zinc, Neutral, Stone, Red, Orange, Amber, Yellow, Lime, Green, Emerald, Teal, Cyan, Sky, Blue, Indigo, Violet, Purple, Fuchsia, Pink, Rose, Primary, Secondary, Destructive, Success, Warning, Info, Muted

## Density

All widgets support `.Density(Density.Small)`, `.Density(Density.Medium)`, `.Density(Density.Large)`.
Convenience methods: `.Small()`, `.Medium()`, `.Large()`.
Density adjusts the overall visual size of a widget (text, padding, etc.).
There is no `ButtonSize` enum — use `Density` for all widgets.

## Size

`.Width(Size.X)` and `.Height(Size.X)` set widget dimensions.
`.Size(Size.X)` sets both width and height.

Common Size values:

- Size.Units(n) — Tailwind spacing scale (n × 0.25rem)
- Size.Full() — 100%
- Size.Fit() — fit-content
- Size.Auto() — auto
- Size.Px(n) — exact pixels
- Size.Fraction(0.5f) — percentage, Size.Half(), Size.Third()

Size is NOT the same as Density. Size controls dimensions; Density controls visual density.

## Event Handling

new Button("Click Me")
  .Primary()
  .OnClick(() => {
    count.Set(count.Value + 1);
})

new TextInput().Default()
    .Value(text.Value)
    .OnChange(text.Set)
    .OnBlur(() => Console.WriteLine("Blurred"))

## Hooks

- Most hooks that you know from React are available in Ivy. They follow the same principles as in React.
- Hooks should only be called at the top level of a Build() method, not inside loops or conditions.

### UseState

var nameState = UseState("World");
var iconsState = UseState<Icons[]>();

If you don't specify a value, default(T) is used.

UseState hook returns a state object IState<T> that provides:

> WARNING: UseState returns `IState<T>`, NOT `State<T>`. There is no `State<T>` type in Ivy.

- .Value property to read the current state.
- .Set(newValue) method to update the state in UseEffect or in an event handler.

Always use immutable types (e.g. records) with `UseState` — mutable classes that are modified in-place and passed back via `.Set()` will not trigger a re-render because the reference hasn't changed. Instead, create a new instance (e.g. using `with` expressions on records) before calling `.Set()`.

### UseEffect

void UseEffect(Action effect, params IEffectTriggers[] triggers)
void UseEffect(Func<Task> asyncEffect, params IEffectTriggers[] triggers)
void UseEffect(Func<IDisposable> effectWithCleanup, params IEffectTriggers[] triggers)
void UseEffect(Func<Task<IDisposable>> asyncEffectWithCleanup, IEffectTriggers object[] triggers)

- EffectTrigger.OnBuild() - runs after every build
- EffectTrigger.OnMount() - runs once when the view is first mounted
- EffectTrigger.OnStateChange(IState<T>) - runs when the specified state changes

- IState<T> is automatically converted to EffectTrigger.OnStateChange
- If no triggers are provided, the effect trigger is assumed to be OnMount.

### UseQuery

UseQuery is the **preferred pattern for data fetching** in Ivy. It should be favored over the UseEffect + UseState fetch pattern.

var query = UseQuery(
    key: "my-data",
    fetcher: async (ct) => await LoadDataAsync(ct)
);

if (query.Loading) return Skeleton.Card();
if (query.Error is { } error) return Callout.Error(error.Message);

// Use query.Value

QueryResult<T> properties:

- .Value — the fetched data (default until loaded)
- .Loading — true during initial fetch (no value yet)
- .Validating — true during background revalidation
- .Error — exception if the fetch failed
- .Mutator — provides .Revalidate(), .Invalidate(), .Mutate(value, revalidate)

**Do not combine UseQuery with DataTable for EF Core data.** When a DataTable can receive an EF Core `IQueryable` directly, pass it to `.ToDataTable()` without UseQuery. DataTables handle their own server-side data loading. (API data fetched via UseQuery → `.ToDataTable()` is fine.)

Key conventions:

- String: `"my-data"`
- Tuple: `(nameof(MyBlade), entityId)`

Common options (QueryOptions):

- KeepPrevious: true — show stale data while revalidating with a new key
- RevalidateOnMount: false — skip initial fetch when using initialValue
- RefreshInterval: TimeSpan — poll at an interval
- Scope: QueryScope.View — isolate cache to the view instance (default is Server)

Tag-based invalidation (cross-component):
var queryService = UseService<IQueryService>();
queryService.RevalidateByTag(typeof(Product[]));        // collection
queryService.RevalidateByTag((typeof(Product), id));    // single entity

Static "hooks" pattern (reusable across views):
public static QueryResult<T[]> UseMyRecords(IViewContext context, string filter)
{
    return context.UseQuery(
        key: (nameof(UseMyRecords), filter),
        fetcher: async ct => { /*fetch*/ },
        tags: [typeof(T[])],
        options: new QueryOptions { KeepPrevious = true }
    );
}

Dependent fetching (wait for another query):
var user = UseQuery(key: "user", fetcher: async ct => await GetUser(ct));
var projects = UseQuery(
    () => user.Value?.Id,   // null = idle, no fetch
    async (userId, ct) => await GetProjects(userId, ct));

[UseRef](https://docs.ivy.app/hooks/core/use-ref.md)
[UseContext](https://docs.ivy.app/hooks/core/use-context.md)
[UseQuery](https://docs.ivy.app/hooks/core/use-query.md)
[UseMutation](https://docs.ivy.app/hooks/core/use-mutation.md)
[UseSignal](https://docs.ivy.app/hooks/core/use-signal.md)
[UseService](https://docs.ivy.app/hooks/core/use-service.md)
[UseArgs](https://docs.ivy.app/hooks/core/use-args.md)
[UseDownload](https://docs.ivy.app/hooks/core/use-download.md)
[UseRefreshToken](https://docs.ivy.app/hooks/core/use-refresh-token.md)
[UseTrigger](https://docs.ivy.app/hooks/core/use-trigger.md)
[UseWebhook](https://docs.ivy.app/hooks/core/use-webhook.md)
[UseAlert](https://docs.ivy.app/onboarding/concepts/alerts.md)

## Inputs

Ivy has several Input widgets for handling user input. There are rarely used directly - instead we use
extension methods on IState<T> to bind state to inputs.

var userNameState = UseState("");
var input = userNameState.ToTextInput().Placeholder("Enter your name");

Most inputs have extension methods for common configurations:
userNameState.ToTextInput().Required().MaxLength(50).Placeholder("Enter your name");

[TextInput](https://docs.ivy.app/widgets/inputs/text-input.md)
[NumberInput](https://docs.ivy.app/widgets/inputs/number-input.md)
[BoolInput](https://docs.ivy.app/widgets/inputs/bool-input.md)
[SelectInput](https://docs.ivy.app/widgets/inputs/select-input.md)
[AsyncSelectInput](https://docs.ivy.app/widgets/inputs/async-select-input.md)
[DateTimeInput](https://docs.ivy.app/widgets/inputs/date-time-input.md)
[DateRangeInput](https://docs.ivy.app/widgets/inputs/date-range-input.md)
[ColorInput](https://docs.ivy.app/widgets/inputs/color-input.md)
[CodeInput](https://docs.ivy.app/widgets/inputs/code-input.md)
[FeedbackInput](https://docs.ivy.app/widgets/inputs/feedback-input.md)
[FileInput](https://docs.ivy.app/widgets/inputs/file-input.md)

## Common Hallucinations

- Base class is `ViewBase` (NOT `AppBase` there is no `AppBase` class)
- `Text` is a static helper - use `Text.P()`, `Text.H2()`, ...
- `UseState<T>()` returns `IState<T>`, NOT `State<T>`
- All types are in the `Ivy` namespace
- `Colors` is a flat enum (e.g. `Colors.Red`, `Colors.Blue`) we have no shade levels
- `DbContext` must never be injected directly! Always resolve `IDbContextFactory<T>` via `UseService` and create scoped instances with `CreateDbContextAsync()` inside query/mutation lambdas
- **Nested layouts MUST use parentheses** — `Layout.Vertical() | (Layout.Horizontal() | child1 | child2)` — without parentheses, C# left-to-right `|` evaluation adds children to the outer layout, not the inner one. See the nested layout example in the Layouts section above.

## CLI Commands

Prefer using `ivy cli explain` for command discovery over MCP server tools as it provides a reliable, built-in structural breakdown.

## Further Reading

[Forms](https://docs.ivy.app/onboarding/concepts/forms.md)
[DataTable](https://docs.ivy.app/widgets/advanced/data-table.md)
[Table](https://docs.ivy.app/widgets/common/table.md)
[Details](https://docs.ivy.app/widgets/common/details.md) - Display structured label-value pairs
[Program.cs](https://docs.ivy.app/onboarding/concepts/program.md)
[Size](https://docs.ivy.app/api-reference/ivy-shared/size.md)
[Align](https://docs.ivy.app/api-reference/ivy-shared/align.md)
[Downloads](https://docs.ivy.app/hooks/core/use-download.md)
[Icons](https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/refs/heads/main/src/Ivy/Shared/Icons.cs)

All Ivy documentation pages are listed on: <https://docs.ivy.app/sitemap.xml>.
Add ".md" to the end of any URL to go directly to the Markdown version of the doc.
