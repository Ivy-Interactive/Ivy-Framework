$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
rm -r ${scriptDir}\Generated
dotnet run --project ..\Ivy.Docs.Tools\Ivy.Docs.Tools.csproj -- convert ${scriptDir}\Docs\*.md ${scriptDir}\Generated