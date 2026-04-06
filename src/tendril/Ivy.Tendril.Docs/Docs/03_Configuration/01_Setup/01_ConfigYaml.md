---
searchHints:
  - config
  - yaml
  - configuration
  - settings
  - projects
---

# config.yaml

<Ingress>
The `config.yaml` file is the primary configuration file for Tendril. It defines your projects, agent settings, and system preferences.
</Ingress>

## Location

Tendril looks for `config.yaml` in the `TENDRIL_HOME` directory. If it doesn't exist, the onboarding wizard will help you create one.

## Structure

```yaml
agentCommand: claude
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

## Key Fields

### `agentCommand`
The CLI command used to invoke Claude. Typically `claude`.

### `maxConcurrentJobs`
Maximum number of promptware jobs that can run simultaneously.

### `projects`
Array of project configurations. Each project defines:

| Field | Description |
|-------|-------------|
| `name` | Display name for the project |
| `repo` | Absolute path to the repository |
| `verifications` | List of verification steps to run after execution |
| `meta.slackEmoji` | Emoji used in Slack notifications |
| `meta.color` | Color used in the dashboard and badges |

### `coworkers`
List of team members for PR assignment and collaboration features.

## Verifications

Available verification types:

| Type | Description |
|------|-------------|
| `DotnetBuild` | Runs `dotnet build` on the project |
| `DotnetFormat` | Checks code formatting with `dotnet format` |
| `DotnetTest` | Runs the project's test suite |
| `CheckResult` | Validates the agent's execution result |
| `IvyFramework` | Ivy-specific checks (samples, docs) |
