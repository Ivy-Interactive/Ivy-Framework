param(
    [Parameter(Mandatory)]
    [string]$PlanFolder,

    [string]$StorageContainer = $env:TENDRIL_STORAGE_CONTAINER,

    [string]$StorageDomain = $env:TENDRIL_STORAGE_DOMAIN
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
        if (-not $StorageContainer) {
            Write-Error "No storage container specified. Set TENDRIL_STORAGE_CONTAINER env var or pass -StorageContainer"
            return ""
        }
        $raw = & storage upload $StorageContainer $file.FullName 2>&1 | Out-String
        # Strip ANSI codes and collapse whitespace to reconstruct wrapped URLs
        $clean = $raw -replace "\x1B\[[0-9;]*m", ""
        $noWs = $clean -replace "\s+", ""
        if ($StorageDomain) {
            $escapedDomain = [regex]::Escape($StorageDomain)
            $urlPattern = "(https://${escapedDomain}/[^""]+)"
        } else {
            $urlPattern = "(https://[^/]+\.blob\.core\.windows\.net/[^""]+)"
        }
        if ($noWs -match $urlPattern) {
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
