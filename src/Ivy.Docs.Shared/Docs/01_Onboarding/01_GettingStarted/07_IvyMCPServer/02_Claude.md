# Getting Started: Claude

<Ingress>
The Ivy MCP Server enables AI assistants to directly interact with the Ivy Framework, providing them with the capability to read documentation, query widget properties, and build complex Ivy applications. By connecting your AI tools to the Ivy MCP Server, you can unlock powerful agentic coding workflows tailored for the Ivy ecosystem.
</Ingress>

## Prerequisites

Before connecting your IDE, you must install Ivy CLI to ensure MCP support.

```bash
dotnet tool install -g Ivy.Console
```

## Setup

The fastest way to get started is scaffolding the sample `--hello` project, which configures the IDE-specific MCP settings in one command.

```bash
ivy init --hello --claude
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
