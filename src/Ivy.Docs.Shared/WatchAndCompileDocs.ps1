$scriptDir = $PSScriptRoot
$docsPath = Join-Path $scriptDir "Docs"
$outputPath = Join-Path $scriptDir "Generated"

Write-Host "Watching for .md changes in $docsPath" -ForegroundColor Green
Write-Host "Output: $outputPath" -ForegroundColor Gray
Write-Host "Press Ctrl+C to stop..." -ForegroundColor Yellow
Write-Host ""

# Build file hash cache
$fileHashes = @{}
Get-ChildItem -Path $docsPath -Filter "*.md" -Recurse | ForEach-Object {
    $fileHashes[$_.FullName] = (Get-FileHash $_.FullName -Algorithm MD5).Hash
}

Write-Host "Tracking $($fileHashes.Count) markdown files" -ForegroundColor Cyan
Write-Host ""

function Invoke-Compile {
    param([string]$changedFilePath)

    $fileName = Split-Path $changedFilePath -Leaf
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] Changed: $fileName" -ForegroundColor Cyan
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] Compiling..." -ForegroundColor Yellow

    # Run from the Ivy.Docs.Shared directory with relative paths (matching how MSBuild runs it)
    Push-Location $scriptDir
    try {
        & dotnet ivy-docs-cli convert "Docs" "Generated" --skip-if-not-changed
    }
    finally {
        Pop-Location
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] Done" -ForegroundColor Green
    } else {
        Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] Failed" -ForegroundColor Red
    }
    Write-Host ""
}

# Polling loop
while ($true) {
    Start-Sleep -Milliseconds 500

    $currentFiles = Get-ChildItem -Path $docsPath -Filter "*.md" -Recurse

    foreach ($file in $currentFiles) {
        $hash = (Get-FileHash $file.FullName -Algorithm MD5).Hash

        if (-not $fileHashes.ContainsKey($file.FullName)) {
            # New file
            $fileHashes[$file.FullName] = $hash
            Invoke-Compile -changedFilePath $file.FullName
        }
        elseif ($fileHashes[$file.FullName] -ne $hash) {
            # Changed file
            $fileHashes[$file.FullName] = $hash
            Invoke-Compile -changedFilePath $file.FullName
        }
    }
}
