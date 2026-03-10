---
title: MCP Feedback Command
---

# `ivy mcp tool feedback`

<Ingress>
Submit direct feedback regarding the accuracy or utility of conversational context.
</Ingress>

## Overview

The `ivy mcp tool feedback` command allows logging arbitrary string data to the MCP telemetry pipelines. When an AI interface acts undesirably or lacks required context, this tool registers structured feedback for analysis.

It utilizes the `FeedbackTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy mcp tool feedback <MESSAGE>
```

## Arguments

- `<MESSAGE>`: The feedback message you wish to submit.

## Options

### General Options

- `--help`, `-h`: Show help and usage information.
- `--path`, `-p`: Path to the project directory.

## Example

```terminal
>ivy mcp tool feedback "The provided code sample for Authelia is out of date."
```
