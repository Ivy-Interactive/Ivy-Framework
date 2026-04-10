# Bootstrap-Modules.ps1 — Shared module installation and import
# Source this file from any PowerShell script that needs the powershell-yaml module:
#   . (Join-Path (Split-Path $PSScriptRoot) ".shared/Bootstrap-Modules.ps1")

# Ensure powershell-yaml is available
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    Write-Host "Installing powershell-yaml module..." -ForegroundColor Yellow
    Install-Module -Name powershell-yaml -RequiredVersion 0.4.12 -Force -Scope CurrentUser
}

# Import the module
Import-Module powershell-yaml -ErrorAction Stop
