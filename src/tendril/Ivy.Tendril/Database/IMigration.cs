using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database;

public interface IMigration
{
    /// <summary>
    /// Migration version number. Must be sequential (1, 2, 3, ...).
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Human-readable description of what this migration does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Apply the migration to the database.
    /// Should be idempotent if possible (use IF NOT EXISTS, IF EXISTS, etc.).
    /// MUST set PRAGMA user_version = {Version} at the end.
    /// </summary>
    void Apply(SqliteConnection connection);
}
