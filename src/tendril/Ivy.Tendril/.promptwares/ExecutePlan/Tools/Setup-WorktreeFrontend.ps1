<#
.SYNOPSIS
Sets up .npmrc authentication and runs pnpm install for worktree frontend directories.

.DESCRIPTION
Detects frontend directories in worktrees, creates .npmrc files with authentication
for private @ivy-interactive packages, runs pnpm install, and optionally cleans up.

.PARAMETER WorktreeRoot
Path to the worktrees directory (e.g., <PlanFolder>/worktrees)

.PARAMETER RegistryToken
Authentication token for the npm registry. If not provided, attempts to read from
.NET user secrets (DotnetUserSecrets:Npm:RegistryToken) or NPM_TOKEN environment variable.

.PARAMETER SkipCleanup
If set, does not delete .npmrc files after install (useful for debugging).

.EXAMPLE
Setup-WorktreeFrontend.ps1 -WorktreeRoot "D:\Plans\01234-Example\worktrees"

.EXAMPLE
Setup-WorktreeFrontend.ps1 -WorktreeRoot ".\worktrees" -SkipCleanup
#>
param(
    [Parameter(Mandatory)]
    [string]$WorktreeRoot,

    [string]$RegistryToken,

    [switch]$SkipCleanup
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Resolve token if not provided
if (-not $RegistryToken) {
    # Try .NET user secrets first
    try {
        $secretsJson = dotnet user-secrets list --json 2>$null | ConvertFrom-Json
        $RegistryToken = $secretsJson."Npm:RegistryToken"
    } catch {
        # Ignore errors
    }

    # Fallback to environment variable
    if (-not $RegistryToken) {
        $RegistryToken = $env:NPM_TOKEN
    }

    if (-not $RegistryToken) {
        Write-Error "No registry token provided. Set NPM_TOKEN env var, configure .NET user secrets (Npm:RegistryToken), or pass -RegistryToken"
        exit 1
    }
}

# Find all frontend directories in worktrees
$frontendDirs = Get-ChildItem -Path $WorktreeRoot -Directory -Recurse -Filter "frontend" |
    Where-Object { Test-Path (Join-Path $_.FullName "package.json") }

if ($frontendDirs.Count -eq 0) {
    Write-Host "No frontend directories found in worktrees."
    exit 0
}

Write-Host "Found $($frontendDirs.Count) frontend directories:"
$frontendDirs | ForEach-Object { Write-Host "  - $($_.FullName)" }

$createdFiles = @()

try {
    foreach ($dir in $frontendDirs) {
        $npmrcPath = Join-Path $dir.FullName ".npmrc"

        Write-Host "`nSetting up $npmrcPath..."

        # Create .npmrc with registry authentication and node-linker=hoisted
        # node-linker=hoisted is required for pnpm in worktrees to avoid
        # ERR_PACKAGE_IMPORT_NOT_DEFINED errors with @voidzero-dev/vite-plus-core
        $npmrcContent = @"
node-linker=hoisted
@ivy-interactive:registry=https://npm.pkg.github.com/
//npm.pkg.github.com/:_authToken=$RegistryToken
"@

        Set-Content -Path $npmrcPath -Value $npmrcContent -NoNewline
        $createdFiles += $npmrcPath

        Write-Host "Running pnpm install in $($dir.FullName)..."
        Push-Location $dir.FullName
        try {
            npx -y pnpm@10.33.0 install --frozen-lockfile
            if ($LASTEXITCODE -ne 0) {
                throw "pnpm install failed with exit code $LASTEXITCODE"
            }
        } finally {
            Pop-Location
        }
    }

    Write-Host "`nAll frontend directories set up successfully."

} finally {
    # Cleanup .npmrc files unless -SkipCleanup is set
    if (-not $SkipCleanup -and $createdFiles.Count -gt 0) {
        Write-Host "`nCleaning up .npmrc files..."
        foreach ($file in $createdFiles) {
            if (Test-Path $file) {
                Remove-Item $file -Force
                Write-Host "  Removed $file"
            }
        }
    }
}
