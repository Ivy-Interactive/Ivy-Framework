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

### 3. Run Tendril

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

