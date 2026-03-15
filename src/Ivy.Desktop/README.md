![logo](https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/assets/logo_green_w200.png)

[![NuGet](https://img.shields.io/nuget/v/Ivy.Desktop?style=flat)](https://www.nuget.org/packages/Ivy.Desktop)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Ivy.Desktop?style=flat)](https://www.nuget.org/packages/Ivy.Desktop)
[![License](https://img.shields.io/github/license/Ivy-Interactive/Ivy-Framework?style=flat)](../LICENSE)
[![website](https://img.shields.io/badge/website-ivy.app-green?style=flat)](https://ivy.app)

# Ivy.Desktop: Run Ivy Apps as Native Desktop Applications

[Ivy](https://github.com/Ivy-Interactive/Ivy-Framework) is a modern C# framework for building full-stack applications in pure C#.

**Ivy.Desktop** allows you to seamlessly host and run your Ivy applications natively as cross-platform desktop applications (Windows, macOS, Linux) using [Photino](https://tryphotino.io). 

Leverage the power of Web UI on the desktop without shipping a heavy Chromium instance (Electron) while still keeping everything in 100% C#.

## Quick Start

### 1. Install the Package

Ensure you have your Ivy application project ready, then install the `Ivy.Desktop` NuGet package:

```bash
dotnet add package Ivy.Desktop
```

### 2. Initialize the Desktop App

Instead of passing your application to a web host builder, you initialize it within an `AppDescriptor` and pass it to the `DesktopWindow.Run()` method.

Here's an example:

```csharp
using Ivy;
using Ivy.Desktop;

public class MyDesktopApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            Text.Title("Hello from Ivy.Desktop!"),
            Text.Subtitle("Native desktop UI powered by C#")
        );
    }
}

// In your Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var appDescriptor = new AppDescriptor()
        {
            RootComponent = typeof(MyDesktopApp),
            InitialTitle = "My Desktop App",
        };

        DesktopWindow.Run(appDescriptor, args);
    }
}
```

### 3. Run Your Application

```bash
dotnet run
```

A native desktop window will open displaying your Ivy application!

## Learn More

* [Ivy Framework GitHub Repository](https://github.com/Ivy-Interactive/Ivy-Framework)
* [Ivy Documentation](https://docs.ivy.app)
* [Ivy Samples](https://samples.ivy.app)
