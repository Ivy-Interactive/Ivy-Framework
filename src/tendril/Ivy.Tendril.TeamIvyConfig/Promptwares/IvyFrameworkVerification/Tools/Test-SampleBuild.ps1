param(
    [Parameter(Mandatory = $true)]
    [string]$SampleProjectDir
)

<#
.SYNOPSIS
    Pre-flight build validation for IvyFrameworkVerification sample projects.
.DESCRIPTION
    Runs dotnet build and dotnet run --describe on the sample project,
    returning structured output about build success, registered apps, and errors.
#>

$result = @{
    success = $false
    apps    = @()
    errors  = @()
}

# Validate project directory
if (-not (Test-Path $SampleProjectDir)) {
    $result.errors += "Sample project directory not found: $SampleProjectDir"
    $result | ConvertTo-Json -Depth 3
    exit 1
}

$csproj = Get-ChildItem -Path $SampleProjectDir -Filter "*.csproj" | Select-Object -First 1
if (-not $csproj) {
    $result.errors += "No .csproj file found in $SampleProjectDir"
    $result | ConvertTo-Json -Depth 3
    exit 1
}

# Step 1: dotnet build
Write-Host "Building $($csproj.Name)..." -ForegroundColor Cyan
$buildOutput = dotnet build $csproj.FullName 2>&1
$buildExitCode = $LASTEXITCODE

if ($buildExitCode -ne 0) {
    $errorLines = $buildOutput | Where-Object { $_ -match '(error\s+(CS|MSB)\d+|Build FAILED)' }
    $result.errors += $errorLines | ForEach-Object { $_.ToString().Trim() }
    if ($result.errors.Count -eq 0) {
        $result.errors += "dotnet build failed with exit code $buildExitCode"
    }
    $result | ConvertTo-Json -Depth 3
    exit 1
}

# Step 2: dotnet run --describe
Write-Host "Checking app registration..." -ForegroundColor Cyan
$describeOutput = dotnet run --project $csproj.FullName --no-build -- --describe 2>&1
$describeExitCode = $LASTEXITCODE

if ($describeExitCode -ne 0) {
    $result.errors += "dotnet run --describe failed with exit code $describeExitCode"
    $result | ConvertTo-Json -Depth 3
    exit 1
}

# Parse app names from --describe output (lines with app IDs)
$apps = @($describeOutput | Where-Object {
    $_ -match '^\s*\w+' -and $_ -notmatch '(^info:|^warn:|^fail:|^Starting|^Application|^Now listening|^Content root)'
} | ForEach-Object { $_.ToString().Trim() } | Where-Object { $_ -ne '' })

$result.success = $true
$result.apps = $apps

Write-Host "Build succeeded. Found $($apps.Count) app(s)." -ForegroundColor Green
$result | ConvertTo-Json -Depth 3
