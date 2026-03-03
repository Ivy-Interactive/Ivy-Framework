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
