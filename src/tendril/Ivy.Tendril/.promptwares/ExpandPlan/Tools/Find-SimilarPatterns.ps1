# Find Similar Implementation Patterns in Codebase
# Usage: .\Find-SimilarPatterns.ps1 -Pattern "StateHasChanged" -Context "Dialog"
param(
    [Parameter(Mandatory=$true)]
    [string]$Pattern,

    [string]$Context = "",

    [string]$SearchPath = "D:\Repos\_Ivy\Ivy-Framework\src",

    [int]$ContextLines = 3
)

Write-Host "Searching for pattern: $Pattern" -ForegroundColor Cyan
if ($Context) {
    Write-Host "With context: $Context" -ForegroundColor Cyan
}

$results = @()

# Search for the pattern using ripgrep
$rgArgs = @(
    $Pattern,
    $SearchPath,
    "--type", "cs",
    "--type", "tsx",
    "--type", "ts",
    "--line-number",
    "--context", $ContextLines,
    "--heading",
    "--color", "never"
)

if ($Context) {
    # If context provided, filter results
    $allMatches = rg @rgArgs
    $results = $allMatches | Select-String -Pattern $Context -Context ($ContextLines * 2)
} else {
    $results = rg @rgArgs
}

if ($results) {
    Write-Host "`nFound $($results.Count) matches:" -ForegroundColor Green
    $results | ForEach-Object {
        Write-Host $_ -ForegroundColor Gray
    }
} else {
    Write-Host "`nNo matches found" -ForegroundColor Yellow
}

# Group by file for summary
$fileGroups = $results | Group-Object { ($_ -split ':')[0] } | Select-Object -First 10

Write-Host "`n=== Top Files with Pattern ===" -ForegroundColor Cyan
$fileGroups | ForEach-Object {
    Write-Host "$($_.Count) matches: $($_.Name)" -ForegroundColor White
}
