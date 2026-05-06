# SQLite-Specific Notes

The target database is SQLite using Microsoft.EntityFrameworkCore.Sqlite.

## Enums

SQLite does not support enums natively. For every enum in DBML, generate:

1. A lookup entity with `Id` (int) and `DescriptionText` (string), seeded with the enum values.
2. For every referencing entity: an int FK property and a navigation property.

Example for enum `Gender { Male, Female, Other }`:

```csharp
public class Gender
{
    public int Id { get; set; }
    public string DescriptionText { get; set; } = null!;

    public static Gender[] GetSeedData() =>
    [
        new() { Id = 1, DescriptionText = "Male" },
        new() { Id = 2, DescriptionText = "Female" },
        new() { Id = 3, DescriptionText = "Other" }
    ];
}
```

In OnModelCreating:
```csharp
modelBuilder.Entity<Gender>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).ValueGeneratedNever();
    entity.Property(e => e.DescriptionText).IsRequired().HasMaxLength(200);
    entity.HasData(Gender.GetSeedData());
});
```

## Composite Primary Keys (Junction Tables)

Junction/join tables with composite primary keys (from DBML `indexes { (FkA, FkB) [pk] }`) MUST be configured in `OnModelCreating` using `HasKey`:

```csharp
modelBuilder.Entity<StartupFounder>(entity =>
{
    entity.HasKey(e => new { e.StartupId, e.FounderId });
});
```

This is required because `[Key]` data annotations cannot express composite keys. Without this configuration, EF Core will fail at runtime with "requires a primary key to be defined".

## DateTimeOffset LINQ Limitation

IMPORTANT: When using SQLite, `DateTimeOffset` columns cannot be compared in LINQ `Where` clauses. The SQLite EF Core provider cannot translate `DateTimeOffset` comparisons to SQL, causing runtime errors ("LINQ expression could not be translated") even though the code compiles successfully.

Prefer `DateTime` over `DateTimeOffset` for date/time properties in SQLite models. If `DateTimeOffset` must be used (e.g. from the DBML spec), add a comment on those properties warning about the LINQ limitation:

```csharp
/// <summary>
/// WARNING: DateTimeOffset cannot be compared in LINQ Where clauses on SQLite.
/// Filter in memory after loading, or convert to DateTime.
/// </summary>
public DateTimeOffset ClockInAt { get; set; }
```
