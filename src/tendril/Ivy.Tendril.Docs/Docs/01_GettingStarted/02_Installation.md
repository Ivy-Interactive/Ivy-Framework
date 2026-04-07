---
searchHints:
  - install
  - setup
  - prerequisites
  - getting-started
icon: Download
---

# Installation

<Ingress>
Get Tendril up and running on your machine.
</Ingress>

## Prerequisites

### For Running Tendril
- [Claude CLI](https://docs.anthropic.com/en/docs/claude-code) (`claude`)
- [GitHub CLI](https://cli.github.com/) (`gh`)
- PowerShell
- Git

### For Development
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

## Setup

### 1. Clone the repo

```bash
git clone https://github.com/Ivy-Interactive/Ivy-Framework.git
cd Ivy-Framework/src/tendril/Ivy.Tendril
```

### 2. Configure `config.yaml`

Copy the example config and edit it:

```bash
cp example.config.yaml config.yaml
```

Key fields:
- `projects` — List of projects with their repo paths, verifications, and context
- `codingAgent` — The coding agent to use (claude, codex, or gemini)

### 3. Set `TENDRIL_HOME` environment variable

Point `TENDRIL_HOME` to your Tendril data directory:

```bash
export TENDRIL_HOME=~/.tendril
mkdir -p "$TENDRIL_HOME"
```

Tendril will populate this with `Plans/`, `Inbox/`, `Trash/`, and `config.yaml` at runtime. If `TENDRIL_HOME` is not set, Tendril will launch the onboarding wizard.

### 4. Run

```bash
dotnet run
```
