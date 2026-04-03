# NotifySlack.ps1 — After-hook for MakePr to send Slack notifications
# Receives: $env:TENDRIL_PLAN_FOLDER, $env:TENDRIL_JOB_STATUS

$ErrorActionPreference = "Stop"
$env:NO_COLOR = "1"

$planFolder = $env:TENDRIL_PLAN_FOLDER
if (-not $planFolder) {
    Write-Error "TENDRIL_PLAN_FOLDER not set"
    exit 1
}

$slackChannel = "done-by-niels"

# Read plan.yaml
$planYamlPath = Join-Path $planFolder "plan.yaml"
if (-not (Test-Path $planYamlPath)) {
    Write-Error "plan.yaml not found at $planYamlPath"
    exit 1
}

$planContent = Get-Content $planYamlPath -Raw

# Parse title
$title = ""
if ($planContent -match "(?m)^title:\s*(.+)$") {
    $title = $Matches[1].Trim()
}

# Parse project
$project = ""
if ($planContent -match "(?m)^project:\s*(.+)$") {
    $project = $Matches[1].Trim()
}

# Parse PRs
$prs = @()
if ($planContent -match "(?ms)^prs:\s*\n((?:- .+\n?)+)") {
    $prs = [regex]::Matches($Matches[1], "- (.+)") | ForEach-Object { $_.Groups[1].Value.Trim() }
}

if ($prs.Count -eq 0) {
    Write-Output "No PRs found in plan.yaml, skipping Slack notification"
    exit 0
}

# Read config.yaml to find slackEmoji for the project
$configPath = Join-Path (Split-Path (Split-Path $planFolder -Parent) -Parent) "config.yaml"
# Try standard config location; fall back to repo path
if (-not (Test-Path $configPath)) {
    $configPath = "D:\Repos\_Ivy\Ivy-Tendril\config.yaml"
}

$slackEmoji = ""
if (Test-Path $configPath) {
    $configContent = Get-Content $configPath -Raw
    # Find the project section and extract slackEmoji
    if ($configContent -match "(?ms)name:\s*$([regex]::Escape($project)).*?slackEmoji:\s*""([^""]+)""") {
        $slackEmoji = $Matches[1]
    }
}

# Check for custom PR options (slackComment)
$slackComment = ""
$customOptionsPath = Join-Path $planFolder ".custom-pr-options.yaml"
if (Test-Path $customOptionsPath) {
    $optionsContent = Get-Content $customOptionsPath -Raw
    if ($optionsContent -match "(?m)^slackComment:\s*(.+)$") {
        $slackComment = $Matches[1].Trim().Trim('"', "'")
    }
}

# Build PR links for Slack
$prLinks = @()
foreach ($pr in $prs) {
    # Extract owner/repo#number from URL like https://github.com/owner/repo/pull/123
    if ($pr -match "github\.com/([^/]+/[^/]+)/pull/(\d+)") {
        $repoSlug = $Matches[1]
        $prNumber = $Matches[2]
        $prLinks += "<$pr|$repoSlug#$prNumber>"
    } else {
        $prLinks += $pr
    }
}
$prLinksText = $prLinks -join ", "

# Build project display with emoji
$projectDisplay = if ($slackEmoji) { "$slackEmoji $project" } else { $project }

# Check for screenshot URLs in artifacts
$screenshotUrl = ""
$screenshotsDir = Join-Path $planFolder "artifacts/screenshots"
if (Test-Path $screenshotsDir) {
    $pngFiles = Get-ChildItem -Path $screenshotsDir -Filter "*.png" -File | Sort-Object Name | Select-Object -First 1
    if ($pngFiles) {
        # Upload the screenshot to get a URL
        $raw = & storage upload ivy-tendril $pngFiles.FullName 2>&1 | Out-String
        $clean = $raw -replace "\x1B\[[0-9;]*m", ""
        $noWs = $clean -replace "\s+", ""
        if ($noWs -match "(https://stivytelemetry\.blob\.core\.windows\.net/[^""]+)") {
            $screenshotUrl = $Matches[1].Trim()
        }
    }
}

# Build message text
$messageText = "*Title:* $title`n*Project:* $projectDisplay`n*PR:* $prLinksText"
if ($slackComment) {
    $messageText += "`n$slackComment"
}

if ($screenshotUrl) {
    # Block Kit JSON with image accessory
    $escapedText = $messageText -replace '\\', '\\\\' -replace '"', '\"'
    $escapedUrl = $screenshotUrl -replace '"', '\"'
    $json = '{"blocks":[{"type":"section","text":{"type":"mrkdwn","text":"' + $escapedText + '"},"accessory":{"type":"image","image_url":"' + $escapedUrl + '","alt_text":"screenshot"}}]}'
    & notify slack $slackChannel --json $json
} else {
    # Plain text
    & notify slack $slackChannel --message $messageText
}
