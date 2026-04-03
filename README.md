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

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Claude CLI](https://docs.anthropic.com/en/docs/claude-code) (`claude`)
- PowerShell
- Git

## Setup

1. **Clone the repo**

   ```bash
   git clone https://github.com/Ivy-Interactive/Ivy-Tendril.git
   cd Ivy-Tendril
   ```

2. **Configure `config.yaml`**

   Copy the example config and edit it:

   ```bash
   cp example.config.yaml config.yaml
   ```

   Key fields:
   - `tendrilData` -- Path to the data directory where plans, jobs, and artifacts are stored
   - `projects` -- List of projects with their repo paths, verifications, and context
   - `agentCommand` -- The Claude CLI command used to run agents

3. **Create the data directory**

   Create the directory specified in `tendrilData`:

   ```bash
   mkdir -p /path/to/tendril-data
   ```

   Tendril will populate this with `Plans/`, `Jobs/`, and other subdirectories at runtime.

4. **Run**

   ```bash
   dotnet run
   ```

## Project Structure

| Folder | Description |
|--------|-------------|
| `Services/` | Core services -- config loading, plan reading, job management, Git/GitHub integration |
| `Apps/` | TUI app screens -- plans list, jobs view, dashboard, review, PR creation |
| `AppShell/` | Application shell and navigation |
| `.promptwares/` | Agent promptwares for each lifecycle stage (MakePlan, ExecutePlan, etc.) |
| `Views/` | Shared UI components and views |
| `Controllers/` | Action controllers for plan operations |

## How It Works

Tendril manages plans through a structured lifecycle:

1. **MakePlan** -- An agent drafts a plan from a description or issue, producing a structured revision with problem, solution, tests, and verification steps.
2. **ExpandPlan** -- Optionally expands a plan with more detail, or splits large plans into smaller ones via **SplitPlan**.
3. **ExecutePlan** -- An agent creates a git worktree, implements the plan, runs verifications (build, format, tests), and commits the result.
4. **Review** -- You review the diff, run sample apps, and approve or send back with comments.
5. **MakePr** -- An agent creates a GitHub pull request from the worktree branch.

Each stage is powered by a promptware -- a structured prompt with tools and memory that runs via the Claude CLI. Jobs are tracked with live status, cost, and token metrics.
