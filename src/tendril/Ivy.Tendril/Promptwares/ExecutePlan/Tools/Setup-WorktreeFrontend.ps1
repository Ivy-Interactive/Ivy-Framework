<#
.SYNOPSIS
Sets up .npmrc authentication and runs pnpm install for worktree frontend directories.

.DESCRIPTION
Detects frontend directories in worktrees, creates .npmrc files with authentication
for private scoped packages, runs pnpm install, and optionally cleans up.

.PARAMETER WorktreeRoot
Path to the worktrees directory (e.g., <PlanFolder>/worktrees)

.PARAMETER RegistryToken
Authentication token for the npm registry. If not provided, attempts to read from
.NET user secrets (DotnetUserSecrets:Npm:RegistryToken) or NPM_TOKEN environment variable.

.PARAMETER NpmScope
The npm scope for private packages (e.g., "@my-org"). Falls back to NPM_SCOPE env var.

.PARAMETER NpmRegistry
The npm registry URL for the scoped packages (e.g., "https://npm.pkg.github.com/").
Falls back to NPM_REGISTRY env var.

.PARAMETER PnpmVersion
The pnpm version to use (e.g., "10.33.0"). Falls back to PNPM_VERSION env var, defaults to "latest".

.PARAMETER SkipCleanup
If set, does not delete .npmrc files after install (useful for debugging).

.EXAMPLE
Setup-WorktreeFrontend.ps1 -WorktreeRoot "D:\Plans\01234-Example\worktrees"

.EXAMPLE
Setup-WorktreeFrontend.ps1 -WorktreeRoot ".\worktrees" -NpmScope "@my-org" -NpmRegistry "https://npm.pkg.github.com/" -SkipCleanup
#>
param(
    [Parameter(Mandatory)]
    [string]$WorktreeRoot,

    [string]$RegistryToken,

    [string]$NpmScope,

    [string]$NpmRegistry,

    [string]$PnpmVersion,

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

# Resolve npm scope
if (-not $NpmScope) { $NpmScope = $env:NPM_SCOPE }
if (-not $NpmScope) {
    Write-Error "No npm scope provided. Set NPM_SCOPE env var or pass -NpmScope (e.g., '@my-org')"
    exit 1
}

# Resolve npm registry
if (-not $NpmRegistry) { $NpmRegistry = $env:NPM_REGISTRY }
if (-not $NpmRegistry) {
    Write-Error "No npm registry provided. Set NPM_REGISTRY env var or pass -NpmRegistry (e.g., 'https://npm.pkg.github.com/')"
    exit 1
}

# Resolve pnpm version
if (-not $PnpmVersion) { $PnpmVersion = $env:PNPM_VERSION }
if (-not $PnpmVersion) { $PnpmVersion = "latest" }

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
        # ERR_PACKAGE_IMPORT_NOT_DEFINED errors
        $registryHost = ([Uri]$NpmRegistry).Authority
        $npmrcContent = @"
node-linker=hoisted
${NpmScope}:registry=$NpmRegistry
//${registryHost}/:_authToken=$RegistryToken
"@

        Set-Content -Path $npmrcPath -Value $npmrcContent -NoNewline
        $createdFiles += $npmrcPath

        Write-Host "Running pnpm install in $($dir.FullName)..."
        Push-Location $dir.FullName
        try {
            npx -y "pnpm@$PnpmVersion" install --frozen-lockfile
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
