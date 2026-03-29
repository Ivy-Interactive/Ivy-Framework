param(
    [Parameter(Mandatory=$true)]
    [string]$Description,
    [string]$Project = "[Auto]"
)

. "$PSScriptRoot\.shared\Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath

$logFile = GetNextLogFile $programFolder
$Description | Set-Content $logFile
Write-Host "Log file: $logFile"

$sessionId = [guid]::NewGuid().ToString()

$promptFile = PrepareFirmware $PSScriptRoot $logFile $programFolder @{
    Args = $Description
    ClaudeSessionId = $sessionId
    PlansDirectory = $script:PlansDir
    Project = $Project
}

$agent = GetAgentCommandFromConfig

Write-Host "Starting Agent..."
Push-Location $programFolder
$extraArgs = @()
if ($agent.Executable -eq "claude") {
    $extraArgs += @("--session-id", $sessionId)
}
& $agent.Executable @($agent.Args) @extraArgs -- (Get-Content $promptFile -Raw)
Pop-Location

Remove-Item $promptFile
