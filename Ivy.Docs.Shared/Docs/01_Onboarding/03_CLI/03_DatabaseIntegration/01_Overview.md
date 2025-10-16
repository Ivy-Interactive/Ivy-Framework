---
searchHints:
  - database
  - entityframework
  - sql
  - connection
  - integration
  - db
---

# Ivy Database Integration

<Ingress>
Connect your Ivy application to various databases with automatic Entity Framework configuration for SQL Server, PostgreSQL, MySQL, SQLite, and more.
</Ingress>

The `ivy db` commands allow you to add and manage database connections in your Ivy project. Ivy supports a wide range of database providers and automatically generates the necessary Entity Framework configurations.

## Adding a Database Connection

To add a database connection to your Ivy project, run:

```terminal
>ivy db add
```

When you run this command without specifying options, Ivy will guide you through an interactive setup:

1. **Select Database Provider**: Choose from the available providers
2. **Connection Name**: Enter a name for your connection (PascalCase recommended)
3. **Connection String**: Provide the connection string or other information to let Ivy build it for you, depending on the provider.

### Command Options

`--provider <PROVIDER>` or `-p <PROVIDER>` - Specify the database provider directly:

```terminal
>ivy db add --provider Postgres
```

Available providers: `SqlServer`, `Postgres`, `MySql`, `MariaDb`, `Sqlite`, `Supabase`, `Airtable`, `Oracle`, `Spanner`, `ClickHouse`, `Snowflake`

`--name <CONNECTION_NAME>` or `-n <CONNECTION_NAME>` - Specify the connection name in PascalCase:

```terminal
>ivy db add --name MyDatabase
```

`--connection-string <CONNECTION_STRING>` - Provide the connection string directly:

```terminal
>ivy db add --provider Postgres --connection-string YourConnectionString
```

`--schema <SCHEMA>` - Specify the database schema (for providers that support it):

```terminal
>ivy db add --provider Postgres --schema public
```

`--verbose` or `-v` - Enable verbose output for detailed logging:

```terminal
>ivy db add --verbose
```

## Generating A Database Schema

This command opens a GUI to guide you through the process of designing a database schema using generative AI:

```terminal
>ivy db generate
```

First, you will be prompted for a choice of database provider, a format for table and field names, and a description of your database schema. Currently, database schema generation supports the providers SQLite, SQL Server, PostgreSQL, and MySQL, and the casing schemes `PascalCase`, `camelCase` and `snake_case`. The defaults are SQLite and `snake_case`.

![Ivy Database Generator UI, start page](assets/db_generator_1.webp "Ivy Database Generator UI")

If you're having trouble coming up with a schema description, Ivy includes a variety of examples to serve as a starting point:

![Ivy Database Generator UI, samples dropdown](assets/db_generator_2.webp "Ivy Database Generator UI")

On the next page, you can preview Ivy's generated schema on the left, with a graph of relationships on the right:

![Ivy Database Generator UI, preview page](assets/db_generator_3.webp "Ivy Database Generator UI")

Finally, you can choose which apps to generate, make a final selection of database provider, tweak the connection string, and choose whether or not to generate seed data:

![Ivy Database Generator UI, app selection page](assets/db_generator_4.webp "Ivy Database Generator UI")

Ivy will then generate your database schema and associated apps:

![Ivy Database Generator UI, generation page](assets/db_generator_5.webp "Ivy Database Generator UI")

## Supported Database Providers

Ivy supports the following database providers. Click on any provider for detailed setup instructions:

### Relational Databases

- **[SQL Server](SqlServer.md)** - Microsoft's enterprise database
- **[PostgreSQL](PostgreSql.md)** - Advanced open-source database
- **[MySQL](MySql.md)** - Popular open-source database
- **[MariaDB](MariaDb.md)** - MySQL fork with enhanced features
- **[SQLite](SQLite.md)** - Lightweight file-based database
- **[Oracle](Oracle.md)** - Enterprise database system

### Cloud Databases

- **[Supabase](Supabase.md)** - Open-source Firebase alternative with PostgreSQL
- **[Google Spanner](GoogleSpanner.md)** - Globally distributed database
- **[Snowflake](Snowflake.md)** - Cloud data platform

### Specialized Databases

- **[ClickHouse](ClickHouse.md)** - Column-oriented database for analytics
- **[Airtable](Airtable.md)** - Spreadsheet-database hybrid

## Connection Configuration Details

Ivy automatically configures:

- **Connection strings** stored securely using .NET User Secrets
- **Entity Framework Core** with the appropriate provider, and generated context and entity classes
- **Ivy connection** to facilitate communication between Ivy apps and the database provider

### Security and Secrets Management

Ivy automatically configures .NET User Secrets for secure connection string storage:

```terminal
>dotnet user-secrets list
```

#### Environment Variables

You can also use environment variables for connection strings (with the exception of SQLite connection strings):

```text
export ConnectionStrings__MY_DATABASE_CONNECTION_STRING="Host=localhost;Database=mydb;Username=user;Password=pass"
```

### Connection Structure

Each database connection has its own a folder structure:

```text
Connections/
└── [ConnectionName]/
    ├── [ConnectionName]Context.cs             # Entity Framework DbContext
    ├── [ConnectionName]ContextFactory.cs      # DbContext factory
    ├── [ConnectionName]Connection.cs          # Connection configuration
    └── [EntityName].cs...                     # One or more generated entity classes
```

#### Entity Framework Integration

**Connection** - Ivy generates a class for each connection to facilitate communication with the database provider:

```csharp
public class MyDatabaseConnection : IConnection
{
    public string GetContext(string connectionPath)
    {
        var connectionFile = nameof(MyDatabaseConnection) + ".cs";
        var contextFactoryFile = nameof(MyDatabaseContextFactory) + ".cs";
        var files = Directory.GetFiles(connectionPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(connectionFile) && !f.EndsWith(contextFactoryFile))
            .Select(File.ReadAllText)
            .ToArray();
        return string.Join(Environment.NewLine, files);
    }

    public string GetName() => nameof(MyDatabase);

    public string GetNamespace() => typeof(MyDatabaseConnection).Namespace;

    public ConnectionEntity[] GetEntities()
    {
        return typeof(MyDatabaseContext)
            .GetProperties()
            .Where(e => e.PropertyType.IsGenericType && e.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(e => new ConnectionEntity(e.PropertyType.GenericTypeArguments[0].Name, e.Name))
            .ToArray();
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<MyDatabaseContextFactory>();
    }
}
```

**DbContext** - A DbContext class is also generated for each connection:

```csharp
public partial class MyDatabaseContext : DbContext
{
    public MyDatabaseContext(DbContextOptions<MyDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
```

**DbContext Factory** - Finally, a context factory class is generated for dependency injection:

```csharp
public class MyDatabaseContextFactory(ServerArgs args) : IDbContextFactory<MyDatabaseContext>
{
    public MyDatabaseContext CreateDbContext()
    {
        var configuration = new ConfigurationBuilder()
           .AddEnvironmentVariables()
           .AddUserSecrets(Assembly.GetExecutingAssembly())
           .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MyDatabaseContext>();

        var connectionString = configuration.GetConnectionString("MY_DATABASE_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'MY_DATABASE_CONNECTION_STRING' is not set.");
        }

        // Ivy will use the appropriate method for your chosen DB provider here
        optionsBuilder.UseDbProvider(connectionString);

        if (args.Verbose)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }

        return new MyDatabaseContext(optionsBuilder.Options);
    }
}
```

### Multiple Database Connections

You can add multiple database connections to a single project:

```terminal
>ivy db add --provider Postgres --name PrimaryDb
>ivy db add --provider Sqlite --name LogDb
>ivy db add --provider ClickHouse --name AnalyticsDb
```

### Troubleshooting

**Connection String Issues** - Ensure the connection string format is correct for your provider, verify database server is running and accessible (if applicable), and check firewall settings and network connectivity.

**Entity Framework Issues** - Ensure required NuGet packages are installed, verify .NET EF tools are installed: `dotnet tool install -g dotnet-ef`, and check for conflicting Entity Framework versions.

**Authentication Issues** - Ensure you're logged in: `ivy login` and verify your Ivy account has the necessary permissions.

## Examples

**Basic PostgreSQL Setup**

```terminal
>ivy db add --provider Postgres --name MyProjectDb
```

**SQL Server with Custom Schema**

```terminal
>ivy db add --provider SqlServer --name InventoryDb --schema dbo
```

**Supabase Integration**

```terminal
>ivy db add --provider Supabase --name UserDb
```

**Multiple Databases**

```terminal
>ivy db add --provider Postgres --name PrimaryDb
>ivy db add --provider Sqlite --name LogDb
>ivy db add --provider ClickHouse --name AnalyticsDb
```

## Default schema

The ivy db add command includes a `--use-default-schema` parameter that automatically uses the database's default schema without prompting.

```terminal
>ivy db add --provider postgres --connection-string "..." --name MyDb --use-default-schema

```

The default schemas for each database provider are:

- `PostgreSQL/Supabase` – public
- `SQL Server` – dbo
- `Oracle` – Uses the connected username as the default schema
- `ClickHouse` – default
- `Snowflake` – PUBLIC

<Callout Type="Warning">
You cannot use both --schema and --use-default-schema parameters together. Choose one based on whether you want to specify a custom schema or use the database default.
</Callout>

## Related Commands

- `ivy init` - Initialize a new Ivy project
- `ivy auth add` - Add authentication providers
- `ivy app create` - Create apps
- `ivy deploy` - Deploy your project
