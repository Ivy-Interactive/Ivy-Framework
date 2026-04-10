using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_005_CostsLogTimestampIndex : IMigration
{
    public int Version => 5;
    public string Description => "Add index on Costs.LogTimestamp for hourly burn queries";

    public void Apply(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE INDEX IF NOT EXISTS idx_costs_logtimestamp ON Costs(LogTimestamp);

            PRAGMA user_version = 5;
            """;
        cmd.ExecuteNonQuery();
    }
}
