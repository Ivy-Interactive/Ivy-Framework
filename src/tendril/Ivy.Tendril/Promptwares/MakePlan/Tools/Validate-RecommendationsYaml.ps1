param(
    [Parameter(Mandatory=$false)]
    [string]$PlansDirectory = "$env:TENDRIL_HOME/Plans",

    [Parameter(Mandatory=$false)]
    [switch]$Fix,

    [Parameter(Mandatory=$false)]
    [switch]$ShowDetails
)

$ErrorActionPreference = "Stop"

function Repair-YamlContent {
    param([string]$content)

    $lines = $content -split "`n"
    $result = @()

    foreach ($line in $lines) {
        $trimmedLine = $line.TrimEnd("`r")

        # Fix unquoted title values starting with special YAML characters
        if ($trimmedLine -match '^(\s*-\s*title:\s*)(.+)$' -or $trimmedLine -match '^(\s*title:\s*)(.+)$') {
            $prefix = $Matches[1]
            $value = $Matches[2]

            if ($value -match '^[`\[\]{}>|*&!%@#]' -and $value -notmatch '^[''"]') {
                $escaped = $value -replace '\\', '\\' -replace '"', '\"'
                $trimmedLine = "$prefix`"$escaped`""
            }
            elseif ($value -match '(?<!^)[:\[\]{}]' -and $value -notmatch '^[''"]') {
                $escaped = $value -replace '\\', '\\' -replace '"', '\"'
                $trimmedLine = "$prefix`"$escaped`""
            }
        }

        # Fix unquoted description values (flow-style only, not block scalars)
        if ($trimmedLine -match '^(\s*description:\s*)(.+)$' -and $trimmedLine -notmatch '^\s*description:\s*[|>]') {
            $prefix = $Matches[1]
            $value = $Matches[2]

            if ($value -match '^[`\[\]{}>|*&!%@#]' -and $value -notmatch '^[''"]') {
                $escaped = $value -replace '\\', '\\' -replace '"', '\"'
                $trimmedLine = "$prefix`"$escaped`""
            }
        }

        $result += $trimmedLine
    }

    return ($result -join "`n")
}

# Locate YamlDotNet assembly — try script-relative first, then original repo via git
$yamlDotNetDll = $null
$searchBins = @((Join-Path $PSScriptRoot ".." ".." ".." "bin" "Debug"))

$gitCommonDir = git -C $PSScriptRoot rev-parse --git-common-dir 2>$null
if ($gitCommonDir) {
    $originalRepoRoot = Split-Path $gitCommonDir
    $searchBins += (Join-Path $originalRepoRoot "src" "tendril" "Ivy.Tendril" "bin" "Debug")
}

foreach ($binDir in $searchBins) {
    $targetDirs = Get-ChildItem -Path $binDir -Directory -Filter "net*" -ErrorAction SilentlyContinue | Sort-Object Name -Descending
    foreach ($td in $targetDirs) {
        $candidate = Join-Path $td.FullName "YamlDotNet.dll"
        if (Test-Path $candidate) {
            $yamlDotNetDll = $candidate
            break
        }
    }
    if ($yamlDotNetDll) { break }
}
if (-not $yamlDotNetDll) {
    Write-Host "ERROR: YamlDotNet.dll not found. Run 'dotnet build' on Ivy.Tendril first."
    exit 1
}

# Load YamlDotNet
Add-Type -Path $yamlDotNetDll

# Build deserializer matching YamlHelper.Deserializer configuration
$deserializer = (New-Object YamlDotNet.Serialization.DeserializerBuilder).
    WithNamingConvention([YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention]::Instance).
    IgnoreUnmatchedProperties().
    Build()

# Find all recommendations.yaml files
$files = Get-ChildItem -Path "$PlansDirectory/*/artifacts/recommendations.yaml" -ErrorAction SilentlyContinue
if ($files.Count -eq 0) {
    Write-Host "No recommendations.yaml files found in $PlansDirectory"
    exit 0
}

Write-Host "Scanning $($files.Count) recommendations.yaml files..."
if ($ShowDetails) { Write-Host "Plans directory: $PlansDirectory" }

$validCount = 0
$invalidCount = 0
$fixedCount = 0
$errors = @()

foreach ($file in $files) {
    $planFolder = Split-Path (Split-Path $file.FullName)
    $planName = Split-Path (Split-Path $planFolder) -Leaf

    try {
        $yaml = [System.IO.File]::ReadAllText($file.FullName)
        $null = $deserializer.Deserialize[object]($yaml)
        $validCount++
        if ($ShowDetails) { Write-Host "  OK: $planName" }
    }
    catch {
        $invalidCount++
        $errorMsg = $_.Exception.Message
        $errors += [PSCustomObject]@{
            PlanName = $planName
            FilePath = $file.FullName
            Error    = $errorMsg
        }

        Write-Host "  FAIL: $planName - $errorMsg"

        if ($Fix) {
            try {
                $rawContent = [System.IO.File]::ReadAllText($file.FullName)
                $fixedContent = Repair-YamlContent $rawContent

                if ($fixedContent -ne $rawContent) {
                    # Verify the fix parses
                    $null = $deserializer.Deserialize[object]($fixedContent)
                    [System.IO.File]::WriteAllText($file.FullName, $fixedContent)
                    $fixedCount++
                    Write-Host "  FIXED: $planName"
                }
                else {
                    Write-Host "  UNFIXABLE: $planName - no automatic fix available"
                }
            }
            catch {
                Write-Host "  UNFIXABLE: $planName - $($_.Exception.Message)"
            }
        }
    }
}

# Summary
Write-Host ""
Write-Host "=== Summary ==="
Write-Host "Total files:   $($files.Count)"
Write-Host "Valid:         $validCount"
Write-Host "Invalid:       $invalidCount"
if ($Fix) {
    Write-Host "Fixed:         $fixedCount"
    Write-Host "Unfixable:     $($invalidCount - $fixedCount)"
}

if ($errors.Count -gt 0 -and -not $Fix) {
    Write-Host ""
    Write-Host "=== Malformed Files ==="
    foreach ($err in $errors) {
        Write-Host ""
        Write-Host "Plan: $($err.PlanName)"
        Write-Host "File: $($err.FilePath)"
        Write-Host "Error: $($err.Error)"
    }
}

if ($invalidCount -gt 0 -and (-not $Fix -or ($invalidCount - $fixedCount) -gt 0)) {
    exit 1
}
exit 0
