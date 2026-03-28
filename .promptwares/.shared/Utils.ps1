# Ensure claude CLI is on the PATH
$claudeDir = Join-Path $env:USERPROFILE ".local\bin"
if (Test-Path $claudeDir) {
    if ($env:PATH -notlike "*$claudeDir*") {
        $env:PATH = "$claudeDir;$env:PATH"
    }
}

$script:PlansDir = "D:\Plans"

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

    $header = ($Values.GetEnumerator() | Sort-Object Name | ForEach-Object { "$($_.Key): $($_.Value)" }) -join "`n"

    $sharedFolder = Join-Path $ScriptRoot ".shared"
    $firmware = Get-Content "$sharedFolder\Firmware.md" -Raw
    $firmware = $firmware.Replace("[HEADER]", $header)
    $firmware = $firmware.Replace("[LOGFILE]", $LogFile)
    $firmware = $firmware.Replace("[PROGRAMFOLDER]", $ProgramFolder)
    $firmware = $firmware.Replace("[SHAREDFOLDER]", $sharedFolder)

    $promptFile = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $promptFile -Value $firmware -NoNewline
    return $promptFile
}

function GetLatestSessionId {
    param([string]$Path = (Join-Path (Get-Location).Path ".ivy\session.ldjson"))

    if (-not (Test-Path $Path)) {
        Write-Host "Error: $Path not found." -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }

    $lastLine = (Get-Content $Path -Tail 1).Trim()
    if ($lastLine -eq "") {
        Write-Host "Error: $Path is empty." -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }

    return ($lastLine | ConvertFrom-Json).sessionId
}

function InvokeOrOutputPrompt {
    param(
        [string]$ProgramFolder,
        [string]$PromptFile,
        [string]$Prompt,
        [string]$LogFile,
        [switch]$GetPrompt,
        [switch]$GetTaskPrompt,
        [string[]]$ExtraClaudeArgs = @()
    )

    if ($GetPrompt) {
        Get-Content $PromptFile -Raw
        Remove-Item $PromptFile
        return
    }

    if ($GetTaskPrompt) {
        $programMd = Join-Path $ProgramFolder "Program.md"
        $content = if (Test-Path $programMd) { Get-Content $programMd -Raw } else { "(No Program.md found)" }
        Remove-Item $PromptFile
        Write-Output @"
## Task: $([System.IO.Path]::GetFileName($ProgramFolder))

**Working Directory:** $ProgramFolder
**Log File:** $LogFile
**Args:** $Prompt

$content
"@
        return
    }

    Write-Host "Log file: $LogFile"
    Write-Host "Starting Agent..."
    Push-Location $ProgramFolder

    $firmware = Get-Content $PromptFile -Raw
    Remove-Item $PromptFile

    $env:CLAUDECODE = $null
    $env:CLAUDE_CODE_ENTRYPOINT = $null
    claude --dangerously-skip-permissions @ExtraClaudeArgs -- $firmware

    Pop-Location
    return $false
}

function CreatePlanFolder {
    param(
        [string]$Description,
        [string]$Project = "General",
        [string]$ProjectContext = ""
    )

    $counterFile = Join-Path $script:PlansDir ".counter"
    if (-not (Test-Path $script:PlansDir)) {
        New-Item -ItemType Directory -Path $script:PlansDir | Out-Null
    }

    $counter = if (Test-Path $counterFile) { [int](Get-Content $counterFile).Trim() } else { 1087 }
    $id = $counter
    Set-Content -Path $counterFile -Value ($counter + 1).ToString()

    $safeTitle = $Description
    if ($safeTitle.Length -gt 60) { $safeTitle = $safeTitle.Substring(0, 60) }
    $safeTitle = $safeTitle -replace '[^a-zA-Z0-9]', ''
    if ($safeTitle -eq '') { $safeTitle = 'Untitled' }

    $folderName = "{0:D5}-{1}" -f $id, $safeTitle
    $folderPath = Join-Path $script:PlansDir $folderName

    New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $folderPath "revisions") -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $folderPath "logs") -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $folderPath "worktrees") -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $folderPath "artifacts") -Force | Out-Null

    $now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    $yamlContent = @"
state: Draft
project: $Project
level: NiceToHave
title: "$Description"
repos: []
created: $now
updated: $now
initialPrompt: "$Description"
prs: []
commits: []
"@

    Set-Content -Path (Join-Path $folderPath "plan.yaml") -Value $yamlContent

    # Write project context if provided
    if ($ProjectContext) {
        Set-Content -Path (Join-Path $folderPath "project-context.md") -Value $ProjectContext
    }

    $revisionContent = @"
# $Description

## Problem

$Description

## Solution

## Tests

## Finish

Commit!
"@

    Set-Content -Path (Join-Path $folderPath "revisions" "001.md") -Value $revisionContent

    Write-Host "Created plan folder: $folderPath" -ForegroundColor Green
    return $folderPath
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
    Set-Content -Path $planYamlPath -Value $content
    Write-Host "Plan state updated to: $NewState" -ForegroundColor Cyan
}

function WritePlanLog {
    param(
        [string]$PlanFolderPath,
        [string]$Action
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
    $logContent = @"
# $Action

- **Completed:** $now
- **Status:** Completed
"@

    Set-Content -Path $logPath -Value $logContent
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
        $tempFile = [System.IO.Path]::GetTempFileName()
        Write-Host "No arguments provided. Opening Notepad - save the file and close it to continue."
        Start-Process -FilePath "notepad.exe" -ArgumentList $tempFile -Wait
        $joined = ((Get-Content $tempFile) -join " ").Trim()
        Remove-Item $tempFile
    }

    if ($joined -eq "") {
        Write-Host "No arguments provided. Exiting."
        exit
    }

    return $joined
}

function GetProjectContextFromConfig {
    param([string]$ProjectName)

    if ($ProjectName -eq "[Auto]" -or $ProjectName -eq "General" -or $ProjectName -eq "") {
        return ""
    }

    $configPath = Join-Path (Split-Path $PSScriptRoot) "config.yaml"
    if (-not (Test-Path $configPath)) {
        return ""
    }

    try {
        $yaml = Get-Content $configPath -Raw

        # Simple regex-based extraction of project context
        $pattern = "(?s)- name:\s*$([regex]::Escape($ProjectName))\s+repos:.*?context:\s*\|\s*(.*?)(?=\n\s{0,2}\S|\z)"
        $match = [regex]::Match($yaml, $pattern)

        if ($match.Success) {
            return $match.Groups[1].Value.Trim()
        }
    }
    catch {
        Write-Host "Warning: Could not parse project context from config.yaml" -ForegroundColor Yellow
    }

    return ""
}

function GetAgentCommandFromConfig {
    $configPath = Join-Path (Split-Path $PSScriptRoot) "config.yaml"
    if (-not (Test-Path $configPath)) {
        return "claude"
    }

    try {
        $yaml = Get-Content $configPath -Raw
        $pattern = "(?m)^agentCommand:\s*(.+)$"
        $match = [regex]::Match($yaml, $pattern)

        if ($match.Success) {
            return $match.Groups[1].Value.Trim()
        }
    }
    catch {
        Write-Host "Warning: Could not parse agentCommand from config.yaml, using 'claude'" -ForegroundColor Yellow
    }

    return "claude"
}
