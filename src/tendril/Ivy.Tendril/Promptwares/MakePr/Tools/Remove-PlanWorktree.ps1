#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Removes a plan worktree with Windows-aware fallback strategies.

.DESCRIPTION
    Attempts to remove a plan worktree using multiple strategies to handle Windows file locking issues:
    1. git worktree remove --force (standard approach)
    2. git worktree prune + rm -rf + git branch -D (fallback for detached worktrees)
    3. PowerShell Remove-Item -Recurse -Force (final fallback for locked files/long paths)

.PARAMETER OriginalRepoPath
    Path to the original repository (e.g., D:\Repos\_Ivy\Ivy-Framework)

.PARAMETER WorktreePath
    Path to the worktree directory to remove (e.g., D:\Tendril\Plans\03246-...\worktrees\Ivy-Framework)

.PARAMETER BranchName
    Name of the branch to delete after worktree removal (e.g., tendril/03246-Jobs-Status-Not-Updated-When-Plan-Stopped)

.EXAMPLE
    .\Remove-PlanWorktree.ps1 -OriginalRepoPath "D:\Repos\_Ivy\Ivy-Framework" -WorktreePath "D:\Tendril\Plans\03246-Jobs-Status-Not-Updated-When-Plan-Stopped\worktrees\Ivy-Framework" -BranchName "tendril/03246-Jobs-Status-Not-Updated-When-Plan-Stopped"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$OriginalRepoPath,

    [Parameter(Mandatory = $true)]
    [string]$WorktreePath,

    [Parameter(Mandatory = $true)]
    [string]$BranchName
)

$ErrorActionPreference = "Continue"

Push-Location $OriginalRepoPath

try {
    Write-Host "Attempting worktree cleanup for: $WorktreePath"

    # Strategy 1: Standard git worktree remove
    Write-Host "  [1/3] Trying: git worktree remove --force"
    $gitRemoveResult = git worktree remove $WorktreePath --force 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "    ✓ Standard removal succeeded"
        git branch -D $BranchName 2>&1 | Out-Null
        return
    }
    Write-Host "    ✗ Standard removal failed: $gitRemoveResult"

    # Strategy 2: Prune + rm + branch delete
    Write-Host "  [2/3] Trying: git worktree prune + rm -rf + git branch -D"
    git worktree prune 2>&1 | Out-Null

    # Use bash rm -rf for better path handling
    bash -c "rm -rf `"$WorktreePath`"" 2>&1 | Out-Null

    if (-not (Test-Path $WorktreePath)) {
        Write-Host "    ✓ Prune + rm succeeded"
        git branch -D $BranchName 2>&1 | Out-Null
        return
    }
    Write-Host "    ✗ Prune + rm failed (directory still exists)"

    # Strategy 3: PowerShell Remove-Item (handles Windows locks better)
    Write-Host "  [3/3] Trying: PowerShell Remove-Item -Recurse -Force"
    Remove-Item -Path $WorktreePath -Recurse -Force -ErrorAction SilentlyContinue

    if (-not (Test-Path $WorktreePath)) {
        Write-Host "    ✓ PowerShell Remove-Item succeeded"
        git branch -D $BranchName 2>&1 | Out-Null
        return
    }
    Write-Host "    ✗ PowerShell Remove-Item failed (directory still exists)"

    # All strategies failed - log warning but don't fail the script
    Write-Warning "All cleanup strategies failed. The worktree directory remains at: $WorktreePath"
    Write-Warning "This is non-critical. The directory can be manually cleaned up later."

} finally {
    Pop-Location
}
