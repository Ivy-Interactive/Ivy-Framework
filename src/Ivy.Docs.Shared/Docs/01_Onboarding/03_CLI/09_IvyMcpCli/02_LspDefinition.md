---
title: MCP LSP Definition Command
---

# `ivy mcp tool lsp-definition`

<Ingress>
Extract the full source code definition block of a targeted symbol.
</Ingress>

## Overview

The `ivy mcp tool lsp-definition` command reads the symbol located at the provided `(line, column)` inside `<FILE_PATH>`, then navigates Roslyn's syntax tree to retrieve that symbol's source code declaration in its entirety—including from external NuGet packages or referenced projects.

It utilizes the `LspDefinitionTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool lsp-definition <FILE_PATH> --line <L> --column <C>
```

## Arguments

- `<FILE_PATH>`: The absolute path to the C# file being queried.

## Options

### Required Options

- `--line`: The zero-based line offset targeting the symbol.
- `--column`: The zero-based character offset targeting the symbol.

### General Options

- `--help`, `-h`: Show help and usage information.
- `--path`, `-p`: Path to the project directory.

## Example

```terminal
>ivy mcp tool lsp-definition "/Users/rory/git/project/Models/User.cs" --line 40 --column 12
```
