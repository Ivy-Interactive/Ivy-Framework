# Ivy Framework Development

This directory contains the source code for the Ivy Framework. 

## Build Requirements

To build the Ivy Framework solution (`Ivy-Framework.slnx`) properly, you must have **vp (Vite+)** installed on your system. 

Vite+ is used to manage the frontend assets and build pipeline integrated into the MSBuild process.

### Install vp (Vite+)

#### macOS / Linux
```bash
curl -fsSL https://vite.plus | bash
```

#### Windows (PowerShell)
```bash
irm https://vite.plus/ps1 | iex
```

After installation, restart your terminal and verify the installation:
```bash
vp --version
```

For more information and advanced usage, visit the [Vite+ Documentation](https://viteplus.dev/guide/).

## Getting Started

1. Open `Ivy-Framework.slnx` in your IDE (Visual Studio, Rider) or use the dotnet CLI.
2. Ensure `vp` is installed.
3. Build the solution:
   ```bash
   dotnet build
   ```
