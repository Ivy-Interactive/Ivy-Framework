![logo](https://cdn.ivy.app/logo_green_w200.png)

# Build Internal Applications with AI and Pure C\#

Ivy - The ultimate framework for building internal tools with LLM code generation by unifying front-end and back-end into a single C# codebase. With Ivy, you can build robust internal tools and dashboards using C# and AI assistance based on your existing database.

<https://github.com/user-attachments/assets/ba2bf5a5-8dc7-4501-9072-6af7483be4f7>

## Features

- 🛠️ **CLI**: Init new projects, add data providers, generate apps using AI and deployments.
- 💡 **Authentication**: Integrations with Supabase, Microsoft Entra, and more.
- 🗄️ **Databases**: Easy integration with SqlServer, Postgres, Supabase, MariaDB, Mysql, Airtable, Oracle, Google Spanner, Clickhouse, Snowflake and Big Query.
- 🤖 **LLM Code Agent**: Generate entire backoffice based on your database schema.
- 🕵️ **Secrets Management**
- 🚀 **Container Deployment**: Easily deploy to Azure, AWS, or Google Cloud.
- 🔥 **Hot Reload**: With maintained state!
- 🧩 **Dependency Injection**
- 📍 **State Management**
- 🧭 **Routing**
- 🧱 **External Widget Framework**: Integrate any React, Angular, or Vue component (coming soon)
- 🔢 **Data Tables**: Sort, filter, and paginate data. (coming soon)

## Current State

Ivy is still early in development and we are working on more robust documentation. We release new versions almost daily.  

## Inspirations

- React
- FuncUI
- Streamlit
- LINQPad ("Dump" method)

## Usage

### Quick Start

Make sure you have the .Net 9 SDK installed.

1. **Install Ivy CLI**:

   ```bash
   dotnet tool install -g Ivy.Console
   ```

2. **Create a new project**:

   ```bash
    ivy init
    ```

3. **Add a data provider**:

   ```bash
   ivy connect db
   ```

4. **Run**:

   ```bash
   dotnet watch
   ```

5. **Open** [http://localhost:5010](http://localhost:5010) in your browser.

You can also run `ivy samples` to see all the components that Ivy offers and `ivy docs` for documentation.  

### Developer Build

1. **Install dependencies**:
   - [Node & npm](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)
   - [Vite](https://vitejs.dev/)
   - [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
2. **Fork and clone this repository**.
3. **Build the frontend**:

   ```bash
   cd frontend
   npm install
   npm run build
   npm run dev
   ```

4. **Run Ivy.Samples backend**:

   ```bash
   cd Ivy.Samples
   dotnet watch
   ```

5. **Open** [http://localhost:5137](http://localhost:5173) in your browser.

Changes in /frontend will be hot-reloaded by Vite and changes in /Ivy.Samples will be hot-reloaded by Ivy.

### Code Formatting

To format .NET code:

```bash
dotnet format
```

## Contributing

1. **Fork** the repository.
2. **Create** a new feature branch.
3. **Submit** a pull request.

Feel free to file issues and feature requests.
