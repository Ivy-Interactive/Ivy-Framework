# SQLite Seeding Notes

The target database is SQLite using Microsoft.EntityFrameworkCore.Sqlite.

## DateTime Handling

SQLite stores DateTime values as TEXT, REAL, or INTEGER (depending on configuration). Default EF Core behavior stores DateTime as TEXT in ISO 8601 format. Both `DateTime.Now` and `DateTime.UtcNow` work correctly.

## DateTimeOffset LINQ Limitation

IMPORTANT: When using SQLite, `DateTimeOffset` columns cannot be compared in LINQ `Where` clauses. The SQLite EF Core provider cannot translate `DateTimeOffset` comparisons to SQL, causing runtime errors ("LINQ expression could not be translated") even though the code compiles successfully.

For date filtering, either:
- Store dates as `DateTime` (not `DateTimeOffset`) in SQLite models
- Or load records first, then filter `DateTimeOffset` ranges in memory:
  ```csharp
  var entries = await db.TimeEntries.Where(t => t.CompanyId == id).ToListAsync();
  var filtered = entries.Where(t => t.ClockInAt >= start && t.ClockInAt <= end);
  ```
