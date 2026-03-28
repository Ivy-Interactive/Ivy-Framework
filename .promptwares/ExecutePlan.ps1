param(
    [Parameter(Mandatory=$true)]
    [string]$PlanPath
)

. "$PSScriptRoot\.shared\Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath

# PlanPath is a folder path
if (-not (Test-Path $PlanPath)) {
    Write-Host "Plan folder not found: $PlanPath" -ForegroundColor Red
    exit 1
}

$planYamlPath = Join-Path $PlanPath "plan.yaml"
if (-not (Test-Path $planYamlPath)) {
    Write-Host "plan.yaml not found in: $PlanPath" -ForegroundColor Red
    exit 1
}

# Verify plan is in Approved state
$planYamlContent = Get-Content $planYamlPath -Raw
$stateMatch = [regex]::Match($planYamlContent, '(?m)^state:\s*(.+)$')
$currentState = if ($stateMatch.Success) { $stateMatch.Groups[1].Value.Trim() } else { "Unknown" }

if ($currentState -ne "Approved") {
    Write-Host "Plan is not in Approved state (current: $currentState): $PlanPath" -ForegroundColor Red
    exit 1
}

# Update state to Executing in plan.yaml
UpdatePlanState $PlanPath "Executing"

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

# Read project from plan.yaml
$projectMatch = [regex]::Match($planYamlContent, '(?m)^project:\s*(.+)$')
$project = if ($projectMatch.Success) { $projectMatch.Groups[1].Value.Trim() } else { "General" }

# Determine working directory based on project
$workDir = switch ($project) {
    "IvyFramework" { "D:\Repos\_Ivy\Ivy-Framework" }
    "IvyAgent" { "D:\Repos\_Ivy\Ivy-Agent" }
    "IvyConsole" { "D:\Repos\_Ivy\Ivy" }
    "IvyMcp" { "D:\Repos\_Ivy\Ivy-Mcp" }
    "Scripts" { "D:\Repos\_Personal\Scripts" }
    "Tendril" { "D:\Repos\_Ivy\Ivy-Tendril" }
    default { "D:\Repos\_Ivy" }
}

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder @{
    Args = $PlanPath
    WorkDir = $workDir
    PlanFolder = $PlanPath
    Project = $project
}

$agent = GetAgentCommandFromConfig

Write-Host "Starting Agent in $workDir..."
Push-Location $workDir

try {
    & $agent.Executable @($agent.Args) -- (Get-Content $promptFile -Raw)
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        # Success - update state to Completed
        WritePlanLog $PlanPath "ExecutePlan"
        UpdatePlanState $PlanPath "Completed"
        Write-Host "Plan execution completed successfully" -ForegroundColor Green
    } else {
        # Failure - update state to Failed
        WritePlanLog $PlanPath "ExecutePlan-Failed"
        UpdatePlanState $PlanPath "Failed"
        Write-Host "Plan execution failed with exit code: $exitCode" -ForegroundColor Red
    }
}
catch {
    # Error - update state to Failed
    WritePlanLog $PlanPath "ExecutePlan-Error"
    UpdatePlanState $PlanPath "Failed"
    Write-Host "Plan execution error: $_" -ForegroundColor Red
    throw
}
finally {
    Pop-Location
    Remove-Item $promptFile -ErrorAction SilentlyContinue
}
