# Team Ivy Config - Tendril Configuration

This folder contains the Team Ivy configuration for Tendril, a self-contained configuration package that can be used by pointing `TENDRIL_HOME` to this directory.

## Quick Setup

### 1. Set Environment Variables

**Windows (PowerShell):**
```powershell
# Set permanently
[System.Environment]::SetEnvironmentVariable('TENDRIL_HOME', 'D:\Repos\_Ivy\Ivy-Framework\src\tendril\Ivy.Tendril.TeamIvyConfig', 'User')
[System.Environment]::SetEnvironmentVariable('REPOS_HOME', 'D:\Repos\_Ivy', 'User')

# Or set for current session only
$env:TENDRIL_HOME = "D:\Repos\_Ivy\Ivy-Framework\src\tendril\Ivy.Tendril.TeamIvyConfig"
$env:REPOS_HOME = "D:\Repos\_Ivy"
```

**macOS/Linux (Bash/Zsh):**
```bash
# Add to ~/.bashrc or ~/.zshrc for persistence
export TENDRIL_HOME=~/repos/Ivy-Framework/src/tendril/Ivy.Tendril.TeamIvyConfig
export REPOS_HOME=~/repos

# Or set for current session only
export TENDRIL_HOME=~/repos/Ivy-Framework/src/tendril/Ivy.Tendril.TeamIvyConfig
export REPOS_HOME=~/repos
```

### 2. Configure User Secrets (Optional - for LLM integration)

If you want to use the LLM features, configure your API credentials using .NET user secrets:

```bash
# Navigate to this directory
cd D:\Repos\_Ivy\Ivy-Framework\src\tendril\Ivy.Tendril.TeamIvyConfig

# Set your OpenAI credentials
dotnet user-secrets set "OpenAi:Endpoint" "https://api.openai.com/v1"
dotnet user-secrets set "OpenAi:ApiKey" "sk-your-api-key-here"

# Verify secrets are set
dotnet user-secrets list
```

**Note:** The `TeamIvyConfig.csproj` file in this folder contains the `UserSecretsId` needed for .NET user secrets to work.

### 3. Configure Slack Notifications (Optional)

Tendril can send Slack notifications when PRs are created using the `notify` CLI tool and hook scripts.

#### Install notify CLI

Install the `notify` CLI tool as a .NET global tool:

```bash
dotnet tool install -g notify.console
```

#### Configure Slack Webhook

1. Create a Slack app and incoming webhook by following the [Slack webhooks guide](https://api.slack.com/messaging/webhooks).
2. Copy the webhook URL for your target channel.
3. Configure a profile in the `notify` tool for your channel (e.g. `done-by-niels`):

```bash
notify config slack done-by-niels --webhook-url "https://hooks.slack.com/services/YOUR/WEBHOOK/URL"
```

The profile name should match the `$slackChannel` variable in `Hooks/NotifySlack.ps1`.

#### Testing

Verify the setup by sending a test message:

```bash
notify slack done-by-niels --message "Test message from Tendril"
```

#### How it works

The `config.yaml` `hooks` section defines a `SlackNotify` hook that runs **after** the `MakePr` promptware completes:

```yaml
hooks:
- name: SlackNotify
  when: after
  promptwares:
  - MakePr
  action: pwsh -NoProfile -File Hooks/NotifySlack.ps1
```

The `NotifySlack.ps1` script receives these environment variables:
- `TENDRIL_PLAN_FOLDER` — path to the plan being processed
- `TENDRIL_JOB_STATUS` — the job's completion status
- `TENDRIL_CONFIG` — path to config.yaml

It reads `plan.yaml` to extract the plan title, project name, and PR URLs, then sends a formatted Slack message that includes:
- Plan title and project name (with the project's `slackEmoji` from config)
- Clickable PR links
- A screenshot from `artifacts/screenshots/` if one exists (uploaded to blob storage)

### 4. Run Tendril

```bash
# From the Ivy.Tendril application directory
cd D:\Repos\_Ivy\Ivy-Framework\src\tendril\Ivy.Tendril
dotnet run
```

Tendril will automatically:
- Load `config.yaml` from `%TENDRIL_HOME%` (this folder)
- Load user secrets from this folder's `.csproj`
- Use this folder for Tendril data storage
- Expand `%REPOS_HOME%` variables in the config to your repos location

