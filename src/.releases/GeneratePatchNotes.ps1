#!/usr/bin/env pwsh
param (
    [Parameter(Mandatory=$true)]
    [string]$FromRef,

    [Parameter(Mandatory=$true)]
    [string]$ToRef,

    [string]$RepoPath,

    [Parameter(Mandatory=$true)]
    [string]$Output
)

$ErrorActionPreference = "Stop"

# Default RepoPath to the root of the repository where this script resides
if ([string]::IsNullOrWhiteSpace($RepoPath)) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoPath = Split-Path (Split-Path $scriptDir -Parent) -Parent
}

$repoAbsPath = (Resolve-Path $RepoPath).Path
if (-not (Test-Path (Join-Path $repoAbsPath ".git"))) {
    Write-Error "$repoAbsPath is not a git repository."
    exit 1
}

function Get-GitOutput {
    param (
        [string[]]$ArgsList
    )
    $res = & git -C $repoAbsPath @ArgsList
    return $res
}

function Is-FrontendPath {
    param (
        [string]$Path
    )
    $parts = $Path -split '[/\\]'
    return $parts -contains "frontend"
}

try {
    # Get all commits in range
    $commitHashes = Get-GitOutput -ArgsList @("log", "--no-merges", "--format=%H", "${FromRef}..${ToRef}")
    
    $filteredCommits = [System.Collections.Generic.List[PSCustomObject]]::new()

    foreach ($hash in $commitHashes) {
        if ([string]::IsNullOrWhiteSpace($hash)) { continue }
        
        # Get changed files
        $files = Get-GitOutput -ArgsList @("diff-tree", "--no-commit-id", "--name-only", "-r", $hash)
        if ($null -eq $files -or $files.Count -eq 0) {
            continue
        }

        # Check if all changed files are frontend-only
        $allFrontend = $true
        foreach ($file in $files) {
            if (-not (Is-FrontendPath -Path $file)) {
                $allFrontend = $false
                break
            }
        }

        if ($allFrontend) {
            continue
        }

        # Get commit metadata
        $author = (Get-GitOutput -ArgsList @("log", "-n", "1", "--format=%an", $hash)) -join "`n"
        $subject = (Get-GitOutput -ArgsList @("log", "-n", "1", "--format=%s", $hash)) -join "`n"
        $body = (Get-GitOutput -ArgsList @("log", "-n", "1", "--format=%b", $hash)) -join "`n"

        # Exclude frontend files from changed files list in output
        $nonFeFiles = [System.Collections.Generic.List[string]]::new()
        foreach ($file in $files) {
            if (-not (Is-FrontendPath -Path $file)) {
                $nonFeFiles.Add($file)
            }
        }

        $commitObj = [PSCustomObject]@{
            hash          = $hash
            author        = $author
            subject       = $subject
            body          = $body.Trim()
            changed_files = $nonFeFiles.ToArray()
        }

        $filteredCommits.Add($commitObj)
    }

    # Convert to JSON and save
    $json = ConvertTo-Json -InputObject $filteredCommits -Depth 5
    $outputPath = [System.IO.Path]::GetFullPath($Output)
    
    # Ensure parent directory of output exists
    $outputDir = Split-Path $outputPath
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }
    
    [System.IO.File]::WriteAllText($outputPath, $json, [System.Text.Encoding]::UTF8)

    Write-Host "Success! Filtered commits successfully written to: $Output"

} catch {
    Write-Error $_
    exit 1
}
