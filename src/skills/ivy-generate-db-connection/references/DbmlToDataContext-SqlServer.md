# SQL Server-Specific Notes

The target database is SQL Server using Microsoft.EntityFrameworkCore.SqlServer.

## Enums

SQL Server does not support enums natively. For every enum in DBML, generate:

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
