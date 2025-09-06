[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$Repo,
    
    [Parameter(Mandatory=$true)]
    [string]$OutputFolder,
    
    [Parameter(ParameterSetName='DateRange')]
    [datetime]$FromDate,
    
    [Parameter(ParameterSetName='DateRange')]
    [datetime]$ToDate,
    
    [Parameter(ParameterSetName='LastDays')]
    [int]$LastDays,
    
    [Parameter()]
    [string]$Prefix = ""
)

# Validate parameters
if ($PSCmdlet.ParameterSetName -eq 'DateRange') {
    if (-not $FromDate -or -not $ToDate) {
        Write-Error "Both FromDate and ToDate must be specified when using date range"
        exit 1
    }
    if ($FromDate -gt $ToDate) {
        Write-Error "FromDate must be before ToDate"
        exit 1
    }
} elseif ($PSCmdlet.ParameterSetName -eq 'LastDays') {
    if ($LastDays -le 0) {
        Write-Error "LastDays must be a positive number"
        exit 1
    }
    # Calculate date range from LastDays
    $ToDate = Get-Date
    $FromDate = $ToDate.AddDays(-($LastDays - 1)).Date
    $ToDate = $ToDate.Date.AddDays(1).AddSeconds(-1) # End of today
} else {
    Write-Error "Either specify FromDate and ToDate, or LastDays"
    exit 1
}

Write-Host "Processing commits from $($FromDate.ToString('yyyy-MM-dd')) to $($ToDate.ToString('yyyy-MM-dd'))" -ForegroundColor Cyan

# Convert to absolute path
$OutputFolder = [System.IO.Path]::GetFullPath($OutputFolder)

# Create output folder if needed
if (-not (Test-Path $OutputFolder)) {
    Write-Host "Creating output folder: $OutputFolder" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null
}

# Clone the repo to a temp directory
$tempDir = Join-Path $env:TEMP "gitclone_$(Get-Random)"
Write-Host "Cloning repository to temporary location..." -ForegroundColor Yellow

try {
    # Clone with quiet flag to suppress progress output
    git clone -q $Repo $tempDir 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to clone repository"
    }
    
    Push-Location $tempDir
    
    # Get commits in the date range
    $fromDateStr = $FromDate.ToString('yyyy-MM-dd')
    $toDateStr = $ToDate.ToString('yyyy-MM-dd')
    
    Write-Host "Fetching commits from $fromDateStr to $toDateStr..." -ForegroundColor Yellow
    
    # Get commit hashes in the date range (reverse order for oldest first)
    $commits = git log --since="$fromDateStr" --until="$toDateStr" --format="%H" --no-merges --reverse
    
    if (-not $commits) {
        Write-Host "No commits found in the specified date range" -ForegroundColor Yellow
        Pop-Location
        Remove-Item -Path $tempDir -Recurse -Force
        exit 0
    }
    
    $commitArray = $commits -split "`n" | Where-Object { $_ -ne "" }
    $totalCommits = $commitArray.Count
    Write-Host "Found $totalCommits commit(s) to process" -ForegroundColor Green
    
    $processed = 0
    foreach ($commitHash in $commitArray) {
        if ([string]::IsNullOrWhiteSpace($commitHash)) {
            continue
        }
        
        $processed++
        $shortHash = $commitHash.Substring(0, [Math]::Min(7, $commitHash.Length))
        
        # Get commit message
        $commitMessage = git log -1 --format="%B" $commitHash
        $subject = git log -1 --format="%s" $commitHash
        
        Write-Host "[$processed/$totalCommits] Processing commit $shortHash - $subject" -ForegroundColor Cyan
        
        # Extract repo owner and name from URL
        if ($Repo -match "github\.com[/:]([^/]+)/([^/\.]+)") {
            $owner = $Matches[1]
            $repoName = $Matches[2]
            $baseUrl = "https://github.com/$owner/$repoName"
        } else {
            $baseUrl = $Repo.TrimEnd('/', '.git')
        }
        
        # Get list of changed files
        $changedFiles = git diff-tree --no-commit-id --name-only -r $commitHash
        $filesList = ""
        if ($changedFiles) {
            $filesList = "`n## Changed Files`n`n"
            foreach ($file in ($changedFiles -split "`n" | Where-Object { $_ -ne "" })) {
                # Use raw.githubusercontent.com for direct file access
                if ($Repo -match "github\.com[/:]([^/]+)/([^/\.]+)") {
                    $fileUrl = "https://raw.githubusercontent.com/$owner/$repoName/$commitHash/$file"
                } else {
                    $fileUrl = "$baseUrl/blob/$commitHash/$file"
                }
                $filesList += "- [$file]($fileUrl)`n"
            }
        }
        
        # Get the diff
        $diff = git show --format="" $commitHash
        
        # Create markdown content
        $mdContent = @"
$commitMessage
$filesList
``````diff
$diff
``````
"@
        
        # Save to file with numeric prefix and optional custom prefix
        $numericPrefix = "{0:D5}" -f $processed
        if ($Prefix) {
            $fileName = "$Prefix-$numericPrefix-$shortHash.md"
        } else {
            $fileName = "$numericPrefix-$shortHash.md"
        }
        $filePath = Join-Path $OutputFolder $fileName
        $mdContent | Out-File -FilePath $filePath -Encoding UTF8
        
        Write-Host "  Saved to: $fileName" -ForegroundColor Gray
    }
    
    Write-Host "`nSuccessfully processed $processed commit(s)" -ForegroundColor Green
    Write-Host "Output saved to: $OutputFolder" -ForegroundColor Green
    
    # Clear the terminal
    Clear-Host
    
} catch {
    Write-Error "Error: $_"
    exit 1
} finally {
    Pop-Location
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Recurse -Force
    }
}