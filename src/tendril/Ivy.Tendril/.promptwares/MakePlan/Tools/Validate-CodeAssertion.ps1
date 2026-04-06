param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,

    [Parameter(Mandatory=$false)]
    [string]$ExpectedCode,

    [Parameter(Mandatory=$false)]
    [string]$Description,

    [Parameter(Mandatory=$false)]
    [int]$LineStart,

    [Parameter(Mandatory=$false)]
    [int]$LineEnd
)

# Read file content
if (-not (Test-Path $FilePath)) {
    Write-Host "FAIL: File not found at $FilePath"
    exit 1
}

$content = Get-Content $FilePath -Raw

# If specific line range given, extract those lines
if ($LineStart -and $LineEnd) {
    $lines = Get-Content $FilePath
    $content = ($lines[($LineStart-1)..($LineEnd-1)] -join "`n")
}

# Validate against expected code (exact match, normalized whitespace)
if ($ExpectedCode) {
    $normalizedExpected = $ExpectedCode -replace '\s+', ' ' -replace '^\s+|\s+$', ''
    $normalizedActual = $content -replace '\s+', ' ' -replace '^\s+|\s+$', ''

    if ($normalizedActual -like "*$normalizedExpected*") {
        Write-Host "PASS: Code matches expected"
        exit 0
    } else {
        Write-Host "FAIL: Code does not match"
        Write-Host "Expected: $normalizedExpected"
        Write-Host "Actual: $normalizedActual"
        exit 1
    }
}

# Validate against description (pattern matching)
if ($Description) {
    if ($content -match $Description) {
        Write-Host "PASS: Code matches description"
        exit 0
    } else {
        Write-Host "FAIL: Code does not match description"
        Write-Host "Description: $Description"
        Write-Host "Content: $($content.Substring(0, [Math]::Min(200, $content.Length)))..."
        exit 1
    }
}

Write-Host "FAIL: No validation criteria provided"
exit 1
