---
title: Ivy MCP CLI
searchHints:
  - mcp
  - model context protocol
  - lsp
  - ai
  - tools
  - agent
---

# Ivy MCP CLI

<Ingress>
Integrate AI tools directly into your workflow with the Ivy Model Context Protocol (MCP) CLI. Access deep source analysis, codebase documentation, and more directly from your terminal.
</Ingress>

Ivy MCP CLI exposes [Ivy's Model Context Protocol server Tools](../../../03_Advanced/01_MCP/01_McpOverview.md) as individual, stand-alone CLI commands. This enables both human developers and standalone agentic bots to query your project's architecture, look up documentation, and manipulate the environment.

## Available Commands

All MCP CLI tools exist under the `ivy mcp tool <command>` namespace. The available tools are:

- [**`build`**](02_Build.md): Performs an isolated build of your application.
- [**`feedback`**](02_Feedback.md): Submits contextual feedback about agent interactions.
- [**`get-doc-content`**](02_GetDocContent.md): Retrieves the exact markdown content of a documentation page.
- [**`list-all-docs`**](02_ListAllDocs.md): Lists all available comprehensive documentation topics.
- [**`list-all-widgets`**](02_ListAllWidgets.md): Lists all exposed external widgets.
- [**`lsp-definition`**](02_LspDefinition.md): Finds symbol definitions securely.
- [**`lsp-hover`**](02_LspHover.md): Extracts code documentation blocks (intellisense tooltips).
- [**`lsp-completion`**](02_LspCompletion.md): Suggests automated completions based on scope.
- [**`questions`**](02_Questions.md): Queries the AI framework logic against Ivy documentation context.

## Discovery via Help

To see a list of tools available on your local installation, simply run:

```terminal
>ivy mcp tool --help
```

This will produce the complete list of integrated Tools your instance is capable of running.

## Using MCP Tools

CLI tools natively bridge STDIO payloads to full structured JSON invocations against the MCP server. This means they run the exact same underlying logic that an AI Assistant (like Claude or Cursor) uses behind the scenes.

For instance, checking how an Ivy element works:

```terminal
>ivy mcp tool questions "How do I create a database connection in Ivy?"
```

This leverages the `QuestionsTool` connected to the `Ivy.Mcp.Api` RAG system, securely analyzing the context specific to your local application structure.
