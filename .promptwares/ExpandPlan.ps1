param(
    [Parameter(Mandatory=$true)]
    [string]$PlanPath
)

. "$PSScriptRoot\.shared\Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath

# PlanPath is now a folder path
if (-not (Test-Path $PlanPath)) {
    Write-Host "Plan folder not found: $PlanPath" -ForegroundColor Red
    exit 1
}

$planYamlPath = Join-Path $PlanPath "plan.yaml"
if (-not (Test-Path $planYamlPath)) {
    Write-Host "plan.yaml not found in: $PlanPath" -ForegroundColor Red
    exit 1
}

# Update state to Building
UpdatePlanState $PlanPath "Building"

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

# Read project from plan.yaml
$planYamlContent = Get-Content $planYamlPath -Raw
$projectMatch = [regex]::Match($planYamlContent, '(?m)^project:\s*(.+)$')
$project = if ($projectMatch.Success) { $projectMatch.Groups[1].Value.Trim() } else { "General" }

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder @{ Args = $PlanPath; WorkDir = (Get-Location).Path; PlanFolder = $PlanPath; Project = $project }

$agent = GetAgentCommandFromConfig

Write-Host "Starting Agent..."
Push-Location $programFolder
& $agent.Executable @($agent.Args) -- (Get-Content $promptFile -Raw)
Pop-Location

# Write log and transition state back to Draft
WritePlanLog $PlanPath "ExpandPlan"
UpdatePlanState $PlanPath "Draft"

Remove-Item $promptFile
