param(
    [Parameter(Mandatory = $true)]
    [string]$PlansDirectory,
    [string[]]$Repos = @()
)

# Find all plans in Building or Executing state in a single call
$stateMatches = Select-String -Path "$PlansDirectory/*/plan.yaml" -Pattern "^state: (Building|Executing)" -ErrorAction SilentlyContinue

if (-not $stateMatches) { return }

$results = @()

foreach ($match in $stateMatches) {
    $yamlPath = $match.Path
    $folderName = Split-Path (Split-Path $yamlPath -Parent) -Leaf
    $yamlContent = Get-Content $yamlPath -Raw

    $state = if ($yamlContent -match '(?m)^state:\s*(.+)') { $Matches[1].Trim() } else { "Unknown" }
    $title = if ($yamlContent -match '(?m)^title:\s*(.+)') { $Matches[1].Trim() } else { "" }

    # Extract repos from the yaml (lines starting with "- D:" or "- /")
    $planRepos = @()
    foreach ($line in ($yamlContent -split "`n")) {
        if ($line -match '^\s*-\s+(D:\\|D:/|/)(.+)') {
            $planRepos += $line.Trim().TrimStart('- ')
        }
    }

    $reposStr = ($planRepos -join ",")

    # If caller specified repos, check for overlap
    if ($Repos.Count -gt 0) {
        $overlap = $false
        foreach ($r in $Repos) {
            $normalizedR = $r.Replace('\', '/').TrimEnd('/')
            foreach ($pr in $planRepos) {
                $normalizedPr = $pr.Replace('\', '/').TrimEnd('/')
                if ($normalizedR -eq $normalizedPr) {
                    $overlap = $true
                    break
                }
            }
            if ($overlap) { break }
        }
        if (-not $overlap) { continue }
    }

    $results += "$folderName|$title|$state|$reposStr"
}

if ($results.Count -gt 0) {
    $results -join "`n"
}
