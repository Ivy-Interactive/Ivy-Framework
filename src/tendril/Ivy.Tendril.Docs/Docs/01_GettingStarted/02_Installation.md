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

## Prerequisites

Before installing Tendril, ensure your system has the following requirements:

- **.NET 10.0 SDK** (Required for the Tendril runtime)
- **GitHub CLI** (`gh` for automated Pull Request interactions)
- **Claude CLI** (`claude` for the default coding agent execution)
- **Git** (Required for worktree isolation)
- **PowerShell** (Used internally for Promptware scripts)

## System Installation

### macOS (One-Liner)

For macOS users, we provide a streamlined, automated installation script that sets up Tendril and its dependencies (including the GitHub CLI and .NET 10 if missing).

```bash
curl -fsSL https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/scripts/install.sh | sh
```

### Windows & Linux (.NET Tool)

Tendril can be installed globally as a .NET Tool from the provided NuGet packages.

```bash
dotnet tool install -g Ivy.Tendril --prerelease
```

*Note: You may need to specify a custom NuGet feed if the package is hosted on an internal registry or GitHub Packages.*

## Initial Setup

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

### Manual Configuration

If you prefer to set up manually:

```bash
export TENDRIL_HOME=~/.tendril
mkdir -p "$TENDRIL_HOME"
```

Then, you can utilize the internal **Setup App** inside Tendril to edit your `config.yaml` file visually.

## Development Setup

If you are contributing to the Ivy Framework and want to build Tendril from source:

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Ivy-Interactive/Ivy-Framework.git
   cd Ivy-Framework/src/tendril/Ivy.Tendril
   ```

2. **Configure your development environment:**
   Copy the example config to your local environment.
   ```bash
   cp example.config.yaml config.yaml
   ```

3. **Run from source:**
   ```bash
   dotnet run
   ```
