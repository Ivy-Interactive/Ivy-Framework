param(
    [Parameter(Mandatory)]
    [string]$PlanFolder
)

$ErrorActionPreference = "Stop"

# Dot-source Utils.ps1
$sharedFolder = Join-Path $PSScriptRoot "../../.shared"
. "$sharedFolder/Utils.ps1"

Write-Host "=== MakePr Execution ===" -ForegroundColor Cyan
Write-Host "Plan: $PlanFolder" -ForegroundColor Cyan

# Read plan.yaml
$planYamlPath = Join-Path $PlanFolder "plan.yaml"
$planYaml = Get-Content $planYamlPath -Raw | ConvertFrom-Yaml

# Step 0: Check if already completed
if ($planYaml.state -eq "Completed") {
    Write-Host "`nPlan is already completed. PRs:" -ForegroundColor Green
    $planYaml.prs | ForEach-Object { Write-Host "  - $_" -ForegroundColor Green }
    return @{
        Status = "AlreadyCompleted"
        PRs = $planYaml.prs
    }
}

# Step 1: Get config and prRule
Write-Host "`n[Step 1] Reading configuration..." -ForegroundColor Yellow
$config = Get-ConfigYaml
$project = $planYaml.project
$projectConfig = $config.projects | Where-Object { $_.name -eq $project } | Select-Object -First 1

if (-not $projectConfig) {
    Write-Error "Project '$project' not found in config.yaml"
}

# Read latest revision
$revisionsFolder = Join-Path $PlanFolder "revisions"
$latestRevision = Get-ChildItem -Path $revisionsFolder -Filter "*.md" -File | Sort-Object Name -Descending | Select-Object -First 1
$revisionContent = Get-Content $latestRevision.FullName -Raw

# Extract title and description
$title = $planYaml.title
$planId = [System.IO.Path]::GetFileName($PlanFolder) -replace '^(\d+).*', '$1'

# Step 2: Process each worktree
Write-Host "`n[Step 2] Processing worktrees..." -ForegroundColor Yellow
$worktreesDir = Join-Path $PlanFolder "worktrees"
$prUrls = @()

if (Test-Path $worktreesDir) {
    $worktrees = Get-ChildItem -Path $worktreesDir -Directory

    foreach ($worktree in $worktrees) {
        $worktreePath = $worktree.FullName
        Write-Host "`nProcessing: $($worktree.Name)" -ForegroundColor Cyan

        Push-Location $worktreePath
        try {
            # Get remote URL
            $remoteUrl = git remote get-url origin
            Write-Host "  Remote: $remoteUrl"

            # Extract owner/repo
            if ($remoteUrl -match "github\.com[:/]([^/]+)/([^/.]+)") {
                $owner = $Matches[1]
                $repo = $Matches[2]
                $ownerRepo = "$owner/$repo"
                Write-Host "  Repo: $ownerRepo"
            } else {
                Write-Error "Could not parse owner/repo from: $remoteUrl"
            }

            # Get branch name
            $branch = git rev-parse --abbrev-ref HEAD
            Write-Host "  Branch: $branch"

            # Get default branch to check commits ahead
            $defaultBranch = gh repo view $ownerRepo --json defaultBranchRef -q .defaultBranchRef.name

            # Check if there are commits ahead of base branch
            $commitsAhead = git rev-list --count origin/$defaultBranch..HEAD
            if ($commitsAhead -eq 0) {
                Write-Host "  No commits ahead of $defaultBranch - skipping PR creation" -ForegroundColor Gray
                Pop-Location
                continue
            }
            Write-Host "  Commits ahead: $commitsAhead"

            # Find prRule for this repo
            $repoPath = $planYaml.repos | Where-Object { $_.Contains($repo) } | Select-Object -First 1
            if (-not $repoPath) {
                Write-Warning "Could not find repo path in plan.yaml, using first repo"
                $repoPath = $planYaml.repos[0]
            }

            $repoConfig = $projectConfig.repos | Where-Object {
                $p = if ($_ -is [hashtable]) { $_.path } else { "$_" }
                $expandedPath = [Environment]::ExpandEnvironmentVariables($p)
                $expandedPath -eq $repoPath
            } | Select-Object -First 1

            $prRule = if ($repoConfig -is [hashtable] -and $repoConfig.prRule) {
                $repoConfig.prRule
            } else {
                "default"
            }

            Write-Host "  PrRule: $prRule" -ForegroundColor Magenta

            # Push branch
            Write-Host "`n  [2.1] Pushing branch..."
            git push -u origin $branch
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Failed to push branch"
            }
            Write-Host "  Branch pushed successfully" -ForegroundColor Green

            # Step 2.5: Upload artifacts
            Write-Host "`n  [2.5] Uploading artifacts..."
            $artifactsScript = Join-Path $PSScriptRoot "Upload-Artifacts.ps1"
            try {
                $artifactMarkdown = & $artifactsScript -PlanFolder $PlanFolder -ErrorAction SilentlyContinue
                if ($artifactMarkdown) {
                    Write-Host "  Artifacts uploaded successfully" -ForegroundColor Green
                } else {
                    Write-Host "  No artifacts to upload" -ForegroundColor Gray
                }
            } catch {
                Write-Host "  Artifact upload skipped (no storage configured)" -ForegroundColor Gray
                $artifactMarkdown = ""
            }

            # Step 3: Create PR
            Write-Host "`n  [3] Creating pull request..."

            # Get default branch
            $defaultBranch = gh repo view $ownerRepo --json defaultBranchRef -q .defaultBranchRef.name
            Write-Host "  Base branch: $defaultBranch"

            # Build PR body
            $prTitle = "[$planId] $title"

            # Use artifacts/summary.md if it exists
            $summaryPath = Join-Path $PlanFolder "artifacts/summary.md"
            if (Test-Path $summaryPath) {
                $prBody = Get-Content $summaryPath -Raw
            } else {
                # Extract from revision
                if ($revisionContent -match "(?s)## Problem\s*(.+?)(?=##|\z)") {
                    $problem = $Matches[1].Trim()
                }
                if ($revisionContent -match "(?s)## Solution\s*(.+?)(?=##|\z)") {
                    $solution = $Matches[1].Trim()
                }
                $prBody = "## Problem`n`n$problem`n`n## Solution`n`n$solution"
            }

            # Add commits list
            $commits = $planYaml.commits -join ", "
            $prBody += "`n`n## Commits`n`n- $commits"

            # Add artifacts if any
            if ($artifactMarkdown) {
                $prBody += "`n`n## Artifacts`n`n$artifactMarkdown"
            }

            # Create PR
            $prUrl = gh pr create --repo $ownerRepo --base $defaultBranch --head $branch --title $prTitle --body $prBody
            Write-Host "  PR created: $prUrl" -ForegroundColor Green
            $prUrls += $prUrl

            # Extract PR number
            if ($prUrl -match "/pull/(\d+)") {
                $prNumber = $Matches[1]
            } else {
                Write-Error "Could not extract PR number from: $prUrl"
            }

            # Step 4: Apply PR rule
            Write-Host "`n  [4] Applying PR rule: $prRule" -ForegroundColor Yellow

            if ($prRule -eq "yolo") {
                Write-Host "  Checking mergeability..."

                # Poll mergeability
                $maxAttempts = 6
                $mergeable = "UNKNOWN"
                for ($i = 0; $i -lt $maxAttempts; $i++) {
                    $mergeable = gh pr view $prNumber --repo $ownerRepo --json mergeable -q .mergeable
                    if ($mergeable -ne "UNKNOWN") {
                        break
                    }
                    Start-Sleep -Seconds 5
                }

                Write-Host "  Mergeable status: $mergeable"

                if ($mergeable -eq "CONFLICTING") {
                    Write-Error "PR has merge conflicts. Manual resolution required."
                } elseif ($mergeable -eq "MERGEABLE") {
                    Write-Host "  Merging PR..."

                    try {
                        gh pr merge $prNumber --repo $ownerRepo --merge --delete-branch --admin
                        Write-Host "  PR merged successfully" -ForegroundColor Green
                    } catch {
                        # Retry with squash if merge commits not allowed
                        Write-Host "  Merge failed, retrying with squash..."
                        gh pr merge $prNumber --repo $ownerRepo --squash --delete-branch --admin
                        Write-Host "  PR squashed and merged successfully" -ForegroundColor Green
                    }

                    # Pull default branch
                    Write-Host "  Pulling $defaultBranch in original repo..."
                    Push-Location $repoPath
                    try {
                        git pull origin $defaultBranch
                        Write-Host "  Pulled successfully" -ForegroundColor Green
                    } finally {
                        Pop-Location
                    }

                    # Step 5: Clean up worktree
                    Write-Host "`n  [5] Cleaning up worktree..."
                    Pop-Location  # Pop before removing worktree

                    Push-Location $repoPath
                    try {
                        git worktree remove $worktreePath --force
                        git branch -D $branch 2>$null
                        Write-Host "  Worktree cleaned up" -ForegroundColor Green
                    } catch {
                        Write-Warning "Worktree cleanup failed: $_"
                    } finally {
                        Pop-Location
                    }
                } else {
                    Write-Error "PR mergeability unknown after polling. Status: $mergeable"
                }
            } else {
                Write-Host "  PR rule is 'default' - PR left open for manual review" -ForegroundColor Gray
                Pop-Location  # Pop for default rule
            }

        } catch {
            Pop-Location -ErrorAction SilentlyContinue
            throw
        }
    }

    # Remove worktrees directory if all cleaned up
    if ($prRule -eq "yolo" -and (Test-Path $worktreesDir)) {
        $remaining = Get-ChildItem -Path $worktreesDir -Directory
        if ($remaining.Count -eq 0) {
            Remove-Item $worktreesDir -Recurse -Force
            Write-Host "`nWorktrees directory removed" -ForegroundColor Green
        }
    }
}

# Step 6: Update plan.yaml
Write-Host "`n[Step 6] Updating plan.yaml..." -ForegroundColor Yellow

# Append PRs
$planYaml.prs = @($planYaml.prs) + $prUrls

# Update state if yolo
if ($prRule -eq "yolo") {
    $planYaml.state = "Completed"
    Write-Host "Plan state updated to: Completed" -ForegroundColor Green
}

# Update timestamp
$planYaml.updated = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")

# Write back
$updatedYaml = ConvertTo-Yaml $planYaml
Set-Content -Path $planYamlPath -Value $updatedYaml -NoNewline -Encoding UTF8

Write-Host "`nplan.yaml updated" -ForegroundColor Green
Write-Host "`n=== MakePr Complete ===" -ForegroundColor Cyan
Write-Host "Created PRs:" -ForegroundColor Green
$prUrls | ForEach-Object { Write-Host "  - $_" -ForegroundColor Green }

return @{
    Status = "Success"
    PRs = $prUrls
    PrRule = $prRule
}
