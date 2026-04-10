param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

# Cannot use Get-SharedFolder here since we need this path to dot-source Utils.ps1
# User-defined promptwares use TENDRIL_SHARED to access built-in shared utilities
$sharedDir = if ($env:TENDRIL_SHARED) { $env:TENDRIL_SHARED } else { "$PSScriptRoot/.shared" }
. "$sharedDir/Utils.ps1"

$programFolder = GetProgramFolder $PSCommandPath
$planYamlPath = ValidatePlanPath $PlanPath
$planInfo = ReadPlanYaml $planYamlPath

$logFile = GetNextLogFile $programFolder
$PlanPath | Set-Content $logFile
Write-Host "Log file: $logFile"

# Ensure verification and artifacts dirs exist
$verificationDir = Join-Path $PlanPath "verification"
$artifactsDir = Join-Path $PlanPath "artifacts"
foreach ($dir in @($verificationDir, "$artifactsDir\tests", "$artifactsDir\screenshots", "$artifactsDir\sample")) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}

# Kill only THIS plan's sample processes (not other concurrent plans)
Write-Host "Cleaning up leftover sample processes for this plan..." -ForegroundColor Yellow
$escapedPlanPath = [regex]::Escape((Resolve-Path $PlanPath).Path)
$killed = 0
Get-Process -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -and $_.Path -match "^$escapedPlanPath"
} | ForEach-Object {
    Write-Host "  Killing: $($_.ProcessName) (PID $($_.Id)) from $($_.Path)" -ForegroundColor Yellow
    try {
        $_ | Stop-Process -Force -ErrorAction Stop
        $killed++
    } catch {
        Write-Warning "  Failed to kill PID $($_.Id): $_"
    }
}
if ($killed -gt 0) {
    Write-Host "  Killed $killed process(es). Waiting for file handles to release..." -ForegroundColor Yellow
    Start-Sleep -Milliseconds 2000
} else {
    Write-Host "  No leftover processes found." -ForegroundColor Green
}

# Set ARTIFACTS_DIR so Playwright tests can write directly to plan artifacts
$env:ARTIFACTS_DIR = $artifactsDir

# --- Pre-build: resolve framework path and build frontend/Ivy.dll ---
$repoName = "Ivy-Framework"
$mainRepoPath = Join-Path $env:REPOS_HOME $repoName
$worktreePath = Join-Path $PlanPath "worktrees/$repoName"
$frameworkPath = if (Test-Path $worktreePath) { $worktreePath } else { $mainRepoPath }
$frontendDir = Join-Path $frameworkPath "src/frontend"

# Check if worktree has frontend (.ts/.tsx) changes requiring a rebuild
$needsFrontendRebuild = $false
if (Test-Path $worktreePath) {
    $tsChanges = git -C $worktreePath diff --name-only origin/main...HEAD -- "*.ts" "*.tsx" 2>$null
    $needsFrontendRebuild = ($tsChanges.Count -gt 0)
}

if ($needsFrontendRebuild) {
    Write-Host "Pre-building frontend from $frontendDir..." -ForegroundColor Cyan
    Push-Location $frontendDir
    npx vite build --outDir dist
    if (-not (Test-Path "dist/index.html")) {
        Write-Warning "Frontend build produced no index.html!"
    }
    # Create build stamp to prevent MSBuild re-trigger
    New-Item -Path "dist/.build-stamp" -ItemType File -Force | Out-Null
    Pop-Location

    # Clean Ivy obj and rebuild to embed fresh assets
    $ivyCsprojDir = Join-Path $frameworkPath "src/Ivy"
    Remove-Item -Path (Join-Path $ivyCsprojDir "obj") -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Building Ivy.csproj with fresh frontend assets..." -ForegroundColor Cyan
    dotnet build (Join-Path $ivyCsprojDir "Ivy.csproj") --no-incremental
}

$env:IVY_FRAMEWORK_PATH = $frameworkPath

# Pre-install Playwright browser with mutex to prevent concurrent download contention
Write-Host "Ensuring Playwright Chromium is installed..." -ForegroundColor Cyan
$mutex = [System.Threading.Mutex]::new($false, "Global\TendrilPlaywrightInstall")
try {
    if ($mutex.WaitOne(120000)) {  # 2-minute timeout
        npx playwright install chromium 2>$null
    } else {
        Write-Warning "Timed out waiting for Playwright install lock — assuming another instance completed it"
    }
} finally {
    try { $mutex.ReleaseMutex() } catch {}
    $mutex.Dispose()
}

InvokePromptwareAgent $PSScriptRoot $programFolder $logFile @{
    Args             = $PlanPath
    PlanFolder       = $PlanPath
    Project          = $planInfo.Project
    VerificationDir  = $verificationDir
    ArtifactsDir     = $artifactsDir
    IvyFrameworkPath = $frameworkPath
} -PlanPath $PlanPath -Action "IvyFrameworkVerification" -Promptware "IvyFrameworkVerification"

# Post-agent cleanup: kill orphaned Chromium processes spawned by Playwright tests.
# These live under ms-playwright, not under the plan folder, so the pre-run cleanup misses them.
Write-Host "Cleaning up orphaned Playwright browser processes..." -ForegroundColor Yellow
$playwrightKilled = 0
Get-Process -Name "chrome","chromium" -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -and $_.Path -match 'ms-playwright'
} | ForEach-Object {
    Write-Host "  Killing: $($_.ProcessName) (PID $($_.Id))" -ForegroundColor Yellow
    try {
        $_ | Stop-Process -Force -ErrorAction Stop
        $playwrightKilled++
    } catch {
        Write-Warning "  Failed to kill PID $($_.Id): $_"
    }
}
if ($playwrightKilled -gt 0) {
    Write-Host "  Killed $playwrightKilled Playwright browser process(es)." -ForegroundColor Yellow
} else {
    Write-Host "  No orphaned Playwright browsers found." -ForegroundColor Green
}
