param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath,
    [Parameter(Mandatory = $true)]
    [string]$Repo,
    [string]$Assignee = "",
    [string]$Comment = "",
    [string]$Labels = ""
)

. "$PSScriptRoot/.shared/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanProject $planYamlPath

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

InvokePromptwareAgent $PSScriptRoot $programFolder $logFile @{
    Args       = $PlanPath
    PlanFolder = $PlanPath
    Project    = $planInfo.Project
    Repo       = $Repo
    Assignee   = $Assignee
    Comment    = $Comment
    Labels     = $Labels
} -PlanPath $PlanPath -Action "CreateIssue" -Promptware "CreateIssue"
