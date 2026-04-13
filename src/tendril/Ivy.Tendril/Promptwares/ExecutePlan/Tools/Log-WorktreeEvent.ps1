param(
    [Parameter(Mandatory)]
    [ValidateSet("Creation", "CreationFailed", "CleanupAttempt", "CleanupSuccess", "CleanupFailed")]
    [string]$Event,

    [Parameter(Mandatory)]
    [string]$PlanId,

    [Parameter(Mandatory)]
    [string]$WorktreePath,

    [hashtable]$Metadata = @{}
)

$logDir = Join-Path $env:TENDRIL_HOME "Logs"
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

$logPath = Join-Path $logDir "worktrees.log"
$timestamp = (Get-Date).ToUniversalTime().ToString("o")
$metaStr = ($Metadata.GetEnumerator() | ForEach-Object { "$($_.Key)=`"$($_.Value)`"" }) -join " "
$entry = "[$timestamp] [$PlanId] [$Event] worktree=`"$WorktreePath`" $metaStr".TrimEnd()

Add-Content -Path $logPath -Value $entry -Encoding UTF8
