param(
    [Parameter(Mandatory = $true)]
    [string]$PlansDirectory,
    [Parameter(Mandatory = $true)]
    [string[]]$Keywords,
    [string]$Project = ""
)

# Extract all plan titles in a single call
$titleMatches = Select-String -Path "$PlansDirectory/*/plan.yaml" -Pattern "^title:" -ErrorAction SilentlyContinue

if (-not $titleMatches) { return }

# Build keyword list (lowercase for case-insensitive matching)
$lowerKeywords = $Keywords | ForEach-Object { $_.ToLower() }

$results = @()

foreach ($match in $titleMatches) {
    $title = ($match.Line -replace '^title:\s*', '').Trim()
    $lowerTitle = $title.ToLower()

    # Count keyword overlap
    $hitCount = 0
    foreach ($kw in $lowerKeywords) {
        if ($lowerTitle.Contains($kw)) { $hitCount++ }
    }

    # Require at least 2 keyword hits (or 1 if only 1 keyword provided)
    $threshold = if ($lowerKeywords.Count -le 1) { 1 } else { 2 }
    if ($hitCount -lt $threshold) { continue }

    $yamlPath = $match.Path
    $folderName = Split-Path (Split-Path $yamlPath -Parent) -Leaf

    # Read state and project from the matched plan.yaml
    $yamlContent = Get-Content $yamlPath -Raw
    $state = if ($yamlContent -match '(?m)^state:\s*(.+)') { $Matches[1].Trim() } else { "Unknown" }
    $planProject = if ($yamlContent -match '(?m)^project:\s*(.+)') { $Matches[1].Trim() } else { "" }

    # Filter by project if specified
    if ($Project -and $planProject -and $planProject -ne $Project) { continue }

    $results += "$folderName|$title|$state"
}

if ($results.Count -gt 0) {
    $results -join "`n"
}
