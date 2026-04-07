param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

. "$PSScriptRoot/.shared/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanYaml $planYamlPath

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder @{
    Args = $PlanPath; PlanFolder = $PlanPath; Project = $planInfo.Project
}

$agent = GetAgentCommand -Promptware "MakePr"
$sessionId = $env:TENDRIL_SESSION_ID
if (-not $sessionId) {
    $sessionId = [guid]::NewGuid().ToString()
    Write-Warning "TENDRIL_SESSION_ID not set, generated fallback: $sessionId"
}

Write-Host "Starting Agent..."
Push-Location $programFolder

$heartbeat = Start-Heartbeat
try {
    $extraArgs = @()
    if ($agent.Executable -eq "claude") {
        $extraArgs += @("--session-id", $sessionId)
    }
    $rawLogFile = [System.IO.Path]::ChangeExtension($logFile, ".raw.jsonl")
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
        WritePlanLog $PlanPath "MakePr" $summary
        UpdatePlanState $PlanPath "Completed"
        Write-Host "MakePr completed successfully" -ForegroundColor Green
    }
    else {
        WritePlanLog $PlanPath "MakePr-Failed" $summary
        UpdatePlanState $PlanPath "ReadyForReview"
        Write-Host "MakePr failed with exit code: $exitCode — plan returned to ReadyForReview" -ForegroundColor Red
    }
}
catch {
    WritePlanLog $PlanPath "MakePr-Error" "$_"
    UpdatePlanState $PlanPath "ReadyForReview"
    Write-Host "MakePr error: $_ — plan returned to ReadyForReview" -ForegroundColor Red
    throw
}
finally {
    Stop-Heartbeat $heartbeat
    Pop-Location
    Remove-Item $promptFile -ErrorAction SilentlyContinue
}
