---
name: ivy-create-db-connection
description: >
  Add a database connection to an Ivy project. Supports SQL Server, Postgres,
  Supabase, MySQL, MariaDB, SQLite, Airtable, Oracle, Spanner, ClickHouse,
  and Snowflake. Scaffolds a DbContext from an existing database with Entity
  Framework. Use when the user wants to connect to a database.
allowed-tools: Bash(dotnet:*) Bash(ivy:*) Read Write Edit Glob Grep
effort: medium
argument-hint: "[database provider, e.g. Postgres, SqlServer, Sqlite]"
---

# ivy-create-db-connection

Add a database connection to an existing Ivy project. This skill guides you through selecting a database provider, collecting connection details, and generating a DbContext with Entity Framework scaffolding.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-db-connection.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Reference Files

Read before implementing:
- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference (widgets, hooks, layouts, inputs, colors)

## Step 1: Validate the Project

1. Verify this is a valid Ivy project. Check for a `.csproj` file and `Program.cs` in the working directory. If this is not an Ivy project, tell the user and stop.

2. Clean up any empty connection folders under `Connections/` if they exist.

## Step 2: Choose a Database Provider

3. If the user has not already specified a provider, ask them to choose one from the supported list:

| Provider | Display Name | Connection String Template | Supports Schemas | Default Schema |
|---|---|---|---|---|
| **SqlServer** | SQL Server | `Server=localhost;Database={name};Trusted_Connection=True;TrustServerCertificate=True;` | Yes | `dbo` |
| **Postgres** | Postgres | `Host=localhost;Database={name};Username=postgres;Password=postgres;` | Yes | `public` |
| **Supabase** | Supabase | `Host=db.<project-ref>.supabase.co;Database=postgres;Username=postgres;Password=<password>;Port=5432;` | Yes | `public` |
| **MySql** | MySQL | `Server=localhost;Database={name};User=root;Password=root;` | No | -- |
| **MariaDb** | MariaDB | `Server=localhost;Database={name};User=root;Password=root;` | No | -- |
| **Sqlite** | SQLite | `Data Source={name}.db;` | No | -- |
| **Airtable** | Airtable | (custom) | No | -- |
| **Oracle** | Oracle | `Data Source=localhost:1521/XEPDB1;User Id=system;Password=oracle;` | No | -- |
| **Spanner** | Spanner | `Data Source=projects/<project>/instances/<instance>/databases/<database>;` | No | -- |
| **ClickHouse** | ClickHouse | `Host=localhost;Port=8123;Database={name};` | No | -- |
| **Snowflake** | Snowflake | `account=<account>;host=<account>.snowflakecomputing.com;user=<user>;password=<password>;db={name};schema=PUBLIC;warehouse=<warehouse>;` | No | -- |

## Step 3: Collect Connection Details

4. Ask the user for a connection name in **PascalCase** (e.g. "MyDatabase", "SalesDb"). The name must match the regex `^[A-Z][a-zA-Z0-9]*$`. If the name is invalid, ask the user to provide a valid one.

5. Collect the connection string. The user may provide either:
   - A complete connection string -- pass it through as-is
   - Individual database credentials (host, port, user, password, database name) -- construct the full connection string using the provider's template

   Connection string formats by provider:
   - **Postgres / Supabase**: `Host={host};Port={port};Database={db};Username={user};Password={password}`
   - **SQL Server**: `Server={host},{port};Database={db};User Id={user};Password={password};TrustServerCertificate=True`
   - **MySQL / MariaDB**: `Server={host};Port={port};Database={db};User={user};Password={password};`
   - **Oracle**: `Data Source={host}:{port}/{service};User Id={user};Password={password};`

   If the connection string cannot be inferred from context, ask the user for it.

6. For providers that support schemas (SQL Server, Postgres, Supabase), ask the user which database schema to use. Recommend the provider's default schema (e.g. `dbo` for SQL Server, `public` for Postgres/Supabase).

## Step 4: Generate the Connection

7. Create the connection directory at `Connections/[ConnectionName]/`.

8. The connection generation involves:
   - Adding the provider-specific EF Core NuGet packages
   - Storing the connection string in dotnet user secrets as `ConnectionStrings:[ConnectionName]`
   - Scaffolding the DbContext and entity classes from the database using `dotnet ef dbcontext scaffold`
   - Creating the connection class implementing `IConnection`

9. Initialize user secrets and store the connection string:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:[ConnectionName]" "[connection-string]"
```

10. Use `ivy docs` or `ivy ask` to look up provider-specific guidance if you need details about the EF Core provider package name, scaffolding options, or connection registration pattern.

Expected directory structure after completion:

```
[UserProject]/
├── [UserProject].csproj
├── Program.cs
├── Connections/
│   └── [ConnectionName]/
│       ├── [ConnectionName]Connection.cs
│       ├── [ConnectionName]Context.cs
│       └── (Entity classes...)
```

## Step 5: Verify

11. Run `dotnet build` to verify everything compiles. Fix any errors.

12. Run `dotnet build` to verify the project compiles. Then test the connection by running `dotnet run` and verifying the app starts without errors. If the test fails, investigate and fix the issue. Common causes:
    - Build errors: identify and fix compilation issues
    - Network/auth errors: verify connection string, credentials, and server reachability
    - SSL issues: check TrustServerCertificate or SSL mode settings

13. If the project is in a git repository, create a commit with a descriptive message, for example: "Added [ProviderDisplayName] connection '[ConnectionName]'."

14. Tell the user the connection is ready and summarize:
    - Connection name and provider
    - Files generated in `Connections/[ConnectionName]/`
    - Connection string stored in dotnet user secrets as `ConnectionStrings:[ConnectionName]`
    - Schema used (if applicable)

15. After confirming the connection works, review the user's original request. If they asked to generate apps from the database (e.g., "generate apps for each table", "create CRUD apps"), use the `/ivy-create-crud` or `/ivy-create-app` skill for each entity. Use `ivy cli explain connections/[ConnectionName]` to get the list of entities.

## Recovery

If the setup fails:

1. Diagnose the root cause (build errors, SSL settings, credentials, network issues):
   - For build errors: use `dotnet build` to identify and fix compilation issues
   - For network/auth errors: verify connection string, credentials, and server reachability

2. After fixing the underlying issue, either retry using the `/ivy-create-db-connection` skill from scratch, or manually create the connection:
   - Create a DbContext class in `Connections/[ConnectionName]/`
   - Create entity classes matching the database tables
   - Register the connection in Program.cs
   - Test the connection with a simple query

3. Use `ivy ask "How do I create a DbContext?"` or `ivy ask "How do I register a database connection?"` for API guidance.

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-db-connection.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
