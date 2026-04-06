# Test script that verifies TENDRIL_SESSION_ID is accessible
param()

$sessionId = $env:TENDRIL_SESSION_ID

if (-not $sessionId) {
    Write-Error "TENDRIL_SESSION_ID environment variable not set"
    exit 1
}

# Output the session ID to stdout for verification
Write-Output "SESSION_ID:$sessionId"
exit 0
