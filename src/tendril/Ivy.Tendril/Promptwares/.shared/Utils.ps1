# Ensure claude CLI is on the PATH
$homeDir = $null
if ($IsWindows) { $homeDir = $env:USERPROFILE } else { $homeDir = $env:HOME }
$claudeDir = $null
if ($homeDir) { $claudeDir = Join-Path $homeDir ".local/bin" }
if ($claudeDir -and (Test-Path $claudeDir)) {
    if ($env:PATH -notlike "*$claudeDir*") {
        $sep = if ($IsWindows) { ";" } else { ":" }
        $env:PATH = "$claudeDir$sep$env:PATH"
    }
}

# TENDRIL_HOME is required
if (-not $env:TENDRIL_HOME) {
    Write-Error "TENDRIL_HOME environment variable is not set. Please set it to your Tendril configuration directory."
    exit 1
}

# TENDRIL_CONFIG is required
if (-not $env:TENDRIL_CONFIG) {
    Write-Error "TENDRIL_CONFIG environment variable is not set. This variable must be set by JobService before invoking promptware scripts."
    exit 1
}

$script:ConfigPath = $env:TENDRIL_CONFIG
$script:PlansDir = Join-Path $env:TENDRIL_HOME "Plans"

$script:CachedConfigContent = $null
$script:CachedConfigYaml = $null

# Bootstrap required PowerShell modules
. (Join-Path $PSScriptRoot "Bootstrap-Modules.ps1")

# Resolves the shared folder path. Uses $env:TENDRIL_SHARED if set, otherwise
# falls back to the ".shared" directory relative to the given script root.
# Note: Scripts that need to dot-source Utils.ps1 itself cannot use this helper
# (they need the path before Utils.ps1 is loaded). Those scripts must use the
# inline pattern instead.
function Get-SharedFolder {
    param([string]$ScriptRoot)

    if ($env:TENDRIL_SHARED) {
        return $env:TENDRIL_SHARED
    } else {
        return Join-Path $ScriptRoot ".shared"
    }
}

function GetProgramFolder {
    param([string]$ScriptPath)

    $scriptName = [System.IO.Path]::GetFileNameWithoutExtension($ScriptPath)
    $parentDir = Split-Path $ScriptPath
    $parentDirName = Split-Path $parentDir -Leaf

    # Self-contained layout: script is inside its own program folder
    if ($parentDirName -eq $scriptName) {
        $scriptFolder = $parentDir
    } else {
        # Legacy layout: script is alongside its folder
        $scriptFolder = Join-Path $parentDir $scriptName
    }

    if (-not (Test-Path $scriptFolder)) {
        New-Item -ItemType Directory -Path $scriptFolder | Out-Null
    }
    return $scriptFolder
}

function GetNextLogFile {
    param([string]$ProgramFolder)

    $logsFolder = Join-Path $ProgramFolder "Logs"
    if (-not (Test-Path $logsFolder)) {
        New-Item -ItemType Directory -Path $logsFolder | Out-Null
    }

    $existing = Get-ChildItem -Path $logsFolder -Filter "*.md" -File |
    Where-Object { $_.BaseName -match '^\d+$' } |
    ForEach-Object { [int]$_.BaseName } |
    Sort-Object -Descending |
    Select-Object -First 1

    $next = if ($existing) { $existing + 1 } else { 1 }
    return Join-Path $logsFolder ("{0:D5}.md" -f $next)
}

function PrepareFirmware {
    param(
        [string]$ScriptRoot,
        [string]$LogFile,
        [string]$ProgramFolder,
        [hashtable]$Values = @{}
    )

    # Auto-inject common values
    if (-not $Values.ContainsKey("CurrentTime")) {
        $Values["CurrentTime"] = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    }
    $header = ($Values.GetEnumerator() | Sort-Object Name | ForEach-Object { "$($_.Key): $($_.Value)" }) -join "`n"

    $sharedFolder = Get-SharedFolder $ScriptRoot
    $firmware = Get-Content "$sharedFolder\Firmware.md" -Raw
    $firmware = $firmware.Replace("[HEADER]", $header)
    $firmware = $firmware.Replace("[LOGFILE]", $LogFile)
    $firmware = $firmware.Replace("[PROGRAMFOLDER]", $ProgramFolder)
    $firmware = $firmware.Replace("[SHAREDFOLDER]", $sharedFolder)

    $promptFile = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $promptFile -Value $firmware -NoNewline -Encoding UTF8
    return $promptFile
}

function AllocatePlanId {
    $counterFile = Join-Path $script:PlansDir ".counter"
    if (-not (Test-Path $script:PlansDir)) {
        New-Item -ItemType Directory -Path $script:PlansDir | Out-Null
    }

    # Use a lock file to prevent concurrent access
    $lockFile = Join-Path $script:PlansDir ".counter.lock"
    $lock = $null
    try {
        # Retry acquiring lock for up to 10 seconds
        for ($i = 0; $i -lt 20; $i++) {
            try {
                $lock = [System.IO.File]::Open($lockFile, [System.IO.FileMode]::OpenOrCreate, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
                break
            }
            catch {
                Start-Sleep -Milliseconds 500
            }
        }
        if (-not $lock) {
            Write-Host "Error: Could not acquire counter lock" -ForegroundColor Red
            exit 1
        }

        $counter = if (Test-Path $counterFile) { [int](Get-Content $counterFile).Trim() } else { 1 }
        $id = $counter
        Set-Content -Path $counterFile -Value ($counter + 1).ToString() -Encoding UTF8
        return $id
    }
    finally {
        if ($lock) { $lock.Close() }
    }
}

function UpdatePlanState {
    param(
        [string]$PlanFolderPath,
        [string]$NewState
    )

    $planYamlPath = Join-Path $PlanFolderPath "plan.yaml"
    if (-not (Test-Path $planYamlPath)) {
        Write-Host "plan.yaml not found: $planYamlPath" -ForegroundColor Red
        return
    }

    $content = Get-Content $planYamlPath -Raw
    $now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    $yaml = $content | ConvertFrom-Yaml -Ordered
    $yaml["state"] = $NewState
    $yaml["updated"] = $now
    $content = ConvertTo-Yaml $yaml
    Set-Content -Path $planYamlPath -Value $content -NoNewline -Encoding UTF8
    Write-Host "Plan state updated to: $NewState" -ForegroundColor Cyan
}

function WritePlanLog {
    param(
        [string]$PlanFolderPath,
        [string]$Action,
        [string]$Summary = ""
    )

    $logsDir = Join-Path $PlanFolderPath "logs"
    if (-not (Test-Path $logsDir)) {
        New-Item -ItemType Directory -Path $logsDir | Out-Null
    }

    $existing = Get-ChildItem -Path $logsDir -Filter "*.md" -File |
    ForEach-Object {
        $dashIdx = $_.BaseName.IndexOf('-')
        if ($dashIdx -ge 0) { [int]$_.BaseName.Substring(0, $dashIdx) } else { 0 }
    } |
    Sort-Object -Descending |
    Select-Object -First 1

    $next = if ($existing) { $existing + 1 } else { 1 }
    $logPath = Join-Path $logsDir ("{0:D3}-{1}.md" -f $next, $Action)

    $now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    $logContent = "# $Action`n`n- **Completed:** $now`n- **Status:** Completed"
    if ($Summary) {
        $logContent += "`n`n$Summary"
    }

    Set-Content -Path $logPath -Value $logContent -Encoding UTF8
    Write-Host "Log written: $logPath" -ForegroundColor Green
}

function ValidatePlanPath {
    param([string]$PlanPath)

    if (-not (Test-Path $PlanPath)) {
        Write-Host "Plan folder not found: $PlanPath" -ForegroundColor Red
        exit 1
    }

    $planYamlPath = Join-Path $PlanPath "plan.yaml"
    if (-not (Test-Path $planYamlPath)) {
        Write-Host "plan.yaml not found in: $PlanPath" -ForegroundColor Red
        exit 1
    }

    return $planYamlPath
}

<#
.SYNOPSIS
Reads plan.yaml and extracts project name and parsed YAML structure.

.PARAMETER PlanYamlPath
Absolute path to the plan.yaml file to read

.OUTPUTS
Returns a hashtable with three keys:
- Content: Raw YAML file content as string
- Project: Project name from YAML, or "Auto" if not specified
- Yaml: Parsed YAML as hashtable/ordered dictionary

.EXAMPLE
$planInfo = ReadPlanYaml "D:\Plans\01234-MyPlan\plan.yaml"
$projectName = $planInfo.Project  # "Tendril"
$repos = $planInfo.Yaml.repos     # Array of repo paths
#>
function ReadPlanYaml {
    param([string]$PlanYamlPath)

    $content = Get-Content $PlanYamlPath -Raw
    $yaml = $content | ConvertFrom-Yaml
    $project = if ($yaml.project) { $yaml.project } else { "Auto" }
    return @{ Content = $content; Project = $project; Yaml = $yaml }
}

# ExtractRepoPathsFromYaml — Centralized repo path extraction
#
# Use this function whenever you need to extract repository paths from YAML config.
# Handles both formats:
# - Plain strings: repos: ["D:\Repos\Foo"]
# - Objects: repos: [{ path: "%REPOS_HOME%/Foo", prRule: "yolo" }]
#
# Automatically expands environment variables (%REPOS_HOME%, %TENDRIL_HOME%, etc.)
# and optionally validates paths exist on disk.
#
# Examples:
#   # From plan.yaml (plan repos are plain strings or objects)
#   $repoPaths = ExtractRepoPathsFromYaml -ReposArray $planYaml.repos -ValidateExists
#
#   # From config.yaml project entry
#   $projectConfig = $config.projects | Where-Object { $_.name -eq $project } | Select-Object -First 1
#   $repoPaths = ExtractRepoPathsFromYaml -ReposArray $projectConfig.repos
#
#   # Get first repo path only (for working directory)
#   $workDir = (ExtractRepoPathsFromYaml -ReposArray $projectConfig.repos)[0]
#
function ExtractRepoPathsFromYaml {
    <#
    .SYNOPSIS
    Extracts and resolves repository paths from YAML repo entries.

    .DESCRIPTION
    Centralized utility for extracting repository paths from config.yaml or plan.yaml.
    Handles both plain string entries and object entries with a .path property.
    Automatically expands environment variables (e.g. %REPOS_HOME%).

    .PARAMETER Repos
    Array of repo entries from YAML (can be hashtables with .path or plain strings)

    .PARAMETER ValidateExists
    If true, only returns paths that exist on disk

    .PARAMETER ReturnFirst
    If true, returns only the first valid path (for GetProjectWorkDir compatibility)

    .EXAMPLE
    $repoPaths = ExtractRepoPathsFromYaml $planInfo.Yaml.repos -ValidateExists

    .EXAMPLE
    $projectConfig = $config.projects | Where-Object { $_.name -eq $project } | Select-Object -First 1
    $repoPaths = ExtractRepoPathsFromYaml $projectConfig.repos

    .EXAMPLE
    $workDir = ExtractRepoPathsFromYaml $projectConfig.repos -ReturnFirst
    #>
    param(
        [Parameter(Mandatory = $true)]
        $Repos,

        [switch]$ValidateExists,
        [switch]$ReturnFirst
    )

    if (-not $Repos) {
        if ($ReturnFirst) { return $null }
        return @()
    }

    $paths = @()
    foreach ($repo in $Repos) {
        $p = if ($repo -is [hashtable] -or $repo -is [System.Collections.IDictionary]) {
            $repo.path
        } else {
            "$repo"
        }

        if ($p) {
            $p = [Environment]::ExpandEnvironmentVariables($p)

            if ($ValidateExists) {
                if (Test-Path $p) {
                    $paths += $p
                    if ($ReturnFirst) { return $p }
                }
            } else {
                $paths += $p
                if ($ReturnFirst) { return $p }
            }
        }
    }

    if ($ReturnFirst) { return $null }
    return $paths
}

<#
.SYNOPSIS
Resolves the first repository path for a given project from config.yaml.

.PARAMETER Project
The project name to look up in config.yaml (e.g., "MyProject", "WebApp")

.OUTPUTS
Returns the first repository path for the project from config.yaml, or the current working directory if project not found

.EXAMPLE
$workDir = GetProjectWorkDir "MyProject"
# Returns: the first repo path from MyProject's config

.EXAMPLE
$workDir = GetProjectWorkDir "NonExistent"
# Returns: (Get-Location).Path (falls back to current directory)
#>
function GetProjectWorkDir {
    param([string]$Project)

    $config = Get-ConfigYaml
    if ($config) {
        $projectEntry = $config.projects | Where-Object { $_.name -eq $Project } | Select-Object -First 1
        if ($projectEntry -and $projectEntry.repos -and $projectEntry.repos.Count -gt 0) {
            return ExtractRepoPathsFromYaml $projectEntry.repos -ReturnFirst
        }
    }

    return (Get-Location).Path
}

function Get-ConfigYaml {
    if (-not (Test-Path $script:ConfigPath)) {
        return $null
    }

    $currentContent = Get-Content $script:ConfigPath -Raw

    if ($script:CachedConfigContent -eq $currentContent -and $script:CachedConfigYaml) {
        return $script:CachedConfigYaml
    }

    try {
        $script:CachedConfigYaml = $currentContent | ConvertFrom-Yaml
        $script:CachedConfigContent = $currentContent
        return $script:CachedConfigYaml
    }
    catch {
        Write-Warning "Failed to parse config.yaml: $_"
        return $null
    }
}

<#
.SYNOPSIS
Invokes a Claude agent with firmware preparation, cost tracking, and logging.

.PARAMETER ScriptRoot
Root directory of the calling promptware script (usually $PSScriptRoot)

.PARAMETER ProgramFolder
Program folder path (from GetProgramFolder)

.PARAMETER LogFile
Path to the log file for this session

.PARAMETER FirmwareValues
Hashtable of key-value pairs to inject into firmware header (e.g., @{ PlanId = "01234"; Project = "MyProject" })

.PARAMETER WorkDir
Working directory for the agent (defaults to ProgramFolder)

.PARAMETER PlanPath
Optional path to plan folder (enables plan-specific logging and state updates)

.PARAMETER Action
Optional action name for logging (e.g., "MakePlan", "ExecutePlan")

.PARAMETER FinalState
Optional final state to set in plan.yaml after execution completes (e.g., "Draft", "ReadyForReview")

.PARAMETER ExtraAgentArgs
Optional array of additional arguments to pass to the claude CLI

.PARAMETER Promptware
Promptware name to look up model and allowedTools config overrides (e.g., "MakePlan", "ExecutePlan")

.EXAMPLE
InvokePromptwareAgent `
    -ScriptRoot $PSScriptRoot `
    -ProgramFolder $programFolder `
    -LogFile $logFile `
    -FirmwareValues @{ PlanId = $planId; Project = "MyProject" } `
    -WorkDir $repoPath `
    -PlanPath $planFolder `
    -Action "MakePlan" `
    -FinalState "Draft" `
    -Promptware "MakePlan"
#>
function InvokePromptwareAgent {
    param(
        [string]$ScriptRoot,
        [string]$ProgramFolder,
        [string]$LogFile,
        [hashtable]$FirmwareValues,
        [string]$WorkDir = $ProgramFolder,
        [string]$PlanPath = $null,
        [string]$Action = $null,
        [string]$FinalState = $null,
        [string[]]$ExtraAgentArgs = @(),
        [string]$Promptware = ""
    )

    # Generate session-id for cost tracking
    $sessionId = [guid]::NewGuid().ToString()
    $FirmwareValues["ClaudeSessionId"] = $sessionId

    $promptFile = PrepareFirmware $ScriptRoot $LogFile $ProgramFolder $FirmwareValues
    $agent = GetAgentCommand -Promptware $Promptware

    # Determine coding agent provider
    $codingAgent = $agent.CodingAgent
    $sharedFolder = Get-SharedFolder -ScriptRoot $ScriptRoot
    $invokeCodingAgentScript = Join-Path $sharedFolder "Invoke-CodingAgent.ps1"

    # Create raw output log
    $rawLogFile = [System.IO.Path]::ChangeExtension($LogFile, ".raw.jsonl")

    $agentInfo = $codingAgent
    if ($agent.Model) { $agentInfo += ", model=$($agent.Model)" }
    if ($agent.Effort) { $agentInfo += ", effort=$($agent.Effort)" }
    if ($agent.Arguments -and $agent.Arguments.Count -gt 0) { $agentInfo += ", args=$($agent.Arguments -join ' ')" }
    Write-Host "Starting Agent ($agentInfo)..."
    if ($Action) { SendStatusMessage "Running $Action" }
    Push-Location $WorkDir
    $heartbeat = Start-Heartbeat
    try {
        $startTs = (Get-Date).ToUniversalTime().ToString("o")
        Add-Content -Path $rawLogFile -Value "[tendril] Agent invocation started at $startTs (provider: $codingAgent)" -Encoding UTF8

        if ($codingAgent -ne "claude" -and (Test-Path $invokeCodingAgentScript)) {
            # Use Invoke-CodingAgent.ps1 for non-Claude providers
            $cliName = switch ($codingAgent) {
                "codex" { "Codex" }
                "gemini" { "Gemini" }
                default { "Claude" }
            }

            Add-Content -Path $rawLogFile -Value "[tendril] Command: Invoke-CodingAgent.ps1 -Cli $cliName -Model $($agent.Model) -Effort $($agent.Effort)" -Encoding UTF8

            $output = & $invokeCodingAgentScript `
                -Cli $cliName `
                -Model $agent.Model `
                -Effort $agent.Effort `
                -PromptFile $promptFile `
                -AllowedTools $agent.AllowedTools `
                -SessionId $sessionId `
                -WorkingDirectory $WorkDir `
                -ExtraArgs $ExtraAgentArgs 2>&1 |
            ForEach-Object {
                $line = if ($_ -is [System.Management.Automation.ErrorRecord]) {
                    "[stderr] $_"
                }
                else {
                    "$_"
                }
                Add-Content -Path $rawLogFile -Value $line -Encoding UTF8
                $_
            }
            $output | Write-Output
        }
        else {
            # Use Claude CLI directly (original path)
            if ($agent.Executable -eq "claude") {
                $ExtraAgentArgs += @("--session-id", $sessionId)
            }

            Add-Content -Path $rawLogFile -Value "[tendril] Command: $($agent.Executable) $($agent.Args -join ' ') $($ExtraAgentArgs -join ' ')" -Encoding UTF8

            $output = & $agent.Executable @($agent.Args) @ExtraAgentArgs -- (Get-Content $promptFile -Raw) 2>&1 |
            ForEach-Object {
                $line = if ($_ -is [System.Management.Automation.ErrorRecord]) {
                    "[stderr] $_"
                }
                else {
                    "$_"
                }
                Add-Content -Path $rawLogFile -Value $line -Encoding UTF8
                $_
            }
            $output | Write-Output
        }
    }
    finally {
        Stop-Heartbeat $heartbeat
    }
    Pop-Location

    # Extract summary from agent's stream-json result
    $summary = ""
    if ($output) {
        $resultLine = ($output | Select-String '"type":"result"' | Select-Object -Last 1)
        if ($resultLine) {
            try {
                $resultJson = $resultLine.Line | ConvertFrom-Json
                $summary = $resultJson.result
            }
            catch { }
        }
    }

    # Report cost for this session
    $costData = GetSessionCost $sessionId
    if ($costData -and $PlanPath) {
        LogPlanCost $PlanPath $Action $costData.Tokens $costData.Cost
    }

    if ($Action) { SendStatusMessage "$Action Completed" }

    if ($PlanPath -and $Action) {
        WritePlanLog $PlanPath $Action $summary
    }
    if ($PlanPath -and $FinalState) {
        UpdatePlanState $PlanPath $FinalState
    }

    Remove-Item $promptFile -ErrorAction SilentlyContinue
}

function SendStatusMessage {
    param([string]$Message)

    $jobId = $env:TENDRIL_JOB_ID
    $url = $env:TENDRIL_URL
    if (-not $jobId -or -not $url) { return }

    try {
        $body = @{ message = $Message } | ConvertTo-Json
        Invoke-RestMethod -Uri "$url/api/jobs/$jobId/status" -Method Post -Body $body -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
    }
    catch { }
}

function Start-Heartbeat {
    param([int]$IntervalSeconds = 120)
    $job = Start-ThreadJob -ScriptBlock {
        param($interval)
        while ($true) {
            Start-Sleep -Seconds $interval
            $ts = (Get-Date).ToUniversalTime().ToString("o")
            [Console]::Out.WriteLine("{""type"":""heartbeat"",""timestamp"":""$ts""}")
            [Console]::Out.Flush()
        }
    } -ArgumentList $IntervalSeconds
    return $job
}

function Stop-Heartbeat {
    param($Job)
    if ($Job) {
        Stop-Job $Job -ErrorAction SilentlyContinue
        Remove-Job $Job -ErrorAction SilentlyContinue
    }
}

function GetAgentCommand {
    param([string]$Promptware = "")

    $configPath = $script:ConfigPath
    $allowedTools = @()
    $codingAgent = "claude"
    $model = ""
    $effort = ""
    $extraArgs = @()

    if (Test-Path $configPath) {
        try {
            $config = Get-ConfigYaml

            # Read codingAgent setting (claude|codex|gemini)
            if ($config.codingAgent) {
                $codingAgent = $config.codingAgent.ToLower()
            }

            # Start with _default as base, then overlay specific promptware entry
            $pwConfig = $null
            $defaultConfig = $config.promptwares._default
            $specificConfig = if ($Promptware) { $config.promptwares.$Promptware } else { $null }

            if ($defaultConfig -or $specificConfig) {
                $pwConfig = @{}

                # Layer 1: _default values
                if ($defaultConfig) {
                    if ($defaultConfig.profile) { $pwConfig.profile = $defaultConfig.profile }
                    if ($defaultConfig.allowedTools) { $pwConfig.allowedTools = $defaultConfig.allowedTools }
                }

                # Layer 2: specific promptware values override
                if ($specificConfig) {
                    if ($specificConfig.profile) { $pwConfig.profile = $specificConfig.profile }
                    if ($specificConfig.allowedTools) { $pwConfig.allowedTools = $specificConfig.allowedTools }
                }
            }

            if ($pwConfig) {
                # Apply allowedTools with environment variable expansion
                if ($pwConfig.allowedTools) {
                    foreach ($tool in $pwConfig.allowedTools) {
                        $resolvedTool = [Environment]::ExpandEnvironmentVariables($tool)
                        # Normalize to forward slashes for claude CLI path patterns
                        $resolvedTool = $resolvedTool -replace '\\', '/'
                        $allowedTools += $resolvedTool
                    }
                }
            }

            # Resolve model, effort, and arguments from agent profile if profile is specified
            if ($pwConfig -and $pwConfig.profile -and $config.codingAgents) {
                $agentEntry = $config.codingAgents | Where-Object {
                    ($_.name -eq "ClaudeCode" -and $codingAgent -eq "claude") -or
                    ($_.name -eq "Codex" -and $codingAgent -eq "codex") -or
                    ($_.name -eq "Gemini" -and $codingAgent -eq "gemini") -or
                    ($_.name.ToLower() -eq $codingAgent)
                } | Select-Object -First 1
                if ($agentEntry) {
                    # Base args from agent entry (applied to all profiles)
                    if ($agentEntry.arguments) {
                        $extraArgs += ($agentEntry.arguments -split '\s+' | Where-Object { $_ })
                    }
                    $profile = $agentEntry.profiles | Where-Object { $_.name -eq $pwConfig.profile } | Select-Object -First 1
                    if ($profile) {
                        if ($profile.model) { $model = $profile.model }
                        if ($profile.effort) { $effort = $profile.effort }
                        # Per-profile args appended after base args (can override/extend)
                        if ($profile.arguments) {
                            $extraArgs += ($profile.arguments -split '\s+' | Where-Object { $_ })
                        }
                    }
                }
            }

        }
        catch {
            Write-Host "Warning: Could not parse config.yaml" -ForegroundColor Yellow
        }
    }

    # Build command from codingAgent
    $raw = switch ($codingAgent) {
        "claude"  { "claude --print --verbose --output-format stream-json --dangerously-skip-permissions" }
        "codex"   { "codex --full-auto" }
        "gemini"  { "gemini --sandbox" }
        default   { "claude --print --verbose --output-format stream-json --dangerously-skip-permissions" }
    }

    # Apply model override if set
    if ($model) {
        $raw = $raw -replace '--model\s+\S+', ''
        $raw += " --model $model"
    }

    # Apply effort level with agent-specific flag name
    if ($effort) {
        switch ($codingAgent) {
            "claude" { $raw += " --effort $effort" }
            "codex"  { $raw += " --reasoning-effort $effort" }
            "gemini" { Write-Warning "Effort '$effort' is configured but the Gemini CLI does not support an effort flag — ignoring." }
        }
    }

    # Build args with quote-aware parsing
    $parts = @()
    $current = ""
    $inQuote = $false
    foreach ($char in $raw.ToCharArray()) {
        if ($char -eq '"' -and !$inQuote) { $inQuote = $true; continue }
        elseif ($char -eq '"' -and $inQuote) { $inQuote = $false; continue }
        elseif ($char -eq ' ' -and !$inQuote -and $current) {
            $parts += $current; $current = ""; continue
        }
        elseif ($char -eq ' ' -and !$inQuote) { continue }
        $current += $char
    }
    if ($current) { $parts += $current }

    # Append allowedTools as a single comma-separated argument
    # Note: avoid using $args as it's an automatic variable in PowerShell
    # Only pass --allowedTools to Claude; Codex/Gemini handle tool permissions differently
    $cmdArgs = @($parts[1..($parts.Length - 1)])
    if ($allowedTools.Count -gt 0 -and $codingAgent -eq "claude") {
        $cmdArgs += "--allowedTools"
        $cmdArgs += ($allowedTools -join ",")
    }

    # Append extra arguments from agent/profile config (base args + per-profile args)
    if ($extraArgs.Count -gt 0) {
        $cmdArgs += $extraArgs
    }

    return @{
        Executable   = $parts[0]
        Args         = $cmdArgs
        CodingAgent  = $codingAgent
        Model        = $model
        Effort       = $effort
        AllowedTools = $allowedTools
        Arguments    = $extraArgs
    }
}

function GetSessionCost {
    param([string]$SessionId)

    if (-not $SessionId) { return $null }

    # Currently only Claude tracks session costs via ~/.claude/projects/*.jsonl
    # Codex and Gemini cost tracking will be added as their CLIs mature
    try {
        $tendrilUrl = $env:TENDRIL_URL
        if ($tendrilUrl) {
            $response = Invoke-RestMethod -Uri "$tendrilUrl/api/costs/session/$SessionId" -Method Get -ErrorAction SilentlyContinue
            if ($response) {
                return @{
                    Tokens = $response.totalTokens
                    Cost   = $response.totalCost
                }
            }
        }
    }
    catch { }

    return $null
}

function LogPlanCost {
    param(
        [string]$PlanPath,
        [string]$Action,
        [int]$Tokens,
        [double]$Cost
    )

    if (-not $PlanPath -or (-not $Tokens -and -not $Cost)) { return }

    $costsFile = Join-Path $PlanPath "costs.yaml"

    $entry = @{
        action    = $Action
        timestamp = (Get-Date).ToUniversalTime().ToString("o")
        tokens    = $Tokens
        cost      = [math]::Round($Cost, 4)
    }

    $yamlEntry = "- action: $($entry.action)`n  timestamp: $($entry.timestamp)`n  tokens: $($entry.tokens)`n  cost: $($entry.cost)`n"

    Add-Content -Path $costsFile -Value $yamlEntry -Encoding UTF8
}