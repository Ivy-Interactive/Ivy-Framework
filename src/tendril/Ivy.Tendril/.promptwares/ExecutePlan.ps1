param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

. "$PSScriptRoot/.shared/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanProject $planYamlPath

# Verify plan is in Building state
$stateMatch = [regex]::Match($planInfo.Content, '(?m)^state:\s*(.+)$')
$currentState = if ($stateMatch.Success) { $stateMatch.Groups[1].Value.Trim() } else { "Unknown" }

if ($currentState -ne "Building") {
    Write-Host "Plan is not in Building state (current: $currentState): $PlanPath" -ForegroundColor Red
    exit 1
}

UpdatePlanState $PlanPath "Executing"

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

$workDir = GetProjectWorkDir $planInfo.Project

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder @{
    Args       = $PlanPath
    PlanFolder = $PlanPath
    Project    = $planInfo.Project
}

$agent = GetAgentCommandFromConfig -Promptware "ExecutePlan"
$sessionId = [guid]::NewGuid().ToString()

Write-Host "Starting Agent in $workDir..."
SendStatusMessage "Executing Plan"
Push-Location $workDir

$rawLogFile = [System.IO.Path]::ChangeExtension($logFile, ".raw.jsonl")

$heartbeat = Start-Heartbeat
try {
    $extraArgs = @()
    if ($agent.Executable -eq "claude") {
        $extraArgs += @("--session-id", $sessionId)
    }

    $startTs = (Get-Date).ToUniversalTime().ToString("o")
    Add-Content -Path $rawLogFile -Value "[tendril] Claude invocation started at $startTs" -Encoding UTF8
    Add-Content -Path $rawLogFile -Value "[tendril] Command: $($agent.Executable) $($agent.Args -join ' ') $($extraArgs -join ' ')" -Encoding UTF8

    $output = & $agent.Executable @($agent.Args) @extraArgs -- (Get-Content $promptFile -Raw) 2>&1 |
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
    $exitCode = $LASTEXITCODE

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

    if ($exitCode -eq 0) {
        SendStatusMessage "Checking Verifications"
        WritePlanLog $PlanPath "ExecutePlan" $summary

        # Check verification statuses before transitioning
        $planYaml = Get-Content (Join-Path $PlanPath "plan.yaml") -Raw
        $verificationStatuses = [regex]::Matches($planYaml, '(?m)^\s+status:\s*(.+)$') |
        ForEach-Object { $_.Groups[1].Value.Trim() }

        $hasFailed = $verificationStatuses | Where-Object { $_ -eq "Fail" }
        $hasPending = $verificationStatuses | Where-Object { $_ -eq "Pending" }

        if ($hasFailed -or $hasPending) {
            UpdatePlanState $PlanPath "Failed"
            Write-Host "Plan has incomplete or failed verifications" -ForegroundColor Red
            exit 1  # Signal failure to JobService
        }
        else {
            UpdatePlanState $PlanPath "ReadyForReview"
            Write-Host "Plan execution completed - ready for review" -ForegroundColor Green
        }
    }
    else {
        SendStatusMessage "Execution failed (exit code: $exitCode)"
        WritePlanLog $PlanPath "ExecutePlan-Failed" $summary
        UpdatePlanState $PlanPath "Failed"
        Write-Host "Plan execution failed with exit code: $exitCode" -ForegroundColor Red
    }
}
catch {
    WritePlanLog $PlanPath "ExecutePlan-Error" "$_"
    UpdatePlanState $PlanPath "Failed"
    Write-Host "Plan execution error: $_" -ForegroundColor Red
    throw
}
finally {
    Stop-Heartbeat $heartbeat
    Pop-Location
    Remove-Item $promptFile -ErrorAction SilentlyContinue
}
