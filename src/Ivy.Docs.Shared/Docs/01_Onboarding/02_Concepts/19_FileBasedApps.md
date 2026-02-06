---
searchHints:
  - file-based
  - single file
  - script
  - dotnet run
  - quick start
  - minimal
  - no project
---

# File-Based Apps

<Ingress>
Run an Ivy app from a single `.cs` file—no project scaffolding, no `ivy init`, no solution or folder structure. Ideal for quick experiments, demos, and learning.
</Ingress>

Usually you create Ivy apps with [ivy init](../03_CLI/02_Init.md) and run them with [ivy run](../03_CLI/03_Run.md). **File-based apps** let you write one file and run it with `dotnet run YourFile.cs`, without any other project files.

## Prerequisites

- **.NET 10** or later (single-file `dotnet run` is supported from .NET 10).
- Ivy NuGet package (referenced via a file-level directive in the script).

## Minimal Example

Create a file, for example `HelloApp.cs`:

```csharp
#: package Ivy@*

using Ivy;
using Ivy.Views;
using Ivy.Apps;

var server = new Server();
server.AddApp<HelloApp>();
await server.RunAsync();

[App]
class HelloApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Center(
            new Card(
                Text.P("Hello")
            ).Width(60)
        );
    }
}
```

Run it from the same directory:

```terminal
dotnet run HelloApp.cs
```

The app starts (by default on port 5010). Open the URL shown in the terminal to see your app.

## File-Level Directive: Package

At the top of the file, use the **package** directive so the file can use Ivy without a `.csproj`:

```csharp
#: package Ivy@*
```

- `#: package` – file-level NuGet package reference (no project file needed).
- `Ivy@*` – the [Ivy](https://www.nuget.org/packages/Ivy) package; `*` means latest version. You can pin a version, e.g. `Ivy@1.2.0`.

## Usings

Include the namespaces you use in the file. For a typical small Ivy app:

| Namespace   | Use for |
|------------|---------|
| `Ivy`      | `Server`, server configuration and `RunAsync()`. |
| `Ivy.Views`| `ViewBase`, `Layout`, and built-in widgets (`Card`, `Text`, `Button`, etc.). |
| `Ivy.Apps` | The `[App]` attribute for your app class. |

Example:

```csharp
using Ivy;
using Ivy.Views;
using Ivy.Apps;
```

If you use only certain widgets or types, you might need extra namespaces (for example from other Ivy packages). Add `using` directives as you would in a normal C# project.

## Structure of a File-Based App

1. **Package directive** – `#: package Ivy@*` (and any other packages).
2. **Usings** – `Ivy`, `Ivy.Views`, `Ivy.Apps`, and any others you need.
3. **Top-level statements** – create a `Server`, add your app, and run:
   - `var server = new Server();`
   - `server.AddApp<YourAppClass>();`
   - `await server.RunAsync();`
4. **App class** – a class marked with `[App]` that inherits from `ViewBase` and implements `Build()` (same as in a full [Program](./01_Program.md) / [Apps](./10_Apps.md) project).

You can add more `[App]` classes and register them with `server.AddApp<AnotherApp>();` in the top-level code.

## Running the File

From the directory that contains your `.cs` file:

```terminal
dotnet run HelloApp.cs
```

If you need a specific port or other server settings, configure the `Server` in code (for example via `ServerArgs` as in [Program](./01_Program.md)) or use environment variables (e.g. `PORT`) if your setup supports them.

## When to Use File-Based Apps

- **Quick demos and experiments** – try an idea without creating a full project.
- **Learning Ivy** – focus on views and widgets in one file.
- **Script-like or one-off UIs** – small tools or internal pages.

For long-lived apps, multiple apps, [authentication](../03_CLI/04_Authentication/01_AuthenticationOverview.md), [databases](../03_CLI/05_DatabaseIntegration/01_DatabaseOverview.md), or [deployment](../03_CLI/06_Deployment/01_DeploymentOverview.md), use `ivy init` and a normal project with [Program.cs](./01_Program.md) and [ivy run](../03_CLI/03_Run.md).
