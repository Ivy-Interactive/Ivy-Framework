---
title: CLI Overview
searchHints:
  - cli
  - command-line
  - terminal
  - tools
  - commands
  - ivy-console
---

# CLI Overview

<Ingress>
Streamline your Ivy development workflow with powerful CLI tools for project initialization, database integration, authentication, and deployment.
</Ingress>

Ivy CLI is a powerful tool designed to streamline the development of Ivy projects. It provides:

- **Database Integration**: Connect to multiple database providers (SQL Server, PostgreSQL, MySQL, SQLite, and more). See [Database Overview](05_DatabaseIntegration/01_DatabaseOverview.md) and [Connections](../02_Concepts/26_Connections.md).
- **Authentication**: Add [authentication](04_Authentication/01_AuthenticationOverview.md) providers (Auth0, Supabase, Authelia, Basic Auth)
- **Deployment**: [Deploy](06_Deployment/01_DeploymentOverview.md) to cloud platforms (AWS, Azure, GCP)
- **Project Management**: Initialize and manage Ivy projects with ease

## Quick Start

Get started with Ivy CLI in just a few commands:

```terminal
>ivy init
>ivy upgrade
>ivy db add
>ivy auth add
>ivy deploy
```

<Callout Type="tip">
If you're using a specific operating system, read the instructions in your terminal after installing Ivy.Console.
You can always see all available commands by using `ivy --help`.
</Callout>

## Key Features

### Database Support

- **SQL Server** - Microsoft's enterprise database
- **PostgreSQL** - Advanced open-source database
- **MySQL/MariaDB** - Popular open-source databases
- **SQLite** - Lightweight file-based database
- **Supabase** - Open-source Firebase alternative
- **Airtable** - Spreadsheet-database hybrid
- **Oracle** - Enterprise database system
- **Google Spanner** - Globally distributed database
- **ClickHouse** - Column-oriented database
- **Snowflake** - Cloud data platform

### Authentication Providers

- **Auth0** - Universal authentication platform
- **Supabase Auth** - Built-in authentication
- **Authelia** - Open-source identity provider
- **Basic Auth** - Simple username/password authentication

### Deployment Options

- **AWS** - Amazon Web Services
- **Azure** - Microsoft Azure
- **GCP** - Google Cloud Platform

## Project Structure

An Ivy project follows a standardized structure:

```text
YourProject/
├── Program.cs              # Main project entry point
├── YourProject.csproj      # .NET project file
├── README.md               # Project documentation
├── Apps/                   # User interface code
├── Connections/            # Database connections
│   └── [ConnectionName]/   # Individual connection configs
├── .ivy/                   # Ivy-specific configuration, only created by Ivy CLI when necessary
└── .gitignore              # Git ignore file
```

See [Program](../02_Concepts/01_Program.md) for the entry point, [Apps](../02_Concepts/10_Apps.md) for application UI code, and [Connections](../02_Concepts/26_Connections.md) for database connections.

## Getting Help

- Use `ivy --help` for general help
- Use `ivy [command] --help` for command-specific help
- Use `ivy cli explain` for a reliable, built-in structural breakdown of all available CLI commands (preferred over MCP tools)
- Use `ivy docs` to open documentation
- Use `ivy samples` to see example projects

Most Ivy commands require authentication. Use `ivy login` to authenticate with your Ivy account.

<Callout Type="tip">
On macOS, if you get a `command not found: ivy` error, you likely need to add the .NET tools directory to your PATH by adding `export PATH="$PATH:$HOME/.dotnet/tools"` to your `~/.zshrc`.
</Callout>

## Next Steps

1. **Initialize a project**: `ivy init`
2. **Upgrade your project**: `ivy upgrade`
3. **Add a database**: `ivy db add`
4. **Add authentication**: `ivy auth add`
5. **Deploy your project**: `ivy deploy`

For detailed information on each feature, see the specific documentation files:

- [Project Initialization](02_Init.md)
- [Project Upgrade](08_Upgrade.md)
- [Database Integration](05_DatabaseIntegration/01_DatabaseOverview.md)
- [Authentication Setup](04_Authentication/01_AuthenticationOverview.md)
- [Deployment Guide](06_Deployment/01_DeploymentOverview.md)
- [Framework Information](09_Question.md)
- [Documentation Index](10_Docs.md)
