---
title: MCP List All Widgets Command
---

# `ivy mcp tool list-all-widgets`

<Ingress>
Yield a serialized YAML collection of available External Widgets.
</Ingress>

## Overview

The `ivy mcp tool list-all-widgets` command extracts property bags and functional definitions for all standard external UI elements supported by your instance's `ivyVersion`. It surfaces prop shape and standard configurations as raw YAML for automated consumption.

It utilizes the `ListAllWidgetsTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool list-all-widgets
```

## Options

### General Options

- `--help`, `-h`: Show help and usage information.
- `--path`, `-p`: Path to the project directory.

## Example

```terminal
>ivy mcp tool list-all-widgets
```
