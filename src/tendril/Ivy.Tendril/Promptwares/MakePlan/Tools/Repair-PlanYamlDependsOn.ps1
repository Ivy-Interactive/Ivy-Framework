<#
.SYNOPSIS
    Repairs plan.yaml files where 'priority' was incorrectly nested inside 'dependsOn'.

.DESCRIPTION
    Scans all plan.yaml files in the plans directory and fixes three known patterns:
      Pattern 1: dependsOn:\n  priority: N        (mapping under dependsOn)
      Pattern 2: dependsOn:\n  'priority: N'      (quoted string under dependsOn)
      Pattern 3: dependsOn:\n  - item\n  priority: N  (mixed list + mapping)

.PARAMETER PlansDirectory
    Path to the plans directory containing plan folders.

.PARAMETER DryRun
    If set, reports what would be changed without modifying files.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$PlansDirectory,

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

$fixed = 0
$skipped = 0
$errors = 0

$planFolders = Get-ChildItem -Path $PlansDirectory -Directory | Where-Object { $_.Name -match '^\d{5}-' }

foreach ($folder in $planFolders) {
    $yamlPath = Join-Path $folder.FullName "plan.yaml"
    if (-not (Test-Path $yamlPath)) { continue }

    $content = Get-Content $yamlPath -Raw
    if (-not $content) { continue }

    # Check if priority appears indented under dependsOn
    # Pattern 1: dependsOn:\n  priority: N
    # Pattern 2: dependsOn:\n  'priority: N'
    # Pattern 3: dependsOn:\n  - items...\n  priority: N
    $hasBug = $content -match '(?m)^dependsOn:\s*\r?\n(?:  - .+\r?\n)*  ''?priority[:=]'

    if (-not $hasBug) {
        $skipped++
        continue
    }

    try {
        $original = $content

        # Extract the priority value from inside dependsOn block
        $priorityValue = 0
        if ($content -match "(?m)^  '?priority:\s*(\d+)'?\s*$") {
            $priorityValue = [int]$Matches[1]
        }

        # Remove indented priority line from dependsOn block (all patterns)
        $content = $content -replace "(?m)^  'priority:\s*\d+'\s*\r?\n", ""
        $content = $content -replace "(?m)^  priority:\s*\d+\s*\r?\n", ""

        # Check if dependsOn now has remaining list items
        $dependsOnMatch = [regex]::Match($content, '(?m)^dependsOn:\s*\r?\n((?:  - .+\r?\n)*)')
        if ($dependsOnMatch.Success -and [string]::IsNullOrWhiteSpace($dependsOnMatch.Groups[1].Value)) {
            # dependsOn is empty after removing priority — normalize to empty
            $content = $content -replace '(?m)^dependsOn:\s*$', 'dependsOn: '
        }

        # Ensure priority exists at root level
        if ($content -match '(?m)^priority:\s') {
            # Replace existing root-level priority
            $content = $content -replace '(?m)^priority:\s.*$', "priority: $priorityValue"
        }
        else {
            # Add priority after dependsOn block (find end of dependsOn section)
            $content = $content -replace '(?m)^(dependsOn:(?:\s*\r?\n(?:  - .+\r?\n)*)?)(?=\S)', "`$1priority: $priorityValue`n"
            # If that didn't work (dependsOn at end or followed by blank line), try simpler approach
            if ($content -notmatch '(?m)^priority:') {
                $content = $content -replace '(?m)^(dependsOn:[^\r\n]*)\r?\n', "`$1`npriority: $priorityValue`n"
            }
        }

        if ($content -eq $original) {
            Write-Host "  UNCHANGED: $($folder.Name) (pattern detected but no change needed)" -ForegroundColor Yellow
            $skipped++
            continue
        }

        if ($DryRun) {
            Write-Host "  WOULD FIX: $($folder.Name) (priority: $priorityValue)" -ForegroundColor Cyan
        }
        else {
            Set-Content $yamlPath $content -NoNewline
            Write-Host "  FIXED: $($folder.Name) (priority: $priorityValue)" -ForegroundColor Green
        }
        $fixed++
    }
    catch {
        Write-Host "  ERROR: $($folder.Name): $_" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""
Write-Host "Results:" -ForegroundColor White
Write-Host "  Fixed:   $fixed" -ForegroundColor Green
Write-Host "  Skipped: $skipped" -ForegroundColor Gray
Write-Host "  Errors:  $errors" -ForegroundColor $(if ($errors -gt 0) { 'Red' } else { 'Gray' })
