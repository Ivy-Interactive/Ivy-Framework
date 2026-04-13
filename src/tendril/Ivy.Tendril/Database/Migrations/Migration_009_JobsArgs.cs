using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_009_JobsArgs : IMigration
{
    public int Version => 9;
    public string Description => "Add Args column to Jobs table";

    public void Apply(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "ALTER TABLE Jobs ADD COLUMN Args TEXT;";
        cmd.ExecuteNonQuery();

        using var setVersionCmd = connection.CreateCommand();
        setVersionCmd.CommandText = "PRAGMA user_version = 9;";
        setVersionCmd.ExecuteNonQuery();
    }
}
