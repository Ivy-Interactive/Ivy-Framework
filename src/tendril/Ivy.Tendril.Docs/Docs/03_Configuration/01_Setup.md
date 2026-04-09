---
icon: Wrench
searchHints:
  - config
  - yaml
  - configuration
  - settings
  - projects
  - gui
---

# Setup & Settings

<Ingress>
Tendril can be configured via a visually intuitive Settings App (Setup Menu) inside the GUI, or manually by editing the `config.yaml` file in the `TENDRIL_HOME` directory. It defines your projects, agent tool chains, runtime levels, and system preferences.
</Ingress>

## The Settings App

You can open the setup screen directly from Tendril to configure all system settings without touching a configuration file. It provides separate views for:

- **General Settings**: Select your default coding agent (e.g., `claude`, `codex`), adjust concurrent job limits, and register Slack emojis/team members.
- **Levels**: Define architectural complexity levels (e.g., L1, L2, L3) with customized prompt weighting to determine how agents treat large vs small features.
- **Verifications**: Setup strict testing and linting commands (`DotnetBuild`, `NpmTest`) that agents must pass.
- **Promptwares**: Register paths to your custom Agent Prompts and Tool sets.
- **Projects**: Register new Git repositories that agents can check out and mutate.

## Manual Configuration (`config.yaml`)

If you prefer Infrastructure-as-Code, Tendril looks for `config.yaml` in the `TENDRIL_HOME` directory. Any change made in the visual Settings App is written to this YAML file instantly.

### Structure Example

```yaml
codingAgent: claude
maxConcurrentJobs: 3

projects:
  - name: MyProject
    repo: D:\Repos\MyProject
    verifications:
      - DotnetBuild
      - DotnetFormat
      - DotnetTest
      - CheckResult
    meta:
      slackEmoji: ":rocket:"
      color: "#3B82F6"

coworkers:
  - github: username
    name: Display Name
```

### Key Fields

| Field | Description |
|-------|-------------|
| `codingAgent` | The provider agent logic (supported: `claude`, `codex`, `gemini`). |
| `maxConcurrentJobs` | Maximum number of executing agents that can run simultaneously across worktrees. |
| `projects` | Array of tracked git repositories |
| `coworkers` | Team members for assigning AI-created PRs. |

## Verifications

Available verification types to plug into the pipeline:

| Type | Description |
|------|-------------|
| `DotnetBuild` | Runs `dotnet build` |
| `DotnetFormat` | Checks code formatting with `dotnet format` |
| `DotnetTest` | Runs the automated test suite |
| `Npm*` / `Cargo*` | Ecosystem equivalent hooks to validate success |
| `CheckResult` | Internally analyses standard IO to validate agent success parameters |
