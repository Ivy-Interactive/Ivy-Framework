<#
.SYNOPSIS
    One-time cleanup of deeply nested Plans directories within worktrees.

.DESCRIPTION
    Removes pre-existing recursive Plans artifacts that are nested 2+ levels deep
    (e.g., Plans/*/worktrees/*/Plans/*/worktrees/*/Plans/...).

    These are remnants from crashed plan executions before CleanupRecursiveArtifacts
    was implemented. The regular cleanup service can't reach them because they're
    beyond the depth it scans.

    This script is destructive and should only be run once as an operational cleanup.

.PARAMETER PlansDirectory
    Root Plans directory to scan. Defaults to $env:TENDRIL_PLANS_DIRECTORY.

.PARAMETER WhatIf
    Show what would be deleted without actually deleting.

.PARAMETER Verbose
    Show detailed progress as directories are removed.

.EXAMPLE
    .\Remove-DeepNestedPlanArtifacts.ps1 -WhatIf
    Preview what would be deleted without making changes.

.EXAMPLE
    .\Remove-DeepNestedPlanArtifacts.ps1 -Verbose
    Run cleanup and show detailed progress.
#>
param(
    [string]$PlansDirectory = $env:TENDRIL_PLANS_DIRECTORY,
    [switch]$WhatIf,
    [switch]$Verbose
)

$ErrorActionPreference = 'Continue'

if (-not $PlansDirectory -or -not (Test-Path $PlansDirectory)) {
    Write-Error "Plans directory not found: $PlansDirectory"
    exit 1
}

Write-Host "Scanning for deeply nested Plans artifacts in: $PlansDirectory"
Write-Host ""

# Find all nested Plans directories at depth 2+ within worktrees
# Pattern: Plans/*/worktrees/**/Plans/
$nestedPlanDirs = Get-ChildItem -Path $PlansDirectory -Directory -Recurse -Filter "Plans" -ErrorAction SilentlyContinue |
    Where-Object {
        # Must be inside a worktrees directory
        $_.FullName -like "*\worktrees\*\Plans" -or $_.FullName -like "*\worktrees\*\Plans\*"
    }

if ($nestedPlanDirs.Count -eq 0) {
    Write-Host "No deeply nested Plans artifacts found." -ForegroundColor Green
    exit 0
}

Write-Host "Found $($nestedPlanDirs.Count) nested Plans directories to remove:" -ForegroundColor Yellow
Write-Host ""

$totalSize = 0
foreach ($dir in $nestedPlanDirs) {
    $relativePath = $dir.FullName.Replace("$PlansDirectory\", "")
    $size = (Get-ChildItem -Path $dir.FullName -Recurse -File -ErrorAction SilentlyContinue |
             Measure-Object -Property Length -Sum).Sum
    $totalSize += $size
    $sizeMB = [math]::Round($size / 1MB, 2)

    Write-Host "  $relativePath ($sizeMB MB)"
}

Write-Host ""
Write-Host "Total disk space to free: $([math]::Round($totalSize / 1MB, 2)) MB" -ForegroundColor Cyan

if ($WhatIf) {
    Write-Host ""
    Write-Host "WhatIf: No changes made. Run without -WhatIf to delete." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
$confirmation = Read-Host "Delete these directories? (y/n)"
if ($confirmation -ne 'y') {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Removing nested Plans directories..." -ForegroundColor Yellow

$removed = 0
$failed = 0

foreach ($dir in $nestedPlanDirs) {
    $relativePath = $dir.FullName.Replace("$PlansDirectory\", "")

    try {
        if ($Verbose) {
            Write-Host "  Removing: $relativePath"
        }

        Remove-Item -Path $dir.FullName -Recurse -Force -ErrorAction Stop
        $removed++
    }
    catch {
        Write-Warning "Failed to remove $relativePath : $_"
        $failed++
    }
}

Write-Host ""
Write-Host "Cleanup complete:" -ForegroundColor Green
Write-Host "  Removed: $removed directories"
if ($failed -gt 0) {
    Write-Host "  Failed: $failed directories" -ForegroundColor Yellow
}
Write-Host "  Disk space freed: ~$([math]::Round($totalSize / 1MB, 2)) MB"
