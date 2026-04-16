#!/bin/bash

# Get the directory where this script is located
scriptDir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Remove the Generated directory
rm -rf "${scriptDir}/Generated"

# Regenerate docs using the ivy-docs-cli NuGet tool
dotnet tool restore
dotnet ivy-docs-cli convert "${scriptDir}/Docs"/*.md "${scriptDir}/Generated"
