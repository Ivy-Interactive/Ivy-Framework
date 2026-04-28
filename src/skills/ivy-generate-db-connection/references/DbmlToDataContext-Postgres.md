# PostgreSQL-Specific Notes

The target database is PostgreSQL using Npgsql.EntityFrameworkCore.PostgreSQL.

## Critical DateTime Handling

PostgreSQL `timestamptz` only accepts UTC DateTime values. Configure in OnModelCreating:
```csharp
.HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
```
Alternative: Use `DateTimeOffset` for timezone-aware dates.

## Data Type Mappings

- JSON: `[Column(TypeName = "jsonb")]`
- Arrays: Use C# arrays or `List<T>` directly
- UUID: Use `Guid`, configure as `.HasColumnType("uuid")`
- Serial/Identity: `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]`

## Enum Handling

PostgreSQL supports native enum types. To use them correctly with Npgsql v10+, configuration must happen in two places:

### 1. In the DbContext's `OnModelCreating`

Register each enum type in the model:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasPostgresEnum<TheEnumType>("the_postgres_enum_name");
    // ... rest of model config
}
```

### 2. In the Connection's `RegisterServices` method

The `NpgsqlDataSourceBuilder` must map each enum BEFORE the data source is used. This goes in the Connection class's `RegisterServices(Server server)` method — NOT in `OnConfiguring` or `Program.cs`:
```csharp
var connectionString = server.Configuration["ConnectionStrings:DbName"];
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<TheEnumType>("the_postgres_enum_name");
var dataSource = dataSourceBuilder.Build();
```

Then pass the data source (not the connection string) to `UseNpgsql`:
```csharp
server.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(dataSource));
```

**Important**: The enum type name string must match the actual PostgreSQL enum type name exactly (e.g., `"test_run_state"`), including schema if not `public`.

**Note**: Do NOT use `OnConfiguring` for this. Do NOT modify `Program.cs`. All service registration is self-contained in the Connection's `RegisterServices` method — a connection must be removable by deleting its folder.
