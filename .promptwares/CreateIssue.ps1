param(
    [Parameter(Mandatory=$true)]
    [string]$PlanPath,
    [Parameter(Mandatory=$true)]
    [string]$Repo,
    [string]$Assignee = "",
    [string]$Comment = ""
)

. "$PSScriptRoot\.shared\Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanProject $planYamlPath

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

InvokePromptwareAgent $PSScriptRoot $programFolder $logFile @{
    Args = $PlanPath
    PlanFolder = $PlanPath
    Project = $planInfo.Project
    Repo = $Repo
    Assignee = $Assignee
    Comment = $Comment
} -PlanPath $PlanPath -Action "CreateIssue"
