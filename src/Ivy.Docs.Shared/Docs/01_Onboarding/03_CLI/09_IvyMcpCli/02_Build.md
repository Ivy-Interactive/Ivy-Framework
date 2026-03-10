---
title: MCP Build Command
---

# `ivy mcp tool build`

<Ingress>
Execute a sterile workspace compilation using the internal project builder.
</Ingress>

## Overview

The `ivy mcp tool build` command mirrors the behavior of `dotnet build`, but isolates the execution to a temporary location to bypass common file locking scenarios present when the agentic system invokes concurrent reads.

It utilizes the `DotnetBuildTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool build
```

## Options

### General Options

- `--help`, `-h`: Show help and usage information.
- `--path`, `-p`: Path to the project directory.

## Example

```terminal
>ivy mcp tool build --path ./src/MyApi/
```
