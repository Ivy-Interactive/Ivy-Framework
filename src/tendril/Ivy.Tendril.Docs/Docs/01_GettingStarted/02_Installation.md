---
searchHints:
  - install
  - setup
  - prerequisites
  - getting-started
  - macos
  - windows
icon: Download
---

# Installation

<Ingress>
Install Tendril on macOS, Linux, or Windows using one of the methods below.
</Ingress>

## Quick Install (macOS / Linux)

One-liner: installs Tendril and required backend dependencies.

```bash
curl -sSf https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.sh | sh
```

## .NET Tool

Global install from NuGet (common on Windows):

```bash
dotnet tool install -g Ivy.Tendril --prerelease
```

*With `dotnet tool` only, install PowerShell 7+, Git, `gh`, and `claude` yourself.*

## Run

```bash
tendril
```

### First run

No `TENDRIL_HOME` yet – the **Onboarding** app opens: set home (default `~/.tendril`), add API keys (Anthropic, GitHub), and configure a project.
