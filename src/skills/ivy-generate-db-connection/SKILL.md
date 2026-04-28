---
name: ivy-generate-db-connection
description: >
  Generate a new database with AI-assisted schema design, EF Core Code First
  entities, migrations, and Bogus data seeding. Use when the user asks to generate
  a database, create a database schema, design a data model, set up Entity Framework,
  or work with DBML schemas in their Ivy project.
allowed-tools:
  - Bash(dotnet:*)
  - Read
  - Write
  - Edit
  - Glob
  - Grep
effort: high
---

# Generate a Database Connection

This skill generates a complete database connection in an Ivy project using AI-assisted schema design. It walks through DBML schema generation, database provider configuration, EF Core entity generation, migrations, and Bogus data seeding.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-generate-db-connection.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Prerequisites

- The working directory must be a valid Ivy project.
- `dotnet ef` tool will be installed automatically if not present.
- `dotnet user-secrets` will be initialized automatically.

## Reference Documents

The following reference files in the `references/` folder contain detailed rules for each generation phase. Read the relevant ones at each step:

- `DbmlGeneratePrompt.md` -- DBML schema generation syntax and rules
- `DbmlToDataContextPrompt.md` -- EF Core entity generation rules (converting DBML to C# entities and DbContext)
- `DbmlToDataContext-Sqlite.md` -- SQLite-specific entity generation notes
- `DbmlToDataContext-Postgres.md` -- PostgreSQL-specific entity generation notes
- `DbmlToDataContext-SqlServer.md` -- SQL Server-specific entity generation notes
- `DbmlToDataContext-MySql.md` -- MySQL/MariaDB-specific entity generation notes
- `DataSeederPrompt.md` -- Bogus data seeder generation rules (comprehensive, ~825 lines)
- `DataSeeder-Sqlite.md` -- SQLite-specific seeding notes
- `DataSeeder-Postgres.md` -- PostgreSQL-specific seeding notes
- `DataSeeder-SqlServer.md` -- SQL Server-specific seeding notes
- `DataSeeder-MySql.md` -- MySQL/MariaDB-specific seeding notes

## Workflow

### 1. Ensure Prerequisites

Before starting, verify:

- `dotnet ef` is installed (run `dotnet ef --version`). If not installed, run `dotnet tool install --global dotnet-ef`.
- Initialize user-secrets: `dotnet user-secrets init` in the project directory.

Snapshot the `.csproj` file before any modifications so it can be restored on failure.

### 2. Collect Database Description

Ask the user to describe the database they want to create. Explain that you will generate a database schema from their description, create Entity Framework Core entities, and set up migrations.

Ask the user about:
- What kind of data will be stored (e.g., users, products, orders)
- The relationships between entities (e.g., a user has many orders)
- Any specific fields or constraints they need
- Any enums or status fields

If the user gives a general category (e.g., "CRM", "e-commerce", "blog") or says they want something standard, proceed with a reasonable default schema for that domain. Do NOT keep asking for more details -- the user will be able to review and modify the schema in the next step.

If the user has already specified a database type (e.g., "sqlite", "postgres"), note it so it will not be asked again later.

### 3. Generate DBML Schema

Read `references/DbmlGeneratePrompt.md` for the detailed DBML generation rules.

Generate a DBML schema from the user's description following these key rules:
- Use PascalCase for all table and column names
- Table names must be singular
- Use .NET-friendly data types
- Define tables in DAG order (independent tables first)
- Place all `Ref:` declarations at the end
- Do NOT create empty enums
- Include Id primary keys and CreatedAt/UpdatedAt timestamps

Present the generated DBML schema to the user for review.

### 4. Handle Revisions

If the user requests changes to the schema, revise the DBML accordingly and present it again. Repeat until the user approves.

Once approved, save the DBML schema and proceed.

### 5. Configure Database Provider

Collect the database provider configuration. If the database type was specified in step 2, use it. Otherwise, ask the user to choose from:

- **SQLite** -- local file-based database
- **Postgres** -- PostgreSQL (also used for Supabase)
- **SqlServer** -- Microsoft SQL Server
- **MySql** -- MySQL (also used for MariaDB)

Depending on the provider, collect connection details:
- For SQLite: the database file path (defaults to a local `.db` file)
- For external databases: host, port, database name, username, password

**Test the connection:**
- For SQLite: verify the directory is writable
- For external databases: attempt a basic connectivity check

If the connection fails, ask the user if they want to try with different settings.

**Store the connection string** in dotnet user-secrets as `ConnectionStrings:[ConnectionName]` (for non-SQLite providers).

If the target database is not empty, ask the user whether to:
1. Drop and recreate (data will be lost)
2. Use a different database
3. Cancel

### 6. Generate EF Core Entities

Read `references/DbmlToDataContextPrompt.md` for detailed entity generation rules. Also read the provider-specific reference for the chosen database:
- SQLite: `references/DbmlToDataContext-Sqlite.md`
- Postgres/Supabase: `references/DbmlToDataContext-Postgres.md`
- SQL Server: `references/DbmlToDataContext-SqlServer.md`
- MySQL/MariaDB: `references/DbmlToDataContext-MySql.md`

Generate individual C# files:
- One file per entity class in `Connections/[ConnectionName]/Models/` (e.g., `Connections/[ConnectionName]/Models/User.cs`)
- The DbContext class in `Connections/[ConnectionName]/[ConnectionName]Context.cs`

Key rules:
- Use namespace `[ProjectNamespace].Connections.[ConnectionName].Models` for entity classes
- Use namespace `[ProjectNamespace].Connections.[ConnectionName]` for the DbContext
- The DbContext constructor must accept `DbContextOptions<[ConnectionName]Context>`
- Do NOT include an `OnConfiguring` method
- Use PascalCase for all C# names
- Initialize all non-nullable string properties with `= null!;` and all navigation properties with `= null!;`
- Generate COMPLETE code for ALL entities -- do not skip or abbreviate
- Always include explicit `using` directives in every file
- The DbContext MUST contain `DbSet<T>` properties for every entity

### 7. Build and Fix

Build the project with `dotnet build`. If the build fails, read the affected files, fix the errors, and build again. Repeat until the build succeeds.

After a successful build, validate that the DbContext contains `DbSet<T>` properties for every entity defined in the DBML. If any are missing:

1. Ensure a model class exists in `Connections/[ConnectionName]/Models/`
2. Add the missing `DbSet<T>` property to the DbContext
3. Add any required `OnModelCreating` configuration
4. Do not remove or modify existing DbSet properties
5. Build again

This validation can be attempted up to 3 times. If entities are still missing after 3 attempts, the workflow fails.

### 8. Create and Apply Migration

Create the initial migration:

```
dotnet ef migrations add AddEntities --output-dir Connections/[ConnectionName]/Migrations --context [ConnectionName]Context -- [ConnectionString]
```

Apply the migration:

```
dotnet ef database update --context [ConnectionName]Context -- [ConnectionString]
```

Both commands will be retried up to 3 times on failure with increasing delays.

### 9. Generate Data Seeder

Read `references/DataSeederPrompt.md` for detailed seeder generation rules. Also read the provider-specific seeder reference:
- SQLite: `references/DataSeeder-Sqlite.md`
- Postgres/Supabase: `references/DataSeeder-Postgres.md`
- SQL Server: `references/DataSeeder-SqlServer.md`
- MySQL/MariaDB: `references/DataSeeder-MySql.md`

First, add the Bogus NuGet package: `dotnet add package Bogus`

Generate a `DataSeeder.cs` file in `Connections/[ConnectionName]/` with:

- Class name: `DataSeeder`
- Namespace: `[ProjectNamespace].Connections.[ConnectionName]`
- Two methods:
  - `public async System.Threading.Tasks.Task SeedAsync([ConnectionName]Context context)` -- seeds bogus data
  - `public async System.Threading.Tasks.Task ClearAsync([ConnectionName]Context context)` -- deletes all non-enum/lookup table data in reverse dependency order
- Uses the Bogus library for fake data generation
- Seeds entities in dependency order (independent tables first)
- Checks `AnyAsync()` before seeding each entity type to avoid duplicates
- Does NOT seed enum/lookup tables already seeded via `HasData()` in migrations -- only queries them for FK references

Build the project after generating the seeder. If the build fails, fix and rebuild.

### 10. Run the Seeder

Run the seeder to populate the database with fake data. The seeder is executed via a helper tool.

If the seeder fails at runtime:
1. Analyze the runtime errors carefully
2. Read the `DataSeeder.cs` file and any relevant entity/DbContext files
3. Fix the errors in `DataSeeder.cs`
4. Common runtime issues:
   - Foreign key constraint violations (seeding in wrong order)
   - Duplicate key violations (unique constraint on seeded data)
   - Type mismatches (wrong data types for columns)
   - Null reference exceptions (missing required fields)
   - Re-seeding enum/lookup tables already populated via `HasData()` in migrations
   - Missing `SaveChangesAsync()` between dependent entity groups
5. For "no such table" errors: run `dotnet ef database update --context [ContextName]` to re-apply existing migrations. Do NOT run `dotnet ef migrations add` -- migrations already exist.
6. Scope is limited to fixing `DataSeeder.cs` and the DbContext. Do not restructure project service registration or migration infrastructure.

The seeder will be retried up to 3 times. If it still fails after 3 attempts, the database structure is intact but unseeded.

### 11. Completion

Report the results to the user:

- Connection name and provider
- DBML schema that was generated
- EF Core entities created in `Connections/[ConnectionName]/`
- DbContext class name: `[ConnectionName]Context`
- Initial migration created and applied
- Connection string stored in user secrets (for non-SQLite)
- Whether fake data was seeded successfully

If git is available, commit the changes with a message like: `Generated [Provider] database '[ConnectionName]' with AI-assisted schema.`

## Failure Recovery

If the workflow fails at any point:
- The `.csproj` file is rolled back to its pre-workflow state
- The connection folder at `Connections/[ConnectionName]/` is cleaned up
- The user can retry the skill from scratch

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-generate-db-connection.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
