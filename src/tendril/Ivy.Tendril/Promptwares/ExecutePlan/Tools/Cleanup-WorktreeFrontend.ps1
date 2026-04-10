<#
.SYNOPSIS
Cleans up temporary .npmrc files created by ExecutePlan in worktree frontend directories.

.DESCRIPTION
Removes .npmrc files that were created during worktree setup, but preserves files
that were already tracked in git before the worktree was created.

.PARAMETER WorktreeRoot
Path to the worktrees directory (e.g., <PlanFolder>/worktrees)

.EXAMPLE
Cleanup-WorktreeFrontend.ps1 -WorktreeRoot "D:\Plans\01234-Example\worktrees"
#>
param(
    [Parameter(Mandatory)]
    [string]$WorktreeRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Find all worktree directories
[array]$worktrees = @(Get-ChildItem -Path $WorktreeRoot -Directory)

if ($worktrees.Count -eq 0) {
    Write-Host "No worktrees found in $WorktreeRoot"
    exit 0
}

foreach ($worktree in $worktrees) {
    Write-Host "`nProcessing worktree: $($worktree.Name)"

    # Find all .npmrc files anywhere in the worktree (except inside node_modules).
    # Program.md's Setup step creates .npmrc next to every package.json that isn't under
    # node_modules — which includes non-"frontend" dirs like src/tendril. Matching the
    # creation scope here prevents stray .npmrc files from being left behind.
    [array]$npmrcFiles = @(Get-ChildItem -Path $worktree.FullName -Recurse -Filter ".npmrc" |
        Where-Object { $_.FullName -notmatch '[\\/]node_modules[\\/]' })

    foreach ($npmrcFile in $npmrcFiles) {
        $relativePath = $npmrcFile.FullName.Replace($worktree.FullName + "\", "").Replace("\", "/")

        # Check if this file is tracked in git
        Push-Location $worktree.FullName
        try {
            $isTracked = git ls-files --error-unmatch $relativePath 2>$null

            if ($isTracked) {
                Write-Host "  Preserving tracked file: $relativePath"
            } else {
                # Only delete if it matches patterns we create (with or without auth token)
                $content = Get-Content $npmrcFile -Raw
                if ($content -match "node-linker=hoisted") {
                    Write-Host "  Removing created file: $relativePath"
                    Remove-Item $npmrcFile -Force
                } else {
                    Write-Host "  Preserving untracked file (not created by us): $relativePath"
                }
            }
        } finally {
            Pop-Location
        }
    }
}

Write-Host "`nCleanup complete."
