param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

. "$PSScriptRoot/.shared/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath

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

$planInfo = ReadPlanProject $planYamlPath

UpdatePlanState $PlanPath "Updating"

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

InvokePromptwareAgent $PSScriptRoot $programFolder $logFile @{
    Args = $PlanPath; PlanFolder = $PlanPath; Project = $planInfo.Project
} -PlanPath $PlanPath -Action "UpdatePlan" -FinalState "Draft" -Promptware "UpdatePlan"
