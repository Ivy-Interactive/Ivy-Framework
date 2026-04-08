param(
    [Parameter(Mandatory = $true)]
    [string]$Description,
    [string]$Project = "[Auto]",
    [string]$SourcePath = ""
)

. "$PSScriptRoot/.shared/Utils.ps1"

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
    Add-Content -Path $rawLogFile -Value "[tendril] Claude invocation started at $startTs" -Encoding UTF8
    Add-Content -Path $rawLogFile -Value "[tendril] Command: $($agent.Executable) $($agent.Args -join ' ') $($extraArgs -join ' ')" -Encoding UTF8

    & $agent.Executable @($agent.Args) @extraArgs -- (Get-Content $promptFile -Raw) 2>&1 |
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
