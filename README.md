![logo](https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/assets/logo_green_w200.png)

[![NuGet](https://img.shields.io/nuget/v/Ivy?style=flat)](https://www.nuget.org/packages/Ivy)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Ivy?style=flat)](https://www.nuget.org/packages/Ivy)
[![License](https://img.shields.io/github/license/Ivy-Interactive/Ivy-Framework?style=flat)](LICENSE)
[![CI](https://img.shields.io/github/actions/workflow/status/Ivy-Interactive/Ivy-Framework/backend-checks-linux.yml?style=flat\&label=CI)](https://github.com/Ivy-Interactive/Ivy-Framework/actions/workflows/backend-checks-linux.yml)
[![website](https://img.shields.io/badge/website-ivy.app-green?style=flat)](https://ivy.app)
[![codespaces](https://img.shields.io/badge/codespaces-try-blue?style=flat\&logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Devcontainer&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json&location=EuropeWest)
[![AGENTS.md](https://img.shields.io/badge/AGENTS.md-copy-purple?style=flat)](https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/refs/heads/main/AGENTS.md)

# Build Full-Stack Applications in Pure C#

Ivy is a modern C# framework that lets you build reactive full-stack web applications entirely in pure C# - using familiar React-style components, hooks, and declarative patterns.
No frontend/backend split, no HTML/CSS/JS - just write type-safe C# code and ship beautiful, production-ready internal tools at lightning speed.

[Quick Start](https://docs.ivy.app/onboarding/getting-started/introduction) • [Docs](https://docs.ivy.app) • [Samples](https://samples.ivy.app) • [Examples](https://github.com/Ivy-Interactive/Ivy-Examples) • [Current Sprint](https://github.com/orgs/Ivy-Interactive/projects/8) • [Roadmap](https://github.com/orgs/Ivy-Interactive/projects/7)

---

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

---

## Features

### ⚙️ Architecture

* **Rich Widget Library:** Extensive set of pre-built widgets to build any app. If you need more, an external widget framework is coming soon.
* **External Widget Framework:** Easily integrate any third-party React component.
* **Hooks:** Familiar React-style hooks for state management, side effects, and lifecycle events.

### 🎨 UI Components

* **Forms:** Create complex CRUD forms with validation and data binding.
* **Data Tables:** Sort, filter, and paginate data.
* **Charts/Dashboards:** Build interactive charts and dashboards with ease.

### ⚡ Developer Experience

* **Hot-Reloading:** Full support for hot-reloading with maintained state as much as possible.
* **LLM Code-Generation Compatibility:** Designed to maximize compatibility with LLM code generation tools.

---

## Tools

### 🚀 Getting Started

* **Project Initialization:** Quickly set up new Ivy projects with predefined templates.
* **AI-Powered App Generation:** Generate applications using AI based on your specifications.
* **MCP:** Teach any coding agent to use Ivy Framework for building full-stack applications.

### 🔌 Integrations

* **Authentication:** Supabase, Auth0, Clerk, Microsoft Entra.
* **Database:** SQL Server, Postgres, Supabase, MariaDB, MySQL, Airtable, Oracle, Google Spanner, Clickhouse, Snowflake, BigQuery.

### 📦 Production

* Deployment Management: Azure, AWS, Google Cloud, Sliplane.
* Secrets Management: Securely manage sensitive information.

**[See Demo Video](https://www.youtube.com/watch?v=krH7sBLjUrM)**

---

## Usage

> ⚠️ Ivy.Console is still in beta. Agentic features require an account. [Register](https://ivy.app/auth/sign-up) for free.

1. Install Ivy CLI:

```bash
dotnet tool install -g Ivy.Console
```

2. Create a new project:

```bash
ivy init --hello
```

3. Run:

```bash
ivy run --browse
```

4. Open [http://localhost:5010](http://localhost:5010) in your browser.

You can also run `ivy samples` to see all the components that Ivy offers and `ivy docs` for documentation.

---

## 🏢 Real-World Project Structure

Organize larger internal tools by feature for maintainability and scalability.

### Suggested Feature-Based Structure

```
/Features
  /Users
    UsersView.cs
    UsersService.cs
    UsersModel.cs

  /Orders
    OrdersView.cs
    OrdersService.cs
    OrdersModel.cs
```

This approach:

* Keeps business logic separated from UI components
* Makes scaling to 50+ screens easier
* Encourages clean architecture practices
* Aligns with enterprise C# structures

### API Integration Tip

```csharp
builder.Services.AddHttpClient<IMyApiClient, MyApiClient>();
```

---

## Want to help build Ivy Framework?

* [Contribution Guidelines](CONTRIBUTING.md)
* [Internal Developer Wiki](https://github.com/Ivy-Interactive/Ivy-Framework/wiki)
