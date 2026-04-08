# Tendril

A TUI-based agent orchestration platform for managing AI-driven development plans.

## What is Tendril?

Tendril is a terminal UI application built on [Ivy Framework](https://github.com/Ivy-Interactive/Ivy-Framework) that manages AI coding plans end-to-end. It orchestrates Claude-based agents through a structured lifecycle -- from plan creation and expansion to execution, verification, and PR generation. Tendril tracks jobs, costs, tokens, and verification results, giving you full visibility into your AI-assisted development workflow.

## Features

- **Plan lifecycle management** -- Draft, Execute, Review, and PR stages with state tracking
- **Multi-project support** -- Configure multiple repos with per-project verifications
- **Job monitoring** -- Live cost and token tracking for running agents
- **Claude agent orchestration** -- Promptwares for each stage (MakePlan, ExecutePlan, ExpandPlan, MakePr, etc.)
- **Dashboard** -- Activity statistics and plan counts at a glance
- **GitHub PR integration** -- Automated pull request creation from completed plans
- **Plan review workflow** -- Review diffs, run sample apps, approve or send back for revision

## Prerequisites

### For Running Tendril

- [Claude CLI](https://docs.anthropic.com/en/docs/claude-code) (`claude`)
- [GitHub CLI](https://cli.github.com/) (`gh`)
- PowerShell
- Git

### For Development

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

## Setup

1. **Clone the repo**

   ```bash
   git clone https://github.com/Ivy-Interactive/Ivy-Framework.git
   cd Ivy-Framework/src/tendril/Ivy.Tendril
   ```

2. **Configure `config.yaml`**

   Copy the example config and edit it:

   ```bash
   cp example.config.yaml ~/.tendril/config.yaml
   ```

   Key fields:
   - `projects` -- List of projects with their repo paths, verifications, and context
   - `codingAgent` -- The coding agent to use (claude, codex, or gemini)

3. **Set `TENDRIL_HOME` environment variable**

   Point `TENDRIL_HOME` to your Tendril data directory:

   ```bash
   export TENDRIL_HOME=~/.tendril
   mkdir -p "$TENDRIL_HOME"
   ```

   Tendril will populate this with `Plans/`, `Inbox/`, `Trash/`, and `config.yaml` at runtime. If `TENDRIL_HOME` is not set, Tendril will launch the onboarding wizard.

4. **Run**

    ```bash
    dotnet run --project Ivy.Tendril/Ivy.Tendril.csproj
    ```

### Running as Desktop App

1. **Build and Run**

   ```bash
   dotnet run --project Ivy.Tendril.Desktop/Ivy.Tendril.Desktop.csproj
   ```

   This launches Tendril in a native cross-platform window using `Ivy.Desktop`.

### Installing as Global CLI Tool (NPM)

You can run Tendril from any directory using `npx` or by installing it globally via `npm`.

1. **Via `npx`**

   ```bash
   npx @ivy/tendril
   ```

2. **Global Install**

   ```bash
   npm install -g @ivy/tendril
   dotnet tool install -g Ivy.Tendril
   tendril
   ```

   *(Note: The NPM package is a wrapper for the `dotnet tool`. Both must be available for the `tendril` command to work.)*

## Project Structure

| Folder | Description |
|---|---|
| `Services/` | Core services -- config loading, plan reading, job management, Git/GitHub integration |
| `Apps/` | TUI app screens -- plans list, jobs view, dashboard, review, PR creation |
| `AppShell/` | Application shell and navigation |
| `.promptwares/` | Agent promptwares for each lifecycle stage (MakePlan, ExecutePlan, etc.) |
| `Views/` | Shared UI components and views |
| `Controllers/` | Action controllers for plan operations |
| `Database/` | SQLite database schema and migrations |

## How It Works

Tendril manages plans through a structured lifecycle:

1. **MakePlan** -- An agent drafts a plan from a description or issue, producing a structured revision with problem, solution, tests, and verification steps.
2. **ExpandPlan** -- Optionally expands a plan with more detail, or splits large plans into smaller ones via **SplitPlan**.
3. **ExecutePlan** -- An agent creates a git worktree, implements the plan, runs verifications (build, format, tests), and commits the result.
4. **Review** -- You review the diff, run sample apps, and approve or send back with comments.
5. **MakePr** -- An agent creates a GitHub pull request from the worktree branch.

Each stage is powered by a promptware -- a structured prompt with tools and memory that runs via the Claude CLI. Jobs are tracked with live status, cost, and token metrics.
