# MySQL/MariaDB-Specific Notes

The target database is MySQL using Pomelo.EntityFrameworkCore.MySql.

## Data Type Mappings

- Boolean: MySQL uses TINYINT(1). EF Core handles this automatically.
- GUID/UUID: No native UUID. Use `[Column(TypeName = "char(36)")]` for Guid properties.
- DateTime precision: Use `[Column(TypeName = "datetime(6)")]` for microseconds.
- JSON: MySQL 5.7+ supports native JSON. Use `[Column(TypeName = "json")]`.

## Enum Handling

MySQL supports ENUM types with EnumToStringConverter:
```csharp
modelBuilder.Entity<TheEntity>(eb =>
{
    eb.Property(e => e.TheEnumProperty)
      .HasConversion(new EnumToStringConverter<TheEnumType>());
});
```

For simplicity in Code First generation, prefer using lookup tables (same pattern as SQLite/SqlServer) instead of native MySQL enums.
