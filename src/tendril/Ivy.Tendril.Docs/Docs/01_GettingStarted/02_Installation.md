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
Tendril is distributed as a multi-platform application. Follow these instructions to get Tendril up and running on your device.
</Ingress>

## Quick Install

These scripts make sure all the required tools are installed.

### macOS / Linux

```bash
curl -sSf https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.sh | sh
```

### Windows

```powershell
Invoke-RestMethod -Uri https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.ps1 | Invoke-Expression
```

## .NET Tool Installation

Tendril can also be installed globally as a native .NET Tool from the provided NuGet packages.

```bash
dotnet tool install -g Ivy.Tendril --prerelease
```

<Callout type="Tip">
*Note: If you only use the `dotnet tool` command, you must manually ensure that PowerShell 7+, Git, the `gh` CLI, and the `claude` CLI are installed on your system to get started.*
</Callout>

## Booting Tendril

Once installed, you can launch Tendril by simply typing `tendril` in your terminal.

```bash
tendril
```

### Onboarding Wizard

If you are running Tendril for the first time or do not have a configured `TENDRIL_HOME` directory, Tendril will automatically launch the **Onboarding App**.

The wizard will guide you through:

1. Setting up your `TENDRIL_HOME` directory (defaults to `~/.tendril`).
2. Providing your necessary API keys (Anthropic, GitHub).
3. Configuring your first project.
