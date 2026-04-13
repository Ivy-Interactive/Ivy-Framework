![logo](https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/assets/logo_green_w200.png)

[![NuGet](https://img.shields.io/nuget/v/Ivy?style=flat)](https://www.nuget.org/packages/Ivy)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Ivy?style=flat)](https://www.nuget.org/packages/Ivy)
[![License](https://img.shields.io/github/license/Ivy-Interactive/Ivy-Framework?style=flat)](LICENSE)
[![CI](https://img.shields.io/github/actions/workflow/status/Ivy-Interactive/Ivy-Framework/backend-checks-linux.yml?style=flat&label=CI)](https://github.com/Ivy-Interactive/Ivy-Framework/actions/workflows/backend-checks-linux.yml)
[![website](https://img.shields.io/badge/website-ivy.app-green?style=flat)](https://ivy.app)
[![codespaces](https://img.shields.io/badge/codespaces-try-blue?style=flat&logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Devcontainer&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json&location=EuropeWest)
[![AGENTS.md](https://img.shields.io/badge/AGENTS.md-copy-purple?style=flat)](https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/refs/heads/main/AGENTS.md)

# Build Full-Stack Applications in Pure C\#

Ivy is a modern C# framework that lets you build reactive full-stack web applications entirely in pure C# - using familiar React-style components, hooks, and declarative patterns.
No frontend/backend split, no HTML/CSS/JS - just write type-safe C# code and ship beautiful, production-ready internal tools at lightning speed.

[Quick Start](https://docs.ivy.app/onboarding/getting-started/introduction) &nbsp;&nbsp;•&nbsp;&nbsp; [Docs](https://docs.ivy.app) &nbsp;&nbsp;•&nbsp;&nbsp; [Samples](https://samples.ivy.app) &nbsp;&nbsp;•&nbsp;&nbsp; [Examples](https://github.com/Ivy-Interactive/Ivy-Examples) &nbsp;&nbsp;•&nbsp;&nbsp; [Current Sprint](https://github.com/orgs/Ivy-Interactive/projects/8) &nbsp;&nbsp;•&nbsp;&nbsp; [Roadmap](https://github.com/orgs/Ivy-Interactive/projects/7)

## Simple Example

Ivy takes a lot of inspiration from frameworks like React. If you know React, you'll feel right at home. Here's a simple counter app built with Ivy:

```csharp
public class SimpleCounterApp : ViewBase
{
   public override object? Build()
   {
       var count = UseState(0);

       UseEffect(() =>
       {
           Console.WriteLine($"Count changed to: {count.Value}");
       }, [count]);

       return Layout.Vertical(
           Text.Block($"Count: {count.Value}"),
           new Button("Increment", onClick: _ => count.Set(count.Value + 1))
       );
   }
}
```

## Features

### ⚙️ Architecture
- **Rich Widget Library:** Extensive set of pre-built widgets to build any app. If you need more, an external widget framework is coming soon, where you can integrate any React, Angular, or Vue component.
- **External Widget Framework:** Easily integrate any third-party React component.
- **Hooks:** Familiar React-style hooks for state management, side effects, and lifecycle events.

### 🎨 UI Components
- **Forms:** Create complex CRUD forms with validation and data binding.
- **Data Tables:** Sort, filter, and paginate data.
- **Charts/Dashboards:** Build interactive charts and dashboards with ease.

### ⚡ Developer Experience
- **Hot-Reloading:** Full support for hot-reloading with maintained state as much as possible.
- **LLM Code-Generation Compatibility:** Designed to maximize compatibility with LLM code generation tools.

Ivy maintains state on the server and sends updates over WebSocket. The frontend consists of a pre-built React-based rendering engine. With Ivy, you never need to touch any HTML, CSS, or JavaScript. Only if you want to add your own widgets.

## Tools

The Ivy.Console CLI provides a suite of tools to streamline your development workflow:

### 🚀 Getting Started
- **Project Initialization:** Quickly set up new Ivy projects with predefined templates.
- **AI-Powered App Generation:** Generate applications using AI based on your specifications.
- **MCP:** Teach any coding agent to use Ivy Framework for building full-stack applications.

### 🛠️ Developer Tools
- **In-Browser DevTools:** An interactive widget inspector overlay. Hover to inspect any widget's type and properties, click to open an AI-powered prompt dialog. Enable with `--enable-dev-tools`.
- **DevTools REST API:** When DevTools are enabled, `GET /ivy/dev-tools/widget-schema` returns a JSON Schema of all registered widgets and their props/events. `GET /ivy/dev-tools/env-info` returns environment details.
- **Roslyn Analyzer (`Ivy.Analyser`):** Compile-time diagnostics that enforce Rules of Hooks, widget child constraints, constructor requirements, and service injection best practices — with auto-fix code actions.
- **Hot Reload:** Method-level changes are injected instantly without losing state; structural changes trigger automatic rebuild/restart. Enabled by default with `ivy run`.
- **`ivy fix`:** AI-powered build error diagnosis and auto-fix. Reads build output, sends it to an AI model, and applies corrections. Supports `--use-claude-code` for Claude Code integration.
- **`ivy ask` / `ivy question`:** Q&A over the framework knowledge base. Ask natural-language questions about Ivy APIs, patterns, and best practices.
- **`ivy docs`:** Browse framework documentation directly in the terminal. Use `ivy docs list` to see all topics.

### 🔌 Integrations
- **Authentication:** Built-in support for popular authentication providers like Supabase, Auth0, Clerk, and Microsoft Entra.
- **Database:** Easy integration with SQL Server, Postgres, Supabase, MariaDB, MySQL, Airtable, Oracle, Google Spanner, Clickhouse, Snowflake, and BigQuery.

### 📦 Production
- **Deployment Management:** Manage deployments to Azure, AWS, Google Cloud, or Sliplane with ease.
- **Secrets Management:** Securely manage sensitive information within your applications.

## Usage

### Quick Start

> ⚠️ **Note:** Ivy.Console is still in beta, and the agentic features require an account. [Register](https://ivy.app/auth/sign-up) for a free account to be among the first to try these features.

Make sure you have the following prerequisites installed:
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Rust Toolchain](https://rustup.rs/) (latest stable via `rustup`)

1. **Install Ivy CLI**:

   ```bash
   dotnet tool install -g Ivy.Console
   ```

2. **Install Vite+ CLI**:
   Ivy uses the `vp` (Vite+) CLI for frontend orchestration. Install it globally:

   ```bash
   npm install -g vite-plus
   ```

3. **Create a new project**:

   ```bash
    ivy init --hello
    ```

4. **Run**:

   ```bash
   ivy run --browse
   ```

5. **Open** [https://localhost:5010](https://localhost:5010) in your browser.

You can also run `ivy samples` to see all the components that Ivy offers and `ivy docs` for documentation.

## Want to help build Ivy Framework?

- [Contribution Guidelines](CONTRIBUTING.md)
- [Internal Developer Wiki](https://github.com/Ivy-Interactive/Ivy-Framework/wiki)
