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
Configure Tendril in the in-app **Settings** UI or by editing `TENDRIL_HOME/config.yaml` (projects, agents, levels, verifications, preferences).
</Ingress>

## Settings app

From Tendril, open setup without hand-editing YAML. Sections include:

- **General** — Default coding agent (`claude`, `codex`, …), max concurrent jobs, Slack emoji / coworkers.
- **Levels** — Complexity tiers (e.g. L1–L3) and how agents weight large vs. small work.
- **Verifications** — Build / test / lint commands agents must satisfy.
- **Promptwares** — Paths to custom promptware folders and tools.
- **Projects** — Repos agents may clone and change.

## `config.yaml`

Same data lives in `TENDRIL_HOME/config.yaml`. Changes in the UI write here immediately.

### Example

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

### Common fields

| Field | Purpose |
|-------|---------|
| `codingAgent` | Agent runtime (`claude`, `codex`, `gemini`, …). |
| `maxConcurrentJobs` | Cap on parallel agent runs (worktrees). |
| `projects` | Registered repositories and their settings. |
| `coworkers` | GitHub users for PR assignment / team features. |

## Verifications

Wire these names into project `verifications` (and define behavior in config as needed):

| Name | Role |
|------|------|
| `DotnetBuild` | `dotnet build` |
| `DotnetFormat` | `dotnet format` checks |
| `DotnetTest` | Test suite |
| `Npm*` / `Cargo*` | Same idea for other stacks |
| `CheckResult` | Parse stdout/stderr to decide pass/fail |
