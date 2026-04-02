# Setup Local Development Environment
# This script configures dotnet user-secrets with values from Azure Key Vault

$ErrorActionPreference = "Stop"
$SubscriptionId = "d2a0987f-21e8-4526-98c3-3ca3ef8f256d"

Write-Host "=== Local Development Setup ===" -ForegroundColor Cyan

# Check if Azure CLI is installed
Write-Host "`nChecking if Azure CLI is installed..." -ForegroundColor Yellow
try {
    $azVersion = az version --output json 2>$null | ConvertFrom-Json
    Write-Host "Azure CLI is installed (version: $($azVersion.'azure-cli'))" -ForegroundColor Green
} catch {
    Write-Host "Azure CLI is not installed. Please install it from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Check if user is logged in to Azure
Write-Host "`nChecking if you are logged in to Azure..." -ForegroundColor Yellow
try {
    $account = az account show --output json 2>$null | ConvertFrom-Json
    if ($null -eq $account) {
        throw "Not logged in"
    }
    Write-Host "Logged in as: $($account.user.name)" -ForegroundColor Green
} catch {
    Write-Host "You are not logged in to Azure. Initiating login..." -ForegroundColor Yellow
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Login failed. Please try again." -ForegroundColor Red
        exit 1
    }
}

# Verify user can access the subscription
Write-Host "`nVerifying access to subscription $SubscriptionId..." -ForegroundColor Yellow
try {
    $subscription = az account show --subscription $SubscriptionId --output json 2>$null | ConvertFrom-Json
    if ($null -eq $subscription) {
        throw "Cannot access subscription"
    }
    Write-Host "Access verified for subscription: $($subscription.name)" -ForegroundColor Green

    # Set the subscription as active
    az account set --subscription $SubscriptionId
} catch {
    Write-Host "You do not have access to subscription $SubscriptionId. Please contact your administrator." -ForegroundColor Red
    exit 1
}

# Read env.config.yaml
Write-Host "`nReading environment configuration..." -ForegroundColor Yellow
$configPath = Join-Path $PSScriptRoot "env.config.yaml"
if (-not (Test-Path $configPath)) {
    Write-Host "env.config.yaml not found at $configPath" -ForegroundColor Red
    exit 1
}

# Install powershell-yaml module if not already installed
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    Write-Host "Installing powershell-yaml module..." -ForegroundColor Yellow
    Install-Module -Name powershell-yaml -Force -Scope CurrentUser
}

Import-Module powershell-yaml
$configContent = Get-Content $configPath -Raw
$config = ConvertFrom-Yaml $configContent

# Display available environments
Write-Host "`nAvailable environments:" -ForegroundColor Cyan
for ($i = 0; $i -lt $config.environment.Count; $i++) {
    Write-Host "  [$($i + 1)] $($config.environment[$i].name) (Key Vault: $($config.environment[$i].keyVault))" -ForegroundColor White
}

# Prompt user to select an environment
Write-Host "`nYou will need permission to read secrets from the selected Key Vault." -ForegroundColor Yellow
do {
    $selection = Read-Host "`nSelect an environment (1-$($config.environment.Count))"
    $selectionIndex = [int]$selection - 1
} while ($selectionIndex -lt 0 -or $selectionIndex -ge $config.environment.Count)

$selectedEnv = $config.environment[$selectionIndex]
Write-Host "`nSelected environment: $($selectedEnv.name)" -ForegroundColor Green
Write-Host "Key Vault: $($selectedEnv.keyVault)" -ForegroundColor Green

# List all secrets in the Key Vault to understand naming convention
Write-Host "`nListing secrets in Key Vault to determine naming convention..." -ForegroundColor Yellow
try {
    $allSecrets = az keyvault secret list --vault-name $selectedEnv.keyVault --query "[].name" --output json 2>$null | ConvertFrom-Json
    if ($LASTEXITCODE -eq 0 -and $allSecrets.Count -gt 0) {
        Write-Host "Found $($allSecrets.Count) secrets in Key Vault" -ForegroundColor Green
        Write-Host "Sample secret names: $($allSecrets[0..([Math]::Min(4, $allSecrets.Count - 1))] -join ', ')" -ForegroundColor Gray
    }
} catch {
    Write-Host "Warning: Could not list secrets. Proceeding with retrieval..." -ForegroundColor Yellow
    $allSecrets = @()
}

# Retrieve secrets from Key Vault and add to dotnet user-secrets
Write-Host "`nRetrieving secrets from Key Vault and configuring dotnet user-secrets..." -ForegroundColor Yellow

$successCount = 0
$failCount = 0
$failedSecrets = @()

foreach ($secret in $config.secrets) {
    # Try multiple naming conventions
    $namingAttempts = @(
        ($secret -replace ":", "--"),  # Section--Key (Azure Key Vault standard)
        ($secret -replace ":", "__"),  # Section__Key
        ($secret -replace ":", "-"),   # Section-Key
        $secret                        # Section:Key (though unlikely in Key Vault)
    )

    Write-Host "`nProcessing: $secret" -ForegroundColor Cyan
    $secretValue = $null
    $foundName = $null

    foreach ($keyVaultSecretName in $namingAttempts) {
        try {
            Write-Host "  Trying: $keyVaultSecretName" -ForegroundColor Gray
            $secretValue = az keyvault secret show --vault-name $selectedEnv.keyVault --name $keyVaultSecretName --query "value" --output tsv 2>$null

            if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($secretValue)) {
                $foundName = $keyVaultSecretName
                break
            }
        } catch {
            # Continue to next naming attempt
        }
    }

    if ($null -ne $foundName) {
        try {
            # Add to dotnet user-secrets (keeping the original "Section:Key" format)
            $output = dotnet user-secrets set $secret "$secretValue" --project $PSScriptRoot 2>&1

            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✓ Successfully configured: $secret (from $foundName)" -ForegroundColor Green
                $successCount++
            } else {
                $errorMsg = $output -join "`n"
                Write-Host "  ✗ Failed to set user-secret for: $secret" -ForegroundColor Red
                Write-Host "    Error: $errorMsg" -ForegroundColor Gray
                $failCount++
                $failedSecrets += @{ Name = $secret; Reason = $errorMsg }
            }
        } catch {
            Write-Host "  ✗ Failed to set user-secret for: $secret" -ForegroundColor Red
            Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Gray
            $failCount++
            $failedSecrets += @{ Name = $secret; Reason = $_.Exception.Message }
        }
    } else {
        Write-Host "  ✗ Secret not found in Key Vault: $secret" -ForegroundColor Red
        $failCount++
        $failedSecrets += @{ Name = $secret; Reason = "Not found in Key Vault" }
    }
}

# Summary
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Successfully configured: $successCount secrets" -ForegroundColor Green
if ($failCount -gt 0) {
    Write-Host "Failed to configure: $failCount secrets" -ForegroundColor Red
    Write-Host "`nFailed secrets:" -ForegroundColor Yellow
    foreach ($failed in $failedSecrets) {
        if ($failed -is [hashtable]) {
            Write-Host "  - $($failed.Name) ($($failed.Reason))" -ForegroundColor Red
        } else {
            Write-Host "  - $failed" -ForegroundColor Red
        }
    }
    Write-Host "`nTip: Review the Key Vault secret names above to ensure they match the expected naming convention." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "`nAll secrets configured successfully!" -ForegroundColor Green
    Write-Host "You can now run the tests locally." -ForegroundColor Green
}
