#!/bin/bash

# Get the directory where this script is located
scriptDir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Remove the Generated directory
rm -rf "${scriptDir}/Generated"

# Run the rust_cli command to regenerate files
cargo run --release --manifest-path "${scriptDir}/../Ivy.Docs.Tools/rust_cli/Cargo.toml" -- convert "${scriptDir}/Docs"/*.md "${scriptDir}/Generated"