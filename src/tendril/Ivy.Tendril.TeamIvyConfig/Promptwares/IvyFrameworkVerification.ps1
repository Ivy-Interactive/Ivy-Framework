param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

# User-defined promptwares use TENDRIL_SHARED to access built-in shared utilities
$sharedDir = if ($env:TENDRIL_SHARED) { $env:TENDRIL_SHARED } else { "$PSScriptRoot/.shared" }
. "$sharedDir/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanProject $planYamlPath

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

# Kill any leftover sample processes that may lock DLLs
$sampleBinDir = Join-Path $artifactsDir "sample" "bin"
if (Test-Path $sampleBinDir) {
    Get-Process | Where-Object {
        $_.Path -and $_.Path.StartsWith($sampleBinDir, [StringComparison]::OrdinalIgnoreCase)
    } | ForEach-Object {
        Write-Host "Killing leftover process: $($_.ProcessName) ($($_.Id))" -ForegroundColor Yellow
        $_ | Stop-Process -Force -ErrorAction SilentlyContinue
    }
    # Brief wait for file handles to release
    Start-Sleep -Milliseconds 500
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
