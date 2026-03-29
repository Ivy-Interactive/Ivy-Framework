param(
    [Parameter(Mandatory=$true)]
    [string]$PlanPath
)

. "$PSScriptRoot\.shared\Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanProject $planYamlPath

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

# Ensure verification and artifacts dirs exist
$verificationDir = Join-Path $PlanPath "verification"
$artifactsDir = Join-Path $PlanPath "artifacts"
foreach ($dir in @($verificationDir, "$artifactsDir\tests", "$artifactsDir\screenshots", "$artifactsDir\videos", "$artifactsDir\sample")) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}

# Set ARTIFACTS_DIR so Playwright tests can write directly to plan artifacts
$env:ARTIFACTS_DIR = $artifactsDir

InvokePromptwareAgent $PSScriptRoot $programFolder $logFile @{
    Args = $PlanPath
    PlanFolder = $PlanPath
    Project = $planInfo.Project
    VerificationDir = $verificationDir
    ArtifactsDir = $artifactsDir
} -PlanPath $PlanPath -Action "IvyFrameworkVerification"
