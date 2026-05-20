param(
    [switch]$SkipDownloads
)

# Get the directory where this script is located
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$commitsFolder = Join-Path $scriptDir "Commits"

if (-not $SkipDownloads) {
    # Remove the Commits folder if it exists (only when downloading)
    if (Test-Path $commitsFolder) {
        Remove-Item -Path $commitsFolder -Recurse -Force
    }
}

# Ensure the Commits folder exists
if (-not (Test-Path $commitsFolder)) {
    New-Item -ItemType Directory -Path $commitsFolder -Force | Out-Null
}

# Create a markdown file named "weekly-notes-YYYY-MM-DD.md"
$currentDate = Get-Date -Format "yyyy-MM-dd"
$fileName = "weekly-notes-$currentDate.md"
$filePath = Join-Path $scriptDir $fileName

# Check if file already exists
if (Test-Path $filePath) {
    $response = Read-Host "File '$fileName' already exists. Do you want to recreate it? (y/n)"
    if ($response -ne 'y') {
        Write-Host "Keeping existing file." -ForegroundColor Yellow
    }
    else {
        $content = @"
# Ivy Framework Weekly Notes - Week of $currentDate
"@
        $content | Out-File -FilePath $filePath -Encoding UTF8
        Write-Host "File '$fileName' has been recreated." -ForegroundColor Green
    }
}
else {
    $content = @"
# Ivy Framework Weekly Notes - Week of $currentDate
"@
    $content | Out-File -FilePath $filePath -Encoding UTF8
    Write-Host "File '$fileName' has been created." -ForegroundColor Green
}

if (-not $SkipDownloads) {
    $downloadScript = Join-Path $scriptDir "DownloadCommits.ps1"
    & $downloadScript -Repo https://github.com/Ivy-Interactive/Ivy -OutputFolder $commitsFolder -LastDays 7 -Prefix ivy
    & $downloadScript -Repo https://github.com/Ivy-Interactive/Ivy-Framework -OutputFolder $commitsFolder -LastDays 7 -Prefix ivy-framework
}

$promptFile = Join-Path $scriptDir "prompt.md"
$tempPromptFile = Join-Path $scriptDir "temp-prompt.md"
$commitFiles = Join-Path $commitsFolder "*.md"

# Load the prompt template and dynamically replace {{NotesFile}} with the absolute path of the notes file
$promptContent = Get-Content $promptFile -Raw
# Using literal string .Replace to avoid regex escaping issues
$customPrompt = $promptContent.Replace('{{NotesFile}}', $filePath)
$customPrompt | Out-File -FilePath $tempPromptFile -Encoding UTF8

try {
    # Run the LLM notes generator on each commit file
    & "$scriptDir\LlmEach.ps1" $commitFiles -PromptFile $tempPromptFile -Parallel 1 -YesToAll
}
finally {
    # Ensure temporary prompt file is cleaned up
    if (Test-Path $tempPromptFile) {
        Remove-Item -Path $tempPromptFile -Force
    }
}