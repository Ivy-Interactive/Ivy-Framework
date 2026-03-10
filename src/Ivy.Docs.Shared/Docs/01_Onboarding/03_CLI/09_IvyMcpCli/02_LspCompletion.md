---
title: MCP LSP Completion Command
---

# `ivy mcp tool lsp-completion`

<Ingress>
Yield location-based code completion signatures directly from the compiler.
</Ingress>

## Overview

The `ivy mcp tool lsp-completion` command hooks into the Roslyn compiler workspace via the Ivy Language Server Protocol implementations. It provides contextual suggestions (types, members, functions) based on a specific `(line, column)` vector inside a C# document.

It utilizes the `LspCompletionTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool lsp-completion <FILE_PATH> --line <L> --column <C>
```

## Arguments

- `<FILE_PATH>`: The absolute path to the C# file being queried.

## Options

### Required Options

- `--line`: The zero-based line offset for the completion context.
- `--column`: The zero-based character offset for the completion context.

### General Options

- `--help`, `-h`: Show help and usage information.
- `--path`, `-p`: Path to the project directory.

## Example

```terminal
>ivy mcp tool lsp-completion "/Users/rory/git/project/Program.cs" --line 15 --column 30
```
