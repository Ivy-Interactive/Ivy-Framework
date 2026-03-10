---
title: MCP List All Docs Command
searchHints:
  - mcp
  - list docs
---

# `ivy docs list`

<Ingress>
List all available context documentation paths registered inside the MCP framework for subsequent manual investigation.
</Ingress>

## Overview

The `ivy docs list` command provides a structured YAML representation of all available documents indexed by the `Ivy.Mcp.Api`. Use this command to discover `DOC_PATH` arguments available for the `ivy docs <path>` sibling command.

It utilizes the `ListAllDocsTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy docs list
```

### Arguments

N/A - This command takes no arguments.

## Examples

Retrieve the documentation manifest:

```terminal
>ivy docs list
```
