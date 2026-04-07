param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

. "$PSScriptRoot/.shared/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanYaml $planYamlPath

UpdatePlanState $PlanPath "Building"

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

InvokePromptwareAgent $PSScriptRoot $programFolder $logFile @{
    Args = $PlanPath; PlanFolder = $PlanPath; Project = $planInfo.Project
} -PlanPath $PlanPath -Action "ExpandPlan" -FinalState "Draft" -Promptware "ExpandPlan"
