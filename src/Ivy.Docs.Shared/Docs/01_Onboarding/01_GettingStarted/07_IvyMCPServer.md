---
title: Ivy MCP Server
searchHints:
  - mcp
  - server
  - ai-agent
  - claude
  - cursor
  - antigravity
  - vscode
  - windsurf
  - rider
---

# Getting Started: Ivy MCP Server

<Ingress>
The Ivy MCP Server enables AI assistants to directly interact with the Ivy Framework, providing them with the capability to read documentation, query widget properties, and build complex Ivy applications. By connecting your AI tools to the Ivy MCP Server, you can unlock powerful agentic coding workflows tailored for the Ivy ecosystem.
</Ingress>

## Prerequisites

Before connecting your IDE, you must install Ivy CLI to ensure MCP support.

```bash
dotnet tool install -g Ivy.Console
```
## Quick Start: Scaffold a New Project with Claude and Cursor

The fastest way to get started is scaffolding the sample `--hello` project, which configures the IDE-specific MCP settings in one command.

### Claude 

```bash
ivy init --hello --claude
```

### Cursor

```bash
ivy init --hello --cursor
```

## Connecting Other IDEs

If you are adding Ivy to an IDE, follow the steps below for your specific environment.



### VS Code

1. Open your project directory in VS Code.
2. Initialise the Ivy project: `ivy init`
3. Generate the MCP configuration: `ivy mcp config`
4. In the chat, prompt `#AGENTS.md` to sync the project context and agent instructions.

### Antigravity

1. Open Antigravity and navigate to your project.
2. Run: `ivy mcp config`
3. Prompt `@AGENTS.md` in the chat to sync the project context and agent instructions.

### Windsurf (Cascade)

1. Open Windsurf.
2. Run the configuration command to automatically update your `windsurf.json`:

```bash
ivy mcp config
```

3. Restart your Cascade session to enable Ivy tool-calling.

## Manual Configuration (Advanced)

For custom setups or troubleshooting, ensure your `mcp.json` (or equivalent config) follows this structure. Note that the `--path` argument is required for the Language Server Protocol (LSP) to function correctly.

```json
{
  "mcpServers": {
    "ivy-mcp": {
      "command": "/Users/YOUR_USER/.dotnet/tools/ivy",
      "args": [
        "mcp",
        "--path",
        "/Users/YOUR_USER/path/to/your/project"
      ],
      "env": {
        "Ivy__Mcp__ApiUrl": "https://staging.mcp.ivy.app"
      }
    }
  }
}
```