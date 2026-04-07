param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

# Cannot use Get-SharedFolder here since we need this path to dot-source Utils.ps1
# User-defined promptwares use TENDRIL_SHARED to access built-in shared utilities
$sharedDir = if ($env:TENDRIL_SHARED) { $env:TENDRIL_SHARED } else { "$PSScriptRoot/.shared" }
. "$sharedDir/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanYaml $planYamlPath

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

# Ensure verification and artifacts dirs exist
$verificationDir = Join-Path $PlanPath "verification"
$artifactsDir = Join-Path $PlanPath "artifacts"
foreach ($dir in @($verificationDir, "$artifactsDir\tests", "$artifactsDir\screenshots", "$artifactsDir\sample")) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}

# Kill ALL sample processes from any plan's artifacts directory (not just current plan)
# to prevent accumulated zombie processes from blocking builds
Write-Host "Cleaning up any leftover sample processes..." -ForegroundColor Yellow
$killed = 0
Get-Process -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -and $_.Path -match '\\artifacts\\sample\\bin\\'
} | ForEach-Object {
    Write-Host "  Killing: $($_.ProcessName) (PID $($_.Id)) from $($_.Path)" -ForegroundColor Yellow
    try {
        $_ | Stop-Process -Force -ErrorAction Stop
        $killed++
    } catch {
        Write-Warning "  Failed to kill PID $($_.Id): $_"
    }
}
if ($killed -gt 0) {
    Write-Host "  Killed $killed process(es). Waiting for file handles to release..." -ForegroundColor Yellow
    Start-Sleep -Milliseconds 2000
} else {
    Write-Host "  No leftover processes found." -ForegroundColor Green
}

# Set ARTIFACTS_DIR so Playwright tests can write directly to plan artifacts
$env:ARTIFACTS_DIR = $artifactsDir

InvokePromptwareAgent $PSScriptRoot $programFolder $logFile @{
    Args            = $PlanPath
    PlanFolder      = $PlanPath
    Project         = $planInfo.Project
    VerificationDir = $verificationDir
    ArtifactsDir    = $artifactsDir
} -PlanPath $PlanPath -Action "IvyFrameworkVerification" -Promptware "IvyFrameworkVerification"
