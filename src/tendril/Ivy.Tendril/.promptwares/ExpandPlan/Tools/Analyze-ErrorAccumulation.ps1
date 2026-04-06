param(
    [string]$BuildErrorsFile
)

# Analyze error accumulation pattern in langfuse-build-errors.md
$content = Get-Content $BuildErrorsFile -Raw

# Extract each build section
$builds = @()
$buildMatches = [regex]::Matches($content, '## Build #(\d+).*?(?=## Build #|\z)', [System.Text.RegularExpressions.RegexOptions]::Singleline)

foreach ($match in $buildMatches) {
    $buildNum = [int]$match.Groups[1].Value
    $buildContent = $match.Value

    # Count errors in this build
    $errorCount = ([regex]::Matches($buildContent, '- `CS\d+')).Count

    # Count files affected
    $fileMatches = [regex]::Matches($buildContent, '### ([^\r\n]+)')
    $fileCount = $fileMatches.Count

    # Calculate content size (indicator of context size)
    $contentSize = $buildContent.Length

    $builds += [PSCustomObject]@{
        BuildNumber = $buildNum
        ErrorCount = $errorCount
        FilesAffected = $fileCount
        ContentSize = $contentSize
        Status = if ($buildContent -match '✅ OK') { 'SUCCESS' } else { 'FAILED' }
    }
}

Write-Output "`nError Accumulation Analysis:"
Write-Output "="*60
$builds | Format-Table -AutoSize

# Calculate accumulation
Write-Output "`nAccumulation Pattern:"
Write-Output "="*60
Write-Output "Total context at each build (cumulative sum of errors shown):"

$cumulative = 0
foreach ($build in $builds) {
    if ($build.Status -eq 'FAILED') {
        $cumulative += $build.ErrorCount
    }
    Write-Output "Build #$($build.BuildNumber): $cumulative total errors in history"
}

return $builds
