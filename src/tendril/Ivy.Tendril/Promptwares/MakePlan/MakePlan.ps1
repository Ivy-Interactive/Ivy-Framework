param(
    [Parameter(Mandatory = $true)]
    [string]$Description,
    [string]$Project = "[Auto]",
    [string]$SourcePath = "",
    [int]$Priority = 0
)

. "$PSScriptRoot/../.shared/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath

$logFile = GetNextLogFile $programFolder
$Description | Set-Content $logFile
Write-Host "Log file: $logFile"

$sessionId = $env:TENDRIL_SESSION_ID
if (-not $sessionId) {
    $sessionId = [guid]::NewGuid().ToString()
    Write-Warning "TENDRIL_SESSION_ID not set, generated fallback: $sessionId"
}
$planId = AllocatePlanId

$firmwareValues = @{
    Args            = $Description
    ClaudeSessionId = $sessionId
    PlanId          = ("{0:D5}" -f $planId)
    PlansDirectory  = $script:PlansDir
    Project         = $Project
}
if ($SourcePath) { $firmwareValues["SourcePath"] = $SourcePath }
if ($Priority -ne 0) { $firmwareValues["Priority"] = $Priority }

# Parse multi-project selection and aggregate repos for overlap detection
$repos = @()
if ($Project -ne "[Auto]") {
    $projectNames = $Project -split ',' | ForEach-Object { $_.Trim() }

    if (Test-Path $script:ConfigPath) {
        try {
            $config = Get-Content $script:ConfigPath -Raw | ConvertFrom-Yaml

            foreach ($projName in $projectNames) {
                $projectEntry = $config.projects | Where-Object { $_.name -eq $projName } | Select-Object -First 1
                if ($projectEntry -and $projectEntry.repos) {
                    $projectRepos = ExtractRepoPathsFromYaml $projectEntry.repos
                    $repos += $projectRepos
                }
            }

            $repos = $repos | Select-Object -Unique
        }
        catch {
            Write-Warning "Failed to parse config.yaml for multi-project repos: $_"
        }
    }
}

# Pre-compute duplicate detection and active plans (skip duplicates if FORCE flag)
if ($Description -notmatch '\[FORCE\]') {
    $keywords = ($Description -replace '\[.*?\]', '' -split '\s+') | Where-Object { $_.Length -ge 3 }
    if ($keywords) {
        $duplicates = & "$programFolder/Tools/Find-DuplicatePlans.ps1" `
            -PlansDirectory $script:PlansDir -Keywords $keywords -Project $Project
        if ($duplicates) {
            $firmwareValues["DuplicateCandidates"] = $duplicates
        }
    }
}

$activePlans = & "$programFolder/Tools/Find-ActivePlans.ps1" `
    -PlansDirectory $script:PlansDir -Repos $repos
if ($activePlans) {
    $firmwareValues["ActivePlans"] = $activePlans
}

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder $firmwareValues

$agent = GetAgentCommand -Promptware "MakePlan"

Write-Host "Starting Agent..."
SendStatusMessage "Creating Plan"
Push-Location $programFolder
$extraArgs = @()
if ($agent.Executable -eq "claude") {
    $extraArgs += @("--session-id", $sessionId)
}
$heartbeat = Start-Heartbeat
try {
    $rawLogFile = [System.IO.Path]::ChangeExtension($logFile, ".raw.jsonl")
    $startTs = (Get-Date).ToUniversalTime().ToString("o")
    Add-Content -Path $rawLogFile -Value "[tendril] Agent invocation started at $startTs (provider: $($agent.CodingAgent))" -Encoding UTF8
    Add-Content -Path $rawLogFile -Value "[tendril] Command: $($agent.Executable) $($agent.Args -join ' ') $($extraArgs -join ' ')" -Encoding UTF8

    $promptContent = Get-Content $promptFile -Raw
    $agentArgs = if ($agent.CodingAgent -eq "claude") {
        @($agent.Args) + $extraArgs + @("--", $promptContent)
    } else {
        @($agent.Args) + $extraArgs + @($promptContent)
    }
    & $agent.Executable @agentArgs 2>&1 |
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
}
finally {
    Stop-Heartbeat $heartbeat
}
Pop-Location

Remove-Item $promptFile

# Wait for any buffered file writes to complete
Start-Sleep -Milliseconds 200

# Verify the agent actually created a plan folder or a trash entry (duplicate)
$planIdFormatted = "{0:D5}" -f $planId
$planFolder = Get-ChildItem -Path $script:PlansDir -Filter "$planIdFormatted-*" -Directory | Select-Object -First 1
if ($planFolder) {
    Write-Host "Plan created: $($planFolder.Name)" -ForegroundColor Green
    if ($Priority -ne 0) {
        $planYamlPath = Join-Path $planFolder.FullName "plan.yaml"
        if (Test-Path $planYamlPath) {
            $content = Get-Content $planYamlPath -Raw
            if ($content -match '(?m)^priority:\s') {
                $content = $content -replace '(?m)^priority:\s.*$', "priority: $Priority"
            } else {
                $content = $content -replace '(?m)^(level:\s)', "priority: $Priority`n`$1"
            }
            Set-Content $planYamlPath $content -NoNewline
            Write-Host "Set priority: $Priority" -ForegroundColor Cyan
        }
    }
}
else {
    # Check if it was a duplicate (written to Trash)
    $trashDir = if ($env:TENDRIL_HOME) { Join-Path $env:TENDRIL_HOME "Trash" } else { $null }
    if (-not $trashDir) {
        Write-Host "WARNING: TENDRIL_HOME environment variable not set" -ForegroundColor Yellow
    }
    elseif (-not (Test-Path $trashDir)) {
        Write-Host "WARNING: Trash directory does not exist: $trashDir" -ForegroundColor Yellow
    }

    $trashEntry = if ($trashDir -and (Test-Path $trashDir)) {
        Get-ChildItem -Path $trashDir -Filter "$planIdFormatted-*" | Select-Object -First 1
    }
    else { $null }

    if ($trashEntry) {
        Write-Host "Plan $planIdFormatted was identified as duplicate: $($trashEntry.Name)" -ForegroundColor Yellow
    }
    else {
        Write-Host "ERROR: Plan $planIdFormatted was not created. No plan folder or trash entry found." -ForegroundColor Red
        exit 1
    }
}
