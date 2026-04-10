---
name: Tendril Database Migrations
description: Tendril uses DatabaseMigrator pattern for schema changes - add new migrations, don't modify EnsureSchema
type: project
---

**As of plan 01967**, Tendril's PlanDatabaseService refactored from direct `EnsureSchema()` to use a `DatabaseMigrator` class with versioned migrations.

**Structure:**
- `Database/DatabaseMigrator.cs` — loads and applies migrations
- `Database/IMigration.cs` — migration interface
- `Database/Migrations/Migration_NNN_Description.cs` — individual migrations

**When adding new schema changes:**
1. Create a new migration file: `Migration_NNN_Description.cs` (NNN = next sequential number)
2. Implement `IMigration` interface with:
   - `Version` property (sequential integer)
   - `Description` property
   - `Apply(SqliteConnection)` method
3. Add schema changes in `Apply()` method
4. **CRITICAL:** End with `PRAGMA user_version = NNN;` to mark the migration version

**Why:** The migration system tracks database version and applies only pending migrations. Direct schema changes in `EnsureSchema()` will conflict with this system.

**How to apply:** When resolving merge conflicts involving PlanDatabaseService schema changes, extract schema changes into a new migration file rather than merging into constructor or EnsureSchema method (which no longer exists).

**Example:** Plan 01971 initially added FTS5 to EnsureSchema. After merge conflict with main (which had DatabaseMigrator), the FTS5 changes were moved to Migration_002_Fts5Search.cs.
