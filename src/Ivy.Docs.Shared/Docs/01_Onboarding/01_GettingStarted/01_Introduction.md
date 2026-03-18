---
searchHints:
  - overview
  - what-is
  - framework
  - fullstack
---

# Welcome to Ivy

<Ingress>
Ivy is the ultimate framework for building internal tools and dashboards using Pure C#, unifying frontend and backend into a single cohesive codebase. It can also be used to build general-purpose full-stack applications. Ivy's main focus is on developer's experience, UI and UX.
</Ingress>

Ivy eliminates the traditional frontend/backend split by bringing React-like declarative patterns directly to C#. You build your entire project—UI, logic, and data access—in one place.

<Embed Url="https://www.youtube.com/watch?v=pQKSQR9BfD8"/>

## What You Can Do

- **[Database Integration](https://docs.ivy.app/onboarding/cli/database-integration/database-overview)**: Connect to SQL Server, PostgreSQL, Supabase, and more with `ivy db add`.
- **[Authentication](https://docs.ivy.app/onboarding/cli/authentication/authentication-overview)**: Add Auth0, Clerk, or Microsoft Entra with `ivy auth add`.
- **[Deployment](https://docs.ivy.app/onboarding/cli/deploy)**: Deploy to AWS, Azure, GCP, or Sliplane with `ivy deploy`.
- **AI Agentic Features** : Generate entire back-office applications from your database schema using `ivy app create`.

## Getting Started

### Easiest Way: File-based Apps in under 1 minute

The fastest way to try Ivy is using [.NET 10's file-based apps feature](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview). This allows you to create a self-contained full-stack application in a single script-like file without any project files.

Make sure to have latest .NET 10 installed, [download it here](https://dotnet.microsoft.com/en-us/download)

1. Create a new file called `HelloWorldApp.cs`.
2. Paste the following code:

```csharp
#:package Ivy@*

using Ivy;

var server = new Server();
server.AddApp<HelloApp>();
await server.RunAsync();

[App]
class HelloApp : ViewBase
{
    public override object? Build()
    {
        var nameState = UseState<string>();

        return Layout.Center()
               | new Card(
                   Layout.Vertical().Gap(6).Padding(2)
                   | new Confetti(new IvyLogo())
                   | Text.H2("Hello " + (string.IsNullOrEmpty(nameState.Value) ? "there" : nameState.Value) + "!")
                   | Text.Block("Welcome to the fantastic world of Ivy. Let's build something amazing together!")
                   | nameState.ToInput(placeholder: "What is your name?")
                   | new Separator()
                   | Text.Markdown("You'd be a hero to us if you could ⭐ us on [Github](https://github.com/Ivy-Interactive/Ivy-Framework)")
                 )
                 .Width(Size.Units(120).Max(500));
    }
}
```

#### Understanding the Code

Let's break down each part of the file-based application.

##### Package Reference

```csharp
#:package Ivy@*
```

This is a direct [Ivy nuget package](https://www.nuget.org/packages/Ivy) reference. It tells the .NET runtime to download and use the latest version of the framework, allowing you to run this file as a standalone script without a project file.

##### Server Initialization

```csharp
var server = new Server();
server.AddApp<HelloWorldApp>();

await server.RunAsync();
```

This part initializes the Ivy server, registers your `HelloWorldApp` view, and starts the server. The server handles all state management and real-time communication. [Learn more about the Ivy program here](https://docs.ivy.app/onboarding/concepts/program).

##### The View

```csharp
[App]
class HelloWorldApp : ViewBase
{
  // ...
}
```

In Ivy, your UI is organized into views. By inheriting from `ViewBase`, you get access to all the hooks and lifecycle methods. The [[App] attribute](https://docs.ivy.app/onboarding/concepts/apps) tells Ivy to show this view in the main navigation.

##### The Build Method

```csharp
public override object? Build()
{
  return Layout.Center()
      | new Card(
          // ...
      );
}
```

The `Build()` method is where you define your UI. It returns a tree of components that Ivy renders on the client. Just like in React, whenever state changes, this method is called again to determine the new UI structure.

##### UI Composition

```csharp
Layout.Center()
    | new Card(...)
```

Ivy uses a fluent API and the pipe operator (`|`) to compose layouts and widgets. This makes it easy to read and build complex hierarchical UIs. Learn more about the [Card widget here](https://docs.ivy.app/widgets/common/card).

1. Run it immediately using the Ivy CLI:

```terminal
>dotnet run HelloWorldApp.cs
```

Ivy will start a local server at `http://localhost:5010`. Open it in your browser to see your interactive "Hello" app!

---

### Advanced Projects: Regular Projects in under 3 minutes

For larger applications that require multiple files, services, and deep integrations, we recommend creating a regular Ivy project using the CLI.

#### Installing the CLI

To use the `ivy` command, you'll need the **.NET 10 SDK** installed. Then, install the Ivy CLI globally:

```terminal
>dotnet tool install -g Ivy.Console
```

Verify the installation:

```terminal
>ivy --version
```

#### Initializing a Project

Create a new directory for your project and initialize it. We recommend using the `--hello` flag to include an example hello app:

```terminal
>mkdir MyProject
>cd MyProject
>ivy init --hello
```

#### Running Your Project

Run the project with hot reloading enabled:

```terminal
>ivy run
```

## Community & Resources

- **[Ivy Samples](https://samples.ivy.app)**: Real-time demo of all Ivy widgets and layouts.
- **[App Gallery](https://ivy.app/gallery)**: See real-world applications and integrations built with Ivy.
- **[Ivy Framework GitHub](https://github.com/Ivy-Interactive/Ivy-Framework)**: The core framework source code. Open-source and free to use.
- **[Ivy Examples GitHub](https://github.com/Ivy-Interactive/Ivy-Examples)**: A collection of example projects to kickstart your development.
