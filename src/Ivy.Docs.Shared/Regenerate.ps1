$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
rm -r ${scriptDir}\Generated
cargo run --release --manifest-path ${scriptDir}\..\Ivy.Docs.Tools\rust_cli\Cargo.toml -- convert ${scriptDir}\Docs\*.md ${scriptDir}\Generated