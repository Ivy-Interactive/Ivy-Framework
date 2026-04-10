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

## Quick Install

One-liner: installs Tendril and required backend tools.

### macOS / Linux

```bash
curl -sSf https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.sh | sh
```

### Windows

```powershell
Invoke-RestMethod -Uri https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.ps1 | Invoke-Expression
```

## .NET Tool

Global install from NuGet:

```bash
dotnet tool install -g Ivy.Tendril --prerelease
```

<Callout type="Tip">
*With `dotnet tool` only, install PowerShell 7+, Git, `gh`, and `claude` yourself.*
</Callout>

## Run

```bash
tendril
```


