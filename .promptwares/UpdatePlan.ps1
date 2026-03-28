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

# Read latest revision to check for >> comments
$revisionsDir = Join-Path $PlanPath "revisions"
$latestRevision = Get-ChildItem -Path $revisionsDir -Filter "*.md" -File | Sort-Object Name -Descending | Select-Object -First 1
if ($latestRevision) {
    $content = Get-Content $latestRevision.FullName -Raw
    if ($content -notmatch '(?m)^\s*>>') {
        Write-Host "No >> comments found in latest revision: $($latestRevision.FullName)" -ForegroundColor Red
        exit 1
    }
}

# Update state to Updating in plan.yaml
UpdatePlanState $PlanPath "Updating"

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

# Read project from plan.yaml
$planYamlContent = Get-Content $planYamlPath -Raw
$projectMatch = [regex]::Match($planYamlContent, '(?m)^project:\s*(.+)$')
$project = if ($projectMatch.Success) { $projectMatch.Groups[1].Value.Trim() } else { "General" }

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder @{ Args = $PlanPath; WorkDir = (Get-Location).Path; PlanFolder = $PlanPath; Project = $project }

$agentCommand = GetAgentCommandFromConfig

Write-Host "Starting Agent..."
Push-Location $programFolder
& $agentCommand --print --output-format stream-json --dangerously-skip-permissions -- (Get-Content $promptFile -Raw)
Pop-Location

# Write log and transition state back to Draft
WritePlanLog $PlanPath "UpdatePlan"
UpdatePlanState $PlanPath "Draft"

Remove-Item $promptFile
