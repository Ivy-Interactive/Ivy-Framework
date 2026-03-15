## Review Checklist

- [ ] Run IvyFeatureTester with a positional record type and verify `DataTableBuilder.Remove()` renders remaining columns correctly
- [ ] Test with EF Core / database-backed `IQueryable` to confirm the expression tree translates to valid SQL (the default values for removed params should be excluded from the SELECT)
