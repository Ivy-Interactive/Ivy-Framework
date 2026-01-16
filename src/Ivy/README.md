![Ivy Framework](https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/assets/logo_green_w200.png)

[![NuGet](https://img.shields.io/nuget/v/Ivy?style=flat)](https://www.nuget.org/packages/Ivy)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Ivy?style=flat)](https://www.nuget.org/packages/Ivy)
[![License](https://img.shields.io/github/license/Ivy-Interactive/Ivy-Framework?style=flat)](https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/LICENSE)
[![Website](https://img.shields.io/badge/website-ivy.app-green?style=flat)](https://ivy.app)

# Build Full-Stack Applications in Pure C#

Ivy is a modern C# framework that lets you build reactive full-stack web applications entirely in pure C# — using familiar React-style components, hooks, and declarative patterns.

No frontend/backend split, no HTML/CSS/JS — just write type-safe C# code and ship beautiful, production-ready internal tools at lightning speed.

## Quick Start

```csharp
public class CounterApp : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        
        return Layout.Vertical(
            Text.Block($"Count: {count.Value}"),
            new Button("Increment", onClick: _ => count.Set(count.Value + 1))
        );
    }
}
```

## Features

- 🧩 **Rich Widget Library** — Extensive set of pre-built widgets
- 🪝 **Hooks** — React-style hooks for state management
- 📝 **Forms** — Complex CRUD forms with validation
- 📊 **Data Tables** — Sort, filter, and paginate data
- 📈 **Charts/Dashboards** — Interactive visualizations
- 🔥 **Hot-Reloading** — Full support with maintained state
- 🤖 **LLM Compatible** — Designed for AI code generation

## Links

- 📖 [Documentation](https://docs.ivy.app)
- 🎮 [Samples](https://samples.ivy.app)
- 💻 [GitHub Repository](https://github.com/Ivy-Interactive/Ivy-Framework)
- 💬 [Discord Community](https://discord.gg/CffzHm66BW)