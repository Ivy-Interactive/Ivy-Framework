---
title: MCP Get Doc Content Command
---

# `ivy mcp tool get-doc-content`

<Ingress>
Fetch the pure markdown content of a specific documentation URI.
</Ingress>

## Overview

The `ivy mcp tool get-doc-content` command retrieves the raw Markdown text of any published Ivy documentation page. It resolves versioning logically, ensuring you always retrieve documentation relevant to the specific framework instantiation you have targeted.

It utilizes the `GetDocContentTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool get-doc-content <DOC_PATH>
```

## Arguments

- `<DOC_PATH>`: The relative path or `docs://` link of the documentation file (e.g. `docs/ApiReference/IvyShared/Colors.md`).

## Options

### General Options

- `--help`, `-h`: Show help and usage information.
- `--path`, `-p`: Path to the project directory.

## Example

```terminal
>ivy mcp tool get-doc-content "docs/ApiReference/IvyShared/Colors.md"
```
