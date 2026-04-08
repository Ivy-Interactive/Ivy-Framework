using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_003_JobsTable : IMigration
{
    public int Version => 3;
    public string Description => "Add Jobs table for persisting job history";

    public void Apply(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Jobs (
                Id TEXT PRIMARY KEY,
                Type TEXT NOT NULL,
                PlanFile TEXT NOT NULL,
                Project TEXT NOT NULL,
                Status TEXT NOT NULL,
                Provider TEXT NOT NULL DEFAULT 'claude',
                SessionId TEXT,
                StartedAt TEXT,
                CompletedAt TEXT,
                DurationSeconds INTEGER,
                Cost REAL,
                Tokens INTEGER,
                StatusMessage TEXT
            );
            CREATE INDEX idx_jobs_status ON Jobs(Status);
            CREATE INDEX idx_jobs_completed ON Jobs(CompletedAt DESC);

            PRAGMA user_version = 3;
            """;
        cmd.ExecuteNonQuery();
    }
}
