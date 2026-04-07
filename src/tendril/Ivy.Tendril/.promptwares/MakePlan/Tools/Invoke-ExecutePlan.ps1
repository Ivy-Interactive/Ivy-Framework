param(
    [Parameter(Mandatory=$true)]
    [string]$PlanPath
)

# Validate plan path exists
if (-not (Test-Path $PlanPath)) {
    Write-Host "FAIL: Plan path not found: $PlanPath" -ForegroundColor Red
    exit 1
}

# Get the ExecutePlan.ps1 script path (relative to .promptwares root)
$promptwaresRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$executePlanScript = Join-Path $promptwaresRoot "ExecutePlan.ps1"

if (-not (Test-Path $executePlanScript)) {
    Write-Host "FAIL: ExecutePlan.ps1 not found at: $executePlanScript" -ForegroundColor Red
    exit 1
}

Write-Host "Invoking ExecutePlan for: $PlanPath" -ForegroundColor Cyan

try {
    # Invoke ExecutePlan.ps1 with the plan path
    & $executePlanScript -PlanPath $PlanPath
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Write-Host "ExecutePlan completed successfully" -ForegroundColor Green
    } else {
        Write-Host "ExecutePlan failed with exit code: $exitCode" -ForegroundColor Red
    }

    exit $exitCode
}
catch {
    Write-Host "ERROR invoking ExecutePlan: $_" -ForegroundColor Red
    exit 1
}
