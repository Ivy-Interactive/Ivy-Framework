using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_004_SourceUrl : IMigration
{
    public int Version => 4;
    public string Description => "Add SourceUrl column to Plans table";

    public void Apply(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            ALTER TABLE Plans ADD COLUMN SourceUrl TEXT;

            PRAGMA user_version = 4;
            """;
        cmd.ExecuteNonQuery();
    }
}
