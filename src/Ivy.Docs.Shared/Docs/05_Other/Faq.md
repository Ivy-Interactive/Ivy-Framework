# Faq

## How do I copy text to the clipboard in Ivy?

Use the `CopyToClipboard` extension method on `IClientProvider`:

```csharp
var client = UseService<IClientProvider>();
client.CopyToClipboard(content);
client.Toast("Copied to clipboard!");
```

## How do I create a multiline textarea TextInput in Ivy?

Use the `TextInputs.Textarea` variant or the dedicated `ToTextAreaInput` extension:

```csharp
state.ToTextAreaInput(placeholder: "Enter text...")
```

## How do I read CSV data or load external data in Ivy?

Ivy apps are standard C# applications, so you can use any .NET approach to load data:

**Embedded CSV data (hardcoded):**
```csharp
// Define your data as C# records/classes and initialize inline
var data = new[] {
    new { Date = "2012-01-01", TempMax = 12.8, TempMin = 5.0, Weather = "drizzle" },
    // ...
};
```

**Read from a CSV file using CsvHelper (NuGet package):**
```csharp
// Add NuGet package: CsvHelper
using CsvHelper;
using System.Globalization;

using var reader = new StreamReader("data.csv");
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<WeatherRecord>().ToList();
```

**Fetch data from a URL at runtime:**
```csharp
var http = new HttpClient();
var csvText = await http.GetStringAsync("https://example.com/data.csv");
// Parse csvText manually or with CsvHelper
```

For small datasets, embedding data directly as C# collections is simplest. For larger datasets, use CsvHelper or similar libraries.

## How do I create a horizontal layout with items spaced between (like CSS justify-content: space-between)?

Instead, use a `Spacer` with `Size.Grow()` to push items apart:

```csharp
Layout.Horizontal().Align(Align.Center)
    | Text.H1("Title")
    | new Spacer().Width(Size.Grow())
    | new Button("Action", handler)
```

The `Spacer` takes up all remaining space, pushing elements before it to the left and elements after it to the right. You can also use `.Right()` on the layout to align all children to the right:

```csharp
Layout.Horizontal().Right()
    | new Button("Right-aligned", handler)
```

## How do I use UseMutation for async operations in Ivy?

`UseMutation` runs an async function on demand (e.g., when a button is clicked) and tracks loading/error state:

```csharp
var mutation = UseMutation(async () =>
{
    var result = await myService.CallApiAsync(input.Value);
    output.Set(result);
});

return Layout.Vertical()
    | input.ToTextInput().Placeholder("Enter input")
    | new Button("Submit", mutation.Trigger).Loading(mutation.IsLoading)
    | (mutation.Error != null ? Callout.Error(mutation.Error.Message) : null)
    | Text.P(output.Value);
```

`mutation.Trigger` is the action to invoke. `mutation.IsLoading` indicates if the operation is in progress. `mutation.Error` contains any exception that was thrown.

## How do I configure an Ivy project to use local source for the Ivy framework?

To use a local copy of the Ivy framework source code instead of the NuGet package:

1. Set the `IVY_PROJECT_PATH` environment variable to point to your local `Ivy.csproj` file (e.g., `D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Ivy.csproj`)
2. Run `ivy init --local-source` to create a new project with local source references
3. For existing projects, use `ivy debug use-local-source` to convert from PackageReference to ProjectReference

The project file will use a `<ProjectReference>` pointing to the local Ivy.csproj instead of a `<PackageReference>`. This is useful for developing and debugging the Ivy framework itself.

**Important:** Do NOT use direct DLL references (`<Reference Include="Ivy"><HintPath>...</HintPath></Reference>`) — this will fail because transitive NuGet dependencies (Microsoft.Extensions, System.Reactive, etc.) won't be resolved.

## How do I create a Callout or info/warning/error message box in Ivy?

Use the static factory methods on `Callout`:

```csharp
Callout.Info("No items found.")
Callout.Warning("This action cannot be undone.")
Callout.Error("Something went wrong.")
Callout.Success("Operation completed!")
```

## How do I handle enter key press on a TextInput?

Single-line TextInputs automatically blur when the user presses Enter, so use `HandleBlur` to react to the Enter key:

```csharp
var input = UseState("");
input.ToTextInput()
    .Placeholder("Type and press Enter")
    .HandleBlur(() => DoSomething(input.Value))
```

`HandleBlur` takes an `Action` that is invoked when the input loses focus — which happens automatically on Enter for single-line text inputs.

## How do I show streaming text from an API (e.g., OpenAI) in Ivy?

`UseQuery` is for request-response patterns, NOT for streaming. For streaming API responses where you want to show text appearing incrementally, use a regular async method that updates an `IState<string>` inside the streaming loop:

```csharp
var output = UseState("");
var isStreaming = UseState(false);

async Task StreamResponse()
{
    isStreaming.Set(true);
    output.Set("");

    try
    {
        var result = new System.Text.StringBuilder();
        await foreach (var chunk in myStreamingApi.StreamAsync())
        {
            result.Append(chunk);
            output.Set(result.ToString()); // UI updates on each chunk
        }
    }
    finally
    {
        isStreaming.Set(false);
    }
}

return Layout.Vertical()
    | new Button("Generate", async _ => await StreamResponse())
        .Disabled(isStreaming.Value)
    | Text.P(output.Value);
```

Each `state.Set()` call triggers a re-render, so the UI updates incrementally as chunks arrive. Use `UseMutation` if you want built-in loading/error tracking for non-streaming async operations instead.

## How do I handle row actions on a DataTable?

Use `.RowActions()` to define actions and `.HandleRowAction()` to handle clicks:

```csharp
items.ToDataTable()
    .RowActions(
        new RowAction("edit", "Edit", Icons.Pencil),
        new RowAction("delete", "Delete", Icons.Trash2))
    .HandleRowAction("edit", e => EditItem(e.Value))
    .HandleRowAction("delete", async e => await DeleteItem(e.Value))
```

**Important:** The method is `.HandleRowAction()`, NOT `.OnRowAction()`. The `On` prefix does not exist on DataTableBuilder.

## What namespace are Ivy types in? (MetricView, charts, DataTable, ViewBase, IState, etc.)

All Ivy types are in the root `Ivy` namespace. There are no sub-namespaces. You only need:

```csharp
using Ivy;
```

This single using statement gives you access to everything: `ViewBase`, `IState<T>`, `MetricView`, `MetricRecord`, chart views (`LineChartView`, `PieChartView`, `BarChartView`, `AreaChartView`), `DataTable`, `DataTableBuilder<T>`, layout helpers (`Layout.Vertical()`, `Layout.Horizontal()`), `Button`, `TextInput`, all input types, `Card`, `Dialog`, `Sheet`, `Tab`, `Icons`, `RefreshToken`, `IClientProvider`, `IBladeService`, `IConnection`, `IHaveSecrets`, and all other framework types.

**Do NOT use sub-namespaces** like `Ivy.Views.Dashboards`, `Ivy.Widgets.DataTables`, `Ivy.Client`, `Ivy.Hooks`, `Ivy.Services`, or `Ivy.Apps`. These do not exist — the framework source code organizes files in subdirectories but all types use `namespace Ivy;`.

Ivy projects include `<ImplicitUsings>enable</ImplicitUsings>` plus a global using for the project's own namespace, so typically the only explicit using you need is `using Ivy;` and `using Microsoft.EntityFrameworkCore;` (for database connections).

## What are the available ButtonVariant values in Ivy?

`ButtonVariant` has these values: `Primary`, `Destructive`, `Outline`, `Secondary`, `Success`, `Warning`, `Info`, `Ghost`, `Link`, `Inline`, `Ai`.

Set via the `.Variant()` method or shortcut methods:

```csharp
new Button("Save", handler).Variant(ButtonVariant.Primary)
// or use shortcut:
new Button("Save", handler).Primary()

// Other shortcuts: .Secondary(), .Outline(), .Destructive(), .Ghost(), .Link(), .Inline(), .Ai()
```

**Important:** There is no `ButtonVariant.Default`. Use `ButtonVariant.Primary` instead.

## Does the App attribute have a layout parameter?

No. The `[App]` attribute only supports `title`, `icon`, `group`, and `connection` named parameters. Layout is controlled within the `Build()` method using layout helpers:

```csharp
[App(title: "My App", icon: Icons.Layout)]
public class MyApp : ViewBase
{
    public override object? Build()
    {
        // Layout is set here, not in the attribute
        return Layout.TopCenter()
            | (Layout.Vertical().Width(Size.Full().Max(200))
                | Text.H1("My App")
                | ...
            );
    }
}
```

## How do I show an alert dialog to the user?

Use the `UseAlert` hook, which returns a tuple of `(alertView, showAlert)`:

```csharp
var (alertView, showAlert) = UseAlert();

return Layout.Vertical()
    | new Button("Show Alert", _ =>
        showAlert("Are you sure?", result =>
        {
            // result is the AlertResult (Ok, Cancel, Yes, No)
        }, "Confirmation", AlertButtonSet.OkCancel))
    | alertView; // IMPORTANT: alertView must be included in the view tree
```

**Key points:**
- `UseAlert()` returns a **tuple**, not an object — always destructure it
- `showAlert` is a delegate: `showAlert(message, callback, title?, buttonSet?)`
- `alertView` must be rendered somewhere in your view tree for the dialog to appear
- Available button sets: `AlertButtonSet.Ok`, `AlertButtonSet.OkCancel`, `AlertButtonSet.YesNo`, `AlertButtonSet.YesNoCancel`
- For simple toast notifications, use `client.Toast("message")` or `client.Error("message")` via `IClientProvider`
