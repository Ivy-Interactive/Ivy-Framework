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

## Basic Usage

Create a file, for example `HelloApp.cs`:

```terminal
> ivy init --script
```

You will receive a file-based app that you can populate with your own context as needed.

```csharp
#: package Ivy@*

using Ivy;

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
> dotnet run HelloApp.cs
```

The app starts (by default on port 5010). Open the URL shown in the terminal to see your app.

## Prerequisites

- [**.NET 10** or later](https://dotnet.microsoft.com/download/dotnet/10.0) (single-file `dotnet run` is supported from .NET 10). See [Enhanced file-based apps](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/sdk#enhanced-file-based-apps-with-publish-support-and-native-aot) in the Microsoft documentation.
- [Ivy NuGet package](https://www.nuget.org/packages/Ivy) (referenced via a file-level directive in the script).

## File-Level Directive: Package

At the top of the file, use the **package** directive so the file can use Ivy without a `.csproj`:

```csharp
#: package Ivy@*
```

- `#: package` – file-level NuGet package reference (no project file needed).
- `Ivy@*` – the [Ivy](https://www.nuget.org/packages/Ivy) package; `*` means latest version. You can pin a version, e.g. `Ivy@1.2.0`.

## Usings

Include the namespaces you use in the file. Common ones:

| Namespace | Use for |
|-----------|---------|
| `Ivy` | [`Server`](./01_Program.md), server configuration and `RunAsync()`. [`AppDescriptor`](./10_Apps.md), `AppIds`, `AppHelpers`, the `[App]` attribute. [`UseService`](../../03_Hooks/02_Core/11_UseService.md), service registration and resolution. Helper utilities. [`ViewBase`](./02_Views.md), [`Layout`](./04_Layout.md), and built-in [widgets](./03_Widgets.md) ([`Card`](../../02_Widgets/03_Common/04_Card.md), [`Text`](../../02_Widgets/01_Primitives/01_TextBlock.md), [`Button`](../../02_Widgets/03_Common/01_Button.md), etc.). [Chrome](./11_Chrome.md), sidebar, and layout configuration. [Client](./13_Clients.md) and API usage. [Authentication](../03_CLI/04_Authentication/01_AuthenticationOverview.md) providers. [Input widgets](../../02_Widgets/04_Inputs/_Index.md) ([`TextInput`](../../02_Widgets/04_Inputs/02_TextInput.md), [`SelectInput`](../../02_Widgets/04_Inputs/05_SelectInput.md), etc.). |
| `Ivy.Core` | Core Ivy types. |
| `Ivy.Core.Hooks` | [Hooks](../../03_Hooks/01_HookIntroduction.md) ([UseState](../../03_Hooks/02_Core/03_UseState.md), [UseEffect](../../03_Hooks/02_Core/04_UseEffect.md), [UseMemo](../../03_Hooks/02_Core/05_UseMemo.md), etc.). |

Example (minimal for a simple app):

```csharp
using Ivy;
```

If you use only certain widgets or types, you might need extra namespaces (for example from other Ivy packages). Add `using` directives as you would in a normal C# project.

## Running the File

From the directory that contains your `.cs` file:

```terminal
> dotnet run HelloApp.cs
```

If you need a specific port or other server settings, configure the `Server` in code (for example via `ServerArgs` as in [Program](./01_Program.md)) or use environment variables (e.g. `PORT`) if your setup supports them.
