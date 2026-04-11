param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath,

    [Parameter(Mandatory = $false)]
    [string]$Note = ""
)

. "$PSScriptRoot/../.shared/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanYaml $planYamlPath

# Verify plan is in Building state
$currentState = if ($planInfo.Yaml.state) { $planInfo.Yaml.state } else { "Unknown" }

if ($currentState -ne "Building") {
    Write-Host "Plan is not in Building state (current: $currentState): $PlanPath" -ForegroundColor Red
    exit 1
}

UpdatePlanState $PlanPath "Executing"

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

$workDir = GetProjectWorkDir $planInfo.Project

$firmwareValues = @{
    Args       = $PlanPath
    PlanFolder = $PlanPath
    Project    = $planInfo.Project
}
if ($Note) {
    $firmwareValues["Note"] = $Note
}

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder $firmwareValues

$agent = GetAgentCommand -Promptware "ExecutePlan"
$sessionId = $env:TENDRIL_SESSION_ID
if (-not $sessionId) {
    $sessionId = [guid]::NewGuid().ToString()
    Write-Warning "TENDRIL_SESSION_ID not set, generated fallback: $sessionId"
}

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
    Add-Content -Path $rawLogFile -Value "[tendril] Agent invocation started at $startTs (provider: $($agent.CodingAgent))" -Encoding UTF8
    Add-Content -Path $rawLogFile -Value "[tendril] Command: $($agent.Executable) $($agent.Args -join ' ') $($extraArgs -join ' ')" -Encoding UTF8

    # Claude uses -- separator; Codex/Gemini take prompt as positional argument
    $promptContent = Get-Content $promptFile -Raw
    $agentArgs = if ($agent.CodingAgent -eq "claude") {
        @($agent.Args) + $extraArgs + @("--", $promptContent)
    }
    else {
        @($agent.Args) + $extraArgs + @($promptContent)
    }
    $output = & $agent.Executable @agentArgs 2>&1 |
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
        elseif ($agent.CodingAgent -ne "claude") {
            # Non-Claude agents don't emit stream-json; use last non-empty line as summary
            $summary = ($output | Where-Object { "$_".Trim() } | Select-Object -Last 1) -as [string]
        }
    }

    if ($exitCode -eq 0) {
        SendStatusMessage "Checking Verifications"
        WritePlanLog $PlanPath "ExecutePlan" $summary

        # Check verification statuses before transitioning
        $planYamlContent = Get-Content (Join-Path $PlanPath "plan.yaml") -Raw
        $planYamlParsed = $planYamlContent | ConvertFrom-Yaml
        $verificationStatuses = @()
        if ($planYamlParsed.verifications) {
            $verificationStatuses = $planYamlParsed.verifications | ForEach-Object { $_.status }
        }

        $failedNames = @()
        $pendingNames = @()
        foreach ($v in $planYamlParsed.verifications) {
            if ($v.status -eq "Fail") { $failedNames += $v.name }
            if ($v.status -eq "Pending") { $pendingNames += $v.name }
        }

        if ($failedNames.Count -gt 0 -or $pendingNames.Count -gt 0) {
            UpdatePlanState $PlanPath "Failed"
            if ($failedNames.Count -gt 0) {
                Write-Host "Failed verifications: $($failedNames -join ', ')" -ForegroundColor Red
            }
            if ($pendingNames.Count -gt 0) {
                Write-Host "Incomplete verifications (still Pending): $($pendingNames -join ', ')" -ForegroundColor Yellow
            }
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

    if (-not $env:KEEP_WORKTREES) {
        Write-Host "Cleaning up worktrees..." -ForegroundColor Gray
        try {
            $cleanupScript = Join-Path $PSScriptRoot "Tools" "Cleanup-Worktrees.ps1"
            & $cleanupScript -PlanPath $PlanPath
        } catch {
            Write-Warning "Worktree cleanup failed (non-fatal): $_"
        }
    } else {
        Write-Host "KEEP_WORKTREES is set - skipping worktree cleanup" -ForegroundColor Gray
    }
}
