# Ensure claude CLI is on the PATH
$homeDir = $null
if ($IsWindows) { $homeDir = $env:USERPROFILE } else { $homeDir = $env:HOME }
$claudeDir = $null
if ($homeDir) { $claudeDir = Join-Path $homeDir ".local/bin" }
if ($claudeDir -and (Test-Path $claudeDir)) {
    if ($env:PATH -notlike "*$claudeDir*") {
        $sep = if ($IsWindows) { ";" } else { ":" }
        $env:PATH = "$claudeDir$sep$env:PATH"
    }
}

# Read plans directory from config.yaml
$script:ConfigPath = Join-Path (Split-Path (Split-Path $PSScriptRoot)) "config.yaml"
$script:PlansDir = $null
if (Test-Path $script:ConfigPath) {
    try {
        $configYaml = Get-Content $script:ConfigPath -Raw
        # First check explicit planFolder
        $pfMatch = [regex]::Match($configYaml, '(?m)^planFolder:\s*(.+)$')
        if ($pfMatch.Success) {
            $script:PlansDir = $pfMatch.Groups[1].Value.Trim().TrimEnd('\', '/')
        }
        # Otherwise derive from tendrilData
        if (-not $script:PlansDir) {
            $tdMatch = [regex]::Match($configYaml, '(?m)^tendrilData:\s*(.+)$')
            if ($tdMatch.Success) {
                $script:PlansDir = Join-Path $tdMatch.Groups[1].Value.Trim().TrimEnd('\', '/') "Plans"
            }
        }
    }
    catch { }
}
if (-not $script:PlansDir) {
    # Fallback to temp directory if config cannot be read
    $fallbackRoot = Join-Path ([System.IO.Path]::GetTempPath()) "Tendril"
    $script:PlansDir = Join-Path $fallbackRoot "Plans"
}

function GetProgramFolder {
    param([string]$ScriptPath)

    $scriptName = [System.IO.Path]::GetFileNameWithoutExtension($ScriptPath)
    $scriptFolder = Join-Path (Split-Path $ScriptPath) $scriptName
    if (-not (Test-Path $scriptFolder)) {
        New-Item -ItemType Directory -Path $scriptFolder | Out-Null
    }
    return $scriptFolder
}

function GetNextLogFile {
    param([string]$ProgramFolder)

    $logsFolder = Join-Path $ProgramFolder "Logs"
    if (-not (Test-Path $logsFolder)) {
        New-Item -ItemType Directory -Path $logsFolder | Out-Null
    }

    $existing = Get-ChildItem -Path $logsFolder -Filter "*.md" -File |
    Where-Object { $_.BaseName -match '^\d+$' } |
    ForEach-Object { [int]$_.BaseName } |
    Sort-Object -Descending |
    Select-Object -First 1

    $next = if ($existing) { $existing + 1 } else { 1 }
    return Join-Path $logsFolder ("{0:D5}.md" -f $next)
}

function PrepareFirmware {
    param(
        [string]$ScriptRoot,
        [string]$LogFile,
        [string]$ProgramFolder,
        [hashtable]$Values = @{}
    )

    # Auto-inject common values
    if (-not $Values.ContainsKey("CurrentTime")) {
        $Values["CurrentTime"] = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    }
    if (-not $Values.ContainsKey("ConfigPath")) {
        $Values["ConfigPath"] = $script:ConfigPath
    }

    $header = ($Values.GetEnumerator() | Sort-Object Name | ForEach-Object { "$($_.Key): $($_.Value)" }) -join "`n"

    $sharedFolder = Join-Path $ScriptRoot ".shared"
    $firmware = Get-Content "$sharedFolder\Firmware.md" -Raw
    $firmware = $firmware.Replace("[HEADER]", $header)
    $firmware = $firmware.Replace("[LOGFILE]", $LogFile)
    $firmware = $firmware.Replace("[PROGRAMFOLDER]", $ProgramFolder)
    $firmware = $firmware.Replace("[SHAREDFOLDER]", $sharedFolder)

    $promptFile = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $promptFile -Value $firmware -NoNewline -Encoding UTF8
    return $promptFile
}

function AllocatePlanId {
    $counterFile = Join-Path $script:PlansDir ".counter"
    if (-not (Test-Path $script:PlansDir)) {
        New-Item -ItemType Directory -Path $script:PlansDir | Out-Null
    }

    # Use a lock file to prevent concurrent access
    $lockFile = Join-Path $script:PlansDir ".counter.lock"
    $lock = $null
    try {
        # Retry acquiring lock for up to 10 seconds
        for ($i = 0; $i -lt 20; $i++) {
            try {
                $lock = [System.IO.File]::Open($lockFile, [System.IO.FileMode]::OpenOrCreate, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
                break
            }
            catch {
                Start-Sleep -Milliseconds 500
            }
        }
        if (-not $lock) {
            Write-Host "Error: Could not acquire counter lock" -ForegroundColor Red
            exit 1
        }

        $counter = if (Test-Path $counterFile) { [int](Get-Content $counterFile).Trim() } else { 1087 }
        $id = $counter
        Set-Content -Path $counterFile -Value ($counter + 1).ToString() -Encoding UTF8
        return $id
    }
    finally {
        if ($lock) { $lock.Close() }
    }
}

function UpdatePlanState {
    param(
        [string]$PlanFolderPath,
        [string]$NewState
    )

    $planYamlPath = Join-Path $PlanFolderPath "plan.yaml"
    if (-not (Test-Path $planYamlPath)) {
        Write-Host "plan.yaml not found: $planYamlPath" -ForegroundColor Red
        return
    }

    $content = Get-Content $planYamlPath -Raw
    $now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    $content = $content -replace '(?m)^state:\s*.*$', "state: $NewState"
    $content = $content -replace '(?m)^updated:\s*.*$', "updated: $now"
    Set-Content -Path $planYamlPath -Value $content -Encoding UTF8
    Write-Host "Plan state updated to: $NewState" -ForegroundColor Cyan
}

function WritePlanLog {
    param(
        [string]$PlanFolderPath,
        [string]$Action,
        [string]$Summary = ""
    )

    $logsDir = Join-Path $PlanFolderPath "logs"
    if (-not (Test-Path $logsDir)) {
        New-Item -ItemType Directory -Path $logsDir | Out-Null
    }

    $existing = Get-ChildItem -Path $logsDir -Filter "*.md" -File |
    ForEach-Object {
        $dashIdx = $_.BaseName.IndexOf('-')
        if ($dashIdx -ge 0) { [int]$_.BaseName.Substring(0, $dashIdx) } else { 0 }
    } |
    Sort-Object -Descending |
    Select-Object -First 1

    $next = if ($existing) { $existing + 1 } else { 1 }
    $logPath = Join-Path $logsDir ("{0:D3}-{1}.md" -f $next, $Action)

    $now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    $logContent = "# $Action`n`n- **Completed:** $now`n- **Status:** Completed"
    if ($Summary) {
        $logContent += "`n`n$Summary"
    }

    Set-Content -Path $logPath -Value $logContent -Encoding UTF8
    Write-Host "Log written: $logPath" -ForegroundColor Green
}

function CollectArgs {
    param(
        [string[]]$Arguments,
        [switch]$Optional
    )

    $Arguments = $Arguments | Where-Object { $_ -ne $null -and $_.Trim() -ne "" }
    $joined = ($Arguments -join " ").Trim()

    if ($joined -eq "" -and $Optional) {
        return "(No Args)"
    }

    if ($joined -eq "") {
        Write-Host "Error: No arguments provided." -ForegroundColor Red
        exit 1
    }

    return $joined
}

function ValidatePlanPath {
    param([string]$PlanPath)

    if (-not (Test-Path $PlanPath)) {
        Write-Host "Plan folder not found: $PlanPath" -ForegroundColor Red
        exit 1
    }

    $planYamlPath = Join-Path $PlanPath "plan.yaml"
    if (-not (Test-Path $planYamlPath)) {
        Write-Host "plan.yaml not found in: $PlanPath" -ForegroundColor Red
        exit 1
    }

    return $planYamlPath
}

function ReadPlanProject {
    param([string]$PlanYamlPath)

    $content = Get-Content $PlanYamlPath -Raw
    $match = [regex]::Match($content, '(?m)^project:\s*(.+)$')
    $project = if ($match.Success) { $match.Groups[1].Value.Trim() } else { "[Auto]" }
    return @{ Content = $content; Project = $project }
}

function GetProjectWorkDir {
    param([string]$Project)

    if (Test-Path $script:ConfigPath) {
        try {
            $yaml = Get-Content $script:ConfigPath -Raw
            # Match the project block and extract the first repo path
            $pattern = "(?s)- name:\s*$([regex]::Escape($Project))\s+repos:\s*\n((?:\s+-.+\n?)+)"
            $match = [regex]::Match($yaml, $pattern)
            if ($match.Success) {
                # Try new format: - path: D:\...
                $pathLine = [regex]::Match($match.Groups[1].Value, '(?m)path:\s*(.+)$')
                if ($pathLine.Success) {
                    return $pathLine.Groups[1].Value.Trim()
                }
                # Fallback: old format - D:\...
                $repoLine = [regex]::Match($match.Groups[1].Value, '^\s+-\s*(.+)$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
                if ($repoLine.Success) {
                    return $repoLine.Groups[1].Value.Trim()
                }
            }
        }
        catch { }
    }

    return (Get-Location).Path
}

function InvokePromptwareAgent {
    param(
        [string]$ScriptRoot,
        [string]$ProgramFolder,
        [string]$LogFile,
        [hashtable]$FirmwareValues,
        [string]$WorkDir = $ProgramFolder,
        [string]$PlanPath = $null,
        [string]$Action = $null,
        [string]$FinalState = $null,
        [string[]]$ExtraAgentArgs = @(),
        [string]$Promptware = ""
    )

    # Generate session-id for cost tracking
    $sessionId = [guid]::NewGuid().ToString()
    $FirmwareValues["ClaudeSessionId"] = $sessionId

    $promptFile = PrepareFirmware $ScriptRoot $LogFile $ProgramFolder $FirmwareValues
    $agent = GetAgentCommandFromConfig -Promptware $Promptware

    # Pass --session-id when using claude CLI
    if ($agent.Executable -eq "claude") {
        $ExtraAgentArgs += @("--session-id", $sessionId)
    }

    # Create raw output log
    $rawLogFile = [System.IO.Path]::ChangeExtension($LogFile, ".raw.jsonl")

    Write-Host "Starting Agent..."
    if ($Action) { SendStatusMessage "Running $Action" }
    Push-Location $WorkDir
    $heartbeat = Start-Heartbeat
    try {
        $startTs = (Get-Date).ToUniversalTime().ToString("o")
        Add-Content -Path $rawLogFile -Value "[tendril] Claude invocation started at $startTs" -Encoding UTF8
        Add-Content -Path $rawLogFile -Value "[tendril] Command: $($agent.Executable) $($agent.Args -join ' ') $($ExtraAgentArgs -join ' ')" -Encoding UTF8

        $output = & $agent.Executable @($agent.Args) @ExtraAgentArgs -- (Get-Content $promptFile -Raw) 2>&1 |
        ForEach-Object {
            $line = if ($_ -is [System.Management.Automation.ErrorRecord]) {
                "[stderr] $_"
            }
            else {
                "$_"
            }
            Add-Content -Path $rawLogFile -Value $line -Encoding UTF8
            $_
        }
        $output | Write-Output
    }
    finally {
        Stop-Heartbeat $heartbeat
    }
    Pop-Location

    # Extract summary from agent's stream-json result
    $summary = ""
    if ($output) {
        $resultLine = ($output | Select-String '"type":"result"' | Select-Object -Last 1)
        if ($resultLine) {
            try {
                $resultJson = $resultLine.Line | ConvertFrom-Json
                $summary = $resultJson.result
            }
            catch { }
        }
    }

    # Report cost for this session
    $costData = ReportSessionCost $sessionId
    if ($costData -and $PlanPath) {
        LogPlanCost $PlanPath $Action $costData.Tokens $costData.Cost
    }

    if ($Action) { SendStatusMessage "$Action Completed" }

    if ($PlanPath -and $Action) {
        WritePlanLog $PlanPath $Action $summary
    }
    if ($PlanPath -and $FinalState) {
        UpdatePlanState $PlanPath $FinalState
    }

    Remove-Item $promptFile -ErrorAction SilentlyContinue
}

function ReportSessionCost {
    param([string]$SessionId)

    if (-not $SessionId) { return $null }

    try {
        # Find session file
        $claudeProjectsDir = Join-Path $env:USERPROFILE ".claude\projects"
        $sessionFile = Get-ChildItem -Path $claudeProjectsDir -Recurse -Filter "$SessionId.jsonl" |
        Where-Object { $_.FullName -notlike "*\subagents\*" } |
        Select-Object -First 1

        if (-not $sessionFile) { return $null }

        # Load pricing from config.yaml
        $configPath = $script:ConfigPath
        if (-not $configPath -or -not (Test-Path $configPath)) {
            $configPath = Join-Path (Split-Path (Split-Path $PSScriptRoot)) "config.yaml"
        }

        $modelPricing = $null
        if (Test-Path $configPath) {
            try {
                $config = Get-Content $configPath -Raw | ConvertFrom-Yaml
                $modelPricing = $config.modelPricing
            }
            catch {
                Write-Warning "Could not load modelPricing from config.yaml: $_"
            }
        }

        if (-not $modelPricing) {
            Write-Warning "No modelPricing found in config.yaml, using Opus 4 defaults"
            $modelPricing = @{
                "claude-opus-4" = @{ input=15.00; output=75.00; cacheWrite=18.75; cacheRead=1.50 }
            }
        }

        $totalCost = 0.0
        $totalTokens = 0

        # Parse session and calculate cost
        $filesToParse = @($sessionFile.FullName)

        # Add subagent sessions
        $subagentDir = Join-Path ($sessionFile.FullName -replace '\.jsonl$', '') "subagents"
        if (Test-Path $subagentDir) {
            $filesToParse += (Get-ChildItem $subagentDir -Filter "*.jsonl").FullName
        }

        foreach ($filePath in $filesToParse) {
            foreach ($line in (Get-Content $filePath)) {
                if (-not $line.Trim()) { continue }
                try {
                    $obj = $line | ConvertFrom-Json
                    if ($obj.type -eq "assistant" -and $obj.message.usage) {
                        $u = $obj.message.usage
                        $model = $obj.message.model ?? "claude-opus-4"

                        # Find matching pricing tier
                        $prices = $null
                        foreach ($key in $modelPricing.Keys) {
                            if ($model -like "*$key*") {
                                $prices = $modelPricing[$key]
                                break
                            }
                        }

                        # Fallback to Opus if no match
                        if (-not $prices) {
                            $prices = $modelPricing["claude-opus-4"]
                        }

                        # Convert per-million to per-token
                        $priceInput = $prices.input * 1e-6
                        $priceOutput = $prices.output * 1e-6
                        $priceCacheWrite = $prices.cacheWrite * 1e-6
                        $priceCacheRead = $prices.cacheRead * 1e-6

                        # Calculate cost
                        $inputTokens = ($u.input_tokens ?? 0)
                        $outputTokens = ($u.output_tokens ?? 0)
                        $cacheReadTokens = ($u.cache_read_input_tokens ?? 0)

                        $totalTokens += $inputTokens + $outputTokens + $cacheReadTokens
                        $totalCost += $inputTokens * $priceInput
                        $totalCost += $outputTokens * $priceOutput
                        $totalCost += $cacheReadTokens * $priceCacheRead

                        # Handle cache creation
                        if ($u.cache_creation) {
                            $cache5m = ($u.cache_creation.ephemeral_5m_input_tokens ?? 0)
                            $cache1h = ($u.cache_creation.ephemeral_1h_input_tokens ?? 0)
                            $totalTokens += $cache5m + $cache1h
                            $totalCost += ($cache5m + $cache1h) * $priceCacheWrite
                        } else {
                            $cacheCreation = ($u.cache_creation_input_tokens ?? 0)
                            $totalTokens += $cacheCreation
                            $totalCost += $cacheCreation * $priceCacheWrite
                        }
                    }
                }
                catch { }
            }
        }

        # Report to controller (accumulates)
        $jobId = $env:TENDRIL_JOB_ID
        $url = $env:TENDRIL_URL
        if ($jobId -and $url) {
            $body = @{ cost = [math]::Round($totalCost, 2) } | ConvertTo-Json
            Invoke-RestMethod -Uri "$url/api/jobs/$jobId/cost" -Method Post `
                -Body $body -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
        }

        return @{ Tokens = $totalTokens; Cost = $totalCost }
    }
    catch {
        return $null
    }
}

function LogPlanCost {
    param(
        [string]$PlanFolder,
        [string]$Promptware,
        [int]$Tokens,
        [double]$Cost
    )
    if (-not $PlanFolder -or -not (Test-Path $PlanFolder)) { return }

    $csvPath = Join-Path $PlanFolder "costs.csv"
    if (-not (Test-Path $csvPath)) {
        "Promptware,Tokens,Cost" | Set-Content $csvPath -Encoding UTF8
    }
    "$Promptware,$Tokens,$("{0:F4}" -f $Cost)" | Add-Content $csvPath -Encoding UTF8
}

function SendStatusMessage {
    param([string]$Message)

    $jobId = $env:TENDRIL_JOB_ID
    $url = $env:TENDRIL_URL
    if (-not $jobId -or -not $url) { return }

    try {
        $body = @{ message = $Message } | ConvertTo-Json
        Invoke-RestMethod -Uri "$url/api/jobs/$jobId/status" -Method Post -Body $body -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
    }
    catch { }
}

function Start-Heartbeat {
    param([int]$IntervalSeconds = 120)
    $job = Start-ThreadJob -ScriptBlock {
        param($interval)
        while ($true) {
            Start-Sleep -Seconds $interval
            $ts = (Get-Date).ToUniversalTime().ToString("o")
            [Console]::Out.WriteLine("{""type"":""heartbeat"",""timestamp"":""$ts""}")
            [Console]::Out.Flush()
        }
    } -ArgumentList $IntervalSeconds
    return $job
}

function Stop-Heartbeat {
    param($Job)
    if ($Job) {
        Stop-Job $Job -ErrorAction SilentlyContinue
        Remove-Job $Job -ErrorAction SilentlyContinue
    }
}

function GetAgentCommandFromConfig {
    param([string]$Promptware = "")

    $configPath = $script:ConfigPath
    $raw = "claude --print --verbose --output-format stream-json --dangerously-skip-permissions"
    $allowedTools = @()

    if (Test-Path $configPath) {
        try {
            $yaml = Get-Content $configPath -Raw
            # Regex match as first pass or fallback
            $pattern = "(?m)^agentCommand:\s*(.+)$"
            $match = [regex]::Match($yaml, $pattern)
            if ($match.Success) {
                $raw = $match.Groups[1].Value.Trim()
            }

            # Parse config with ConvertFrom-Yaml for structured access
            $config = $yaml | ConvertFrom-Yaml

            if ($config.agentCommand) {
                $raw = $config.agentCommand
            }

            if ($Promptware -and $config.promptwares.$Promptware) {
                $pwConfig = $config.promptwares.$Promptware

                # Apply model override
                if ($pwConfig.model) {
                    # Strip any existing --model from raw if we're overriding it
                    $raw = $raw -replace '--model\s+\S+', ''
                    $raw += " --model $($pwConfig.model)"
                }

                # Apply allowedTools with %TENDRIL_HOME% substitution
                if ($pwConfig.allowedTools) {
                    $tendrilPath = if ($config.tendrilData) { $config.tendrilData -replace '\\', '/' } else { "" }
                    foreach ($tool in $pwConfig.allowedTools) {
                        $resolvedTool = $tool -replace '%TENDRIL_HOME%', $tendrilPath
                        $allowedTools += $resolvedTool
                    }
                }
            }
        }
        catch {
            Write-Host "Warning: Could not parse agentCommand from config.yaml" -ForegroundColor Yellow
        }
    }

    # Build args with quote-aware parsing
    $parts = @()
    $current = ""
    $inQuote = $false
    foreach ($char in $raw.ToCharArray()) {
        if ($char -eq '"' -and !$inQuote) { $inQuote = $true; continue }
        elseif ($char -eq '"' -and $inQuote) { $inQuote = $false; continue }
        elseif ($char -eq ' ' -and !$inQuote -and $current) {
            $parts += $current; $current = ""; continue
        }
        elseif ($char -eq ' ' -and !$inQuote) { continue }
        $current += $char
    }
    if ($current) { $parts += $current }

    # Append allowedTools as individual args
    $args = $parts[1..($parts.Length - 1)]
    if ($allowedTools.Count -gt 0) {
        foreach ($tool in $allowedTools) {
            $args += "--allowedTools"
            $args += $tool
        }
    }

    return @{
        Executable = $parts[0]
        Args       = $args
    }
}