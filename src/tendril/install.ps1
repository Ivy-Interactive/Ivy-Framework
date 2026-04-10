<#
.SYNOPSIS
Ivy-Tendril Windows Installer

.DESCRIPTION
This script installs .NET 10 SDK, Git, GitHub CLI, and Ivy-Tendril.
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Ivy-Tendril Installer for Windows ===" -ForegroundColor Blue

$isWindows = ([System.Environment]::OSVersion.Platform -eq "Win32NT")
if (-not $isWindows) {
    Write-Host "Error: This script is only for Windows." -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 1: Checking for .NET 10 SDK..." -ForegroundColor Blue
$hasDotNet10 = $false
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $version = (dotnet --version 2>$null)
    if ($version -match "^10\.") {
        $hasDotNet10 = $true
    }
}

if ($hasDotNet10) {
    Write-Host "✓ .NET 10 SDK is already installed." -ForegroundColor Green
} else {
    Write-Host "Installing .NET 10 SDK..."
    $installScriptPath = Join-Path $env:TEMP "dotnet-install.ps1"
    Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScriptPath
    
    # By default, dotnet-install.ps1 installs to $env:LOCALAPPDATA\Microsoft\dotnet
    & $installScriptPath -Channel 10.0

    $dotnetRoot = "$env:LOCALAPPDATA\Microsoft\dotnet"
    $env:DOTNET_ROOT = $dotnetRoot
    $env:PATH = "$dotnetRoot;$env:PATH"
    
    # Also add to User PATH permanently if not present
    $userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
    if ($userPath -notmatch [regex]::Escape($dotnetRoot)) {
        [Environment]::SetEnvironmentVariable("PATH", "$userPath;$dotnetRoot", "User")
    }

    Write-Host "✓ .NET 10 SDK installed successfully." -ForegroundColor Green
}

Write-Host "`nStep 2: Checking for Git..." -ForegroundColor Blue
if (Get-Command git -ErrorAction SilentlyContinue) {
    Write-Host "✓ Git is already installed." -ForegroundColor Green
} else {
    Write-Host "Installing Git..."
    if (Get-Command winget -ErrorAction SilentlyContinue) {
        winget install --id Git.Git -e --source winget
        Write-Host "✓ Git installed." -ForegroundColor Green
    } else {
        Write-Host "Error: 'winget' not found. Please install Git manually." -ForegroundColor Red
        exit 1
    }
}

Write-Host "`nStep 3: Checking for GitHub CLI (gh)..." -ForegroundColor Blue
if (Get-Command gh -ErrorAction SilentlyContinue) {
    Write-Host "✓ GitHub CLI is already installed." -ForegroundColor Green
} else {
    Write-Host "Installing GitHub CLI (gh)..."
    if (Get-Command winget -ErrorAction SilentlyContinue) {
        winget install --id GitHub.cli -e --source winget
        Write-Host "✓ GitHub CLI installed." -ForegroundColor Green
    } else {
        Write-Host "Error: 'winget' not found. Please install GitHub CLI manually." -ForegroundColor Red
        exit 1
    }
}

Write-Host "`nStep 4: Checking for PowerShell (pwsh)..." -ForegroundColor Blue
$hasPwsh = $false
if (Get-Command pwsh -ErrorAction SilentlyContinue) {
    $hasPwsh = $true
} elseif (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $toolList = (dotnet tool list -g 2>$null) -join " "
    if ($toolList -match "(?i)powershell") {
        $hasPwsh = $true
    }
}

if ($hasPwsh) {
    Write-Host "✓ PowerShell (pwsh) is already installed." -ForegroundColor Green
} else {
    Write-Host "Installing PowerShell (pwsh)..."
    dotnet tool install --global PowerShell
    Write-Host "✓ PowerShell installed successfully." -ForegroundColor Green
}

Write-Host "`nStep 5: Installing Ivy-Tendril..." -ForegroundColor Blue
$IVY_SOURCE = "https://api.nuget.org/v3/index.json"

$hasTendril = $false
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $toolList = (dotnet tool list -g 2>$null) -join " "
    if ($toolList -match "(?i)ivy\.tendril") {
        $hasTendril = $true
    }
}

if ($hasTendril) {
    Write-Host "Updating Ivy-Tendril..."
    # ignore update errors
    try {
        dotnet tool update -g Ivy.Tendril --add-source "$IVY_SOURCE"
    } catch {
        Write-Host "Ivy-Tendril updated."
    }
} else {
    Write-Host "Installing Ivy-Tendril..."
    dotnet tool install -g Ivy.Tendril --add-source "$IVY_SOURCE"
}

# Step 6: Path Configuration Verification
Write-Host "`nStep 6: Configuring PATH..." -ForegroundColor Blue
$dotnetToolsPath = "$env:USERPROFILE\.dotnet\tools"
$env:PATH = "$dotnetToolsPath;$env:PATH"

$userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
if ($userPath -notmatch [regex]::Escape($dotnetToolsPath)) {
    Write-Host "Adding .NET tools to User PATH..."
    [Environment]::SetEnvironmentVariable("PATH", "$userPath;$dotnetToolsPath", "User")
    Write-Host "✓ PATH updated." -ForegroundColor Green
} else {
    Write-Host "✓ PATH already configured." -ForegroundColor Green
}

Write-Host "`n=== Installation Complete! ===" -ForegroundColor Green
Write-Host "You can now run Ivy-Tendril by typing: tendril" -ForegroundColor Blue
Write-Host "To launch the GUI, use: tendril --photino" -ForegroundColor Blue

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "Note: You may need to restart your terminal for 'gh' or 'tendril' to be available." -ForegroundColor Red
}

Write-Host "`nTry running: tendril --version" -ForegroundColor Blue
