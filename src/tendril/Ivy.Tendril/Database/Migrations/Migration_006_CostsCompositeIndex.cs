using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_006_CostsCompositeIndex : IMigration
{
    public int Version => 6;
    public string Description => "Add composite index on Costs(PlanId, LogTimestamp) for hourly burn queries";

    public void Apply(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE INDEX IF NOT EXISTS idx_costs_plan_logtimestamp ON Costs(PlanId, LogTimestamp);
            DROP INDEX IF EXISTS idx_costs_plan;
            DROP INDEX IF EXISTS idx_costs_logtimestamp;
            PRAGMA user_version = 6;
            """;
        cmd.ExecuteNonQuery();
    }
}
