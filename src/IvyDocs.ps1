param(
    [switch]$BuildFrontend
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

if ($BuildFrontend) {
    Write-Host "Building Ivy Frontend..."
    $frontendDir = Join-Path $scriptDir "frontend"
    Push-Location $frontendDir
    npm install
    npm run build
    Pop-Location
    Write-Host "Frontend build complete."
}

Write-Host "Starting Ivy Docs..."
dotnet run --project (Join-Path $scriptDir "Ivy.Docs\Ivy.Docs.csproj")
