$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$generatedDir = Join-Path $scriptDir "Generated"

if (Test-Path $generatedDir) {
    Remove-Item $generatedDir -Recurse -Force
}

dotnet run --project "../Ivy.Docs.Tools/Ivy.Docs.Tools.csproj" -- convert "$scriptDir/Docs/*.md" "$generatedDir"