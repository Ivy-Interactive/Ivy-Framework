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

Before connecting your IDE, you must install or update the Ivy CLI to the latest version to ensure MCP support.

```bash
dotnet tool install -g Ivy.Console
# Or to update if already installed:
dotnet tool update -g Ivy.Console
```

## Quick Start: Scaffold a New Project

The fastest way to get started is using the `--hello` flag, which scaffolds a project and configures the IDE-specific MCP settings in one command.

### Claude Desktop

```bash
ivy init --hello --claude
```

### Cursor

```bash
ivy init --hello --cursor
```

## Connecting Your IDE

If you are adding Ivy to an existing project, follow the steps below for your specific environment.

### Windsurf (Cascade)

1. Open Windsurf.
2. Run the configuration command to automatically update your `windsurf.json`:

```bash
ivy mcp config --windsurf
```

3. Restart your Cascade session to enable Ivy tool-calling.

### VS Code

1. Open your project directory in VS Code.
2. Initialise the Ivy project: `ivy init`
3. Generate the MCP configuration: `ivy mcp config --vscode`
4. In the chat, prompt `#AGENTS.md` to help the assistant recognize the Ivy context.

### Antigravity

1. Open Antigravity and navigate to your project.
2. Run: `ivy mcp config --antigravity`
3. Prompt `@AGENTS` in the chat to sync the project context and agent instructions.

### JetBrains Rider

1. Open Settings > Tools > MCP Servers.
2. Click `+` to add a new server.
3. **Command**: Input the path to your ivy tool (e.g., `/Users/YOUR_USER/.dotnet/tools/ivy`).
4. **Arguments**:

```text
mcp --path "/your/absolute/project/path"
```

5. **Environment Variables**:

```plaintext
Ivy__Mcp__ApiUrl=https://staging.mcp.ivy.app
```

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