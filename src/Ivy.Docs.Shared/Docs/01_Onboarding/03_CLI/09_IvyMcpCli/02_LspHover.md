---
title: MCP LSP Hover Command
---

# `ivy mcp tool lsp-hover`

<Ingress>
Extract the xml-doc or signature block of a targeted symbol.
</Ingress>

## Overview

The `ivy mcp tool lsp-hover` command mimics the behavior of a user physically resting their cursor over an identifier in an IDE. It returns a concise markdown string representing the type signature and any associated XML documentation comments.

It utilizes the `LspHoverTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool lsp-hover <FILE_PATH> --line <L> --column <C>
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
>ivy mcp tool lsp-hover "/Users/rory/git/project/Utils.cs" --line 10 --column 5
```
