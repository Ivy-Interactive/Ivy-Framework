param(
    [string]$Project = "[Auto]"
)

. "$PSScriptRoot\.shared\Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath

$args = CollectArgs $args

# Get project context from config if a specific project is selected
$projectContext = GetProjectContextFromConfig $Project

# Create plan folder structure with project
$planFolder = CreatePlanFolder $args $Project $projectContext

$logFile = GetNextLogFile $programFolder
$args | Set-Content $logFile
Write-Host "Log file: $logFile"

$sessionId = [guid]::NewGuid().ToString()

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder @{ Args = $args; WorkDir = (Get-Location).Path; ClaudeSessionId = $sessionId; PlanFolder = $planFolder; Project = $Project }

$agent = GetAgentCommandFromConfig

Write-Host "Starting Agent..."
Push-Location $programFolder
$extraArgs = @()
if ($agent.Executable -eq "claude") {
    $extraArgs += @("--session-id", $sessionId)
}
& $agent.Executable @($agent.Args) @extraArgs -- (Get-Content $promptFile -Raw)
Pop-Location

# Write log to plan's logs folder
WritePlanLog $planFolder "MakePlan"

Remove-Item $promptFile
