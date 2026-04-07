param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

. "$PSScriptRoot/.shared/Utils.ps1"

$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanYaml $planYamlPath

# Check plan state
$currentState = if ($planInfo.Yaml.state) { $planInfo.Yaml.state } else { "Unknown" }

$terminalStates = @("Completed", "Failed", "Skipped", "Icebox")
if ($currentState -notin $terminalStates) {
    Write-Host "Plan is not in a terminal state (current: $currentState). Cleanup skipped." -ForegroundColor Yellow
    Write-Host "Cleanup only runs for states: $($terminalStates -join ', ')" -ForegroundColor Yellow
    exit 1
}

$worktreesDir = Join-Path $PlanPath "worktrees"
if (-not (Test-Path $worktreesDir)) {
    Write-Host "No worktrees directory found. Nothing to clean up." -ForegroundColor Green
    exit 0
}

# Extract plan ID from folder name (e.g. "01626" from "01626-WorktreeCleanup")
$planFolderName = Split-Path $PlanPath -Leaf
$planId = if ($planFolderName -match '^(\d+)') { $Matches[1] } else { "" }

# Extract repo paths from plan.yaml
$repoPaths = ExtractRepoPathsFromYaml $planInfo.Yaml.repos -ValidateExists

$worktreeDirs = Get-ChildItem -Path $worktreesDir -Directory -ErrorAction SilentlyContinue
$cleanedCount = 0
$failedCount = 0

foreach ($wtDir in $worktreeDirs) {
    $repoName = $wtDir.Name
    Write-Host "Cleaning up worktree: $repoName" -ForegroundColor Cyan

    # Find the original repo path
    $originalRepo = $repoPaths | Where-Object { (Split-Path $_ -Leaf) -eq $repoName } | Select-Object -First 1

    if ($originalRepo) {
        # Remove worktree via git
        try {
            Push-Location $originalRepo
            $branchName = "plan-$planId-$repoName"
            git worktree remove $wtDir.FullName --force 2>&1 | Write-Host
            git branch -D $branchName 2>&1 | Write-Host
            Pop-Location
            $cleanedCount++
            Write-Host "  Removed worktree and branch: $branchName" -ForegroundColor Green
        }
        catch {
            Pop-Location
            Write-Host "  Git worktree remove failed: $_" -ForegroundColor Yellow
            # Fallback: try to just remove the directory
            try {
                Remove-Item -Path $wtDir.FullName -Recurse -Force -ErrorAction Stop
                $cleanedCount++
                Write-Host "  Removed directory directly" -ForegroundColor Yellow
            }
            catch {
                $failedCount++
                Write-Host "  Failed to remove directory: $_" -ForegroundColor Red
            }
        }
    }
    else {
        # No matching repo found, just remove the directory
        Write-Host "  No matching repo path found for '$repoName'. Removing directory only." -ForegroundColor Yellow
        try {
            # Try git worktree prune from any repo that might own it
            Remove-Item -Path $wtDir.FullName -Recurse -Force -ErrorAction Stop
            $cleanedCount++
        }
        catch {
            $failedCount++
            Write-Host "  Failed to remove directory: $_" -ForegroundColor Red
        }
    }
}

# Remove the worktrees directory if empty
if ((Get-ChildItem $worktreesDir -ErrorAction SilentlyContinue | Measure-Object).Count -eq 0) {
    Remove-Item $worktreesDir -Force -ErrorAction SilentlyContinue
    Write-Host "Removed empty worktrees directory" -ForegroundColor Green
}

# Write cleanup log
WritePlanLog $PlanPath "CleanupPlan" "Cleaned $cleanedCount worktree(s), $failedCount failed."

Write-Host "`nCleanup complete: $cleanedCount removed, $failedCount failed." -ForegroundColor $(if ($failedCount -gt 0) { "Yellow" } else { "Green" })
exit $(if ($failedCount -gt 0) { 1 } else { 0 })
