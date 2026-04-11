param(
    [Parameter(Mandatory=$true)]
    [string]$IntermediateOutputPath
)

$src = "Promptwares"
$staging = Join-Path $IntermediateOutputPath "promptwares-staging"
$zip = Join-Path $IntermediateOutputPath "promptwares.zip"

if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
Copy-Item $src $staging -Recurse -Force

Get-ChildItem $staging -Recurse -Directory | Where-Object { $_.Name -in 'Logs','Memory' } | Remove-Item -Recurse -Force

if (Test-Path $zip) { Remove-Item $zip -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($staging, $zip)

Remove-Item $staging -Recurse -Force
