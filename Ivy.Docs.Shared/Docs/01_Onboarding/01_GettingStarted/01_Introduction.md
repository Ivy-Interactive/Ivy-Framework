---
searchHints:
  - overview
  - what-is
  - framework
  - fullstack
---

# Introduction to Ivy

<Ingress>
Ivy is a full-stack C# web framework that lets you build interactive data applications without the complexity of separate frontend/backend APIs. Think React patterns, but entirely in C#.
</Ingress>

<Embed Url="https://www.youtube.com/watch?v=pQKSQR9BfD8"/>

## What Makes Ivy Different

Ivy eliminates the traditional frontend/backend split by bringing React-like patterns directly to C#. You build your entire project - UI, logic, and data access - in one cohesive C# codebase.

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

The Ivy Framework is a comprehensive solution for building internal business applications. The framework targets scenarios where rapid development, maintainability, and integration with existing enterprise systems are prioritized.

We created Ivy to solve common frustrations with modern web development:

### Cost & Speed Optimization

Everyday tasks should be simple and idiomatic. Complex requirements should remain possible, but building basic CRUD projects shouldn't require weeks of setup.

### Eliminating Boilerplate

Traditional SPA solutions require separate frontend/backend codebases communicating through APIs. This creates massive amounts of boilerplate for simple data operations.

### Avoiding Technical Debt

Many low-code SaaS products are limited, expensive long-term, and create vendor lock-in. Ivy gives you the productivity benefits without the constraints.

## Core Features

### Full-Stack C# Development

```mermaid
graph LR
    A[C# Views] --> B[Widget Tree]
    B --> C[SignalR Hub]
    C --> D[React Frontend]
    D --> E[User Events]
    E --> C
    C --> A
    
```

- Full-stack C# development with no separate API layer needed
- React-like declarative UI patterns using C# syntax
- Views render into strongly-typed Widgets
- Built-in scaffolding for common patterns (Tables, Forms, CRUD operations)

### Real-Time & Interactive

```mermaid
graph LR
    A[User Input] --> B[C# Handler]
    B --> C[State Change]
    C --> D[Auto Re-render]
    D --> E[UI Update]
    
    F[Hot Reload] --> G[State Preserved]
    
```

- WebSocket-based UI updates (similar to Streamlit)
- Hot reloading with state preservation during development
- Any .NET object can be rendered using ContentBuilder pipelines
- Automatic change detection and selective re-rendering

### Modern Frontend Integration

```mermaid
graph LR
    A[Ivy Widgets] --> B[React + Shadcn]
    B --> C[TailwindCSS]
    
    D[External Components] --> E[NuGet Packages]
    E --> A
    
    F[Dark Mode] -->  G[Theming System]
    G --> A
    H[Custom Chromes] --> A
    
```

- Widgets rendered using React + Shadcn + TailwindCSS
- Import external React components as Ivy widgets via NuGet
- Built-in dark mode and theming support
- Customizable application "chromes" (also built in Ivy)

### Enterprise Ready

```mermaid
graph LR
    A[Security] --- B[Data] --- C[Architecture]
```

- Multiple authentication providers (Supabase, Authelia, Basic Auth) with RBAC
- Database integration (SQL Server, PostgreSQL, SQLite, MySQL) via Entity Framework Core
- Secrets management and configuration
- Dependency injection throughout
- Caching and performance optimizations
- Flexible routing system

### Development & Deployment

```mermaid
graph LR
    A[Your Code] --> B[ivy init]
    B --> C[ivy run]
    C --> H[Docker-first deployment]
    
    F[Rich CLI] --> B
    G[Unit Tests] --> A
    
```

- Rich CLI tooling for project scaffolding and deployment
- One-command container deployment to AWS, Azure, GCP, or your own infrastructure
- Unit testing without browser automation complexity
- Docker-first deployment with environment management

## Getting Started

Ready to try Ivy? The fastest way to get started is:

```terminal
>dotnet tool install -g Ivy.Console
>ivy init --namespace MyCompany.InternalProject
>dotnet watch
```

You can install Ivy with a simple command, check its version to verify if it installed correctly and initialize your first project.

That's it! You'll have a running Ivy application with hot reloading enabled.

<Callout Type="tip">
If you want to use Ivy agent features, you will need an Ivy account https://ivy.app/auth/sign-up
</Callout>

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
