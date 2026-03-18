# Getting Started: Antigravity

<Ingress>
The Ivy MCP Server enables AI assistants to directly interact with the Ivy Framework, providing them with the capability to read documentation, query widget properties, and build complex Ivy applications. By connecting your AI tools to the Ivy MCP Server, you can unlock powerful agentic coding workflows tailored for the Ivy ecosystem.
</Ingress>

For more information on configuring MCP servers in Antigravity, please refer to their [official documentation](https://antigravity.google/docs/mcp).

To use the Ivy MCP Server, you first need to install Ivy. Refer to the [installation guide](../02_Installation.md) to learn how.

## Setup

1. Open your project directory in Antigravity
2. Initialise the Ivy project:

   ```terminal
   ivy init 
   ```

3. Generate the MCP configuration:

   ```terminal
   ivy mcp config
   ```

4. In the chat, prompt `@AGENTS.md` to sync the project context and agent instructions.

### Manual Configuration

If you prefer to configure the MCP server manually, use the following configuration:

```json
{
  "mcpServers": {
    "ivy-release": {
      "command": "<path-to-dotnet-tools>/ivy",
      "args": [
        "mcp",
        "--path",
        "<path-to-your-project>"
      ],
      "env": {
        "Ivy__Mcp__ApiUrl": "https://mcp.ivy.app"
      }
    }
  }
}
```
