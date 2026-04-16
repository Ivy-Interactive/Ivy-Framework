$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
rm -r "${scriptDir}\Generated"
dotnet tool restore
dotnet ivy-docs-cli convert "${scriptDir}\Docs\*.md" "${scriptDir}\Generated"
