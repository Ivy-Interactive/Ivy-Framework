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
---

# Getting Started: Ivy MCP Server

<Ingress>
The Ivy MCP Server enables AI assistants to directly interact with the Ivy Framework, providing them with the capability to read documentation, query widget properties, and build complex Ivy applications. By connecting your AI tools to the Ivy MCP Server, you can unlock powerful agentic coding workflows tailored for the Ivy ecosystem.
</Ingress>

## Prerequisites

Before connecting your IDE, you should install the Ivy CLI. This tool provides commands to automatically configure and scaffold Ivy projects with MCP support.

```bash
dotnet tool install -g Ivy.Console
```

## Quick Start: Connecting Your IDE

Select the instructions for your preferred AI assistant or IDE below to set up the Ivy MCP connection.

### Claude  

To use the Ivy MCP Server with the Claude Desktop app, you can use the Ivy CLI to automatically configure it, or add it manually.

#### 1. Automatic Configuration

Run the following command in your terminal to create a specific MCP configuration for your project:

```bash
ivy mcp config
```

*Alternatively, if you are starting a new project, you can scaffold it with Claude support:*

```bash
ivy init --hello --claude
```

#### 2. Manual Configuration

1. Open your `claude_desktop_config.json` file.
2. Add the Ivy server to the `mcpServers` object:

```json
{
  "mcpServers": {
     "ivy-release": {
      "command": "/Users/<your-user>/.dotnet/tools/ivy",
      "args": [
        "mcp",
        "--path",
        "/absolute/path/to/your/project"
      ],
      "env": {
        "Ivy__Mcp__ApiUrl": "https://staging.mcp.ivy.app"
      }
    }
  }
}
```

3. Restart the Claude Desktop app.

### Cursor

Configure Cursor to interact with the Ivy MCP Server via its feature settings or by using the Ivy CLI.

#### 1. Automatic Configuration

Use the Ivy CLI to scaffold a new project with Cursor support:

```bash
ivy init --hello --cursor
```

#### 2. Manual Configuration

1. Navigate to **Settings** > **Features** > **MCP**.
2. Click **+ Add New MCP Server**.
3. Name the server **Ivy**.

- Type: **command**
- Command: `/Users/<your-user>/.dotnet/tools/ivy`
- Arguments: `mcp --path /absolute/path/to/your/project`
- Environment Variables: `Ivy__Mcp__ApiUrl=https://staging.mcp.ivy.app`

4. Click **Save** to finalize the integration.


### VS Code

The Ivy VS Code extension provides the most seamless experience for using the Ivy MCP Server.

1. Open the **Extensions** view in VS Code (`Ctrl+Shift+X`).
2. Search for **"Ivy"**.
3. Click **Install**.

<Callout Type="tip">
Once installed, the extension will automatically manage the configuration of the Ivy MCP Server for your environment.
</Callout>

### Antigravity

Antigravity features native, built-in support for the Ivy MCP Server, providing first-class agentic coding capabilities.

1. Open Antigravity **Settings**.
2. Navigate to **MCP Servers**.
3. Locate the **Ivy MCP Server** entry.
4. Toggle it to **Enabled**.

The server will start automatically and its tools will be immediately available to the built-in AI assistant.


## API Reference

The Ivy MCP Server provides several endpoints for retrieving framework context and documentation.

### Base URL
`https://staging.mcp.ivy.app`

### Endpoints

#### GET /questions
Ask questions about the Ivy Framework and receive markdown-formatted answers.
- **Parameters:**
  - `question` (required): The user's question.
  - `packageVersion`: The package version (defaults to `lts`).
  - `packageId`: The package ID (defaults to `Ivy`).

#### GET /docs
List all available documentation files in YAML format.
- **Parameters:**
  - `version`: The version (defaults to `lts`).

#### GET /docs/\{path\}
Retrieve the content of a specific documentation file.
- **Parameters:**
  - `path` (required): The relative path to the file (e.g., `ApiReference/IvyShared/Colors.md`).
  - `version`: The version.

#### GET /widgets
Get a list of all available Ivy widgets and their properties.
- **Parameters:**
  - `version`: The package version.
  - `packageId`: The package ID.