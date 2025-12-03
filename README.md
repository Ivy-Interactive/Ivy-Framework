![logo](https://cdn.ivy.app/logo_green_w200.png)

# Build Internal Applications with AI and Pure C\#

Ivy - The ultimate framework for building internal tools with LLM code generation by unifying front-end and back-end into a single C# codebase. With Ivy, you can build robust internal tools and dashboards using C# and AI assistance based on your existing database.

[Sign up for our waitlist](https://ivy.app/join-waitlist) to be among the first to get access or [book a 1-on-1 session](https://calendly.com/mikael-ivy/30min) with one of our founding engineers.

[Documentation](https://docs.ivy.app) | [Samples](https://samples.ivy.app) | [Current Sprint](https://github.com/orgs/Ivy-Interactive/projects/8) | [Roadmap](https://github.com/orgs/Ivy-Interactive/projects/7) | [Examples](https://github.com/Ivy-Interactive/Ivy-Examples)

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Devcontainer&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json&location=EuropeWest)

<https://github.com/user-attachments/assets/ba2bf5a5-8dc7-4501-9072-6af7483be4f7>

The code should be familiar to React developers. Views → Components, Build → Render, Widgets → Element.

![3g829iF9gmsQUX](https://github.com/user-attachments/assets/c475d90f-4cca-4e46-8a8e-4ee3f2545751)

## Features

- 🛠️ **CLI**: Init new projects, add data providers, generate apps using AI, and manage deployments.
- 💡 **Authentication**: Integrations with Supabase, Auth0, Clerk, Microsoft Entra (more is coming)
- 🗄️ **Databases**: Easy integration with SQL Server, Postgres, Supabase, MariaDB, MySQL, Airtable, Oracle, Google Spanner, Clickhouse, Snowflake, and BigQuery.
- 🤖 **LLM Code Agent**: Generate an entire back office application based on your database schema.
- 🕵️ **Secrets Management**
- 🚀 **Container Deployment**: Easily deploy to Azure, AWS, or Google Cloud or Sliplane.
- 🔥 Full support for **Hot-Reloading** with maintained state as much as possible (suck on that Blazor).
- 🧩 **Dependency Injection**
- 📍 **State Management**: State is managed on the server, making this very secure.
- 🧱 **Building Blocks**: Extensive set of widgets to build any app. An external widget framework is coming soon, where you can integrate any React, Angular, or Vue component.
- 🧱 **External Widget Framework**:
- 🔢 **Data Tables**: Sort, filter, and paginate data. (coming soon)

We optimise for the 3 X:s - UX (love your end users), DX (let Ivy love you) - LX (minimise LLMs mistakes)

Ivy maintains state on the server and sends updates over WebSocket. The frontend consists of a pre-built React-based rendering engine. With Ivy, you never need to touch any HTML, CSS, or JavaScript. Only if you want to add you’re own widgets.

The whole framework is built around strict enterprise security constraints. As the state is fully maintained on the BE, we can minimise the risk of secrets leakage. This is a major problem with prototype tools like Lovable/vo/Bolt. All authentication integrations are handcrafted and audited.

## Usage

### Quick Start

> ⚠️ **Note:** Ivy.Console is still in beta, and the agentic features require an account. [Sign up for our waitlist](https://ivy.app/join-waitlist) to be among the first to get access.

Make sure you have the [.NET 9 SDK installed](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

1. **Install Ivy CLI**:

   ```bash
   dotnet tool install -g Ivy.Console
   ```

2. **Create a new project**:

   ```bash
    ivy init --hello
    ```

3. **Run**:

   ```bash
   dotnet watch
   ```

4. **Open** [http://localhost:5010](http://localhost:5010) in your browser.

You can also run `ivy samples` to see all the components that Ivy offers and `ivy docs` for documentation.  

## Framework Developer Instructions

If you want to work on the framework itself, you need to set up the following:

1. **Install dependencies**:
   - [Node 22.12+ & npm](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)
2. **Fork and clone this repository**.
3. **Pre-generate documentation files** (first time only):

   **Windows (PowerShell):**

   ```powershell
   cd Ivy.Docs.Shared
   .\Regenerate.ps1
   ```

   **Mac/Linux (Bash):**

   ```bash
   cd Ivy.Docs.Shared
   sh ./Regenerate.sh
   ```

4. **Build the frontend**:

   ```bash
   cd frontend
   npm install
   npm run build
   npm run dev
   ```

5. **Run the backend** (choose one):

   **For Ivy.Samples (testing components):**

   ```bash
   cd Ivy.Samples
   dotnet watch
   ```

   **For Ivy.Docs (documentation):**

   ```bash
   cd Ivy.Docs
   dotnet watch
   ```

6. **Open** [http://localhost:5173/](http://localhost:5173/) in your browser.

Changes in /frontend will be hot-reloaded by Vite and changes in /Ivy.Samples will be hot-reloaded by Ivy.

For detailed contribution guidelines, see [CONTRIBUTING.md](CONTRIBUTING.md).

## Troubleshooting

### Apple Silicon Mac: "Bad CPU type in executable" due to Google Protobuf & ARM

Install Rosetta 2: `/usr/sbin/softwareupdate --install-rosetta --agree-to-license`
