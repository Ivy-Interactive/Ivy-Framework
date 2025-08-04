# Introduction to Ivy

> **TL;DR:** Ivy is a full-stack C# web framework that lets you build interactive data applications without the complexity of separate frontend/backend APIs. Think React patterns, but entirely in C#.

<Embed Url="https://www.youtube.com/watch?v=pQKSQR9BfD8"/>

## What Makes Ivy Different

Ivy eliminates the traditional frontend/backend split by bringing React-like patterns directly to C#. You write your entire application - UI, logic, and data access - in one cohesive C# codebase.

```csharp
[App(icon: Icons.Users)]
public class UserDashboard : ViewBase
{
    public override object? Build()
    {
        var users = UseService<IUserService>();
        var searchTerm = UseState("");
        
        return new Card()
            .Title("User Management")
            | Layout.Vertical(
                searchTerm.ToTextInput(placeholder: "Search users..."),
                new Table(users.SearchUsers(searchTerm.Value))
                    .Columns(
                        col => col.Name,
                        col => col.Email,
                        col => col.LastLogin
                    )
            );
    }
}
```

## Why Ivy Exists

We created Ivy to solve common frustrations with modern web development:

### Cost & Speed Optimization
Everyday tasks should be simple and idiomatic. Complex requirements should remain possible, but building basic CRUD apps shouldn't require weeks of setup.

### Eliminating Boilerplate
Traditional SPA solutions require separate frontend/backend codebases communicating through APIs. This creates massive amounts of boilerplate for simple data operations.

### Avoiding Technical Debt
Many low-code SaaS products are limited, expensive long-term, and create vendor lock-in. Ivy gives you the productivity benefits without the constraints.

### Open-Source & Cloud-Native
Deploy anywhere - AWS, Azure, GCP, or your own infrastructure. No vendor lock-in, no proprietary hosting requirements.

## Core Features

### Framework Architecture
- Full-stack C# development with no separate API layer needed
- React-like declarative UI patterns using C# syntax
- Views render into strongly-typed Widgets
- Built-in scaffolding for common patterns (Tables, Forms, CRUD operations)

### Real-Time & Interactive
- WebSocket-based UI updates (similar to Streamlit)
- Hot reloading with state preservation during development
- Any .NET object can be rendered using ContentBuilder pipelines
- Automatic change detection and selective re-rendering

### Modern Frontend Integration
- Widgets rendered using React + Shadcn + TailwindCSS
- Import external React components as Ivy widgets via NuGet
- Built-in dark mode and theming support
- Customizable application "chromes" (also built in Ivy)

### Enterprise Ready
- Authentication & authorization providers with RBAC
- Entity Framework Core integration
- Secrets management and configuration
- Dependency injection throughout
- Caching and performance optimizations
- Flexible routing system

### Development & Deployment
- One-command container deployment to any cloud provider
- Rich CLI tooling for project scaffolding and deployment
- Unit testing without browser automation complexity
- Docker-first deployment with environment management

## Getting Started

Ready to try Ivy? The fastest way to get started is:

```terminal
>dotnet tool install -g Ivy.Cli
>ivy init
>dotnet watch
```

That's it! You'll have a running Ivy application with hot reloading enabled.

## What's Next

Ivy is actively developed with exciting features on the roadmap:

### Advanced Data Handling
- Apache Arrow integration for massive datasets
- Advanced filtering, sorting, and pagination
- Airtable-like experiences from Entity Framework queries
- Real-time data visualization and dashboards

### AI Development Integration
- Deep integration with modern AI coding tools like Cursor and Claude Code
- AI-powered scaffolding and code generation
- Smart component suggestions and auto-completion
