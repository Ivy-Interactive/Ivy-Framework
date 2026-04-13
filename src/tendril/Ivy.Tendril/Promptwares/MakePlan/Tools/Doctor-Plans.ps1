# Reference implementation for DoctorCommand.DoctorPlans()
#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Scans all plans and generates a health report table
.DESCRIPTION
    Analyzes each plan in PlansDirectory for:
    - YAML parsing health
    - Worktree count
    - Nested worktree detection
    - Overall health status
.PARAMETER PlansDirectory
    Path to the plans directory (default: $env:TENDRIL_HOME/Plans)
.PARAMETER ShowOnlyUnhealthy
    Only show plans with health issues
#>

param(
    [string]$PlansDirectory = "$env:TENDRIL_HOME\Plans",
    [switch]$ShowOnlyUnhealthy
)

function Test-YamlHealth {
    param([string]$YamlPath)

    if (-not (Test-Path $YamlPath)) {
        return @{
            Healthy = $false
            Error = "Missing"
        }
    }

    try {
        # Simple YAML validation - check if file is readable and has basic structure
        $content = Get-Content $YamlPath -Raw -ErrorAction Stop

        if ([string]::IsNullOrWhiteSpace($content)) {
            return @{
                Healthy = $false
                Error = "Empty"
            }
        }

        # Check for basic YAML structure
        if (-not ($content -match "state:") -or -not ($content -match "project:")) {
            return @{
                Healthy = $false
                Error = "Invalid structure"
            }
        }

        return @{
            Healthy = $true
            Error = $null
        }
    }
    catch {
        return @{
            Healthy = $false
            Error = "Parse error: $($_.Exception.Message)"
        }
    }
}

function Get-WorktreeCount {
    param([string]$PlanPath)

    $worktreesPath = Join-Path $PlanPath "worktrees"

    if (-not (Test-Path $worktreesPath)) {
        return 0
    }

    # Count subdirectories in worktrees folder
    $count = (Get-ChildItem -Path $worktreesPath -Directory -ErrorAction SilentlyContinue).Count
    return $count
}

function Test-NestedWorktree {
    param([string]$PlanPath)

    $worktreesPath = Join-Path $PlanPath "worktrees"

    if (-not (Test-Path $worktreesPath)) {
        return $false
    }

    # Check for .git files/folders inside worktree subdirectories
    # This would indicate a nested worktree (worktree inside worktree)
    $nestedGit = Get-ChildItem -Path $worktreesPath -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -eq ".git" }

    return ($null -ne $nestedGit -and $nestedGit.Count -gt 0)
}

function Get-PlanHealth {
    param(
        [string]$PlanPath,
        [string]$PlanName
    )

    $yamlPath = Join-Path $PlanPath "plan.yaml"
    $yamlHealth = Test-YamlHealth -YamlPath $yamlPath
    $worktreeCount = Get-WorktreeCount -PlanPath $PlanPath
    $hasNestedWorktree = Test-NestedWorktree -PlanPath $PlanPath

    # Extract plan ID and title from folder name
    if ($PlanName -match '^(\d{5})-(.+)$') {
        $planId = $Matches[1]
        $planTitle = $Matches[2]
    }
    else {
        $planId = $PlanName
        $planTitle = ""
    }

    # Get state from plan.yaml
    $state = "Unknown"
    if ($yamlHealth.Healthy) {
        try {
            $yamlContent = Get-Content $yamlPath -Raw
            if ($yamlContent -match 'state:\s*(\S+)') {
                $state = $Matches[1]
            }
        }
        catch {
            # Already captured in yamlHealth
        }
    }

    # Determine overall health
    $healthIssues = @()
    if (-not $yamlHealth.Healthy) {
        $healthIssues += "YAML:$($yamlHealth.Error)"
    }
    if ($hasNestedWorktree) {
        $healthIssues += "NestedWorktree"
    }

    $health = if ($healthIssues.Count -eq 0) { "OK" } else { $healthIssues -join "," }

    return [PSCustomObject]@{
        Id = $planId
        Plan = $planTitle
        State = $state
        Worktrees = $worktreeCount
        Health = $health
        IsHealthy = ($healthIssues.Count -eq 0)
    }
}

# Main execution
Write-Host "Scanning plans in: $PlansDirectory" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $PlansDirectory)) {
    Write-Error "Plans directory not found: $PlansDirectory"
    exit 1
}

# Get all plan folders
$planFolders = Get-ChildItem -Path $PlansDirectory -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^\d{5}-' } |
    Sort-Object Name

if ($planFolders.Count -eq 0) {
    Write-Warning "No plans found in $PlansDirectory"
    exit 0
}

# Collect health data
$results = @()
foreach ($folder in $planFolders) {
    $health = Get-PlanHealth -PlanPath $folder.FullName -PlanName $folder.Name
    $results += $health
}

# Filter if requested
if ($ShowOnlyUnhealthy) {
    $results = $results | Where-Object { -not $_.IsHealthy }
}

# Display results
$results | Format-Table -Property Id, Plan, State, Worktrees, Health -AutoSize

# Summary
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Total plans: $($planFolders.Count)"
Write-Host "  Healthy: $(($results | Where-Object { $_.IsHealthy }).Count)"
Write-Host "  Unhealthy: $(($results | Where-Object { -not $_.IsHealthy }).Count)"
Write-Host "  With worktrees: $(($results | Where-Object { $_.Worktrees -gt 0 }).Count)"
Write-Host "  Nested worktrees: $(($results | Where-Object { $_.Health -like '*NestedWorktree*' }).Count)"
