# PostgreSQL Seeding Notes

The target database is PostgreSQL using Npgsql.EntityFrameworkCore.PostgreSQL.

## Critical DateTime Handling

ALWAYS use `DateTime.UtcNow` for timestamps, NEVER `DateTime.Now`.

PostgreSQL `timestamptz` columns require UTC values. Using `DateTime.Now` will cause:
`Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone'`

### DateTime Seeding Rules

1. ALWAYS use `DateTime.UtcNow` for current timestamps
2. DO NOT use `DateTime.Now` - it creates local time which PostgreSQL will reject or convert incorrectly
3. For past/future dates, use the refDate parameter with `DateTime.UtcNow`:
   - `f.Date.Past(1, DateTime.UtcNow)`
   - `f.Date.Future(1, DateTime.UtcNow)`
   - `f.Date.Recent(30, DateTime.UtcNow)`
4. For Between dates, ensure both bounds are UTC:
   - `f.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow)`

### Why UTC is Required

- PostgreSQL's timestamptz stores all timestamps in UTC internally
- When you provide a non-UTC DateTime, PostgreSQL converts it based on the server's timezone
- This can lead to incorrect timestamps and timezone-related bugs
- Using `DateTime.UtcNow` ensures consistent, correct timestamps regardless of server timezone configuration

### Common Mistake

WRONG:
```csharp
.RuleFor(e => e.CreatedAt, f => f.Date.Past(1))  // Missing refDate, uses DateTime.Now
.RuleFor(e => e.UpdatedAt, (f, e) => f.Date.Between(e.CreatedAt, DateTime.Now))
```

CORRECT:
```csharp
.RuleFor(e => e.CreatedAt, f => f.Date.Past(1, DateTime.UtcNow))
.RuleFor(e => e.UpdatedAt, (f, e) => f.Date.Between(e.CreatedAt, DateTime.UtcNow))
```
