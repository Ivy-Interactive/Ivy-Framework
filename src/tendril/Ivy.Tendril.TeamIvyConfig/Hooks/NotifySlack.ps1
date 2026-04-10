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

# Ensure powershell-yaml is available
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    Install-Module -Name powershell-yaml -RequiredVersion 0.4.12 -Force -Scope CurrentUser
}
Import-Module powershell-yaml -ErrorAction Stop

$planContent = Get-Content $planYamlPath -Raw
$plan = ConvertFrom-Yaml $planContent

$title = if ($plan.title) { $plan.title } else { "" }
$project = if ($plan.project) { $plan.project } else { "" }
$prs = if ($plan.prs) { @($plan.prs) } else { @() }

if ($prs.Count -eq 0) {
    Write-Output "No PRs found in plan.yaml, skipping Slack notification"
    exit 0
}

# Read config.yaml to find slackEmoji for the project
$configPath = $env:TENDRIL_CONFIG
if (-not $configPath -or -not (Test-Path $configPath)) {
    Write-Error "TENDRIL_CONFIG not set or config.yaml not found at $configPath"
    exit 1
}

$slackEmoji = ""
if (Test-Path $configPath) {
    $configContent = Get-Content $configPath -Raw
    # Quote unquoted %VAR% patterns that YAML rejects (% is a directive indicator)
    $configContent = $configContent -replace '(?m)(?<=:\s+)(%\w+%.*)$', '''$1'''
    $configContent = $configContent -replace '(?m)^(\s*-\s+)(%\w+%.*)$', '$1''$2'''
    $config = ConvertFrom-Yaml $configContent
    $projectConfig = $config.projects | Where-Object { $_.name -eq $project } | Select-Object -First 1
    if ($projectConfig -and $projectConfig.meta -and $projectConfig.meta.slackEmoji) {
        $slackEmoji = $projectConfig.meta.slackEmoji
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
