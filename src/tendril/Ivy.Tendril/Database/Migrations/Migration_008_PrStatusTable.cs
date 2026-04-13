using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_008_PrStatusTable : IMigration
{
    public int Version => 8;
    public string Description => "Add PR status cache table";

    public void Apply(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS PrStatuses (
                PrUrl TEXT PRIMARY KEY,
                Owner TEXT NOT NULL,
                Repo TEXT NOT NULL,
                Status TEXT NOT NULL,
                LastChecked TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_pr_statuses_owner_repo ON PrStatuses(Owner, Repo);
            CREATE INDEX IF NOT EXISTS idx_pr_statuses_status ON PrStatuses(Status);
            """;
        cmd.ExecuteNonQuery();

        using var setVersionCmd = connection.CreateCommand();
        setVersionCmd.CommandText = "PRAGMA user_version = 8;";
        setVersionCmd.ExecuteNonQuery();
    }
}
