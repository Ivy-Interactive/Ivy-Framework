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

## Quick Install (macOS / Linux)

The easiest way to install Tendril on macOS or Linux is via our automated install script. It automatically sets up Tendril and ensures all required backend tools come preinstalled.

```bash
curl -sSf https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.sh | sh
```

## .NET Tool Installation

Tendril can also be installed globally as a native .NET Tool from the provided NuGet packages (useful for Windows environments).

```bash
dotnet tool install -g Ivy.Tendril --prerelease
```

*Note: If you only use the `dotnet tool` command, you must manually ensure that PowerShell 7+, Git, the `gh` CLI, and the `claude` CLI are installed on your system to get started.*

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


