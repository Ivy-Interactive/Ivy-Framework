---
title: MCP List All Docs Command
---

# `ivy mcp tool list-all-docs`

<Ingress>
Yield the total corpus mapping of Ivy Framework documentation context.
</Ingress>

## Overview

The `ivy mcp tool list-all-docs` command provides a structured YAML representation of all available documents indexed by the `Ivy.Mcp.Api`. Use this command to discover `DOC_PATH` arguments available for the `get-doc-content` sibling command.

It utilizes the `ListAllDocsTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool list-all-docs
```

## Options

### General Options

- `--help`, `-h`: Show help and usage information.
- `--path`, `-p`: Path to the project directory.

## Example

```terminal
>ivy mcp tool list-all-docs
```
