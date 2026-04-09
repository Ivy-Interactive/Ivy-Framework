# CleanupWorktrees.ps1 — Periodic cleanup of stale worktrees from completed plans
# Run via Task Scheduler (e.g. weekly) or manually:
#   pwsh -NoProfile -File %TENDRIL_HOME%/Hooks/CleanupWorktrees.ps1
#
# Optional: -MaxAgeDays <int> (default: 7)

param(
    [int]$MaxAgeDays = 7
)

$ErrorActionPreference = "Continue"

# Set TENDRIL_CONFIG if not already set (for standalone execution)
if (-not $env:TENDRIL_CONFIG -and $env:TENDRIL_HOME) {
    $env:TENDRIL_CONFIG = Join-Path $env:TENDRIL_HOME "config.yaml"
}

# Bootstrap shared utilities (includes Bootstrap-Modules.ps1 and ExtractRepoPathsFromYaml)
$sharedPath = Join-Path (Split-Path (Split-Path $PSScriptRoot)) "Ivy.Tendril/Promptwares/.shared"
. (Join-Path $sharedPath "Utils.ps1")

$plansDir = Join-Path $env:TENDRIL_HOME "Plans"
if (-not (Test-Path $plansDir)) {
    Write-Host "Plans directory not found: $plansDir" -ForegroundColor Yellow
    exit 0
}

$terminalStates = @("Completed", "Failed", "Skipped", "Icebox")
$cutoffDate = (Get-Date).AddDays(-$MaxAgeDays)
$cleanedTotal = 0
$failedTotal = 0

$planFolders = Get-ChildItem -Path $plansDir -Directory | Where-Object { $_.Name -match '^\d{5}-' }

foreach ($planFolder in $planFolders) {
    $worktreesDir = Join-Path $planFolder.FullName "worktrees"
    if (-not (Test-Path $worktreesDir)) { continue }

    $planYamlPath = Join-Path $planFolder.FullName "plan.yaml"
    if (-not (Test-Path $planYamlPath)) { continue }

    $planContent = Get-Content $planYamlPath -Raw
    $yaml = $planContent | ConvertFrom-Yaml

    # Check state
    $state = if ($yaml.state) { $yaml.state } else { "Unknown" }
    if ($state -notin $terminalStates) { continue }

    # Check age (use updated timestamp)
    if ($yaml.updated) {
        $updated = [datetime]::Parse("$($yaml.updated)")
        if ($updated -gt $cutoffDate) { continue }
    }

    # Extract plan ID
    $planId = if ($planFolder.Name -match '^(\d+)') { $Matches[1] } else { "" }

    # Extract repo paths
    $repoPaths = ExtractRepoPathsFromYaml -ReposArray $yaml.repos -ValidateExists

    Write-Host "Cleaning plan $($planFolder.Name) (state: $state)" -ForegroundColor Cyan

    $worktreeDirs = Get-ChildItem -Path $worktreesDir -Directory -ErrorAction SilentlyContinue
    foreach ($wtDir in $worktreeDirs) {
        $repoName = $wtDir.Name
        $originalRepo = $repoPaths | Where-Object { (Split-Path $_ -Leaf) -eq $repoName } | Select-Object -First 1

        if ($originalRepo) {
            try {
                Push-Location $originalRepo
                $branchName = "plan-$planId-$repoName"
                git worktree remove $wtDir.FullName --force 2>&1 | Out-Null
                git branch -D $branchName 2>&1 | Out-Null
                Pop-Location
                $cleanedTotal++
                Write-Host "  Removed: $repoName" -ForegroundColor Green
            }
            catch {
                Pop-Location
                try {
                    Remove-Item -Path $wtDir.FullName -Recurse -Force -ErrorAction Stop
                    $cleanedTotal++
                    Write-Host "  Removed (fallback): $repoName" -ForegroundColor Yellow
                }
                catch {
                    $failedTotal++
                    Write-Host "  Failed: $repoName - $_" -ForegroundColor Red
                }
            }
        }
        else {
            try {
                Remove-Item -Path $wtDir.FullName -Recurse -Force -ErrorAction Stop
                $cleanedTotal++
                Write-Host "  Removed (no repo match): $repoName" -ForegroundColor Yellow
            }
            catch {
                $failedTotal++
                Write-Host "  Failed: $repoName - $_" -ForegroundColor Red
            }
        }
    }

    # Remove worktrees dir if empty
    if ((Get-ChildItem $worktreesDir -ErrorAction SilentlyContinue | Measure-Object).Count -eq 0) {
        Remove-Item $worktreesDir -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "`nWorktree cleanup complete: $cleanedTotal removed, $failedTotal failed." -ForegroundColor $(if ($failedTotal -gt 0) { "Yellow" } else { "Green" })
