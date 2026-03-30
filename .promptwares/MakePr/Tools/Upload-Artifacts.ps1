param(
    [Parameter(Mandatory)]
    [string]$PlanFolder
)

$screenshotsDir = Join-Path $PlanFolder "artifacts/screenshots"
$videosDir = Join-Path $PlanFolder "artifacts/videos"

$screenshotFiles = @()
$videoFiles = @()

if (Test-Path $screenshotsDir) {
    $screenshotFiles = Get-ChildItem -Path $screenshotsDir -Filter "*.png" -File
}

if (Test-Path $videosDir) {
    $videoFiles = Get-ChildItem -Path $videosDir -Filter "*.webm" -File
}

if ($screenshotFiles.Count -eq 0 -and $videoFiles.Count -eq 0) {
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

if ($videoFiles.Count -gt 0) {
    $markdown += "### Videos"
    $markdown += ""
    foreach ($file in $videoFiles) {
        $url = (storage upload ivy-tendril $file.FullName).Trim()
        $markdown += "[▶ $($file.Name)](${url})"
        $markdown += ""
    }
}

return ($markdown -join "`n")
