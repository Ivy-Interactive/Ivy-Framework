# Ensure claude CLI is on the PATH
$claudeDir = Join-Path $env:USERPROFILE ".local\bin"
if (Test-Path $claudeDir) {
    if ($env:PATH -notlike "*$claudeDir*") {
        $env:PATH = "$claudeDir;$env:PATH"
    }
}

# Read plans directory from config.yaml
$script:ConfigPath = Join-Path (Split-Path $PSScriptRoot) "config.yaml"
$script:PlansDir = $null
if (Test-Path $script:ConfigPath) {
    try {
        $configYaml = Get-Content $script:ConfigPath -Raw
        # First check explicit planFolder
        $pfMatch = [regex]::Match($configYaml, '(?m)^planFolder:\s*(.+)$')
        if ($pfMatch.Success) {
            $script:PlansDir = $pfMatch.Groups[1].Value.Trim().TrimEnd('\', '/')
        }
        # Otherwise derive from tendrilData
        if (-not $script:PlansDir) {
            $tdMatch = [regex]::Match($configYaml, '(?m)^tendrilData:\s*(.+)$')
            if ($tdMatch.Success) {
                $script:PlansDir = Join-Path $tdMatch.Groups[1].Value.Trim().TrimEnd('\', '/') "Plans"
            }
        }
    }
    catch { }
}
if (-not $script:PlansDir) {
    $script:PlansDir = "D:\Plans"  # fallback
}

function GetProgramFolder {
    param([string]$ScriptPath)

    $scriptName = [System.IO.Path]::GetFileNameWithoutExtension($ScriptPath)
    $scriptFolder = Join-Path (Split-Path $ScriptPath) $scriptName
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
    if (-not $Values.ContainsKey("ConfigPath")) {
        $Values["ConfigPath"] = $script:ConfigPath
    }

    $header = ($Values.GetEnumerator() | Sort-Object Name | ForEach-Object { "$($_.Key): $($_.Value)" }) -join "`n"

    $sharedFolder = Join-Path $ScriptRoot ".shared"
    $firmware = Get-Content "$sharedFolder\Firmware.md" -Raw
    $firmware = $firmware.Replace("[HEADER]", $header)
    $firmware = $firmware.Replace("[LOGFILE]", $LogFile)
    $firmware = $firmware.Replace("[PROGRAMFOLDER]", $ProgramFolder)
    $firmware = $firmware.Replace("[SHAREDFOLDER]", $sharedFolder)

    $promptFile = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $promptFile -Value $firmware -NoNewline
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
            } catch {
                Start-Sleep -Milliseconds 500
            }
        }
        if (-not $lock) {
            Write-Host "Error: Could not acquire counter lock" -ForegroundColor Red
            exit 1
        }

        $counter = if (Test-Path $counterFile) { [int](Get-Content $counterFile).Trim() } else { 1087 }
        $id = $counter
        Set-Content -Path $counterFile -Value ($counter + 1).ToString()
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
    $content = $content -replace '(?m)^state:\s*.*$', "state: $NewState"
    $content = $content -replace '(?m)^updated:\s*.*$', "updated: $now"
    Set-Content -Path $planYamlPath -Value $content
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

    Set-Content -Path $logPath -Value $logContent
    Write-Host "Log written: $logPath" -ForegroundColor Green
}

function CollectArgs {
    param(
        [string[]]$Arguments,
        [switch]$Optional
    )

    $Arguments = $Arguments | Where-Object { $_ -ne $null -and $_.Trim() -ne "" }
    $joined = ($Arguments -join " ").Trim()

    if ($joined -eq "" -and $Optional) {
        return "(No Args)"
    }

    if ($joined -eq "") {
        Write-Host "Error: No arguments provided." -ForegroundColor Red
        exit 1
    }

    return $joined
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

function ReadPlanProject {
    param([string]$PlanYamlPath)

    $content = Get-Content $PlanYamlPath -Raw
    $match = [regex]::Match($content, '(?m)^project:\s*(.+)$')
    $project = if ($match.Success) { $match.Groups[1].Value.Trim() } else { "General" }
    return @{ Content = $content; Project = $project }
}

function GetProjectWorkDir {
    param([string]$Project)

    if (Test-Path $script:ConfigPath) {
        try {
            $yaml = Get-Content $script:ConfigPath -Raw
            # Match the project block and extract the first repo path
            $pattern = "(?s)- name:\s*$([regex]::Escape($Project))\s+repos:\s*\n((?:\s+-.+\n?)+)"
            $match = [regex]::Match($yaml, $pattern)
            if ($match.Success) {
                # Try new format: - path: D:\...
                $pathLine = [regex]::Match($match.Groups[1].Value, '(?m)path:\s*(.+)$')
                if ($pathLine.Success) {
                    return $pathLine.Groups[1].Value.Trim()
                }
                # Fallback: old format - D:\...
                $repoLine = [regex]::Match($match.Groups[1].Value, '^\s+-\s*(.+)$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
                if ($repoLine.Success) {
                    return $repoLine.Groups[1].Value.Trim()
                }
            }
        }
        catch { }
    }

    return (Get-Location).Path
}

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
        [string[]]$ExtraAgentArgs = @()
    )

    $promptFile = PrepareFirmware $ScriptRoot $LogFile $ProgramFolder $FirmwareValues
    $agent = GetAgentCommandFromConfig

    Write-Host "Starting Agent..."
    Push-Location $WorkDir
    $output = & $agent.Executable @($agent.Args) @ExtraAgentArgs -- (Get-Content $promptFile -Raw)
    $output | Write-Output
    Pop-Location

    # Extract summary from agent's stream-json result
    $summary = ""
    if ($output) {
        $resultLine = ($output | Select-String '"type":"result"' | Select-Object -Last 1)
        if ($resultLine) {
            try {
                $resultJson = $resultLine.Line | ConvertFrom-Json
                $summary = $resultJson.result
            } catch { }
        }
    }

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
    } catch { }
}

function GetAgentCommandFromConfig {
    $configPath = Join-Path (Split-Path $PSScriptRoot) "config.yaml"
    $raw = "claude --print --verbose --output-format stream-json --dangerously-skip-permissions"

    if (Test-Path $configPath) {
        try {
            $yaml = Get-Content $configPath -Raw
            $pattern = "(?m)^agentCommand:\s*(.+)$"
            $match = [regex]::Match($yaml, $pattern)
            if ($match.Success) {
                $raw = $match.Groups[1].Value.Trim()
            }
        }
        catch {
            Write-Host "Warning: Could not parse agentCommand from config.yaml" -ForegroundColor Yellow
        }
    }

    # Split into executable and args
    $parts = $raw -split '\s+', 2
    return @{
        Executable = $parts[0]
        Args = if ($parts.Length -gt 1) { $parts[1] -split '\s+' } else { @() }
    }
}
