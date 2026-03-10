---
title: MCP Questions Command
---

# `ivy mcp tool questions`

<Ingress>
Perform secure Retrieval-Augmented Generation context searches via the Ivy API.
</Ingress>

## Overview

The `ivy mcp tool questions` command executes semantic queries across the comprehensive framework knowledge base. When asked "how" to do something or for code examples regarding Ivy internals, the underlying LLM cross-references the latest indexed state of `Ivy.Docs.Shared`.

It utilizes the `QuestionsTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool questions <QUESTION>
```

## Arguments

- `<QUESTION>`: The question or inquiry to process. Wrap this in quotes if it contains spaces.

## Options

### General Options

- `--help`, `-h`: Show help and usage information.
- `--path`, `-p`: Path to the project directory.

## Example

```terminal
>ivy mcp tool questions "How do I implement a new Application Shell in Ivy?"
```
