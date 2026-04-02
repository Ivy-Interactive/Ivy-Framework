---
searchHints:
  - setup
  - install
  - cli
  - getting-started
  - download
  - prerequisites
  - packages
  - dependencies
  - project-structure
---

# Installation and Project Setup

<Ingress>
This page covers the installation of Ivy Framework components, setting up a new Ivy project, and understanding the basic project structure. It provides the foundational steps needed before building applications with Ivy.
</Ingress>

For information about core Ivy concepts like [Views](../02_Concepts/02_Views.md) and [state management](../../03_Hooks/02_Core/03_UseState.md), see [Core Concepts](../02_Concepts/02_Views.md). For guidance on building your first application, see [Basics](./03_Basics.md). For development tools and CLI commands, see [CLI Tools](../03_CLI/01_CLIOverview.md).

## Quick Start: Using the CLI

The easiest way to set up an Ivy project is using the Ivy [CLI](../03_CLI/01_CLIOverview.md). This will automatically create the project structure, configuration files, and necessary setup.

### Install Ivy Globally

Run the following command in your terminal to install Ivy as a global tool:

```terminal
>dotnet tool install -g Ivy.Console
```

<Callout Type="tip">
If you're using a specific operating system, read the instructions in your terminal after installing Ivy.Console.
You can always see all available commands by using `ivy --help`.
</Callout>

### Create a New Ivy Project

Use the Ivy CLI to scaffold a new project:

```terminal
>ivy init --namespace Acme.InternalProject
>ivy run
```

This will create a new Ivy project with the necessary structure and configuration files. For more details about the generated project structure, see [Ivy Init](../03_CLI/02_Init.md).

## Prerequisites

Ivy Framework strictly requires the following toolchains to develop applications:

- **.NET 10.0 SDK**: All Ivy projects and packages are built against this target framework.
- **Rust Toolchain**: Required for the underlying high-performance JSON-diffing engine. Ensure you have the latest stable compiler installed via [rustup](https://rustup.rs/).
- **vp CLI (Vite+)**: Required for frontend orchestration. Install globally via `npm install -g vite-plus`.

## Manual Setup: Creating Your First Project

If you prefer to set up a project manually, follow these steps:

### Create Console Application

Create a new .NET console application:

```terminal
>dotnet new console -n MyIvyApp
>cd MyIvyApp
```

### Add Ivy Package

```terminal
>dotnet add package Ivy
```

### Basic Server Configuration

Replace the contents of `Program.cs` with minimal server setup:

```csharp
using Ivy;

var server = new Server();
server.UseHotReload();
server.AddAppsFromAssembly();
server.UseAppShell();

await server.RunAsync();
```

This configuration creates a Server instance, enables hot reload for development, automatically discovers apps in the current assembly, uses default app shell (sidebar navigation), and starts the server.

### Project File Configuration

Ensure your `.csproj` targets .NET 10.0:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

## Core Package Installation

The primary Ivy Framework package is installed via NuGet and provides the foundation for your application.

```terminal
>dotnet add package Ivy
```

| Component | Description |
| :--- | :--- |
| **Core Framework** | High-performance server-side engine and application system |
| **Widget System** | Library of strongly-typed UI components (Shadcn/Tailwind) |
| **SignalR Hub** | Real-time state synchronization between C# and React |
| **Embedded Assets** | Pre-built frontend bundle embedded in the DLL |
| **Auth Interfaces** | extensible framework for security and identity |

Extend Ivy's functionality with official extension packages for authentication and data management.

| Package | Purpose |
| :--- | :--- |
| `Ivy.Auth.Supabase` | Identity management via [Supabase](../03_CLI/04_Authentication/02_Supabase.md) |
| `Ivy.Auth.Authelia` | Single Sign-On and 2FA via [Authelia](../03_CLI/04_Authentication/02_Authelia.md) |
| `Ivy.Auth.Entra` | Microsoft [Entra](../03_CLI/04_Authentication/02_MicrosoftEntra.md) ID (Azure AD) integration |
| `Ivy.Database.Generator.Toolkit` | Utilities for AI-powered schema and code generation |

```terminal
>dotnet add package Ivy.Auth.Supabase
>dotnet add package Ivy.Database.Generator.Toolkit
```

The Ivy package abstracts away several modern technologies to provide its seamless developer experience:

- **ASP.NET Core**: Secure and scalable web hosting
- **SignalR**: Low-latency, real-time communication
- **JWT & Auth**: Industrial-grade security protocols
- **System.Reactive**: Event-driven UI updates
- **JSON Patch**: Efficient state synchronization

## Project Structure Overview

A standard Ivy project follows a clean, flattened structure designed for clarity.

| File/Folder | Description |
| :--- | :--- |
| **`Project.csproj`** | Matches the .NET 10.0 target and contains Ivy references |
| **`Program.cs`** | The entry point where you configure and run the Ivy server |
| **`Apps/`** | Where your Views and business logic reside |
| **`Assets/`** | Optional static files (images, custom CSS) |

### Multi-Project Solutions

For enterprise-scale applications, we recommend a multi-project structure to separate concerns:

- **Web Project**: Contains `Program.cs` and server startup configuration.
- **Shared/Core Project**: Contains the majority of your `ViewBase` classes and domain logic.
- **Test Project**: Contains unit and integration tests for your UI components.

## Server Configuration

The server configuration follows a builder pattern where each method configures different aspects of the Ivy application before calling `RunAsync()` to start the web server.

For detailed server configuration options, including `ServerArgs` properties and advanced settings, see [Program](../02_Concepts/01_Program.md).

### Development vs Production

The server automatically optimizes its behavior based on the current environment.

| Feature | Development | Production |
| :--- | :--- | :--- |
| **Hot Reload** | Enabled (instant UI updates) | Disabled (optimized performance) |
| **Error Handling** | Detailed stack traces | Secure, logged exceptions |
| **Caching** | Disabled for immediate changes | Aggressive ETag & compression |
| **Logging** | Debug & Information | Warning & Error only |
| **Port Management** | Conflict detection & auto-shift | Strict port binding |
