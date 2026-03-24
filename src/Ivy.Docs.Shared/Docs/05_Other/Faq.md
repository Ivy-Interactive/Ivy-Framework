---
hidden: true
---
# Faq

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

## How do I configure an Ivy project to use local source for the Ivy framework?

To use a local copy of the Ivy framework source code instead of the NuGet package:

1. Set the `IVY_PROJECT_PATH` environment variable to point to your local `Ivy.csproj` file (e.g., `D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Ivy.csproj`)
2. Run `ivy init --local-source` to create a new project with local source references
3. For existing projects, use `ivy debug use-local-source` to convert from PackageReference to ProjectReference

The project file will use a `<ProjectReference>` pointing to the local Ivy.csproj instead of a `<PackageReference>`. This is useful for developing and debugging the Ivy framework itself.

**Important:** Do NOT use direct DLL references (`<Reference Include="Ivy"><HintPath>...</HintPath></Reference>`) — this will fail because transitive NuGet dependencies (Microsoft.Extensions, System.Reactive, etc.) won't be resolved.

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

## How do I use HttpClient in an Ivy app?

For simple HTTP requests, create an `HttpClient` instance directly:

```csharp
var client = new HttpClient();
var response = await client.GetAsync(url);
```

For apps that need a shared HttpClient with DI, register it through the server in Program.cs:

```csharp
server.Services.AddHttpClient<MyService>();
```

Then access it via `UseService<MyService>()` in your app. Do NOT use `services.AddHttpClient()` directly — use `server.Services`.

## When should I use UseDefaultApp vs UseAppShell in Program.cs?

**UseDefaultApp** is for single-app projects where you want to skip the sidebar and go directly to the app:

```csharp
server.UseDefaultApp(typeof(MyApp));
```

**UseAppShell** is for multi-app projects where users need sidebar navigation between apps:

```csharp
server.UseAppShell(new AppShellSettings().DefaultApp<MyApp>().UseTabs(preventDuplicates: true));
```

For new projects with a single app, prefer `UseDefaultApp` for a cleaner experience. If you later add more apps and need a sidebar, switch to `UseAppShell`.

## How do I get a display name or description from an enum value?

**For dropdowns and selects:** Ivy handles this automatically. When you pass an enum type to a `Select` or `RadioGroup`, `ToOptions()` reads `[Description]` attributes and falls back to splitting PascalCase names. No extra code needed.

**For displaying enum values as text:** Use the built-in `GetDescription()` extension method:

```csharp
using System.ComponentModel;

public enum Status
{
    [Description("Not Started")] NotStarted,
    [Description("In Progress")] InProgress,
    [Description("At Risk")] AtRisk,
}

// Usage:
Text.P(status.GetDescription()) // "Not Started"
```

`GetDescription()` reads the `[Description]` attribute if present, otherwise splits PascalCase automatically (e.g., `NotStarted` → `"Not Started"`).
