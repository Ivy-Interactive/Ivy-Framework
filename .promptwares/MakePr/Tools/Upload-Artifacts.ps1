param(
    [Parameter(Mandatory)]
    [string]$PlanFolder
)

$screenshotsDir = Join-Path $PlanFolder "artifacts/screenshots"

$screenshotFiles = @()

if (Test-Path $screenshotsDir) {
    $screenshotFiles = Get-ChildItem -Path $screenshotsDir -Filter "*.png" -File
}

if ($screenshotFiles.Count -eq 0) {
    return ""
}

$markdown = @()

if ($screenshotFiles.Count -gt 0) {
    $markdown += "### Screenshots"
    $markdown += ""
    foreach ($file in $screenshotFiles) {
        $url = (storage upload ivy-tendril $file.FullName).Trim()
        $name = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
        $markdown += "![${name}](${url})"
        $markdown += ""
    }
}

return ($markdown -join "`n")
