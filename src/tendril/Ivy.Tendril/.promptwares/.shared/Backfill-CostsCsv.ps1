param(
    [switch]$WhatIf
)

<#
.SYNOPSIS
    Backfills missing costs.csv files for plans created April 3-5 2026.

.DESCRIPTION
    Due to a bug fixed in plan 01798, plans created between April 3-5 2026
    are missing costs.csv files. This script finds matching Claude session
    files by timestamp correlation and retroactively calculates costs.

.PARAMETER WhatIf
    Show what would be recovered without writing files.
#>

# --- Prerequisites ---
if (-not $env:TENDRIL_HOME) { throw "TENDRIL_HOME not set" }

$PlansDir = Join-Path $env:TENDRIL_HOME "Plans"
if (-not (Test-Path $PlansDir)) { throw "Plans directory not found: $PlansDir" }

# --- Model pricing (from Ivy.Tendril/Assets/models.yaml, verified 2026-04-03) ---
$ModelPricing = @{
    "claude-opus-4"   = @{ Input = 15.00; Output = 75.00; CacheWrite = 18.75; CacheRead = 1.50 }
    "claude-sonnet-4" = @{ Input = 3.00;  Output = 15.00; CacheWrite = 3.75;  CacheRead = 0.30 }
    "claude-haiku-4"  = @{ Input = 0.80;  Output = 4.00;  CacheWrite = 1.00;  CacheRead = 0.08 }
}
$DefaultPricing = $ModelPricing["claude-opus-4"]

# --- Promptware to Claude projects directory mapping ---
# Each promptware runs with a specific working directory that determines where
# Claude stores session .jsonl files under ~/.claude/projects/
$ClaudeProjectsDir = Join-Path $env:USERPROFILE ".claude/projects"

function Get-ClaudeProjectDir {
    param([string]$Promptware, [string]$Project)

    # Determine the working directory Claude was launched from for each promptware type.
    # TENDRIL_HOME = TeamIvyConfig dir. Promptwares live in sibling Ivy.Tendril/.promptwares/
    # They run with Push-Location to their program folder under .promptwares/<Name>/,
    # while ExecutePlan Push-Locations to the project's primary repo.
    $ivyTendrilDir = Join-Path $env:TENDRIL_HOME "../Ivy.Tendril"
    $ivyTendrilDir = [System.IO.Path]::GetFullPath($ivyTendrilDir)

    switch ($Promptware) {
        "ExecutePlan" {
            # ExecutePlan uses GetProjectWorkDir which returns the first repo path
            switch ($Project) {
                "Tendril"     { return "D:\Repos\_Ivy\Ivy-Framework" }
                "Framework"   { return "D:\Repos\_Ivy\Ivy-Framework" }
                "Agent"       { return "D:\Repos\_Ivy\Ivy-Agent" }
                "TestManager" { return "D:\Repos\_Ivy\Ivy-Agent" }
                "Console"     { return "D:\Repos\_Ivy\Ivy" }
                "Mcp"         { return "D:\Repos\_Ivy\Ivy-Mcp" }
                default       { return "D:\Repos\_Ivy\Ivy-Framework" }
            }
        }
        "MakePlan"   { return Join-Path $ivyTendrilDir ".promptwares/MakePlan" }
        "MakePr"     { return Join-Path $ivyTendrilDir ".promptwares/MakePr" }
        "ExpandPlan" { return Join-Path $ivyTendrilDir ".promptwares/ExpandPlan" }
        "UpdatePlan" { return Join-Path $ivyTendrilDir ".promptwares/UpdatePlan" }
        "SplitPlan"  { return Join-Path $ivyTendrilDir ".promptwares/UpdatePlan" }
        default      { return $ivyTendrilDir }
    }
}

function ConvertTo-ClaudeProjectPath {
    param([string]$WorkingDirectory)

    # Claude converts working directories to folder names by replacing
    # ALL non-alphanumeric characters (except -) with dashes, removing leading dashes
    $safePath = $WorkingDirectory -replace '[^a-zA-Z0-9-]', '-'
    $safePath = $safePath -replace '^-+', ''
    return Join-Path $ClaudeProjectsDir $safePath
}

function Get-ModelPricing {
    param([string]$ModelName)

    foreach ($key in $ModelPricing.Keys) {
        if ($ModelName -and $ModelName.Contains($key)) {
            return $ModelPricing[$key]
        }
    }
    return $DefaultPricing
}

function Calculate-FileCost {
    param([string]$FilePath)

    $totalTokens = 0
    $totalCost = 0.0

    foreach ($line in [System.IO.File]::ReadLines($FilePath)) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }

        try {
            $obj = [System.Text.Json.JsonDocument]::Parse($line)
            $root = $obj.RootElement

            $typeProp = $root.GetProperty("type")
            if ($typeProp.GetString() -ne "assistant") { continue }

            $hasMessage = $false
            $message = $null
            try { $message = $root.GetProperty("message"); $hasMessage = $true } catch { }
            if (-not $hasMessage) { continue }

            $hasUsage = $false
            $usage = $null
            try { $usage = $message.GetProperty("usage"); $hasUsage = $true } catch { }
            if (-not $hasUsage) { continue }

            $model = "claude-opus-4"
            try { $model = $message.GetProperty("model").GetString() } catch { }
            if (-not $model) { $model = "claude-opus-4" }

            $pricing = Get-ModelPricing $model
            $priceInput = $pricing.Input * 1e-6
            $priceOutput = $pricing.Output * 1e-6
            $priceCacheWrite = $pricing.CacheWrite * 1e-6
            $priceCacheRead = $pricing.CacheRead * 1e-6

            $inputTokens = 0; try { $inputTokens = $usage.GetProperty("input_tokens").GetInt32() } catch { }
            $outputTokens = 0; try { $outputTokens = $usage.GetProperty("output_tokens").GetInt32() } catch { }
            $cacheReadTokens = 0; try { $cacheReadTokens = $usage.GetProperty("cache_read_input_tokens").GetInt32() } catch { }

            $totalTokens += $inputTokens + $outputTokens + $cacheReadTokens
            $totalCost += $inputTokens * $priceInput
            $totalCost += $outputTokens * $priceOutput
            $totalCost += $cacheReadTokens * $priceCacheRead

            # Cache creation tokens (new format with object, or legacy single value)
            $hasCacheCreation = $false
            $cacheCreation = $null
            try { $cacheCreation = $usage.GetProperty("cache_creation"); $hasCacheCreation = $true } catch { }

            if ($hasCacheCreation) {
                $cache5m = 0; try { $cache5m = $cacheCreation.GetProperty("ephemeral_5m_input_tokens").GetInt32() } catch { }
                $cache1h = 0; try { $cache1h = $cacheCreation.GetProperty("ephemeral_1h_input_tokens").GetInt32() } catch { }
                $totalTokens += $cache5m + $cache1h
                $totalCost += ($cache5m + $cache1h) * $priceCacheWrite
            }
            else {
                $ccTokens = 0; try { $ccTokens = $usage.GetProperty("cache_creation_input_tokens").GetInt32() } catch { }
                if ($ccTokens -gt 0) {
                    $totalTokens += $ccTokens
                    $totalCost += $ccTokens * $priceCacheWrite
                }
            }
        }
        catch { <# Skip malformed lines #> }
    }

    # Also process subagent files
    $sessionDir = [System.IO.Path]::Combine(
        [System.IO.Path]::GetDirectoryName($FilePath),
        [System.IO.Path]::GetFileNameWithoutExtension($FilePath),
        "subagents"
    )
    if (Test-Path $sessionDir) {
        foreach ($subFile in Get-ChildItem $sessionDir -Filter "*.jsonl") {
            $subResult = Calculate-FileCost $subFile.FullName
            $totalTokens += $subResult.TotalTokens
            $totalCost += $subResult.TotalCost
        }
    }

    return @{ TotalTokens = $totalTokens; TotalCost = $totalCost }
}

function Find-SessionFile {
    param(
        [string]$ProjectDir,
        [DateTime]$CompletionTime,
        [int]$ToleranceMinutes = 5
    )

    if (-not (Test-Path $ProjectDir)) { return $null }

    $minTime = $CompletionTime.AddMinutes(-$ToleranceMinutes)
    $maxTime = $CompletionTime.AddMinutes($ToleranceMinutes)

    $matches = Get-ChildItem $ProjectDir -Filter "*.jsonl" -File |
        Where-Object {
            $_.LastWriteTimeUtc -ge $minTime -and
            $_.LastWriteTimeUtc -le $maxTime
        } |
        Sort-Object LastWriteTimeUtc -Descending

    if ($matches.Count -eq 0) { return $null }

    # Return the best match (closest to completion time)
    return ($matches | Sort-Object { [Math]::Abs(($_.LastWriteTimeUtc - $CompletionTime).TotalSeconds) } |
        Select-Object -First 1)
}

function Find-SessionFileById {
    param(
        [string]$ProjectDir,
        [string]$SessionId
    )

    if (-not (Test-Path $ProjectDir)) { return $null }
    if ([string]::IsNullOrEmpty($SessionId)) { return $null }

    # Session files are named with the session ID (with or without extension)
    $sessionFile = Get-ChildItem $ProjectDir -Filter "$SessionId.jsonl" -File -ErrorAction SilentlyContinue
    if ($sessionFile) {
        return $sessionFile
    }

    # Fallback: search all .jsonl files and check if filename starts with session ID
    # (Claude might use variations like {sessionId}.jsonl or {sessionId}-{timestamp}.jsonl)
    $matches = Get-ChildItem $ProjectDir -Filter "*.jsonl" -File |
        Where-Object { $_.BaseName -eq $SessionId -or $_.BaseName.StartsWith("$SessionId-") }

    if ($matches.Count -gt 0) {
        return $matches | Select-Object -First 1
    }

    return $null
}

function Parse-LogCompletionTime {
    param([string]$LogPath)

    $content = Get-Content $LogPath -Raw
    if ($content -match '\*\*Completed:\*\*\s*(\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2})') {
        try {
            return [DateTime]::Parse($Matches[1], $null, [System.Globalization.DateTimeStyles]::AssumeUniversal -bor [System.Globalization.DateTimeStyles]::AdjustToUniversal)
        }
        catch { }
    }
    return $null
}

function Parse-LogSessionId {
    param([string]$LogPath)

    $content = Get-Content $LogPath -Raw
    if ($content -match '- \*\*SessionId:\*\*\s+([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})') {
        return $Matches[1]
    }
    return $null
}

function Parse-LogStatus {
    param([string]$LogPath)

    $content = Get-Content $LogPath -Raw
    if ($content -match '\*\*Status:\*\*\s*(\w+)') {
        return $Matches[1]
    }
    return $null
}

# --- Main ---

Write-Host "=== Backfill Missing costs.csv ===" -ForegroundColor Cyan
Write-Host "Plans directory: $PlansDir"
if ($WhatIf) { Write-Host "[DRY RUN] No files will be written" -ForegroundColor Yellow }
Write-Host ""

# Ensure powershell-yaml is available for plan.yaml parsing
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    Write-Host "Installing powershell-yaml module..." -ForegroundColor Yellow
    Install-Module -Name powershell-yaml -Force -Scope CurrentUser -AllowClobber
}
Import-Module powershell-yaml -ErrorAction Stop

# Step 1: Identify affected plans
$dateMin = [DateTime]::Parse("2026-04-03T00:00:00Z", $null, [System.Globalization.DateTimeStyles]::AdjustToUniversal)
$dateMax = [DateTime]::Parse("2026-04-05T23:59:59Z", $null, [System.Globalization.DateTimeStyles]::AdjustToUniversal)

$allPlanDirs = Get-ChildItem $PlansDir -Directory | Sort-Object Name

$affectedPlans = @()
foreach ($planDir in $allPlanDirs) {
    $planYamlPath = Join-Path $planDir.FullName "plan.yaml"
    if (-not (Test-Path $planYamlPath)) { continue }

    $yaml = Get-Content $planYamlPath -Raw | ConvertFrom-Yaml

    $created = $null
    try {
        $created = [DateTime]::Parse($yaml.created, $null, [System.Globalization.DateTimeStyles]::AssumeUniversal -bor [System.Globalization.DateTimeStyles]::AdjustToUniversal)
    }
    catch { continue }

    $isInDateRange = $created -ge $dateMin -and $created -le $dateMax
    $hasCostsCsv = Test-Path (Join-Path $planDir.FullName "costs.csv")
    $isFinished = $yaml.state -in @("Completed", "Failed", "ReadyForReview")

    if ($isInDateRange -and (-not $hasCostsCsv) -and $isFinished) {
        $affectedPlans += @{
            Dir     = $planDir
            Yaml    = $yaml
            Created = $created
        }
    }
}

Write-Host "Plans scanned: $($allPlanDirs.Count)"
Write-Host "Affected plans found: $($affectedPlans.Count)"
Write-Host ""

# Step 2-3: Find session files and calculate costs
$totalRecoveredTokens = 0
$totalRecoveredCost = 0.0
$plansWithSessions = 0
$plansWithoutSessions = 0
$csvFilesCreated = 0

foreach ($plan in $affectedPlans) {
    $planName = $plan.Dir.Name
    $project = $plan.Yaml.project
    Write-Host "--- $planName ---" -ForegroundColor White

    # Read log files to find promptware executions
    $logsDir = Join-Path $plan.Dir.FullName "logs"
    if (-not (Test-Path $logsDir)) {
        Write-Host "  No logs directory found, skipping" -ForegroundColor Yellow
        $plansWithoutSessions++
        continue
    }

    $logFiles = Get-ChildItem $logsDir -Filter "*.md" | Sort-Object Name
    $costEntries = @()
    $planHasAnySession = $false
    $seenSessions = @{}  # Track session files already counted to avoid duplicates

    foreach ($logFile in $logFiles) {
        # Parse promptware type from filename (e.g., "001-ExecutePlan.md" -> "ExecutePlan")
        if ($logFile.Name -notmatch '^\d+-(\w+)\.md$') { continue }
        $promptware = $Matches[1]

        # Only process completed runs
        $status = Parse-LogStatus $logFile.FullName
        if ($status -ne "Completed") { continue }

        # Find Claude project directory for this promptware
        $workDir = Get-ClaudeProjectDir -Promptware $promptware -Project $project
        $claudeDir = ConvertTo-ClaudeProjectPath $workDir

        if (-not (Test-Path $claudeDir)) {
            Write-Host "  $($logFile.Name): Claude project dir not found: $claudeDir" -ForegroundColor Yellow
            continue
        }

        # Try SessionId lookup first (available for plans after 2026-04-05)
        $sessionId = Parse-LogSessionId $logFile.FullName
        $sessionFile = $null
        $matchMethod = $null

        if ($sessionId) {
            $sessionFile = Find-SessionFileById -ProjectDir $claudeDir -SessionId $sessionId
            if ($sessionFile) {
                $matchMethod = "SessionId"
            }
        }

        # Fallback to timestamp matching if SessionId not found or didn't work
        if (-not $sessionFile) {
            $completionTime = Parse-LogCompletionTime $logFile.FullName
            if (-not $completionTime) {
                Write-Host "  $($logFile.Name): Could not parse completion time or SessionId" -ForegroundColor Yellow
                continue
            }

            $sessionFile = Find-SessionFile -ProjectDir $claudeDir -CompletionTime $completionTime
            if ($sessionFile) {
                $matchMethod = "Timestamp"
            }
        }

        if (-not $sessionFile) {
            $timeInfo = if ($completionTime) { " ($($completionTime.ToString('HH:mm:ss')))" } else { "" }
            Write-Host "  $promptware${timeInfo}: No matching session file" -ForegroundColor Yellow
            continue
        }

        # Skip if we already counted this session file (duplicate log entries)
        if ($seenSessions.ContainsKey($sessionFile.FullName)) { continue }
        $seenSessions[$sessionFile.FullName] = $true

        # Calculate costs
        $cost = Calculate-FileCost $sessionFile.FullName
        if ($cost.TotalCost -le 0) {
            Write-Host "  $promptware ($($completionTime.ToString('HH:mm:ss'))): Session file found but zero cost" -ForegroundColor Yellow
            continue
        }

        $planHasAnySession = $true
        $costEntries += @{
            Promptware = $promptware
            Tokens     = $cost.TotalTokens
            Cost       = $cost.TotalCost
            SessionFile = $sessionFile.Name
        }

        $costFormatted = "{0:F4}" -f $cost.TotalCost
        Write-Host "  ${promptware}: $($cost.TotalTokens) tokens, `$$costFormatted (session: $($sessionFile.Name), method: $matchMethod)" -ForegroundColor Green
    }

    if ($costEntries.Count -gt 0) {
        $plansWithSessions++
        $planTokens = ($costEntries | Measure-Object -Property Tokens -Sum).Sum
        $planCost = ($costEntries | Measure-Object -Property Cost -Sum).Sum
        $totalRecoveredTokens += $planTokens
        $totalRecoveredCost += $planCost

        if (-not $WhatIf) {
            # Write costs.csv
            $csvPath = Join-Path $plan.Dir.FullName "costs.csv"
            "Promptware,Tokens,Cost" | Set-Content $csvPath -Encoding UTF8
            foreach ($entry in $costEntries) {
                $costFormatted = "{0:F4}" -f $entry.Cost
                "$($entry.Promptware),$($entry.Tokens),$costFormatted" | Add-Content $csvPath -Encoding UTF8
            }
            $csvFilesCreated++
            Write-Host "  -> Wrote costs.csv ($($costEntries.Count) entries)" -ForegroundColor Cyan
        }
        else {
            Write-Host "  -> Would write costs.csv ($($costEntries.Count) entries)" -ForegroundColor Cyan
        }
    }
    else {
        $plansWithoutSessions++
        Write-Host "  No session files matched" -ForegroundColor Yellow
    }

    Write-Host ""
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backfill Summary:" -ForegroundColor Cyan
Write-Host "  Plans scanned:          $($allPlanDirs.Count)"
Write-Host "  Affected plans:         $($affectedPlans.Count)"
Write-Host "  Session files found:    $plansWithSessions"
Write-Host "  Session files not found: $plansWithoutSessions"
Write-Host "  Total tokens recovered: $("{0:N0}" -f $totalRecoveredTokens)"
Write-Host "  Total cost recovered:   `$$("{0:F2}" -f $totalRecoveredCost)"
if ($WhatIf) {
    Write-Host "  costs.csv files:        $plansWithSessions (would create)" -ForegroundColor Yellow
}
else {
    Write-Host "  costs.csv files created: $csvFilesCreated"
}
Write-Host "========================================" -ForegroundColor Cyan
