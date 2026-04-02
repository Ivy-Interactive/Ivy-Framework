param(
    [Parameter(Mandatory)]
    [string]$PlanFolder
)

$env:NO_COLOR = "1"
$screenshotsDir = Join-Path $PlanFolder "artifacts/screenshots"

$screenshotFiles = @()

if (Test-Path $screenshotsDir) {
    $screenshotFiles = Get-ChildItem -Path $screenshotsDir -Filter "*.png" -File | Sort-Object Name
}

if ($screenshotFiles.Count -eq 0) {
    return ""
}

$markdown = @()

if ($screenshotFiles.Count -gt 0) {
    $markdown += "### Screenshots"
    $markdown += ""
    foreach ($file in $screenshotFiles) {
        $raw = & storage upload ivy-tendril $file.FullName 2>&1 | Out-String
        # Strip ANSI codes and collapse whitespace to reconstruct wrapped URLs
        $clean = $raw -replace "\x1B\[[0-9;]*m", ""
        $noWs = $clean -replace "\s+", ""
        if ($noWs -match "(https://stivytelemetry\.blob\.core\.windows\.net/[^""]+)") {
            $url = $Matches[1].Trim()
        } else {
            continue
        }
        $name = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
        $markdown += "![${name}](${url})"
        $markdown += ""
    }
}

return ($markdown -join "`n")
